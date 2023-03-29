#version 330 core

layout (location = 0) out vec4 gNormal;

in vec3 Normal;
in vec2 texCoords;
in vec3 FragPos;

void main()
{
    // also store the per-fragment normals into the gbuffer
    gNormal = vec4(normalize(Normal), 1.0);
}  