#version 460 compatibility

// Matches constants in RenderScene.h
#define STXF_BLEND_OPAQUE   0x00
#define STXF_BLEND_ALPHA    0x10
#define STXF_BLEND_ADD      0x20
#define STXF_BLEND_SHADE    0x30
#define MAX_LIGHT_SOURCES   16

// Light tpes
#define LT_POINT            0
#define LT_AMBIENT          1
#define LT_DIRECTIONAL      2
#define LT_SPOT             3

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
    vec2 uv[4];
    vec4 color;
    vec3 position;  // view-space position
    vec3 normal;    // view-space normal
} fs_in;

layout(location = 0) out vec4 fragColor;

// Texture units
uniform sampler2D texLayer0;
uniform sampler2D texLayer1;
uniform sampler2D texLayer2;
uniform sampler2D texShadow;
uniform sampler2D texSpec;
uniform sampler2D texNormal;
uniform sampler2D texHeight;

// Texture settings
uniform int blendTypes[3];
uniform int activeLayers;
uniform int useShadow;

// Light sources
uniform int useLights;
uniform int activeLights;
layout (std140, binding = 0) uniform lighting
{
    Light lights[MAX_LIGHT_SOURCES];
};

vec3 calculatePointLight(vec3 fragPos, vec3 fragNormal, Light light, bool calcDiffuson)
{
    // Fragment-light vector
    vec3 toLight = light.position.xyz - fragPos;
    // Fragment-light sitance
    float distance = length(toLight);

    // Out of fall-off radious
    if (distance > light.fallOff) {
        return vec3(0.0);
    }

    // Fragment-light normalized vector (direction)
    vec3 lightDir = normalize(toLight);

    // Calc diffusion if needed (depends on light fall angle)
    float diffuse = (calcDiffuson ? max(dot(fragNormal, lightDir), 0.0) : 1.0f);
    
    // Attenuation (max for hot-spot, attenuate until fall-off)
    float attenuation;
    if (distance <= light.hotSpot) {
        attenuation = 1.0;
    } else {
        attenuation = (light.fallOff - distance) / (light.fallOff - light.hotSpot);
        attenuation = clamp(attenuation, 0.0, 1.0);
    }

    // Result color
    return light.color.rgb * diffuse * attenuation;
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

void main() 
{
    vec4 finalColor = vec4(0.0f);

    // Handle regular texture layers
    for (int i = 0; i < activeLayers; i++) 
    {
        vec4 texColor = getLayerColor(i, fs_in.uv[i]);

        // OPAQUE
        if (blendTypes[i] == STXF_BLEND_OPAQUE) {
            finalColor = texColor;
        } 
        // SHADE
        else if (blendTypes[i] == STXF_BLEND_SHADE) { 
            finalColor = 2.0 * texColor * finalColor;
        }
        // ALPHA
        else if (blendTypes[i] == STXF_BLEND_ALPHA) {
            finalColor = mix(finalColor, texColor, texColor.a);
        }
        // ADD
        else if (blendTypes[i] == STXF_BLEND_ADD) { 
            finalColor += texColor;
        }
    }

    // Lighting/shading enabled for polygon
    if(bool(useLights))
    {
        vec3 lighting = vec3(0.0);
        for (int i = 0; i < activeLights; i++) 
        {
            switch(lights[i].type)
            {
                case LT_POINT:
                    lighting += calculatePointLight(fs_in.position, fs_in.normal, lights[i], true);
                break;

                case LT_AMBIENT:
                    lighting += calculatePointLight(fs_in.position, fs_in.normal, lights[i], false);
                break;
            }
        }

        finalColor.rgb *= lighting;
    }

    // TODO: Handle shadow texture

    fragColor = finalColor * fs_in.color;
}