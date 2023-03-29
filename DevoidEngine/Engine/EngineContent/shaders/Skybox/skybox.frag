#version 330 core

out vec4 FragColor;
in vec3 texCoords;

uniform float Intensity = 1.0;

uniform samplerCube skybox;

void main()
{
    vec4 color = texture(skybox, texCoords);

    color = vec4(color.rgb * 0.2 * Intensity, color.a);

    FragColor = vec4(color);
}

