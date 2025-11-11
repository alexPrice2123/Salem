shader_type spatial;

uniform vec3 target_position;        // The point to fade near (e.g. player or camera)
uniform float fade_distance = 5.0;   // Distance over which the fade happens
uniform vec4 base_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float fade_smoothness = 1.0; // Controls how smooth the fade edge is

void fragment() {
    // Get world position of this fragment
    vec3 world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;

    // Distance between this pixel and the target
    float dist = distance(world_pos, target_position);

    // Fade factor: 1 = fully visible, 0 = fully invisible
    float fade = smoothstep(0.0, fade_distance * fade_smoothness, dist);

    ALBEDO = base_color.rgb;
    ALPHA = base_color.a * fade;
}
