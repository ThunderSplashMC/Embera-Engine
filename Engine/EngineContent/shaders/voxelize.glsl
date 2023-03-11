#version 440

layout(local_size_x = 4, local_size_y = 4, local_size_z = 4) in;
layout(binding = 0, rgba8) uniform image3D out_tex;

void main() {

    // get position to read/write data from
    ivec3 imgCoord = ivec3(gl_GlobalInvocationID);    // get value stored in the image
    
    vec4 image = imageLoad(out_tex, imgCoord);

    imageStore(out_tex, imgCoord,vec4(0.0));
}