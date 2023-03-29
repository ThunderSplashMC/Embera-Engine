#version 330 core

layout(location = 0) out int color;

in vec2 v_TexCoord;

uniform int UUID;
uniform vec3 C_VIEWPOS;

void main()
{
	color = UUID;
}