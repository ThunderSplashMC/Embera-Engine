#version 330 core

out vec4 FragColor;
in vec3 texCoords;

uniform samplerCube skybox;

void main()
{
    //FragColor.rgb = texCoords;
    FragColor = texture(skybox, texCoords);
    //FragColor = vec4(0.5);
}

