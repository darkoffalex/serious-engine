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
    vec2 uv;
    vec4 color;
} fs_in;

void main() {
    vec4 texColor = texture2D(tex0, fs_in.uv);
    fragColor = texColor * fs_in.color;
}