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
} fs_in;

layout(location = 0) out vec4 fragColor;

// Texture units
uniform sampler2D texColor;
uniform sampler2D texSpec;
uniform sampler2D texNormal;
uniform sampler2D texHeight;

// Texture settings (usage of color, scpe, normal, height)
uniform int materialUsage[4];

// Light sources
uniform int useLights;
uniform int activeLights;
layout (std140, binding = 0) uniform lighting
{
    Light lights[MAX_LIGHT_SOURCES];
};


void main() 
{
    // Check for color tex
    bool textured = false;
    if(bool(materialUsage[MTL_COLOR]))
    {
        fragColor = texture2D(texColor, fs_in.uv).rgba;
        textured = true;
    }

    // Check for specular tex
    if(bool(materialUsage[MTL_SPECULAR]))
    {
        fragColor = texture2D(texSpec, fs_in.uv).rgba;
        textured = true;
    }

    // Check for normal tex
    if(bool(materialUsage[MTL_NORMAL]))
    {
        fragColor = texture2D(texNormal, fs_in.uv).rgba;
        textured = true;
    }

    if(!textured)
    {
        // No textures
        fragColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
    }
}