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

// "ShaderToy Tutorial - Ray Marching Primitives" 
// by Martijn Steinrucken aka BigWings/CountFrolic - 2018
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
//
// This shader is part of a tutorial on YouTube
// https://youtu.be/Ff0jJyyiVyw

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001

float sdCapsule(vec3 p, vec3 a, vec3 b, float r) {
	vec3 ab = b-a;
    vec3 ap = p-a;
    
    float t = dot(ab, ap) / dot(ab, ab);
    t = clamp(t, 0., 1.);
    
    vec3 c = a + t*ab;
    
    return length(p-c)-r;
}

float sdCylinder(vec3 p, vec3 a, vec3 b, float r) {
	vec3 ab = b-a;
    vec3 ap = p-a;
    
    float t = dot(ab, ap) / dot(ab, ab);
    //t = clamp(t, 0., 1.);
    
    vec3 c = a + t*ab;
    
    float x = length(p-c)-r;
    float y = (abs(t-.5)-.5)*length(ab);
    float e = length(max(vec2(x, y), 0.));
    float i = min(max(x, y), 0.);
    
    return e+i;
}

float sdTorus(vec3 p, vec2 r) {
	float x = length(p.xz)-r.x;
    return length(vec2(x, p.y))-r.y;
}

float dBox(vec3 p, vec3 s) {
	return length(max(abs(p)-s, 0.));
}


float GetDist(vec3 p) {
	vec4 s = vec4(0, 1, 6, 1);
    
    float sphereDist =  length(p-s.xyz)-s.w;
    float planeDist = p.y;
    
    float cd = sdCapsule(p, vec3(3, .5, 6), vec3(3, 2.5, 6), .5); 
    float td = sdTorus(p-vec3(0,.5,6), vec2(1.5, .4));
    float bd = dBox(p-vec3(-3.5, 1, 6), vec3(1,.75,1));
    float cyld = sdCylinder(p, vec3(0, .3, 3), vec3(3, .3, 5), .3);
    
    float d = min(cd, planeDist);
    d = min(d, td);
    d = min(d, bd);
    
    d = min(d, cyld);
    
    return d;
}

float RayMarch(vec3 ro, vec3 rd) {
	float dO=0.;
    
    for(int i=0; i<MAX_STEPS; i++) {
    	vec3 p = ro + rd*dO;
        float dS = GetDist(p);
        dO += dS;
        if(dO>MAX_DIST || dS<SURF_DIST) break;
    }
    
    return dO;
}

vec3 GetNormal(vec3 p) {
	float d = GetDist(p);
    vec2 e = vec2(.001, 0);
    
    vec3 n = d - vec3(
        GetDist(p-e.xyy),
        GetDist(p-e.yxy),
        GetDist(p-e.yyx));
    
    return normalize(n);
}

float GetLight(vec3 p) {
    vec3 lightPos = vec3(0, 5, 6);
    lightPos.xz += vec2(sin(iTime), cos(iTime))*2.;
    vec3 l = normalize(lightPos-p);
    vec3 n = GetNormal(p);
    
    float dif = clamp(dot(n, l), 0., 1.);
    float d = RayMarch(p+n*SURF_DIST*2., l);
    if(d<length(lightPos-p)) dif *= .1;
    
    return dif;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = (fragCoord-.5*iResolution.xy)/iResolution.y;

    vec3 col = vec3(0);
    
    vec3 ro = vec3(0, 3, -8);
    vec3 rd = normalize(vec3(uv.x-0.05, uv.y, 1.2));

    float d = RayMarch(ro, rd);
    
    vec3 p = ro + rd * d;
    
    float dif = GetLight(p);
    col = vec3(dif);
    
    col = pow(col, vec3(.4545));	// gamma correction
    
    fragColor = vec4(col,1.0);
}