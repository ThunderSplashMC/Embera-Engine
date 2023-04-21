#version 460 core
#define EPSILON 0.001

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(binding = 0) restrict writeonly uniform image2D ImgResult;
layout(binding = 0) uniform sampler2D SamplerSrc;
layout(binding = 1) uniform sampler2D gPosition;
layout(binding = 2) uniform sampler2D gNormal;

vec3 SSR(vec3 normal, vec3 fragPos);
void CustomBinarySearch(vec3 samplePoint, vec3 deltaStep, inout vec3 projectedSample);
vec3 ViewToNDC(vec3 viewPos);
vec3 NDCToView(vec3 ndc);

uniform int Samples = 50;
uniform int BinarySearchSamples = 20;
uniform float MaxDist = 20;

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;
uniform vec3 C_VIEWPOS;

void main()
{
    ivec2 imgCoord = ivec2(gl_GlobalInvocationID.xy);
    vec2 uv = ((imgCoord + 0.5)) / imageSize(ImgResult);

    vec4 normalSpec = texture(gNormal, uv);
    float depth = texture(gPosition, uv).z;
    if (normalSpec.a < EPSILON || depth == 1.0)
    {
        imageStore(ImgResult, imgCoord, vec4(0.0));
        return;
    }

    vec3 fragPos = NDCToView(vec3(uv, depth) * 2.0 - 1.0);
    mat3 normalToView = mat3(inverse(W_VIEW_MATRIX));
    normalSpec.xyz = normalize(normalToView * normalSpec.xyz);

    vec3 color = SSR(normalSpec.xyz, fragPos);

    imageStore(ImgResult, imgCoord, vec4(color,1));
}

vec3 SSR(vec3 normal, vec3 fragPos)
{
    // Viewpos is origin in view space 
    //const vec3 VIEW_POS = vec3(0.0);
    vec3 reflectDir = reflect(normalize(fragPos - C_VIEWPOS), normal);
    vec3 maxReflectPoint = fragPos + reflectDir * MaxDist;
    vec3 deltaStep = (maxReflectPoint - fragPos) / Samples;

    vec3 samplePoint = fragPos;
    for (int i = 0; i < Samples; i++)
    {
        samplePoint += deltaStep;

        vec3 projectedSample = ViewToNDC(samplePoint) * 0.5 + 0.5;
        if (any(greaterThanEqual(projectedSample.xy, vec2(1.0))) || any(lessThan(projectedSample.xy, vec2(0.0))) || projectedSample.z > 1.0)
        {
            return vec3(0.0);
        }

        float depth = texture(gPosition, projectedSample.xy).r;
        if (projectedSample.z > depth)
        {
            CustomBinarySearch(samplePoint, deltaStep, projectedSample);
            return texture(SamplerSrc, projectedSample.xy).rgb; 
        }

    }

    vec3 worldSpaceReflectDir = (vec4(reflectDir,0.0) * inverse(W_VIEW_MATRIX)).xyz;
    return vec3(0);//texture(vec4(1,1,1,1), worldSpaceReflectDir).rgb;
}

void CustomBinarySearch(vec3 samplePoint, vec3 deltaStep, inout vec3 projectedSample)
{
    // Go back one step at the beginning because we know we are to far
    deltaStep *= 0.5;
    samplePoint -= deltaStep * 0.5;
    for (int i = 1; i < BinarySearchSamples; i++)
    {
        projectedSample = ViewToNDC(samplePoint) * 0.5 + 0.5;
        float depth = texture(gPosition, projectedSample.xy).z;

        deltaStep *= 0.5;
        if (projectedSample.z > depth)
        {
            samplePoint -= deltaStep;
        }
        else
        {
            samplePoint += deltaStep;
        }
    }
}

vec3 ViewToNDC(vec3 viewPos)
{
    vec4 clipPos = vec4(viewPos, 1.0) * inverse(W_PROJECTION_MATRIX);
    return clipPos.xyz / clipPos.w;
}

vec3 NDCToView(vec3 ndc)
{
    vec4 viewPos = vec4(ndc, 1.0) *  W_PROJECTION_MATRIX;
    return viewPos.xyz / viewPos.w;
}