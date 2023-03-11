#version 450 core

uniform mat4 V;

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBiTangent;

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_MODEL_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;

out vec2 textureCoordinateFrag;
out vec2 texCoords;

// Scales and bias a given vector (i.e. from [-1, 1] to [0, 1]).
vec2 scaleAndBias(vec2 p) { return 0.5f * p + vec2(0.5); }

void main(){
	gl_Position = vec4(aPosition.x, aPosition.y, -0.1, 1.0) * W_MODEL_MATRIX *  W_PROJECTION_MATRIX;	
	textureCoordinateFrag = scaleAndBias(gl_Position.xy);
	texCoords = aTexCoord;
	
}