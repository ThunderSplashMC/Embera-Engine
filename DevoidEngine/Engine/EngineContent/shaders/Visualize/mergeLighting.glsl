#version 460 core
#define PI 3.14159265
#extension GL_ARB_bindless_texture : require

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(binding = 0) restrict writeonly uniform image2D ImgResult;

layout(binding = 0) uniform sampler2D gFinalColor;
layout(binding = 1) uniform sampler2D gVXGI;
layout(binding = 2) uniform sampler2D gSceneAlbedo;

void main()
{
    ivec2 imgCoord = ivec2(gl_GlobalInvocationID.xy);
    vec2 uv = (imgCoord + 0.5) / imageSize(ImgResult);

    vec4 sceneAlbedo = texture(gSceneAlbedo, uv);
    vec4 finalColor = texture(gFinalColor, uv);
    vec4 indirectLighting = texture(gVXGI, uv);

    imageStore(ImgResult, imgCoord, vec4(finalColor.xyz + (indirectLighting.xyz * sceneAlbedo.xyz), 1.0));
}