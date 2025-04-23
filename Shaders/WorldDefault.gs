#version 460 compatibility

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in VS_OUT {
    vec2 uv[4];
    vec4 color;
    vec3 position; // view-space position
} gs_in[];

out GS_OUT {
    vec2 uv[4];
    vec4 color;
    vec3 position; // view-space position (pre-interpoaltion)
    vec3 normal;   // view-space normal (pre-interpolation)
} gs_out;

void main()
{
    // Calculate normal
    vec3 p0 = gs_in[0].position;
    vec3 p1 = gs_in[1].position;
    vec3 p2 = gs_in[2].position;
    vec3 edge1 = p1 - p0;
    vec3 edge2 = p2 - p0;
    vec3 normal = normalize(cross(edge1, edge2));

    // For each triangle vertex
    for (int i = 0; i < 3; i++) {
        gl_Position = gl_in[i].gl_Position;
        gs_out.uv[0] = gs_in[i].uv[0];
        gs_out.uv[1] = gs_in[i].uv[1];
        gs_out.uv[2] = gs_in[i].uv[2];
        gs_out.uv[3] = gs_in[i].uv[3];
        gs_out.color = gs_in[i].color;
        gs_out.position = gs_in[i].position;
        gs_out.normal = normal; // Same normal for all vertices (flat shading)
        EmitVertex();
    }
    EndPrimitive();
}