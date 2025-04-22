#version 460 compatibility

// Matches constants in RenderScene.h
#define STXF_BLEND_OPAQUE   0x00
#define STXF_BLEND_ALPHA    0x10
#define STXF_BLEND_ADD      0x20
#define STXF_BLEND_SHADE    0x30

in VS_OUT {
    vec2 uv[4];
    vec4 color;
} fs_in;

layout(location = 0) out vec4 fragColor;

uniform sampler2D texLayer0;
uniform sampler2D texLayer1;
uniform sampler2D texLayer2;
uniform sampler2D texShadow;
uniform sampler2D texSpec;
uniform sampler2D texNormal;
uniform sampler2D texHeight;
uniform int blendTypes[3]; // For each texture form  tex[3]
uniform int activeLayers;
uniform bool useShadow;

vec4 getTextureColor(int index, vec2 uv) {
    switch (index) {
        case 0: return texture2D(texLayer0, uv);
        case 1: return texture2D(texLayer1, uv);
        case 2: return texture2D(texLayer2, uv);
        default: return vec4(0.0);
    }
}

void main() {
    vec4 finalColor = vec4(0.0f, 0.0f, 0.0f, 1.0f);

    // Handle regular texture layers
    for (int i = 0; i < activeLayers; i++) 
    {
        vec4 texColor = getTextureColor(i, fs_in.uv[i]);

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

    // TODO: Handle shadow

    fragColor = finalColor * fs_in.color;
}