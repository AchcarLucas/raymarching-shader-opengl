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

#define MAX_STEPS 100
#define MAX_DIST 100.0
#define SURF_DIST 0.01

float getDist(vec3 p);
float rayMarch(vec3 ro, vec3 rd);

float drawSphere(vec3 p, vec3 a, float r);
float drawPlane(vec3 p);

float drawSphere(vec3 p, vec3 a, float r)
{
    return length(p - a) - r;
}

float drawPlane(vec3 p)
{
    return p.y;
}

float getDist(vec3 p)
{
    return min(drawSphere(p, vec3(0, 1, 6), 1.0), drawPlane(p));
}

float rayMarch(vec3 ro, vec3 rd) 
{
    // distância inicial
    float d0 = 0.0;

    for(int i = 0; i < MAX_STEPS; ++i) {
        /*
         * move o ponto da origem para a direção multiplicado 
         * pela distância da intersecção da esfera
         * PS: a intersecção não necessariamente será a primeira
         * será N intesecções até o máximo de etapas definidas
         */
        vec3 p = ro + rd * d0;
        // obtém a próxima distância de intersecção
        float _ds = getDist(p);
        // move o ponto de intersecção
        d0 += _ds;
        /*
         * se atingir a distância máxima ou se atingir uma superficie
         * paramos a execução do for
         */
        if(d0 > MAX_DIST || _ds < SURF_DIST) {
            break;
        }
    }

     /*
      * retorna a última distância encontrada seguindo 
      * as regras de MAX_STEPS, MAX_DIST e SURF_DIST
      */
    return d0;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;

    vec3 color = vec3(0);

    // ray origen
    vec3 ro = vec3(0, 1, 0);

    // ray direction
    vec3 rd = normalize(vec3(uv.x, uv.y, 1));

    color = vec3(rayMarch(ro, rd)) / 20.0;
    fragColor = vec4(color, 1.0);
}