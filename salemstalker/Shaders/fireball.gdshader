/*
	動的魔法水球シェーダー by あるる（きのもと 結衣）
	Procedural Magical Waterball Shader by KINOMOTO Yui @arlez80
	MIT License
	Updated for Godot 4.x
*/
shader_type spatial;
render_mode depth_draw_opaque, unshaded, shadows_disabled, cull_disabled;

uniform sampler2D noise_tex : hint_default_white;
uniform vec4 root_color : source_color = vec4(1.0, 0.75, 0.3, 1.0);
uniform vec4 tip_color : source_color = vec4(1.0, 0.03, 0.001, 1.0);
uniform vec4 specular_color : source_color = vec4(0.3, 1.0, 1.0, 1.0);
uniform float fire_alpha : hint_range(0.0, 1.0) = 1.0;
uniform vec2 fire_speed = vec2(0.0, 1.0);
uniform float fire_aperture : hint_range(0.0, 3.0) = 0.22;
uniform vec2 wave_noise_speed = vec2(0.42, 0.23);
uniform float wave_noise_scale = 0.1;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment() {
	vec2 rev_uv = vec2(UV.x, 1.0 - UV.y);
	vec2 shifted_uv = rev_uv + TIME * fire_speed;
	float fire_noise = texture(noise_tex, shifted_uv).r;
	float noise = rev_uv.y * (((rev_uv.y + fire_aperture) * fire_noise - fire_aperture) * 75.0);
	vec4 fire_color = mix(tip_color, root_color, noise * 0.05);
	vec2 wave_shift = ((texture(noise_tex, shifted_uv).xy + texture(noise_tex, shifted_uv * 2.0).xy) * 0.5 - vec2(0.5, 0.5)) * wave_noise_scale;
	vec2 screen_uv = SCREEN_UV + wave_shift;
	ALPHA = clamp(noise, 0.0, 1.0) * fire_alpha;
	ALBEDO = mix(
		texture(SCREEN_TEXTURE, screen_uv).rgb * fire_color.rgb,
		specular_color.rgb,
		clamp(1.0 - abs(dot(VERTEX, NORMAL + vec3(wave_shift * 300.0, 0.0))), 0.0, 1.0)
	);
}