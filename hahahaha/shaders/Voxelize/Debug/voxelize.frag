#version 460 core
//#define EMISSIVE_MATERIAL_MULTIPLIER 5.0
//#define PI 3.14159265
//#define EPSILON 0.001

#extension GL_ARB_bindless_texture : require
#extension GL_NV_shader_atomic_fp16_vector : enable
#extension GL_NV_gpu_shader5 : require

in vec3 FragPos;
uniform mat4 W_ORTHOGRAPHIC_MATRIX;

layout(binding = 0, rgba16f) restrict uniform image3D ImgResult;

ivec3 WorlSpaceToVoxelImageSpace(vec3 worldPos);

void main()
{
    ivec3 voxelPos = WorlSpaceToVoxelImageSpace(FragPos);

    const float ambient = 0.03;

    imageAtomicMax(ImgResult, voxelPos, f16vec4(vec3(1.0), 1.0));

}

ivec3 WorlSpaceToVoxelImageSpace(vec3 worldPos)
{
    vec3 ndc = (W_ORTHOGRAPHIC_MATRIX * vec4(worldPos, 1.0)).xyz;
    ivec3 voxelPos = ivec3((ndc * 0.5 + 0.5) * imageSize(ImgResult));
    return voxelPos;
}