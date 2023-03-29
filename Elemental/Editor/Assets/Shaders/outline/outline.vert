#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBiTangent;

uniform mat4 W_MODEL_MATRIX;
uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;

uniform float outline_rad;

void main() {

	gl_Position = vec4(aPosition + (aNormal * outline_rad), 1.0) * W_MODEL_MATRIX * W_VIEW_MATRIX * W_PROJECTION_MATRIX;
}