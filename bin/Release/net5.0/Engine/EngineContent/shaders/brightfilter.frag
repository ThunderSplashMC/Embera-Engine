#version 330 core
out vec4 FragColor;

in vec2 texCoords;

uniform sampler2D scene;

void main()
{
    vec4 hdrColor = texture2D(scene, texCoords);
    float brightness = dot(hdrColor.rgb, vec3(0.2126, 0.7152, 0.0722));
    FragColor = vec4((hdrColor * brightness).rgb, 1.0);

}