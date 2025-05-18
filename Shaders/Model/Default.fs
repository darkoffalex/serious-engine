#version 460 compatibility

// Matches constants in RenderScene.h
#define MAX_LIGHT_SOURCES   8

// Light tpes
#define LT_POINT            0
#define LT_AMBIENT          1
#define LT_DIRECTIONAL      2
#define LT_SPOT             3

// Material texture types
#define MTL_COLOR           0
#define MTL_SPECULAR        1
#define MTL_NORMAL          2
#define MTL_HEIGHT          3
#define MTL_REFLECTION      4
#define MTL_EMISSION        5

// Height map constants
// TODO: Maybe need uniform variable for it (can depend on texture & mapping)
#define HM_SCALE          0.025f

// Light source description
struct Light
{
    vec4 position;   // world-space (v3 + padding)
    vec4 direction;  // world-space (v3 + padding)
    vec4 color;
    vec4 colorAmbient;
    float fallOff;
    float hotSpot;
    float cutOffMin;
    float cutOffMax;
    int type;
};

in GS_OUT {
    vec2 uv;
    vec4 color;
    vec3 position;       // view-space position
    vec3 normal;         // view-space normal
    mat3 TBN;            // TBN matrix for normal mapping
} fs_in;

layout(location = 0) out vec4 fragColor;

// Texture units
uniform sampler2D texColor;
uniform sampler2D texSpec;
uniform sampler2D texNormal;
uniform sampler2D texHeight;
uniform sampler2D texEmission;

// Texture settings (usage of color, scpe, normal, height, reflection, emission)
uniform int materialUsage[6];

// Emission info
uniform vec3 emissionColor;
uniform float emissionPower;

// Displacement info
uniform float heightScale;

// Light sources
uniform int useLights;
uniform int activeLights;
layout (std140, binding = 0) uniform lighting
{
    Light lights[MAX_LIGHT_SOURCES];
};

vec3 calculatePointLight(vec3 fragPos, vec3 fragNormal, Light light, bool calcDiffusion, float specInstensity, float shininess)
{
    // Fragment-light vector
    vec3 toLight = light.position.xyz - fragPos;
    // Fragment-light distance
    float distance = length(toLight);

    // Out of fall-off radius
    if (distance > light.fallOff) {
        return vec3(0.0);
    }

    // Fragment-light normalized vector (direction)
    vec3 lightDir = normalize(toLight);

    // Calc diffusion if needed (depends on light fall angle)
    float diffuse = (calcDiffusion ? max(dot(fragNormal, lightDir), 0.0) : 1.0f);

    // Calc specular (Blinn-Phong)
    float specular = 0.0;
    if (specInstensity > 0.0 && diffuse > 0.0) { // Only compute specular if diffuse is non-zero
        // View direction (in view space, camera at (0,0,0))
        vec3 viewDir = normalize(-fragPos);
        // Halfway vector
        vec3 halfwayDir = normalize(lightDir + viewDir);
        // Specular term
        specular = pow(max(dot(fragNormal, halfwayDir), 0.0), shininess) * specInstensity;
    }

    // Attenuation (max for hot-spot, attenuate until fall-off)
    float attenuation;
    if (distance <= light.hotSpot) {
        attenuation = 1.0;
    } else {
        attenuation = (light.fallOff - distance) / (light.fallOff - light.hotSpot);
        attenuation = clamp(attenuation, 0.0, 1.0);
    }

    // Result color (combine diffuse and specular)
    return light.color.rgb * (diffuse + specular) * attenuation;
}

