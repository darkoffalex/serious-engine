#version 460 compatibility

out VS_OUT {
    vec2 uv[3];
    vec4 color;
} vs_out;

void main()
{
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;

    vs_out.uv[0] = gl_MultiTexCoord0.xy;
    vs_out.uv[1] = gl_MultiTexCoord1.xy;
    vs_out.uv[2] = gl_MultiTexCoord2.xy;
    vs_out.color = gl_Color;
}