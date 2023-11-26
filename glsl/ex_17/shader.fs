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
// Configuração das Channels (sampler2D, samplerCube or sampler3D)
// Configuration into iChannel.txt
/////////////////////////////////////////////////////////////////////

uniform sampler2D iChannel0;

/////////////////////////////////////////////////////////////////////

#define PI 3.1415926538

#define MAX_STEPS 100
#define MAX_DIST 100.0
#define SURF_DIST 0.001

#define DEG_TO_RAD (PI / 180.0)

vec4 gammaCorrection(vec4 color);

vec3 getNormal(vec3 p);

float getDist(vec3 p);
float rayMarch(vec3 ro, vec3 rd);
float getLight(vec3 p);

float drawSphere(vec3 p, vec3 a, float r);
float drawCapsule(vec3 p, vec3 a, vec3 b, float r);
float drawTorus(vec3 p, float r, float s);
float drawBox(vec3 p, vec3 s);
float drawPlane(vec3 p, vec3 n, float d);

// ********** Operation **********

float smooth_min(float a, float b, float k);
float smooth_max(float a, float b, float k);

float smooth_min(float a, float b, float k) 
{
    float h = clamp(0.5 + 0.5 * (a - b) / k, 0.0, 1.0);
    return mix(a, b, h) - k * h * (1.0 - h);
}

float smooth_max(float a, float b, float k)
{
    return -smooth_min(-a, -b, k);
}

float subtractOp(float a, float b);
float intersectionOp(float a, float b);
float unionOp(float a, float b);
float shellOp(float a, float k);

float smoothSubtractOp(float a, float b, float k);
float smoothIntersectionOp(float a, float b, float k);
float smoothUnionOp(float a, float b, float k);

float subtractOp(float a, float b)
{
    return max(-a, b);
}

float intersectionOp(float a, float b)
{
    return max(a, b);
}

float unionOp(float a, float b)
{
    return min(a, b);
}

float shellOp(float a, float k)
{
    return abs(a) - k;
}

float smoothSubtractOp(float a, float b, float k)
{
    return smooth_max(-a, b, k);
}

float smoothIntersectionOp(float a, float b, float k)
{
    return smooth_max(a, b, k);
}

float smoothUnionOp(float a, float b, float k)
{
    return smooth_min(a, b, k);
}

// *****************************

mat2 rot2DMat(float a);

vec4 gammaCorrection(vec4 color)
{
    float gamma = 2.2;
    return pow(color, 1.0 / vec4(gamma));
}

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
    vec3 p_light = vec3(3, 5, 4);
    // p_light.xz += vec2(sin(iTime), cos(iTime));

    vec3 v_light = normalize(p_light - p);
    vec3 normal = getNormal(p);

    float diffuse = clamp(dot(normal, v_light) * 0.5 + 0.5, 0.0, 1.0);

    float d = rayMarch(p + normal * SURF_DIST * 2.0, v_light);

    // vamos verificar se ocorreu alguma intersecção entre a luz e o ponto
    if(p.y < 0.01 && d < length(p_light - p)) diffuse *= .5;

    return diffuse;
}

float drawSphere(vec3 p, vec3 a, float r)
{
    return length(p - a) - r;
}

float drawCapsule(vec3 p, vec3 a, vec3 b, float r)
{
    vec3 ap = p - a;
    vec3 ab = b - a;

    float t = dot(ap, ab) / dot(ab, ab);
    t = clamp(t, 0.0, 1.0);

    vec3 c = a + t * ab;

    return length(p - c)  - r;
}

float drawTorus(vec3 p, float r, float s)
{
    float x = length(p.xz) - r;
    float y = p.y;

    return length(vec2(x, y)) - s;
}

float drawBox(vec3 p, vec3 s)
{
    vec3 q = abs(p) - s;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)),0.0);
}

float drawPlane(vec3 p, vec3 n, float d)
{
    return dot(p, normalize(n)) - d;
}

float getDist(vec3 p)
{
    return drawBox(p - vec3(0, 1, 0), vec3(1.0, 1.0, 1.0));
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
        if(d0 > MAX_DIST || abs(_ds) < SURF_DIST) {
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
    ro.xz *= rot2DMat(mouse.x * 2.0 * PI);
    
    // ray direction
    vec3 rd = R(uv, ro, vec3(0,0,0), 0.7);

    float d = rayMarch(ro, rd);

    d = (d < MAX_DIST) ? getLight(ro + rd * d) : 0.0;

    //fragColor = gammaCorrection(vec4(d, d, d, 1.0));

    uv = fragCoord/iResolution.xy;
    fragColor = vec4(texture(iChannel0, uv).rgb, 1.0);
}