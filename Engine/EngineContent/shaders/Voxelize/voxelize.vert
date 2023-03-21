#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBiTangent;

uniform mat4 W_MODEL_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;
 
out vec3 WorldPosGS;
out vec3 WorldNormalGS;
out vec3 Normal;
 
void main()
{
    WorldNormalGS = normalize((aNormal * mat3(transpose(inverse(W_MODEL_MATRIX)))));
	WorldPosGS = vec3(vec4(aPosition, 1.0) * W_MODEL_MATRIX);
}