#version 460 compatibility

out vec4 vertexColor;

void main() 
{
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
    vertexColor = gl_Color;
}