#version 460 compatibility

out VS_OUT {
    vec2 uv;
    vec4 color;
    vec3 position;
    vec3 normal;
} vs_out;

void main()
{
    // Final position (projected)
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;

    // Texture mappings & vertex color
    vs_out.uv = gl_MultiTexCoord0.xy;
    vs_out.color = gl_Color;

    // Position in view space (interpolated)
    vec4 viewPosition = gl_ModelViewMatrix * gl_Vertex;
    vs_out.position = viewPosition.xyz;

    // Normal in view space (from vertex data)
    vec4 viewNormal = gl_ModelViewMatrix * vec4(gl_Normal, 0.0f);
    vs_out.normal = viewNormal.xyz;
}