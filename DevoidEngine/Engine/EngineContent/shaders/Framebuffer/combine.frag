#version 330

out vec4 FragColor;

uniform sampler2D S_SOURCE_TEXTURE;
uniform sampler2D S_SCREEN_TEXTURE;

uniform float OPACITY_MIX = 0.5;
uniform bool additive = false;



in vec2 texCoords;

void main() {
  
    if (additive) {
        FragColor = vec4((texture(S_SCREEN_TEXTURE, texCoords).rgb * texture(S_SCREEN_TEXTURE, texCoords).a) + (texture(S_SOURCE_TEXTURE, texCoords).rgb * texture(S_SOURCE_TEXTURE, texCoords).a), texture(S_SCREEN_TEXTURE, texCoords).a);
    } else {
        FragColor = vec4(mix(texture(S_SCREEN_TEXTURE, texCoords).rgb, texture(S_SOURCE_TEXTURE, texCoords).rgb, OPACITY_MIX), texture(S_SOURCE_TEXTURE, texCoords).a);
    }
}