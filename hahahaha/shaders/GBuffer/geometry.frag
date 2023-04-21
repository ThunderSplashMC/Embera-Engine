#version 330 core

layout (location = 0) out vec4 gNormalSpecular;

in vec3 Normal;
in vec2 texCoords;
in vec3 FragPos;

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

uniform Material material;

void main()
{
    // also store the per-fragment normals into the gbuffer
    gNormalSpecular = vec4(normalize(Normal), material.roughness);
}  