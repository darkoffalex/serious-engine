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
    mat3 TBN;      // TBN matrix for normal mapping
} gs_out;

uniform int flatShading;

void main()
{
    vec3 edge1 = gs_in[1].position - gs_in[0].position;
    vec3 edge2 = gs_in[2].position - gs_in[0].position;
    vec2 deltaUV1 = gs_in[1].uv - gs_in[0].uv;
    vec2 deltaUV2 = gs_in[2].uv - gs_in[0].uv;

    float det = (deltaUV1.x * deltaUV2.y - deltaUV2.x * deltaUV1.y);
    float handedness = det < 0.0 ? -1.0 : 1.0;
    float f = 1.0f / det;
    
    vec3 polygonTangent = normalize(vec3(
        f * (deltaUV2.y * edge1.x - deltaUV1.y * edge2.x),
        f * (deltaUV2.y * edge1.y - deltaUV1.y * edge2.y),
        f * (deltaUV2.y * edge1.z - deltaUV1.y * edge2.z)));

    vec3 normal = vec3(0.0f);

    // Normal for flat shading
    if(bool(flatShading))
    {
        normal = normalize(cross(edge1, edge2));
    }
    
    for (int i = 0; i < 3; i++) 
    {
        if(!bool(flatShading))
        {
            normal = normalize(gs_in[i].normal);
        }

        gl_Position = gl_in[i].gl_Position;
        gs_out.uv = gs_in[i].uv;
        gs_out.color = gs_in[i].color;
        gs_out.position = gs_in[i].position;
        gs_out.normal = normal;

        vec3 T = normalize(polygonTangent - dot(polygonTangent, normal) * normal);
        vec3 B = normalize(cross(normal, T) * handedness);
        vec3 N = normal;
        gs_out.TBN = mat3(T, B, N);

        EmitVertex();
    }
    EndPrimitive();
}