#version 440 core

const float PI = 3.14159265359;
const float GAMMA = 2.2;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 EmissionColor;

in vec2 texCoords;
in vec3 Normal;
in vec3 Tangent;
in vec3 BiTangent;
in vec3 FragPos;
in vec3 WorldPos;

uniform vec4 COLOR;
uniform vec3 C_VIEWPOS;

struct PointLight {
    vec3 position;
    float intensity;
    float constant;
    vec3 diffuse;
    int shadows;
};

struct SpotLight {

    vec3 position;
    vec3 diffuse;
    float intensity;
    float cutOff;
    float outerCutOff;
    vec3 direction;

};

struct Material {
    sampler2D ALBEDO_TEX;
    sampler2D ROUGHNESS_TEX;
    sampler2D EMISSION_TEX;
    sampler2D NORMAL_TEX;
    vec3  albedo;
    vec3 emission;
    float emissionStr;
    float metallic;
    float roughness;
    float ao;
};

#define NR_POINT_LIGHTS 8
uniform PointLight L_POINTLIGHTS[NR_POINT_LIGHTS];

#define NR_SPOT_LIGHTS 8
uniform SpotLight L_SPOTLIGHTS[NR_SPOT_LIGHTS];

uniform samplerCube W_SHADOW_BUFFERS[NR_POINT_LIGHTS];

// IMAGE BASED LIGHTING

uniform samplerCube W_IRRADIANCE_MAP;
uniform samplerCube W_PREFILTER_MAP;
uniform sampler2D W_BRDF_LUT_MAP;

uniform Material material;
uniform int USE_TEX_0;
uniform int USE_TEX_1;
uniform int USE_TEX_2;
uniform int USE_TEX_3;
uniform int USE_TEX_4;

float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness);
vec3 IBL(Material material, vec3 N, vec3 V, vec3 F0, vec3 R);

vec3 GammaCorrectTexture(sampler2D tex, vec2 uv)
{
	vec4 samp = texture(tex, uv);
	return vec3(pow(samp.rgb, vec3(GAMMA)));
}

vec3 GammaCorrectTextureAlpha(sampler2D tex, vec2 uv)
{
	vec4 samp = texture(tex, uv);
	return vec3(pow(samp.rgb, vec3(GAMMA)));
}

vec3 GetAlbedo() {
    return (1.0 - USE_TEX_0) * material.albedo + USE_TEX_0 * GammaCorrectTexture(material.ALBEDO_TEX, texCoords);
}

float GetAlbedoAlpha() {
    return (1 - USE_TEX_0) * 1 + USE_TEX_0 * texture(material.ALBEDO_TEX, texCoords).a;
}

float GetGloss()
{
	return (1.0 - USE_TEX_1) * (material.roughness + 0.001) + USE_TEX_1 * texture(material.ROUGHNESS_TEX, texCoords).r;
}

float GetRoughness()
{
	return  GetGloss();
}

vec3 GetEmission() {

    return (1.0 - USE_TEX_2) * material.emission + USE_TEX_2 * GammaCorrectTexture(material.EMISSION_TEX, texCoords);

}

vec3 GetNormal(vec3 N) {

    mat3 toWorld = mat3(Tangent, BiTangent, N);
    vec3 normalMap = texture(material.NORMAL_TEX, texCoords).rgb * 2.0 - 1.0;
    normalMap = toWorld * normalMap;
    return (1.0 - USE_TEX_3) * N + USE_TEX_3 * normalMap;

}

vec3 IBL(vec3 N, vec3 V, vec3 F0, vec3 R)
{

    float roughness = GetRoughness();

    // ambient lighting (we now use IBL as the ambient term)
    vec3 F = fresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);
    
    vec3 kS = F;
    vec3 kD = 1.0 - kS;
    kD *= 1.0 - material.metallic;	  
    
    vec3 irradiance = texture(W_IRRADIANCE_MAP, N).rgb;
    vec3 diffuse      = irradiance * GetAlbedo();
    
    // sample both the pre-filter map and the BRDF lut and combine them together as per the Split-Sum approximation to get the IBL specular part.
    const float MAX_REFLECTION_LOD = 4.0;
    vec3 prefilteredColor = textureLod(W_PREFILTER_MAP, R,  roughness * MAX_REFLECTION_LOD).rgb;    
    vec2 brdf  = texture(W_BRDF_LUT_MAP, vec2(max(dot(N, V), 0.0), roughness)).rg;
    vec3 specular = prefilteredColor * (F * brdf.x + brdf.y);

    return (kD * diffuse + specular) * material.ao;

}

