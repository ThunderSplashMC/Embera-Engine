#version 460 core

layout (binding=0, rgba8) uniform image3D ImgResult;
layout (binding=1, rgba8) uniform image2D ImgOutput;

in vec3 WorldPos;
in vec2 texCoords;

uniform mat4 W_PROJECTION_MATRIX;

ivec3 WorlSpaceToVoxelImageSpace(vec3 worldPos)
{
    vec3 ndc = (W_PROJECTION_MATRIX * vec4(worldPos, 1.0)).xyz;
    ivec3 voxelPos = ivec3((ndc * 0.5 + 0.5) * imageSize(ImgResult));
    return voxelPos;
}

void main() {
    imageStore(ImgOutput, ivec2(texCoords), vec4(1.0));
}