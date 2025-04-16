#version 460 compatibility

out VS_OUT {
    vec2 uv;
    vec4 color;
} vs_out;

void main()
{
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
    vs_out.uv = gl_MultiTexCoord0.xy;
    vs_out.color = gl_Color;
}