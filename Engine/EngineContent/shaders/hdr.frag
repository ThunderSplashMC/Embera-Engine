#version 330

out vec4 FragColor;

uniform sampler2D S_BLOOM_TEXTURE;
uniform sampler2D S_VIGNETTE_TEXTURE;
uniform sampler2D S_RENDERED_TEXTURE;

uniform float U_BLOOM_STRENGTH;
uniform float U_VIGNETTE_STRENGTH;

uniform int TonemapMode = 0;
uniform bool GammaCorrect = true;

in vec2 texCoords;


float luminance(vec3 v)
{
    return dot(v, vec3(0.2126f, 0.7152f, 0.0722f));
}

vec3 change_luminance(vec3 c_in, float l_out)
{
    float l_in = luminance(c_in);
    return c_in * (l_out / l_in);
}

vec3 tonemapACES(in vec3 c)
{
    float a = 2.51f;
    float b = 0.03f;
    float y = 2.43f;
    float d = 0.59f;
    float e = 0.14f;


    return clamp((c * (a * c + b)) / (c * (y * c + d) + e), 0.0, 1.0);
}


vec3 filmic(vec3 x) {
  vec3 X = max(vec3(0.0), x - 0.004);
  vec3 result = (X * (6.2 * X + 0.5)) / (X * (6.2 * X + 1.7) + 0.06);
  return pow(result, vec3(2.2));
}

vec3 reinhard(vec3 v)
{
    return v / (1.0f + v);
}

vec3 reinhard_extended(vec3 v, float max_white_l)
{
    float l_old = luminance(v);
    float numerator = l_old * (1.0f + (l_old / (max_white_l * max_white_l)));
    float l_new = numerator / (1.0f + l_old);
    return change_luminance(v, l_new);
}

vec3 reinhard_jodie(vec3 v)
{
    float l = luminance(v);
    vec3 tv = v / (1.0f + v);
    return mix(v / (1.0f + l), tv, tv);
}

vec3 bloom(vec3 hdrColor) {
    vec3 bloomColor = texture(S_BLOOM_TEXTURE, texCoords).rgb;
    return hdrColor + (bloomColor * U_BLOOM_STRENGTH);
}

vec3 vignette(vec3 hdrColor) {
    vec3 vignetteColor = texture(S_VIGNETTE_TEXTURE, texCoords).rgb;
    return hdrColor + (vignetteColor * U_VIGNETTE_STRENGTH);
}

void main() {

    const float gamma = 2.2;
    vec3 hdrTex = texture(S_RENDERED_TEXTURE, texCoords).rgb;

    vec3 hdrColor = bloom(hdrTex);
    hdrColor = vignette(hdrColor);
  
    vec3 mapped = vec3(0);

    if (TonemapMode == 0) {
        mapped = tonemapACES(hdrColor).rgb;
    } else if (TonemapMode == 1) {
        mapped = filmic(hdrColor);
    } else if (TonemapMode == 2) {
        mapped = reinhard(hdrColor);
    } else if (TonemapMode == 3) {
        mapped = reinhard_extended(hdrColor, 0.7);
    } else if (TonemapMode == 4) {
        mapped = reinhard_jodie(hdrColor);
    }

    if (GammaCorrect) {
        mapped = pow(mapped, vec3(1/gamma));
    }
  
    FragColor = vec4(mapped, 1.0);
}