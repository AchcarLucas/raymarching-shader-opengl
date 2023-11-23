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

void main() 
{
    FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}