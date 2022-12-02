#version 330 core
out vec4 FragColor;

in vec2 texCoords;

uniform sampler2D scene;
uniform sampler2D bloomBlur;
uniform float exposure;
uniform float bloomStrength = 0.04f;

vec3 bloom_new()
{
    vec3 hdrColor = texture(scene, texCoords).rgb;
    vec3 bloomColor = texture(bloomBlur, texCoords).rgb;
    return hdrColor + (bloomColor * (bloomStrength));//mix(hdrColor, bloomColor, bloomStrength); // linear interpolation
    //return bloomColor;
}

void main()
{
    // to bloom or not to bloom
    vec3 result = bloom_new();
    // tone mapping
    result = vec3(1.0) - exp(-result * exposure);
    // also gamma correct while we're at it
    const float gamma = 2.2;
    result = pow(result, vec3(1.0 / gamma));
    FragColor = vec4(result,1.0);
}