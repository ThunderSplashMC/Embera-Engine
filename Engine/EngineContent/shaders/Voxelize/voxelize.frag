#version 440 core


const float PI = 3.14159265359;
const float GAMMA = 2.2;

layout (binding = 0, rgba8) uniform image3D gTexture3D;

uniform vec3 C_VIEWPOS;

in vec3 WorldPos;
in vec3 WorldNormal;

struct Material {
    sampler2D ALBEDO_TEX;
    sampler2D ROUGHNESS_TEX;
    sampler2D EMISSION_TEX;
    sampler2D NORMAL_TEX;
    vec3  albedo;
    vec3 emission;
    float metallic;
    float roughness;
    float ao;
};

struct PointLight {
    vec3 position;
    float intensity;
    float constant;
    vec3 diffuse;
    int shadows;
};

#define NR_POINT_LIGHTS 4
uniform PointLight L_POINTLIGHTS[NR_POINT_LIGHTS];

uniform Material material;

uniform int USE_TEX_0;
uniform int USE_TEX_1;
uniform int USE_TEX_2;
uniform int USE_TEX_3;
uniform int USE_TEX_4;

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
    vec3 radiance     = light.diffuse * attenuation * light.constant;        
        
    // cook-torrance brdf
    float NDF = DistributionGGX(N, H, material.roughness);        
    float G   = GeometrySmith(N, V, L,  material.roughness);      
    vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
        
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - material.metallic;

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular     = numerator / denominator;
            
    // add to outgoing radiance Lo
    float NdotL = max(dot(N, L), 0.0);               

    return (kD * material.albedo / PI + specular) * radiance * NdotL;

}




void main()
{

    vec3 V = normalize(C_VIEWPOS - WorldPos);

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, material.albedo, material.metallic);

    vec3 Lo = vec3(0);

    for (int i = 0; i < NR_POINT_LIGHTS; i++) {
        Lo += CalcPointLight(L_POINTLIGHTS[i], WorldNormal, F0, V);
    }

    vec3 position = WorldPos * 0.5f + 0.5f;
    imageStore(gTexture3D, ivec3(imageSize(gTexture3D) * position), vec4(Lo + vec3(0.06), 1.0f));
}