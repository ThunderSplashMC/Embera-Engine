#version 460 core
out vec4 FragColor;

const float STEP_SIZE = 0.005f;

in vec2 TexCoords;

uniform sampler2D gBackfaceTexture;
uniform sampler2D gFrontfaceTexture;
uniform sampler3D gTexture3D;
uniform vec3 C_VIEWPOS;

uniform mat4 W_ORTHOGRAPHIC_MATRIX;

uniform vec3 GridMax = vec3(3,3,3);
uniform vec3 GridMin = vec3(-3,-3,-3);

bool isInsideUnitCube()
{
    return abs(C_VIEWPOS.x) < 1.2f && abs(C_VIEWPOS.y) < 1.2f && abs(C_VIEWPOS.z) < 1.2f;
}

vec3 resize(vec3 worldPos) {

    vec3 clipPos = (worldPos - GridMin) / (GridMax - GridMin); // [0, 1]
    clipPos = clipPos * 2.0 - 1.0; // [-1, 1]
    return clipPos;
}

vec3 scaleAndBias(const vec3 p) { return 0.5f * p + vec3(0.5f); }

void main() {
    
    vec3 voxelGridWorldSpaceSize = GridMax - GridMin;

    vec3 voxelWorldSpaceSize = voxelGridWorldSpaceSize / textureSize(gTexture3D, 0);

    float voxelMaxLength = max(voxelWorldSpaceSize.x, max(voxelWorldSpaceSize.y, voxelWorldSpaceSize.z));
    float voxelMinLength = min(voxelWorldSpaceSize.x, min(voxelWorldSpaceSize.y, voxelWorldSpaceSize.z));

    vec3 start = texture(gFrontfaceTexture, TexCoords).xyz * voxelMaxLength;
    vec3 direction = (texture(gBackfaceTexture, TexCoords).xyz - start);
    uint numSteps = uint(length(direction) / voxelMaxLength);
    direction = normalize(direction);
    float distFromStart = voxelMaxLength;
    FragColor = vec4(0.0f);
    for(uint step = 0; step < numSteps && FragColor.a < 1.0f; ++step)
    {

        float l = pow(distFromStart, 2);
        vec3 worldPos = start + direction * distFromStart;
        vec3 position = scaleAndBias(worldPos);
        FragColor += (1.0f - FragColor.a) * textureLod(gTexture3D, position, 0);

        distFromStart += 0.9 * (1/64.0) * (1 + 0.05 * l);
    }
    FragColor.rgb = pow(FragColor.rgb, vec3(1.0f / 2.2f));
}