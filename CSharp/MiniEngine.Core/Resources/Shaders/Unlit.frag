#version 450
#extension GL_EXT_nonuniform_qualifier : enable

//-----------------------
//Bindings
layout(binding = 10) uniform sampler2D _sampler_diffuse[];


//-----------------------
//Inputs Vertex attributes
layout(location = 0) in vec2 frag_tex_coord;
layout(location = 1) in flat uint texture_index;


//-----------------------
//Outs variables
layout(location = 0) out vec4 out_color;



//-----------------------
//Main
void main() {
    out_color = texture(_sampler_diffuse[texture_index], frag_tex_coord);
}