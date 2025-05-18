#version 460 compatibility

// Matches constants in RenderScene.h
#define STXF_BLEND_OPAQUE   0x00
#define STXF_BLEND_ALPHA    0x10
#define STXF_BLEND_ADD      0x20
#define STXF_BLEND_SHADE    0x30
#define MAX_LIGHT_SOURCES   32

// Light tpes
#define LT_POINT            0
#define LT_AMBIENT          1
#define LT_DIRECTIONAL      2
#define LT_SPOT             3

// Material texture types
#define MTL_SPECULAR        0
#define MTL_NORMAL          1
#define MTL_HEIGHT          2

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
    vec2 uv[7];
    vec4 color;
    vec3 position;       // view-space position
    vec3 normal;         // view-space normal
    mat3 TBN;            // TBN matrix for normal mapping
    float TBNhandedness; // TBN handeness (orientation) depending on UVs
} fs_in;

layout(location = 0) out vec4 fragColor;

// Texture units
uniform sampler2D texLayer0;
uniform sampler2D texLayer1;
uniform sampler2D texLayer2;
uniform sampler2D texShadow; // Precalcualted & combined by SE for all light-sources!
uniform sampler2D texSpec;
uniform sampler2D texNormal;
uniform sampler2D texHeight;

// Texture settings
uniform int blendTypes[3];
uniform int materialUsage[3];
uniform int activeLayers;
uniform int useShadow;

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

vec4 getLayerColor(int index, vec2 uv) 
{
    switch (index) {
        case 0: return texture2D(texLayer0, uv);
        case 1: return texture2D(texLayer1, uv);
        case 2: return texture2D(texLayer2, uv);
        default: return vec4(0.0);
    }
}

vec2 parallaxOcclusionMapping(vec2 uv, vec3 viewDirTangent, int numStepsMin, int numStepsMax)
{
    // Sample height (white - max height, black - max depth)
    float height = 1.0 - texture(texHeight, uv).r;

    // Slope (cosine) for normalized viewDirTangent (vector to camera in tangent space)
    float slope = abs(viewDirTangent.z);

    // Get actual number of steps depending on view vector slope (more for bigger angles)
    int numSteps = int(mix(float(numStepsMax), float(numStepsMin), slope));

    // Initialize ray marching
    float layerDepth = 1.0f / float(numSteps);                 // One layer depth (step size)
    float safeZ = max(abs(viewDirTangent.z), 0.0001);
    vec2 deltaUV = viewDirTangent.xy * HM_SCALE / safeZ;             // Full UV displacement, scaled by height scale
    vec2 currentUV = uv;                                             // Start from current UV
    float currentDepth = 1.0f;                                       // Start from max depth (z = height scale)
    float lastSampledHeight = height;                                // Last sampled height

    // Ray marching: move downward (z decreases)
    for(int i = 0; i < numSteps; i++)
    {
        // If sampled height is above current depth - intersected (stop marching)
        if(lastSampledHeight > currentDepth){
            break;
        }

        // Move UV and depth in opposite to view-vector direction
        currentUV -= deltaUV * layerDepth;
        currentDepth -= layerDepth;
        lastSampledHeight = 1.0 - texture(texHeight, currentUV).r;
    }

    // Refine with interpolation (use previous and last sampled heights)
    vec2 prevUV = currentUV + deltaUV * layerDepth;
    float prevSampledHeight = 1.0 - texture(texHeight, prevUV).r;
    float afterDepth = lastSampledHeight - currentDepth;
    float beforeDepth = prevSampledHeight - (currentDepth + layerDepth);
    float weight = afterDepth / (afterDepth - beforeDepth);
    return mix(currentUV, prevUV, weight);
}

void main() 
{
    // UV maps for all textures
    vec2 specUV = fs_in.uv[4];
    vec2 normUV = fs_in.uv[5];
    vec2 dispUV = fs_in.uv[6];

    if(bool(materialUsage[MTL_HEIGHT]))
    {
        // Transform view direction (fragment to view) to tangent space
        vec3 viewDir = normalize(fs_in.position);
        vec3 viewDirTangent = normalize(transpose(fs_in.TBN) * viewDir);
        vec2 pomUV = parallaxOcclusionMapping(dispUV, viewDirTangent, 20, 80);
        // All textures now uses POM uv
        specUV = pomUV;
        normUV = pomUV;
        dispUV = pomUV;
    }

    // Get albedo color from texture layers
    vec4 albedo = vec4(0.0f);
    for (int i = 0; i < activeLayers; i++) 
    {
        // If displace map used - use displaced UV's for color layers
        vec2 uv = bool(materialUsage[MTL_HEIGHT]) ? dispUV : fs_in.uv[i];
        vec4 texColor = getLayerColor(i, uv);

        // OPAQUE
        if (blendTypes[i] == STXF_BLEND_OPAQUE) {
            albedo = texColor;
        } 
        // SHADE
        else if (blendTypes[i] == STXF_BLEND_SHADE) { 
            albedo = 2.0 * texColor * albedo;
        }
        // ALPHA
        else if (blendTypes[i] == STXF_BLEND_ALPHA) {
            albedo = mix(albedo, texColor, texColor.a);
        }
        // ADD
        else if (blendTypes[i] == STXF_BLEND_ADD) { 
            albedo += texColor;
        }
    }
    albedo *= fs_in.color;

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
            specIntensity = texture2D(texSpec, specUV).r;
        }

        if(bool(materialUsage[MTL_NORMAL]))
        {
            vec3 normalTangent = texture(texNormal, normUV).rgb * 2.0 - 1.0;
            normalTangent.y *= fs_in.TBNhandedness;
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
        
        if(bool(useShadow))
        {
            lighting *= texture2D(texShadow, fs_in.uv[activeLayers]).r;
        }
    }

    // Final result
    vec4 finalColor = vec4(albedo.rgb * lighting, albedo.a);
    fragColor = finalColor;
}