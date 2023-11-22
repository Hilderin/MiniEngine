#version 450
#extension GL_EXT_nonuniform_qualifier : enable

#include "render_structs.glsl"

//-----------------------
//Structs


//-----------------------
//Bindings
layout(binding = 10) readonly uniform uni_color
{
    vec4 color;
};


//-----------------------
//Inputs Vertex attributes
layout(location = 0) in vec2 frag_tex_coord;
layout(location = 1) in flat uint texture_index;
layout(location = 2) in flat uint instance_index;

//-----------------------
//Outs variables
layout(location = 0) out vec4 out_color;


//-----------------------
//Main
void main() {
    out_color = color;
}