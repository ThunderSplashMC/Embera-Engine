#version 330 core

layout(location = 0) out vec4 color;

in vec4 v_Color;
in vec2 v_TexCoord;
in float v_TexIndex;

uniform sampler2D u_Texture;

void main()
{
    vec4 tex = texture(u_Texture, v_TexCoord);
	color = tex;
}