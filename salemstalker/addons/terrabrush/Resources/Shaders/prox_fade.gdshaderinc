#define USE_DEPTH_TEXTURE(_) uniform sampler2D _DEPTH_TEXTURE : hint_depth_texture, repeat_disable, filter_nearest;
#define PROXIMITY_FADE(dst) (_proximity_fade(dst, VERTEX, _DEPTH_TEXTURE, SCREEN_UV, INV_PROJECTION_MATRIX))
#define PROXIMITY_FADE_TEX(tex, dst) (_proximity_fade(dst, VERTEX, tex, SCREEN_UV, INV_PROJECTION_MATRIX))

float _proximity_fade(float fade_dst, vec3 vertex, sampler2D depth_texture, vec2 screen_uv, mat4 inv_proj_matrix) {
	float depth = textureLod(depth_texture, screen_uv, 0.0).r;
	vec4 world_pos = inv_proj_matrix * vec4(screen_uv*2.0-1.0, depth, 1.0);
	world_pos.xyz /= world_pos.w;

	return clamp(1. - smoothstep(world_pos.z + fade_dst, world_pos.z, vertex.z), 0.0, 1.0);
}