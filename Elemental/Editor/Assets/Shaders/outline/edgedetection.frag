﻿#version 330 core

uniform sampler2D frameBufferTexture;
uniform usampler2D stencilTexture;
uniform int numSteps = 12;
uniform float radius = 3.0;

const float TAU = 6.28318530;

in vec2 UV;
out vec4 fragColor;

// Modification of: https://www.shadertoy.com/view/sltcRf
void main()
{
    vec4 color = texture(frameBufferTexture, UV);
    float stencil = texture(stencilTexture, UV).r;
    
    vec2 aspect = 1.0 / vec2(textureSize(stencilTexture, 0));
    float outlinemask = 0.0;
    for (float i = 0.0; i < TAU; i += TAU / numSteps)
    {
        // Sample image in a circular pattern
        vec2 offset = vec2(sin(i), cos(i)) * aspect * radius;
        float col = texture(stencilTexture, clamp(UV + offset, 0, 1)).r;
        
        outlinemask = mix(outlinemask, 1.0, col);
    }
    outlinemask = mix(outlinemask, 0.0, stencil);

    fragColor = mix(vec4(0.0), vec4(0.5, 0.14, 0, 1.0), clamp(outlinemask, 0.0, 1.0));
}