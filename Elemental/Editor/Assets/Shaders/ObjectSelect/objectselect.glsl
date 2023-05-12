#version 460 core
#define PI 3.14159265

layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout(binding = 0) restrict writeonly uniform image2D ImgResult; // Image Result will be the UUID of the object at that position;

layout(binding = 0) uniform sampler2D UUIDSampler;

uniform vec2 ClickPosition;

void main() {
	
	vec2 uv = (ClickPosition + 0.5) / textureSize(UUIDSampler, 0);

	imageStore(ImgResult, ivec2(0), texture(UUIDSampler, uv));
}