vec3 calculateSpotLight(vec3 fragPos, vec3 fragNormal, Light light, float specInstensity, float shininess)
{
    // Fragment-light vector
    vec3 toLight = light.position.xyz - fragPos;

    // Fragment-light normalized vector (direction)
    vec3 lightDir = normalize(toLight);

    // Light orientation vector
    vec3 lightOrientation = normalize(light.direction.xyz);

    // Calc angle attenuation
    float angleAttenuation = 0.0f;

    float angle = acos(dot(-lightDir, lightOrientation));
    float angleMin = radians(light.cutOffMin);
    float angleMax = radians(light.cutOffMax);

    if(angle < angleMin)
    {
        angleAttenuation = 1.0f;
    }
    else if(angle > angleMin && angle < angleMax)
    {
        float m = (min(angle, angleMax) - angleMin) / (angleMax - angleMin);
        angleAttenuation = mix(1.0f, 0.0f, m);
    }

    return calculatePointLight(fragPos, fragNormal, light, true, specInstensity, shininess) * angleAttenuation;
}

vec3 calculateDirectional(vec3 fragPos, vec3 fragNormal, Light light, float specInstensity, float shininess)
{
    // Directional to light
    vec3 lightDir = normalize(-light.direction.xyz);
    // Calc diffusion
    float diffuse = max(dot(fragNormal, lightDir), 0.0);

    // Calc specular (Blinn-Phong)
    float specular = 0.0;
    if (specInstensity > 0.0 && diffuse > 0.0) { // Only compute specular if diffuse is non-zero
        // View direction (in view space, camera at (0,0,0))
        vec3 viewDir = normalize(-fragPos);
        // Halfway vector
        vec3 halfwayDir = normalize(lightDir + viewDir);
        // Specular term
        specular = pow(max(dot(fragNormal, halfwayDir), 0.0), shininess) * specInstensity;
    }

    // Result color
    return light.color.rgb * (diffuse + specular) + light.colorAmbient.rgb;
}

void main() 
{
    // Get albedo (base) color
    vec4 albedo = vec4(1.0f);
    if(bool(materialUsage[MTL_COLOR]))
    {
        albedo = texture2D(texColor, fs_in.uv).rgba;
    }

    // If using emission texture
    if(bool(materialUsage[MTL_EMISSION]))
    {
        float mask = texture2D(texEmission, fs_in.uv).r;
        if(mask > 0)
        {
            vec3 emission = emissionColor * emissionPower * albedo.rgb;
            fragColor = vec4(emission, albedo.a);
            return;
        }
    }

    // Calculate lighting if needed
    vec3 lighting = vec3(1.0f);
    if(bool(useLights))
    {
        lighting = vec3(0.0);
        float shininess = 64.0f;
        float specIntensity = 0.0f;
        vec3 normal = fs_in.normal;

        if(bool(materialUsage[MTL_SPECULAR]))
        {
            specIntensity = texture2D(texSpec, fs_in.uv).r;
        }

        if(bool(materialUsage[MTL_NORMAL]))
        {
            vec3 normalTangent = texture(texNormal, fs_in.uv).rgb * 2.0 - 1.0;
            normal = normalize(fs_in.TBN * normalTangent);
        }

        for (int i = 0; i < activeLights; i++)
        {
            switch(lights[i].type)
            {
                case LT_POINT:
                    lighting += calculatePointLight(
                        fs_in.position, 
                        normal, 
                        lights[i], 
                        true, 
                        specIntensity, 
                        shininess);
                break;

                case LT_AMBIENT:
                    lighting += calculatePointLight(
                        fs_in.position, 
                        normal, 
                        lights[i], 
                        false, 
                        specIntensity, 
                        shininess);
                break;

                case LT_DIRECTIONAL:
                    lighting += calculateDirectional(
                        fs_in.position,
                        normal, 
                        lights[i], 
                        specIntensity, 
                        shininess);
                break;

                case LT_SPOT:
                    lighting += calculateSpotLight(
                        fs_in.position, 
                        normal, 
                        lights[i], 
                        specIntensity, 
                        shininess);
                break;
            }
        }
    }
    
    // Final result
    fragColor = vec4(albedo.rgb * lighting, albedo.a);
}