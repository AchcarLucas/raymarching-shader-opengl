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

vec4 mainImage(vec2 fragCoord)
{
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec3 color = vec3(0);

    // ray origen
    vec3 ro = vec3(0, 1, 0);
    // ray direction
    vec3 rd = normalize(vec3(uv.x, uv.y, 1));

    return vec4(color, 1.0);
}
