#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

out vec2 texCoords;

uniform mat4 W_PROJECTION_MATRIX;

void main()
{
    
    texCoords = aTexCoord;
    gl_Position = vec4(aPosition, 1.0);// * W_PROJECTION_MATRIX;
    
} 