#version 440

layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout(rgba32f, binding = 0) uniform image2D out_tex;

float rand(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void main() {

    // get position to read/write data from
    ivec2 pos = ivec2( gl_GlobalInvocationID.xy );    // get value stored in the image

    imageStore(out_tex, pos, vec4(sin(pos.x) * 0.2, cos(pos.y) * 0.2, 0, 1));
        
}