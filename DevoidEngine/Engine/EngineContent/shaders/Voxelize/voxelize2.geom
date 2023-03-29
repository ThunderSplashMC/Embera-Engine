#version 460 core

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

uniform mat4 W_PROJECTION_MATRIX;
uniform mat4 W_VIEW_MATRIX;

// Geometry shader main function
void main() {
    vec3 voxelSize = vec3(1.0) / 64;
    vec3 voxelGridCorner = voxelSize * 0.5;
    
    // Iterate over all triangles
    for (int i = 0; i < 3; i++) {
        vec4 v = gl_in[i].gl_Position;
        vec3 p = v.xyz;
        
        // Project into voxel grid space
        vec3 q = (vec4(p, 1.0)).xyz;// * W_VIEW_MATRIX * W_PROJECTION_MATRIX).xyz;
        vec3 voxelCoord = (q) / voxelSize;
        
        // Round to nearest voxel coordinate
        ivec3 voxelIndex = ivec3(
            round(voxelCoord.x),
            round(voxelCoord.y),
            round(voxelCoord.z)
        );
        
        // Emit voxel
        gl_Position = vec4(voxelCoord.x, voxelCoord.y, voxelCoord.z, 1.0);
        EmitVertex();
    }
    EndPrimitive();
}
