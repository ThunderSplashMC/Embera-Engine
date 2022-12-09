#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aTexCoord;

out vec3 texCoords;

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_MODEL_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;

void main()
{
    texCoords = aPosition;
    gl_Position = vec4(aPosition, 1.0) * W_MODEL_MATRIX * W_VIEW_MATRIX * W_PROJECTION_MATRIX;
} 