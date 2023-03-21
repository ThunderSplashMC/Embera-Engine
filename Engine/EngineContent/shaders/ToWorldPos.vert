#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBiTangent;

out vec3 WorldPos;

uniform mat4 W_MODEL_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;
uniform mat4 W_VIEW_MATRIX;

void main()
{
    WorldPos = vec3(vec4(aPosition, 1.0f) * W_MODEL_MATRIX);
    gl_Position = vec4(WorldPos, 1.0f) * W_VIEW_MATRIX * W_PROJECTION_MATRIX;
}
