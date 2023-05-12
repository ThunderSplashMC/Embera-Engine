#version 440 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

out vec2 texCoords;

void main()
{
    
    texCoords = aTexCoord;
    gl_Position = vec4(aPosition.x, aPosition.y, 0.1, 1.0);
    
} 