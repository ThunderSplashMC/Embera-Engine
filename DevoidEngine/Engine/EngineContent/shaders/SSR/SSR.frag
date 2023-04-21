#version 440 core

out vec4 FragColor;

// Texture samplers
uniform sampler2D gColor;
uniform sampler2D gPosition;
uniform sampler2D gNormal;

// Uniforms
uniform mat4 W_PROJECTION_MATRIX;

// Screen size
uniform vec2 uScreenSize = vec2(1920, 1080);

// Reflection intensity
uniform float uReflectionIntensity = 1;

// Debug mode
uniform bool uDebug = false;

// Fragment shader
void main()
{
  // Texture coordinates
  vec2 texCoord = gl_FragCoord.xy / uScreenSize;

  // World position and normal
  vec3 worldPos = texture2D(gPosition, texCoord).xyz;
  vec3 worldNormal = texture2D(gNormal, texCoord).xyz;

  // Reflection vector
  vec3 viewVector = normalize(-worldPos);
  vec3 reflectVector = reflect(viewVector, worldNormal);

  // Project reflection vector into screen space
  vec4 projCoord = vec4(reflectVector, 0.0) * W_PROJECTION_MATRIX;
  vec2 screenCoord = projCoord.xy / projCoord.w;
  vec2 screenTexCoord = vec2(0.5, 0.5) * screenCoord + vec2(0.5, 0.5);

  // Calculate reflection color
  vec3 reflectionColor = texture2D(gColor, screenTexCoord).rgb;

  // Output final color
  if (uDebug) {
    FragColor = vec4(screenTexCoord, 0.0, 1.0);
  } else {
    FragColor = vec4(reflectionColor * uReflectionIntensity, texture2D(gNormal, texCoord).a);
  }
}