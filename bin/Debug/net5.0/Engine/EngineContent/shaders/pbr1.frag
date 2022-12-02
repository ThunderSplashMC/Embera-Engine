#version 330 core

out vec4 FragColor;
in vec2 texCoords;
in vec3 Normal;
in vec3 FragPos;

struct PointLight {
    vec3 position;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct Material {
    vec3  albedo;
    float metallic;
    float roughness;
    float ao;
};

#define NR_POINT_LIGHTS 4
uniform PointLight L_POINTLIGHTS[NR_POINT_LIGHTS];

uniform Material material;

uniform vec4 COLOR;
uniform vec3 C_VIEWPOS;


vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    //diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.metallic);
    //attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));
    //combine results
    vec3 ambient  = light.ambient;//  * vec3(texture(material.diffuse, texCoords));
    vec3 diffuse  = light.diffuse  * diff * material.albedo.rgb;//vec3(texture(material.diffuse, texCoords));
    vec3 specular = light.specular * spec;
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
} 

//void main() {
//    
//    vec4 result = vec4(0);
//    vec3 viewDir = normalize(C_VIEWPOS - FragPos);
//
//
//    for(int i = 0; i < NR_POINT_LIGHTS; i++) {
//        result += vec4(CalcPointLight(L_POINTLIGHTS[i], Normal, FragPos, viewDir), 1.0);
//    }
//
//	FragColor = result * COLOR;
//}

void main() {
    FragColor = COLOR;
}