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

#define MAX_STEPS 100
#define MAX_DIST 100.0
#define SURF_DIST 0.01

vec3 getNormal(vec3 p);

float getDist(vec3 p);
float rayMarch(vec3 ro, vec3 rd);
float getLight(vec3 p);

float drawCylinder(vec3 p, vec3 a, vec3 b, float r);
float drawPlane(vec3 p);

vec3 getNormal(vec3 p)
{
    float d = getDist(p);
    vec2 e = vec2(0.01, 0.0);
    vec3 n = vec3(
                    d - getDist(p - e.xyy),
                    d - getDist(p - e.yxy),
                    d - getDist(p - e.yyx));
    return normalize(n);
}

float getLight(vec3 p)
{
    vec3 p_light = vec3(0, 7, -3);
    p_light.xz += vec2(sin(iTime), cos(iTime));
    vec3 v_light = normalize(p_light - p);
    vec3 normal = getNormal(p);

    float diffuse = clamp(dot(v_light, normal), 0.0, 1.0);

    float d = rayMarch(p + normal * SURF_DIST, v_light);

    // vamos verificar se ocorreu alguma intersecção entre a luz e o ponto
    if(d < length(p_light - p))
    {
        diffuse *= 0.1f;
    }

    return diffuse;
}

float drawPlane(vec3 p)
{
    return p.y;
}

float drawCylinder(vec3 p, vec3 a, vec3 b, float r)
{
    vec3 ap = p - a;
    vec3 ab = b - a;

    float t = dot(ap, ab) / dot(ab, ab);

    vec3 c = a + t * ab;

    float x = length(p - c) - r;
    float y = (abs(t - 0.5) - 0.5) * length(ab);
    float e = length(max(vec2(x, y), 0.0));
    float i = min(max(x, y), 0.0);

    return e + i;
}

float getDist(vec3 p)
{
    float d = min(
        drawCylinder(p, vec3(-2, 0.8, 4), vec3(+2, 0.8, 2), 0.3), 
        drawPlane(p)
    );

    return d;
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
    vec3 ro = vec3(0, 4, -8);

    // ray direction
    vec3 rd = normalize(vec3(uv.x, uv.y - 0.2, 1));

    float d = rayMarch(ro, rd);

    color = vec3(getLight(ro + rd * d));

    fragColor = vec4(color, 1.0);
}