#version 330 core

out vec4 FragColor;

in struct VS_DATA {
    vec3 position;
    vec3 normal;
    vec2 tex;
} vs_out;

layout (std140) uniform General {
    float width;
	float height;
} general;

vec2 iResolution = vec2(general.width, general.height);

vec4 mainImage(vec2 fragCoord);

void main() 
{
    FragColor = mainImage(vec2(vs_out.position));
}

/////////////////////////////////////////////////////////////////////

vec4 mainImage(vec2 fragCoord)
{
    return vec4(0.0, 1.0, 0.0, 1.0);
}
