#version 450
#extension GL_EXT_nonuniform_qualifier : enable

#include "render_structs.glsl"

//-----------------------
//Structs


//-----------------------
//Bindings
layout(binding = 10) uniform sampler2D _sampler_diffuse[];


//-----------------------
//Inputs Vertex attributes
layout(location = 0) in vec2 frag_tex_coord;
layout(location = 1) in flat uint texture_index;
layout(location = 2) in flat uint instance_index;

//-----------------------
//Outs variables
layout(location = 0) out vec4 out_color;

vec4 basic_colors[] = {
    // colors
   vec4(1.0f, 0.0f, 0.0f, 1.0f),
   vec4(0.0f, 1.0f, 0.0f, 1.0f),
   vec4(0.0f, 0.0f, 1.0f, 1.0f),
   vec4(1.0f, 1.0f, 0.0f, 1.0f),
   vec4(0.0f, 1.0f, 1.0f, 1.0f),
   vec4(1.0f, 1.0f, 1.0f, 1.0f),
   vec4(1.0f, 0.0f, 1.0f, 1.0f)
};   


//-----------------------
//Main
void main() {
    out_color = basic_colors[instance_index % basic_colors.length()];
    //out_color = texture(_sampler_diffuse[texture_index], frag_tex_coord);
}