#version 460 compatibility

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in VS_OUT {
    vec2 uv;
    vec4 color;
    vec3 position; // view-space position
    vec3 normal;
} gs_in[];

out GS_OUT {
    vec2 uv;
    vec4 color;
    vec3 position; // view-space position (pre-interpoaltion)
    vec3 normal;
} gs_out;

void main()
{
    // For each triangle vertex
    for (int i = 0; i < 3; i++) {
        gl_Position = gl_in[i].gl_Position;
        gs_out.uv = gs_in[i].uv;
        gs_out.color = gs_in[i].color;
        gs_out.position = gs_in[i].position;
        gs_out.normal = gs_in[i].normal;
        EmitVertex();
    }
    EndPrimitive();
}