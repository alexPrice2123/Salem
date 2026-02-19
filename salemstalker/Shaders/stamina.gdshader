shader_type canvas_item;

uniform float freq_times_PI : hint_range(0.0, 10.0) = 2.0;
uniform float strength : hint_range(0.0, 1.0) = 0.1;
uniform float fill : hint_range(0.0, 1.0) = 0.5;


uniform float wave_speed : hint_range(0.0, 10.0) = 1.0;

uniform float amp_osc_freq : hint_range(0.0, 5.0) = 0.5;

bool draw_circle_polar(float radius,vec2 uv){

	vec2 centered_uv = (uv-0.5)*1.7;

	vec2 polar = vec2(atan(centered_uv.y,centered_uv.x),length(centered_uv));
	return polar.y < radius;
}

bool wave(vec2 uv){

    float amp_mod = sin(TIME * amp_osc_freq);

    float dynamic_amp = strength * amp_mod;
    float wave_y = dynamic_amp * sin(freq_times_PI * PI * uv.x + TIME * wave_speed) + (1.0-fill);
    return uv.y > wave_y;
}

bool reverse_wave(vec2 uv){

    float amp_mod = sin(TIME * amp_osc_freq);

    float dynamic_amp = strength * amp_mod;
    float wave_y = dynamic_amp * sin(freq_times_PI * PI * uv.x + TIME * wave_speed) + (fill-1.0);
    return uv.y > wave_y;
}

void fragment() {
    if (wave(UV)&&draw_circle_polar(0.8,UV)) {
        COLOR = vec4(0.8, 0.7, 1.0, 1.0);
	}else if (reverse_wave(UV)&&draw_circle_polar(0.8,UV)) {
        COLOR = vec4(0.0, 0.0, 0.0, 1.0);
    } else {
        COLOR = vec4(0.0);
    }
}