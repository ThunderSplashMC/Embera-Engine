#version 460 core
layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;
 
in vec3 WorldPosGS[];
in vec3 WorldNormalGS[];
in vec2 texCoordsGS[];
 
uniform vec3 C_VIEWPOS;

out vec3 WorldPos;
out vec3 WorldNormal;
out vec2 texCoords;

int getDominantAxisIdx(vec3 v0, vec3 v1, vec3 v2)
{
    vec3 aN = abs(cross(v1 - v0, v2 - v0));
    
    if (aN.x > aN.y && aN.x > aN.z)
        return 0;
        
    if (aN.y > aN.z)
        return 1;

    return 2;
}
 
void main()
{
    int idx = getDominantAxisIdx(gl_in[0].gl_Position.xyz, gl_in[1].gl_Position.xyz, gl_in[2].gl_Position.xyz);

    // Plane normal
    const vec3 N = abs(cross(WorldPosGS[1] - WorldPosGS[0], WorldPosGS[2] - WorldNormalGS[0]));
    for (int i = 0; i < 3; ++i)
    {
        WorldPos = WorldPosGS[i];
        WorldNormal = WorldNormalGS[i];
        texCoords = texCoordsGS[i];
        if (idx == 0)
        {
            gl_Position = vec4(WorldPos.x, WorldPos.y, 0.0f, 1.0f);
        }
        else if (idx == 1)
        {
            gl_Position = vec4(WorldPos.y, WorldPos.z, 0.0f, 1.0f);
        }
        else
        {
            gl_Position = vec4(WorldPos.x, WorldPos.z, 0.0f, 1.0f);
        }
        EmitVertex();
    }
    EndPrimitive();
}