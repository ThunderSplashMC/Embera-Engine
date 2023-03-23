#version 460 core
layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

in vec3 WorldPosGS[];
in vec3 WorldNormalGS[];

out vec3 WorldPos;
out vec3 WorldNormal;

void main()
{

    vec3 p1 = gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz;
    vec3 p2 = gl_in[2].gl_Position.xyz - gl_in[0].gl_Position.xyz;
    vec3 normalWeights = abs(cross(p1, p2));

    int dominantAxis = normalWeights.y > normalWeights.x ? 1 : 0;
    dominantAxis = normalWeights.z > normalWeights[dominantAxis] ? 2 : dominantAxis;

    for (int i = 0; i < 3; i++)
    {
        WorldPos = WorldPosGS[i];
        WorldNormal = WorldNormalGS[i];
        gl_Position = gl_in[i].gl_Position;

        if (dominantAxis == 0) gl_Position = gl_Position.zyxw;
        else if (dominantAxis == 1) gl_Position = gl_Position.xzyw;

        EmitVertex();
    }
    EndPrimitive();
}