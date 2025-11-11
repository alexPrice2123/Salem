shader_type spatial;
render_mode blend_mix, depth_prepass_alpha, cull_back;

uniform sampler2D albedo_tex;
uniform sampler2D opacity_tex;

uniform float opacity_strength = 1.0; // overall multiplier
uniform float threshold = 0.0;        // cutoff for mask
uniform float feather = 0.02;         // softness around threshold
uniform bool use_smooth = true;

void fragment() {
    vec4 albedo_color = texture(albedo_tex, UV);
    float mask = texture(opacity_tex, UV).r;

    float mask_soft = mask;
    if (use_smooth) {
        mask_soft = smoothstep(threshold - feather, threshold + feather, mask);
    } else {
        mask_soft = mask >= threshold ? 1.0 : 0.0;
    }

    float final_alpha = albedo_color.a * mask_soft * opacity_strength;

    ALBEDO = albedo_color.rgb;
    ALPHA = final_alpha;

    METALLIC = 0.0;
    SPECULAR = mix(0.5, 0.0, 1.0 - final_alpha);

    if (ALPHA <= 0.01) {
        discard;
    }
}
