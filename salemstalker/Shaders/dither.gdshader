shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

// ==== Wiggle ====
uniform float wiggleMult : hint_range(0.0, 100.00, 0.0001) = 0.005;
uniform float wiggleTime : hint_range(0.0, 50.0, 0.1) = 3.0;
uniform float wiggleY : hint_range(0.0, 100.0, 0.1) = 5.0;
uniform float BlendAmount : hint_range(-10.0, 10.0, 0.05) = 0.5;

// ==== Resolution / color depth / dithering ====
uniform bool change_color_depth = false;
uniform int target_color_depth : hint_range(1, 8) = 5;
uniform bool dithering = false;
uniform bool scale_resolution = false;
uniform int target_resolution_scale = 3;

// ==== Dither pattern ====
int dithering_pattern(ivec2 fragcoord) {
	const int pattern[] = {
		-4, +0, -3, +1,
		+2, -2, +3, -1,
		-3, +1, -4, +0,
		+3, -1, +2, -2
	};
	int x = fragcoord.x % 4;
	int y = fragcoord.y % 4;
	return pattern[y * 4 + x];
}

// ==== MAIN ====
void fragment() {
	vec2 iResolution = 1.0 / SCREEN_PIXEL_SIZE;
	vec2 uv = FRAGCOORD.xy / iResolution.xy;

	// --- Base color (normal sample, no wiggle yet) ---
	ivec2 tex_uv = ivec2(FRAGCOORD.xy);
	vec3 color;

	if (scale_resolution) {
		tex_uv = ivec2(FRAGCOORD.xy / float(target_resolution_scale));
		color = texelFetch(screen_texture, tex_uv * target_resolution_scale, 0).rgb;
	} else {
		color = texelFetch(screen_texture, tex_uv, 0).rgb;
	}

	// --- Dithering + color depth ---
	ivec3 c = ivec3(round(color * 255.0));
	if (dithering) c += ivec3(dithering_pattern(tex_uv));

	vec3 processed_color;
	if (change_color_depth) {
		c >>= (8 - target_color_depth);
		processed_color = vec3(c) / float(1 << target_color_depth);
	} else {
		processed_color = vec3(c) / 255.0;
	}

	// --- Apply wiggle distortion *after* processing ---
	float xOffset = sin((wiggleTime + 0.5) * TIME + uv.y * (wiggleY + 1.0))
	              * sin((wiggleTime + 0.9) * TIME + uv.y * wiggleY)
	              * sin((wiggleTime + 0.2 + 0.33) * TIME + uv.y * (wiggleY - 1.0))
	              * wiggleMult;

	vec2 wiggledUV = uv + vec2(xOffset, 0.0);
	vec3 final_color = texture(screen_texture, wiggledUV).rgb;

	// Blend wiggled version with processed version for smoother look
	final_color = mix(processed_color, final_color, BlendAmount);

	COLOR = vec4(final_color, 1.0);
}
