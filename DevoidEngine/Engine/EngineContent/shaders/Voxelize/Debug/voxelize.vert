#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBiTangent;

uniform mat4 W_MODEL_MATRIX;
uniform mat4 W_ORTHOGRAPHIC_MATRIX;

uniform int SwizzleAxis;

out vec3 Normal;
out vec3 FragPos;

out vec2 texCoords;

void main()
{
    Normal = normalize((aNormal * mat3(transpose(inverse(W_MODEL_MATRIX)))));

    FragPos = (vec4(aPosition, 1.0) * W_MODEL_MATRIX).xyz;

    texCoords = aTexCoord;

    gl_Position = vec4(FragPos, 1.0) * W_ORTHOGRAPHIC_MATRIX;


    // Instead of doing a single draw call with a standard geometry shader to select the swizzle
    // we render the scene 3 times, each time with a different swizzle. I have observed this to be slightly faster
    if (SwizzleAxis == 0) gl_Position = gl_Position.zyxw;
    else if (SwizzleAxis == 1) gl_Position = gl_Position.xzyw;
}