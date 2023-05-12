#version 460 core
#define PI 3.14159265
#extension GL_ARB_bindless_texture : require

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(binding = 0) restrict writeonly uniform image2D ImgResult;

layout(binding = 0) uniform sampler3D SamplerVoxelsAlbedo;
layout(binding = 1) uniform sampler2D gNormal;
layout(binding = 2) uniform sampler2D gDepth;
layout(binding = 3) uniform sampler2D gSpecRough;
layout(binding = 4) uniform sampler2D gAlbedo;

// Normal Texture
// Specular Texture
// Diffuse Texture

// Voxel Volume

uniform vec3 GRID_MAX;
uniform vec3 GRID_MIN;
uniform vec3 C_VIEWPOS;
uniform mat4 W_PROJECTION_MATRIX;
uniform mat4 W_VIEW_MATRIX;

vec3 NDCToWorld(vec3 ndc)
{
    vec4 viewPos =  vec4(ndc, 1.0) * (inverse(W_PROJECTION_MATRIX) * inverse(W_VIEW_MATRIX));
    return viewPos.xyz / viewPos.w;
}


vec3 calcWorldPosition(float depth, vec3 view_ray, vec3 cam_position)
{
	view_ray = normalize(view_ray);
	return view_ray * depth - cam_position;
}


vec4 coneTrace(vec3 origin, vec3 dir, vec3 volumeDimensions, float maxDist, float coneRatio)
{
	float minDiameter = 1.0 / volumeDimensions.x;
	float minVoxelDiameterInv = volumeDimensions.x;
	float dist = minDiameter;

	vec4 accum = vec4(0.0);

	// Traverse through voxels until ray exits volume
	while (dist <= maxDist && accum.w < 0.999)
	{
		float sampleDiameter = max(minDiameter, coneRatio * dist);
		float sampleLOD = log2(sampleDiameter * minVoxelDiameterInv);
		sampleLOD = clamp(sampleLOD, 0.0, float(textureQueryLevels(SamplerVoxelsAlbedo) - 1));

		vec3 offset = dir * dist;
		vec3 samplePos = origin + offset;

		vec4 color = textureLod(SamplerVoxelsAlbedo, samplePos, sampleLOD);

		float sampleWeight = (1.0 - accum.w);
		accum += vec4(color * sampleWeight);

		dist += sampleDiameter;
	}


	float occlusion = accum.w;

	return vec4(accum.xyz, occlusion);
}




mat4 rotationMatrix(vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}


vec4 diffuseCone(float numCones, float angle, vec3 rayOrigin, vec3 normal)
{
	float maxDist = 0.55;
	float coneRatio = 0.7;

	float count = 0;
	vec4 sum = vec4(0.0);

	float angle1 = angle;


	vec3 rotAngle = normalize(  cross(vec3(0.0,1.0,0.0), normal)  );
	mat4 rot1 = rotationMatrix(rotAngle, angle1 * PI / 180.0);
	vec3 ref1 = (rot1 * vec4(normal, 0.0)).xyz;
	ref1 = normalize(ref1);

	float num_cones = numCones;
	float inc = floor(360.0 / num_cones);


	for (float ang = 0; ang < 360.0; ang += inc)
	{
		count++;

		vec3 ref2 = (rotationMatrix(normal, ang * PI / 180.0) * (rot1 * vec4(normal, 0.0))).xyz;
		ref2 = normalize(ref2);

		sum += coneTrace(rayOrigin, ref2, textureSize(SamplerVoxelsAlbedo, 0), maxDist, coneRatio);
	}

	return sum * (1.0 / count);
}


vec4 ct_diffuse(vec3 rayOrigin, vec3 normal)
{
	float intensity = 1.0;

	vec4 sum = vec4(0.0);

	sum = coneTrace(rayOrigin, normal, textureSize(SamplerVoxelsAlbedo, 0), 0.6, 0.7);
	sum += diffuseCone(5.0, 70.0, rayOrigin, normal);
	sum += diffuseCone(3.0, 30.0, rayOrigin, normal);
	
	sum.a /= 3.0;
	sum.rgb /= 2.0;

	return sum;	
}

vec4 ct_specular(vec3 rayOrigin, vec3 reflection, vec3 normal, vec4 specular_settings)
{

	float intensity = 1.0;

	float maxDist = 0.9;
	float coneRatio = specular_settings.a;
	coneRatio = clamp(coneRatio, 0.1, 1.0);

	vec4 specular = coneTrace(rayOrigin, reflection, textureSize(SamplerVoxelsAlbedo, 0), maxDist, coneRatio) * intensity;
	specular *= vec4(specular_settings.xyz,1.0);

	return specular;
}




void main()
{

	ivec2 imgCoord = ivec2(gl_GlobalInvocationID.xy);
    vec2 uv = (imgCoord + 0.5) / imageSize(ImgResult);

    float depth = texture(gDepth, uv).r;
    if (depth == 1.0)
    {
        imageStore(ImgResult, imgCoord, vec4(0.0));
        return;
    }

    vec3 fragPos = NDCToWorld(vec3(uv, depth) * 2.0 - 1.0);

	vec3 normal = texture(gNormal, uv).xyz;
	vec4 specularSettings = texture(gSpecRough, uv);
	vec4 diffuse = vec4(texture(gAlbedo, uv).rgb, 1.0);

	vec3 world_position = (GRID_MAX + GRID_MIN)/2;//calcWorldPosition(depth, fragPos, C_VIEWPOS);
	vec3 world_position_biased = ((world_position /* TODO VOL POS*/) / (textureSize(SamplerVoxelsAlbedo, 0).x)) * 0.5 + 0.5;

	vec3 reflection = normalize(reflect(fragPos, normal));
	
	vec4 final = vec4(0.0);

	vec3 shift = ((normal) / (textureSize(SamplerVoxelsAlbedo, 0).xyz * 1.5));
	vec3 origin_shifted = world_position_biased + shift;
	float origin_max = max(origin_shifted.x, max(origin_shifted.y, origin_shifted.z));
	float origin_min = min(origin_shifted.x, min(origin_shifted.y, origin_shifted.z));
	if (!(origin_min < 0.0 || origin_max > 1.0)) 
	{
		vec4 indirect_diffuse = ct_diffuse(origin_shifted, normal);
		vec4 indirect_specular = ct_specular(origin_shifted, reflection, normal, specularSettings);

		float occlusion_diffuse = clamp(indirect_diffuse.a, 0.0, 1.0);
		float occlusion_specular = clamp(indirect_specular.a, 0.0, 1.0);

		// Mix with diffuse
		final = (indirect_diffuse * diffuse);
		final += indirect_specular;
		final = vec4((final).xyz,occlusion_diffuse);
	}
	else
	{
		//final = vec4(0.0, 1.0, 0.0, 0.0);
	}
	imageStore(ImgResult, imgCoord, vec4(final.xyz, 1.0));
}