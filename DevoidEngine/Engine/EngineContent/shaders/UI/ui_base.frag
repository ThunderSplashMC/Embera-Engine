#version 330 core

out vec4 FragColor;

in vec3 Normal;
in vec2 texCoords;
in vec3 FragPos;

uniform vec4 BaseColor;

void main()
{    
	FragColor = BaseColor;
}  