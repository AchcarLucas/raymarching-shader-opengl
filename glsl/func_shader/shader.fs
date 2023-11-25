#version 330 core

out vec4 fragColor;

in struct VS_DATA {
    vec3 position;
    vec3 normal;
    vec2 tex;
} vs_out;

layout (std140) uniform General {
    vec2 resolution;
    vec2 mouse;
    float time;
} general;

vec2 iResolution = general.resolution;
vec2 iMouse = general.mouse;
float iTime = general.time;

void mainImage(out vec4 fragColor, in vec2 fragCoord);

void main()
{
    mainImage(fragColor, gl_FragCoord.xy);
}

/////////////////////////////////////////////////////////////////////

float bias = 0.002f;

float f(float x) {
    return x + 0.2f;
}

float g(float x) {
    return (x * x) + 0.2f;
}

float smooth_func(float func, vec2 uv, float _percentage) {
    float dist = abs(distance(uv, vec2(uv.x, func)));
    return smoothstep(0.0, 1.5f / iResolution.y, dist);
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{    
    vec2 aspect = iResolution.xy / iResolution.y;

    vec2 uv = (fragCoord.xy / iResolution.xy);

    uv.x += -0.5f;
    uv.y += -0.1f;

    uv *= aspect;
    
    vec3 color = vec3(0, 0, 0);
    
    vec3 red = vec3(1.0, 0.0, 0.0);
    vec3 green = vec3(0.0, 1.0, 0.0);
   
    /*if(smooth_func(f(uv.x), uv, bias) < 0.1f) {
        color.r = 1.0f;
    }
    
    if(smooth_func(g(uv.x), uv, bias) < 0.1f) {
        color.r = 1.0f;
    }*/
    
    color = mix(red, color, smooth_func(f(uv.x), uv, bias));
    color = mix(red, color, smooth_func(g(uv.x), uv, bias));
    
    /*if(smooth_func(f_min, uv, bias) < 0.1f) {
        color.g = 1.0f;
        color.r = 0.0f;
    }*/
    
    float f_min = min(g(uv.x), f(uv.x));
    
    color = mix(green, color, smooth_func(f_min, uv, 0.002f));

    fragColor = vec4(color, 1.0);
}