#version 450 core

#define STEP_LENGTH 0.005f
#define INV_STEP_LENGTH (1.0f/STEP_LENGTH)

uniform sampler2D textureBack; // Unit cube back FBO.
uniform sampler2D textureFront; // Unit cube front FBO.
uniform sampler3D texture3D1; // Texture in which voxelization is stored.
uniform vec3 C_VIEWPOS; // World camera position.
uniform int state = 0; // Decides mipmap sample level.

in vec2 textureCoordinateFrag;
in vec2 texCoords;
out vec4 color;

// Scales and bias a given vector (i.e. from [-1, 1] to [0, 1]).
vec3 scaleAndBias(vec3 p) { return 0.5f * p + vec3(0.5f); }

// Returns true if p is inside the unity cube (+ e) centered on (0, 0, 0).
bool isInsideCube(vec3 p, float e) { return abs(p.x) < 1 + e && abs(p.y) < 1 + e && abs(p.z) < 1 + e; }

void main() {
	const float mipmapLevel = state;

	// Initialize ray.
	const vec3 origin = isInsideCube(C_VIEWPOS, 1) ? C_VIEWPOS : texture(textureFront, textureCoordinateFrag).xyz;
	vec3 direction = texture(textureBack, textureCoordinateFrag).xyz - origin;
	const uint numberOfSteps = uint(INV_STEP_LENGTH * length(direction));
	direction = normalize(direction);

	// Trace.
	color = vec4(0.0);
	for(uint step = 0; step < numberOfSteps && color.a < 0.99f; ++step) {
		const vec3 currentPoint = origin + STEP_LENGTH * step * direction;
		vec3 coordinate = scaleAndBias(currentPoint);
		vec4 currentSample = textureLod(texture3D1, scaleAndBias(currentPoint), mipmapLevel);
		color += (1.0f - color.a) * currentSample;
	} 
	//color.rgb = texture(textureBack, textureCoordinateFrag).xyz;//pow(color.rgb, vec3(1.0 / 2.2));
	color.a = 1;

//    float weight = 1.0;
//    float stepSize = 0.01;
//    float maxDistance = 10.0;
//    float coneWidth = tan(45.0 / 2.0);
//
//    for (float t = 0.0; t < maxDistance; t += stepSize) {
//        vec3 samplePoint = origin + t * direction;
//        vec3 voxelPos = floor(samplePoint + 0.5);
//        vec4 voxel = texture(texture3D1, voxelPos);
//
//        if (voxel.a < 0.01) {
//            continue;
//        }
//
//        float distance1 = length(samplePoint - origin);
//        float contribution = exp(-voxel.a * distance1 * weight);
//
//        color.rgb += voxel.rgb * contribution;
//        weight *= voxel.a;
//
//        if (weight < 0.01 || distance1 > maxDistance) {
//            break;
//        }
//
//        float coneFactor = dot(direction, normalize(samplePoint - origin));
//        if (coneFactor < coneWidth) {
//            break;
//        }
//    }
//    color.a = 1;
}