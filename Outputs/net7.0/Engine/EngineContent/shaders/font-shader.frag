#version 330 core

layout(location = 0) out vec4 color;

in vec2 v_TexCoord;
in float v_TexIndex;

uniform sampler2D u_Texture;
uniform int USE_TEX_0;
uniform vec4 v_Color;

void main()
{
    vec4 tex = texture(u_Texture, v_TexCoord);
	color = mix(v_Color, tex, v_Color.a);
	color.a = tex.a;
}