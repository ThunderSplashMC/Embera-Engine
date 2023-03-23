#version 330

out vec4 FragColor;

uniform sampler2D S_SOURCE_TEXTURE;
uniform sampler2D S_SCREEN_TEXTURE;

uniform float OPACITY_MIX = 0.5;



in vec2 texCoords;

void main() {
  
    FragColor = vec4(mix(texture(S_SCREEN_TEXTURE, texCoords).rgb, texture(S_SOURCE_TEXTURE, texCoords).rgb, OPACITY_MIX), 1.0);
}