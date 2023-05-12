#version 440 core

const float PI = 3.14159265359;
const float GAMMA = 2.2;

layout (location = 0) out vec4 gPosition;
layout (location = 1) out vec4 gNormal;
layout (location = 2) out vec4 gSpecRough;
layout (location = 3) out vec4 gSceneAlbedo;
layout (location = 4) out int gObjectUUID;

in vec3 Normal;
in vec2 texCoords;
in vec3 FragPos;
in vec4 VertexPosition;

layout (binding = 0) uniform sampler2D ROUGHNESS_TEX;
layout (binding = 1) uniform sampler2D ALBEDO_TEX;

struct Material {
    sampler2D EMISSION_TEX;
    sampler2D NORMAL_TEX;
    vec3  albedo;
    vec3 emission;
    float emissionStr;
    float metallic;
    float roughness;
    float ao;
};

uniform int USE_TEX_0;
uniform int USE_TEX_1;
uniform int USE_TEX_2;
uniform int USE_TEX_3;
uniform int USE_TEX_4;



uniform Material material;

uniform int OBJECT_UUID;

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
    return (1.0 - USE_TEX_0) * material.albedo + USE_TEX_0 * GammaCorrectTexture(ALBEDO_TEX, texCoords);
}

float GetAlbedoAlpha() {
    return (1 - USE_TEX_0) * 1 + USE_TEX_0 * texture(ALBEDO_TEX, texCoords).a;
}

float GetGloss()
{
	return (1.0 - USE_TEX_1) * (material.roughness + 0.001) + USE_TEX_1 * texture(ROUGHNESS_TEX, texCoords).r;
}

float GetRoughness()
{
	return  GetGloss();
}

vec3 GetEmission() {

    return (1.0 - USE_TEX_2) * material.emission + USE_TEX_2 * GammaCorrectTexture(material.EMISSION_TEX, texCoords);

}

void main()
{
    gPosition = VertexPosition;//vec4(ver,1.0);
    // also store the per-fragment normals into the gbuffer
    gNormal = vec4(normalize(Normal), 1);
    gSpecRough = vec4(material.metallic, GetRoughness(), 0, 1);
    gSceneAlbedo = vec4(GetAlbedo(), GetAlbedoAlpha());

    gObjectUUID = OBJECT_UUID;
}  