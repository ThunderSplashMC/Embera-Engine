#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBiTangent;

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_MODEL_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;

out vec3 Normal;
out vec3 Tangent;
out vec3 BiTangent;
out vec2 texCoords;
out vec3 FragPos;
out vec3 WorldPos;
out mat4 WProjMatrix;

out VS_OUT{
    vec2 TexCoords;
    vec3 Normal;
    mat4 ProjectionMatrix;
    vec3 Tangent;
    vec3 BiTangent;
} vs_out;

void main()
{
    vs_out.Normal = normalize((aNormal * mat3(transpose(inverse(W_MODEL_MATRIX)))));
    vs_out.Tangent = normalize((aTangent * mat3(transpose(inverse(W_MODEL_MATRIX)))));
    vs_out.BiTangent = normalize((aBiTangent * mat3(transpose(inverse(W_MODEL_MATRIX)))));

    WorldPos = vec3(vec4(aPosition, 1.0) * W_MODEL_MATRIX);
    texCoords = aTexCoord;
    FragPos = vec3(vec4(aPosition, 1.0) * W_MODEL_MATRIX);
    vs_out.ProjectionMatrix = W_PROJECTION_MATRIX;
    gl_Position = vec4(aPosition, 1.0) * W_MODEL_MATRIX;
}