#version 460 compatibility

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in VS_OUT {
    vec2 uv[7];
    vec4 color;
    vec3 position; // view-space position
} gs_in[];

out GS_OUT {
    vec2 uv[7];
    vec4 color;
    vec3 position; // view-space position (pre-interpoaltion)
    vec3 normal;   // view-space normal (pre-interpolation)
    mat3 TBN;      // TBN matrix for normal mapping
} gs_out;

void main()
{
    // Get vertex data
    vec3 p0 = gs_in[0].position;
    vec3 p1 = gs_in[1].position;
    vec3 p2 = gs_in[2].position;
    vec2 uv0 = gs_in[0].uv[5]; // 5 - index for normal map texture's UV
    vec2 uv1 = gs_in[1].uv[5];
    vec2 uv2 = gs_in[2].uv[5];

    // Calculate edges
    vec3 edge1 = p1 - p0;
    vec3 edge2 = p2 - p0;

    // Calculate UV deltas
    vec2 deltaUV1 = uv1 - uv0;
    vec2 deltaUV2 = uv2 - uv0;

    // Calculate normal
    vec3 normal = normalize(cross(edge1, edge2));

    // Calculate tangent
    float det = deltaUV1.x * deltaUV2.y - deltaUV2.x * deltaUV1.y;
    float invDet = det != 0.0 ? 1.0 / det : 1.0; // Avoid division by zero
    vec3 tangent = normalize((deltaUV2.y * edge1 - deltaUV1.y * edge2) * invDet);

    // Orthogonalize tangent
    tangent = normalize(tangent - normal * dot(tangent, normal));

    // Calculate bitangent directly
    vec3 bitangent = normalize(cross(normal, tangent));

    // Form TBN matrix
    mat3 TBN = mat3(tangent, bitangent, normal);

    // For each triangle vertex
    for (int i = 0; i < 3; i++) {
        gl_Position = gl_in[i].gl_Position;
        gs_out.uv[0] = gs_in[i].uv[0];
        gs_out.uv[1] = gs_in[i].uv[1];
        gs_out.uv[2] = gs_in[i].uv[2];
        gs_out.uv[3] = gs_in[i].uv[3];

        gs_out.uv[4] = gs_in[i].uv[4];
        gs_out.uv[5] = gs_in[i].uv[5];
        gs_out.uv[6] = gs_in[i].uv[6];

        gs_out.color = gs_in[i].color;
        gs_out.position = gs_in[i].position;
        gs_out.normal = normal; // Same normal for all vertices (flat shading)
        gs_out.TBN = TBN; // Same TBN for all vertices (flat shading)
        EmitVertex();
    }
    EndPrimitive();
}