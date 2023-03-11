#version 440 core

const float PI = 3.14159265359;
const float GAMMA = 2.2;

struct PointLight {
    vec3 position;
    float intensity;
    float constant;
    vec3 diffuse;
    int shadows;
};


#define NR_POINT_LIGHTS 4
uniform PointLight L_POINTLIGHTS[NR_POINT_LIGHTS];

in vec3 WorldPos;
in vec3 WorldNormal;
uniform vec3 C_VIEWPOS;
layout (binding = 0, rgba8) uniform image3D gTexture3D;

uniform vec3 color;
 
bool isInsideUnitCube()
{
    return true;
    return abs(WorldPos.x) < 2.5f && abs(WorldPos.y) < 2.5f && abs(WorldPos.z) < 2.5f;
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

vec3 CalcPointLight(PointLight light, vec3 N, vec3 F0, vec3 V) {
    vec3 L = normalize(light.position - WorldPos);
    vec3 H = normalize(V + L);
    float distance    = length(light.position - WorldPos);
    float attenuation = (1.0 / (distance * distance)) * light.intensity;
    vec3 radiance     = light.diffuse * attenuation;        
        
    // cook-torrance brdf
    float NDF = DistributionGGX(N, H, 0.9);        
    float G   = GeometrySmith(N, V, L, 0.9);      
    vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
        
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - 0.5;

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular     = numerator / denominator;
    float NdotL = max(dot(N, L), 0.0);               

    return (kD * vec3(1.0) / PI + specular) * radiance * NdotL;

}
 
void main()
{
    if (!isInsideUnitCube())
    {
        return;
    }
 
    // ...

    vec3 N = normalize(WorldNormal);
    vec3 V = normalize(C_VIEWPOS - WorldPos);

    

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, vec3(1.0), 0.5);

    vec3 final = vec3(0.2);

    for (int i = 0; i < NR_POINT_LIGHTS; i++) {
        final += CalcPointLight(L_POINTLIGHTS[i], N, F0, V);
    }
 
    vec3 position = WorldPos * 0.5f + 0.5f;
    imageStore(gTexture3D, ivec3(imageSize(gTexture3D) * position), vec4(1.0));
}