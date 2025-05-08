#version 460 compatibility

out VS_OUT {
    vec2 uv[7];
    vec4 color;
    vec3 position;
} vs_out;

void main()
{
    // Final position (projected)
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;

    // Texture mappings & vertex color
    vs_out.uv[0] = gl_MultiTexCoord0.xy;
    vs_out.uv[1] = gl_MultiTexCoord1.xy;
    vs_out.uv[2] = gl_MultiTexCoord2.xy;
    vs_out.uv[3] = gl_MultiTexCoord3.xy;
    vs_out.uv[4] = gl_MultiTexCoord4.xy;
    vs_out.uv[5] = gl_MultiTexCoord5.xy;
    vs_out.uv[6] = gl_MultiTexCoord6.xy;
    vs_out.color = gl_Color;

    // Position in view space (interpolated)
    vec4 viewPosition = gl_ModelViewMatrix * gl_Vertex;
    vs_out.position = viewPosition.xyz;
}