vec3 CalcPointLight(PointLight light, vec3 N, vec3 F0, vec3 V) {
    vec3 L = normalize(light.position - WorldPos);
    vec3 H = normalize(V + L);
    float distance    = length(light.position - WorldPos);
    float attenuation = (1.0 / (distance * distance)) * light.intensity;
    vec3 radiance     = light.diffuse * attenuation * light.constant;        
        
    // cook-torrance brdf
    float NDF = DistributionGGX(N, H, GetRoughness());        
    float G   = GeometrySmith(N, V, L, GetRoughness());      
    vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
        
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - material.metallic;

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular     = numerator / denominator;
            
    // add to outgoing radiance Lo
    float NdotL = max(dot(N, L), 0.0);               

    return (kD * GetAlbedo() / PI + specular) * radiance * NdotL;

}

vec3 CalcSpotLight(SpotLight light, vec3 N, vec3 F0, vec3 V) {
    vec3 L = normalize(light.position - WorldPos);
    vec3 H = normalize(V + L);

    float theta = dot(L, -normalize(light.direction)); 
    float attenuation = smoothstep(light.outerCutOff, light.cutOff, theta);

    // cook-torrance brdf
    float NDF = DistributionGGX(N, H, GetRoughness());
    float G = GeometrySmith(N, V, L, GetRoughness());
    vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - material.metallic;

    float NdotL = max(dot(N, L), 0.0);

    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular = numerator / denominator;

    // add to outgoing radiance Lo
    vec3 radiance = light.diffuse * light.intensity * attenuation;
    
    return (kD * GetAlbedo() / PI + specular) * radiance * NdotL;
}

vec3 sampleOffsetDirections[20] = vec3[]
(
   vec3( 1,  1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1,  1,  1), 
   vec3( 1,  1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1,  1, -1),
   vec3( 1,  1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1,  1,  0),
   vec3( 1,  0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1,  0, -1),
   vec3( 0,  1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0,  1, -1)
);   

float ShadowCalculation(vec3 fragPos,vec3 lightPos, int index)
{
    float samples = 6;
    float offset  = 0.1;
    float shadow = 0;
    float diskRadius = 0.05;
    float bias = 0.05;

    for(int i = 0; i < samples; i++)
    {
        vec3 fragToLight = fragPos - lightPos;
        float closestDepth = texture(W_SHADOW_BUFFERS[index], fragToLight + sampleOffsetDirections[i] * diskRadius).r;
        closestDepth *= 300;   // undo mapping [0;1]
        float currentDepth = length(fragToLight);
        shadow += currentDepth -  bias > closestDepth ? 1.0 : 0.0;
    }

    shadow /= float(samples);

    return shadow;
}

void main() {
    


    vec3 N = normalize(Normal);
    vec3 V = normalize(C_VIEWPOS - WorldPos);

    

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, GetAlbedo(), material.metallic);

    vec3 Lo = vec3(0);

    for (int i = 0; i < NR_POINT_LIGHTS; i++) {
        //float shadow = ShadowCalculation(FragPos, L_POINTLIGHTS[i].position, i);
        Lo += CalcPointLight(L_POINTLIGHTS[i], GetNormal(N), F0, V);
    }

    for (int i = 0; i < NR_SPOT_LIGHTS; i++) {
        Lo += CalcSpotLight(L_SPOTLIGHTS[i], GetNormal(N), F0, V);
    }

    //float ratio = 1.00 / 1.52; // refractive index of glass
    vec3 I = normalize(WorldPos - C_VIEWPOS);
    vec3 R = reflect(I, normalize(Normal));

    vec3 ambient = IBL(N, V, F0, R);

    vec3 color = ambient + Lo;
    
    color = color / (color + vec3(1.0));

    FragColor = vec4(color, GetAlbedoAlpha());
    EmissionColor = vec4(GetEmission() * material.emissionStr, 1.0);
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

// ----------------------------------------------------------------------------
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}