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

float DrawCircle(vec2 uv, vec2 p, float r) {
    float d = length(p - uv) - r;
    return smoothstep(0.0, 1.5f / iResolution.y, d);
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    float color = 0.0f;
    
    color += DrawCircle(uv, vec2(sin(iTime) * 0.8, 0), 0.05);
   
    fragColor = vec4(vec3(color),1.0);
}