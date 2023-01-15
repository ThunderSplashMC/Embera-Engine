#version 330

out vec4 FragColor;

uniform sampler2D S_BLOOM_TEXTURE;
uniform sampler2D S_VIGNETTE_TEXTURE;
uniform sampler2D S_RENDERED_TEXTURE;

uniform float U_BLOOM_STRENGTH;
uniform float U_VIGNETTE_STRENGTH;


in vec2 texCoords;

vec3 tonemapACES(in vec3 c)
{
    float a = 2.51f;
    float b = 0.03f;
    float y = 2.43f;
    float d = 0.59f;
    float e = 0.14f;


    return clamp((c * (a * c + b)) / (c * (y * c + d) + e), 0.0, 1.0);
}

vec3 bloom(vec3 hdrColor) {
    vec3 bloomColor = texture(S_BLOOM_TEXTURE, texCoords).rgb;
    return hdrColor + (bloomColor * U_BLOOM_STRENGTH);
}

vec3 vignette(vec3 hdrColor) {
    vec3 vignetteColor = texture(S_VIGNETTE_TEXTURE, texCoords).rgb;
    return hdrColor + (vignetteColor * U_VIGNETTE_STRENGTH);
}

vec3 filmic(vec3 x) {
  vec3 X = max(vec3(0.0), x - 0.004);
  vec3 result = (X * (6.2 * X + 0.5)) / (X * (6.2 * X + 1.7) + 0.06);
  return pow(result, vec3(2.2));
}

void main() {

    const float gamma = 2.2;
    vec3 hdrTex = texture(S_RENDERED_TEXTURE, texCoords).rgb;

    hdrTex = floor(hdrTex * (31) + 0.5)/31;

    vec3 hdrColor = bloom(hdrTex);
    hdrColor = vignette(hdrColor);
  
    vec3 mapped = tonemapACES(hdrColor).rgb;
    mapped = pow(mapped, vec3(1/gamma));
  
    FragColor = vec4(mapped, 1.0);
}