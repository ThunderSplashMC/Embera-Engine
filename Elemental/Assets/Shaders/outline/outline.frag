#version 330 core

layout(location = 0) out vec4 color;

uniform vec3 C_VIEWPOS;

void main()
{
	color = vec4(1, 0.64, 0, 1.0);
}