#version 330 core

layout (location = 0) out vec4 gPosition;
layout (location = 1) out vec4 gNormal;
layout (location = 2) out vec4 gSpecular;
layout (location = 3) out int EntityID;

in vec3 Normal;
in vec2 texCoords;
in vec3 FragPos;

uniform int EntityIntID;

void main()
{    
    // store the fragment position vector in the first gbuffer texture
    gPosition = vec4(FragPos, 1.0);
    // also store the per-fragment normals into the gbuffer
    gNormal = vec4(normalize(Normal), 1.0);
    gSpecular = vec4(1.0);
//    // and the diffuse per-fragment color
//    gAlbedoSpec.rgb = texture(texture_diffuse1, TexCoords).rgb;
//    // store specular intensity in gAlbedoSpec's alpha component
//    gAlbedoSpec.a = texture(texture_specular1, TexCoords).r;
}  