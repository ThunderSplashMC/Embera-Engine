#version 460 core
#define EPSILON 0.001

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(binding = 0) restrict writeonly uniform image2D ImgResult;

layout (binding = 0) uniform sampler2D gColor; 
layout (binding = 1) uniform sampler2D gPosition; 
layout (binding = 2) uniform sampler2D gNormal; 

uniform mat4 W_PROJECTION_MATRIX;
uniform mat4 W_VIEW_MATRIX;
uniform vec3 C_VIEWPOS;

void main() {
	float maxDistance = 20;
	float resolution  = 0.5;
	int   steps       = 1;
	float thickness   = 0.2;
	
	vec2 texSize  = textureSize(gPosition, 0).xy;
	vec2 texCoord = gl_GlobalInvocationID.xy / texSize;

	vec4 positionFrom     = vec4(texture(gPosition, texCoord).xyz/*  - C_VIEWPOS */,1);
	vec3 unitPositionFrom = normalize(positionFrom.xyz);
	vec3 normal           = normalize((texture(gNormal, texCoord)/*  * W_VIEW_MATRIX */).xyz);
	vec3 pivot            = normalize(reflect(unitPositionFrom, normal));

	vec4 startView = vec4(positionFrom.xyz, 1);
	vec4 endView   = vec4(positionFrom.xyz + (pivot * maxDistance), 1);

	vec4 startFrag = startView * W_PROJECTION_MATRIX;
	// Perform the perspective divide.
	startFrag.xyz /= startFrag.w;
	// Convert the screen-space XY coordinates to UV coordinates.
	startFrag.xy   = startFrag.xy * 0.5 + 0.5;
	// Convert the UV coordinates to fragment/pixel coordnates.
	startFrag.xy  *= texSize;

	vec4 endFrag      = endView;
	endFrag = endFrag * W_PROJECTION_MATRIX;
	endFrag.xyz /= endFrag.w;
	endFrag.xy   = endFrag.xy * 0.5 + 0.5;
	endFrag.xy  *= texSize;

	vec2 frag  = startFrag.xy;

	vec4 uv;

    uv.xy = frag / texSize;

	
	float deltaX    = endFrag.x - startFrag.x;
	float deltaY    = endFrag.y - startFrag.y;

	float useX      = abs(deltaX) >= abs(deltaY) ? 1 : 0;
	float delta     = mix(abs(deltaY), abs(deltaX), useX) * clamp(resolution, 0, 1);

	vec2  increment = vec2(deltaX, deltaY) / max(delta, 0.001);

	float search0 = 0;
	float search1 = 0;

	int hit0 = 0;
	int hit1 = 0;

	float viewDistance = startView.z;

//	imageStore(ImgResult, ivec2(gl_GlobalInvocationID.xy), startFrag);
//	return;


	float depth = thickness;

	vec4 positionTo;

	for (int i = 0; i < int(delta); ++i) {

		frag      += increment;
		uv.xy      = frag / texSize;
		positionTo = texture(gPosition, uv.xy);

		search1 = mix((frag.y - startFrag.y) / deltaY, (frag.x - startFrag.x) / deltaX, useX);

		viewDistance = (startView.z * endView.z) / mix(endView.z, startView.z, search1);

		depth = viewDistance - positionTo.z;

		if (depth > 0 && depth < thickness) {
			hit0 = 1;
			break;
		} else {
			search0 = search1;
		}
	}

	search1 = search0 + ((search1 - search0) / 2);

	steps *= hit0;

	imageStore(ImgResult, ivec2(gl_GlobalInvocationID.xy), texture2D(gColor, uv.xy));
	return;

//	for (int i = 0; i < steps; ++i) {
//
//		frag       = mix(startFrag.xy, endFrag.xy, search1);
//		uv.xy      = frag / texSize;
//		positionTo = texture(gPosition, uv.xy);
//
//		viewDistance = (startView.z * endView.z) / mix(endView.z, startView.z, search1);
//		depth        = viewDistance - positionTo.z;
//
//		if (depth > 0 && depth < thickness) {
//			hit1 = 1;
//			search1 = search0 + ((search1 - search0) / 2);
//		} else {
//			float temp = search1;
//			search1 = search1 + ((search1 - search0) / 2);
//			search0 = temp;
//		}
//
//	}

	float visibility = hit1 * positionTo.w;

	visibility *= ( 1 - max ( dot(-unitPositionFrom, pivot) , 0 ) ) * ( 1 - clamp ( depth / thickness , 0 , 1 ) ) * ( 1 - clamp  (   length(positionTo - positionFrom) / maxDistance , 0 , 1 ) ) * (uv.x < 0 || uv.x > 1 ? 0 : 1) * (uv.y < 0 || uv.y > 1 ? 0 : 1);

	visibility = clamp(visibility, 0, 1);

	uv.ba = vec2(visibility);

	imageStore(ImgResult, ivec2(gl_GlobalInvocationID.xy), uv);
}