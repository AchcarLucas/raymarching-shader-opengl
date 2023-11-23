#version 330 core

out vec4 fragColor;

in struct VS_DATA {
    vec3 position;
    vec3 normal;
    vec2 tex;
} vs_out;

layout (std140) uniform General {
    int width;
	int height;
    float time;
} general;

vec2 iResolution = vec2(general.width, general.height);
float iTime = general.time;

void mainImage(out vec4 fragColor, in vec2 fragCoord);
vec4 gammaCorrection(vec4 color);

vec4 gammaCorrection(vec4 color)
{
    float gamma = 2.2;
    return pow(color, 1.0 / vec4(gamma));
}

void main()
{
    mainImage(fragColor, gl_FragCoord.xy);
    // fragColor = gammaCorrection(fragColor);
}

/////////////////////////////////////////////////////////////////////

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
 fragColor = vec4(1.0, 0.0, 0.0, 1.0);   
}