#version 460 compatibility

layout (location = 0) out vec4 fragColor;

uniform sampler2D tex0;
uniform sampler2D tex1;
uniform sampler2D tex2;
uniform sampler2D texShadow;
uniform sampler2D texSpec;
uniform sampler2D texNormal;
uniform sampler2D texHeight;

in VS_OUT {
    vec2 uv[3];
    vec4 color;
} fs_in;

void main() {
    // TODO: Take in accaount various blending modes & texture layers
    vec4 texColor0 = texture2D(tex0, fs_in.uv[0]);
    vec4 texColor1 = texture2D(tex1, fs_in.uv[1]);
    fragColor = mix(texColor0, texColor1, 0.5f) * fs_in.color;
}