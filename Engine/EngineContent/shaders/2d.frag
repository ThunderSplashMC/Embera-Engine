#version 330 core

layout(location = 0) out vec4 color;

in vec4 v_Color;
in vec2 v_TexCoord;
in float v_TexIndex;

uniform sampler2D u_Texture;
uniform int USE_TEX_0;

void main()
{
    vec4 tex = texture(u_Texture, v_TexCoord);
	color = (1.0 - USE_TEX_0) * v_Color + USE_TEX_0 * tex;
}