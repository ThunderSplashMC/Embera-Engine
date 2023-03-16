#version 330

out vec4 FragColor;

uniform sampler2D S_SOURCE_TEXTURE;
uniform sampler2D S_SCREEN_TEXTURE;



in vec2 texCoords;

void main() {
  
    FragColor = vec4(texture(S_SOURCE_TEXTURE, texCoords).rgb, 1.0);//vec4((texture(S_SCREEN_TEXTURE, texCoords).rgb)  + (texture(S_SOURCE_TEXTURE, texCoords).rgb), 1.0);
}