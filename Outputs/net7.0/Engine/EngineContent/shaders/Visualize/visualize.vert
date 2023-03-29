#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBiTangent;

out vec2 TexCoords;

void main()
{
    TexCoords = aPosition.xy * 0.5f + 0.5f;
    gl_Position = vec4(aPosition, 1.0f);
}