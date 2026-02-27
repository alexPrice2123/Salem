/*
    Magical Shield - Alpha Controlled Version
    Godot 4.5
*/

shader_type spatial;
render_mode unshaded, depth_draw_never, cull_back, blend_mix;

uniform vec2 speed = vec2(0.2, 0.2);
uniform vec4 barrier_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

uniform float barrier_force = 1.2;
uniform float barrier_fog_noise_force = 0.6;

uniform float distortion_strength = 0.15;
uniform float distortion_speed = 1.5;
uniform float vertex_wave_strength = 0.1;

uniform float emission_strength = 2.0;

uniform sampler2D barrier_noise : hint_default_black;

void vertex() {
    float wave = texture(barrier_noise, UV + TIME * 0.1).r;
    float pulse = sin(TIME * distortion_speed) * 0.5 + 0.5;
    VERTEX += NORMAL * wave * vertex_wave_strength * pulse;
}

void fragment() {

    vec2 distortion = texture(
        barrier_noise,
        UV * 2.0 + TIME * speed
    ).rg - 0.5;

    vec2 distorted_uv = UV + distortion * distortion_strength;

    float rim = pow(1.0 - dot(NORMAL, VIEW), 3.5) * barrier_force;

    float fog_noise = texture(
        barrier_noise,
        distorted_uv * 6.0 - TIME * speed
    ).r * barrier_fog_noise_force;

    float computed_alpha = clamp(rim + fog_noise, 0.0, 1.0);

    // 🔹 Multiply by color alpha so inspector controls transparency
    float final_alpha = computed_alpha * barrier_color.a;

    ALBEDO = barrier_color.rgb;
    EMISSION = barrier_color.rgb * emission_strength * rim;
    ALPHA = final_alpha;
}