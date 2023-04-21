#version 330 core

out vec4 FragColor;
in vec2 texCoords;

uniform sampler2D S_RENDERED_TEXTURE;

void main()
{
    //FragColor.rgb = texCoords;
    FragColor = vec4(texture(S_RENDERED_TEXTURE, texCoords).rgb,1.0);
    //FragColor = vec4(0.5);
}

