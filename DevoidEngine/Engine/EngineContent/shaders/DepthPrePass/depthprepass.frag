#version 440 core

out vec4 color;

layout (binding = 0) uniform sampler2D depthTexture;
in vec2 texCoords; //texture coordinates from vertex-shader

void main( void )
{
    gl_FragDepth = texture(depthTexture, texCoords).r;
}