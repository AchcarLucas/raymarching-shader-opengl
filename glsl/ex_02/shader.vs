#version 330 core

layout (location = 0) in vec3 iPosition;
layout (location = 1) in vec3 iNormal;
layout (location = 2) in vec2 iTex;

out struct VS_DATA {
    vec3 position;
    vec3 normal;
    vec2 tex;
} vs_out;

void main()
{
    vs_out.position = iPosition;
    vs_out.normal = iNormal;
    vs_out.tex = iTex;
    gl_Position = vec4(iPosition, 1.0);
}
