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
}

/////////////////////////////////////////////////////////////////////

#define PI 3.1415926538

#define MAX_STEPS 100
#define MAX_DIST 100.0
#define SURF_DIST 0.001

#define DEG_TO_RAD (PI / 180.0)

vec3 getNormal(vec3 p);

float getDist(vec3 p);
float rayMarch(vec3 ro, vec3 rd);
float drawSphere(vec3 p, vec3 position, float radiuns);
float drawPlane(vec3 p);
float getLight(vec3 p);

mat2 rot2DMat(float angle);

mat2 rot2DMat(float a)
{
    float s = sin(a);
    float c = cos(a);
    return mat2(c, -s, s, c);
}

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
    vec3 p_light = vec3(-3, 5, -4);
    // p_light.xz += vec2(sin(iTime), cos(iTime));
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

float drawBox(vec3 p, vec3 s)
{
    vec3 q = abs(p) - s;
    return length(max(q, 0.0)) + min(max(q.x,max(q.y, q.z)),0.0);
}

float getDist(vec3 p)
{
    float d = min(
        drawBox(p - vec3(0, 1, 0), vec3(1.0, 1.0, 1.0)), 
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

vec3 R(vec2 uv, vec3 p, vec3 l, float z) {
    vec3 f = normalize(l-p),
    r = normalize(cross(vec3(0, 1, 0), f)),
    u = cross(f, r),
    c = p + f * z,
    i = c + uv.x * r + uv.y * u,
    d = normalize(i - p);
    return d;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec2 mouse = iMouse.xy / iResolution.xy;

    vec4 color = vec4(0);

    // ray origen
    vec3 ro = vec3(0, 4, -5);
    ro.yz *= rot2DMat(-mouse.y + 0.4);

    // ro.xz *= rot2DMat(iTime * 0.2 - mouse.x * 2 * PI);
    ro.xz *= rot2DMat(mouse.x * 2 * PI);
    
    // ray distance
    vec3 rd = R(uv, ro, vec3(0,0,0), 0.7);

    float d = rayMarch(ro, rd);

    d = getLight(ro + rd * d);

    fragColor = gammaCorrection(vec4(d, d, d, 1.0));
}