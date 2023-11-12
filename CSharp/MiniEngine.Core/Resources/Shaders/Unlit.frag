#version 450
#extension GL_EXT_nonuniform_qualifier : enable

layout(binding = 1) uniform sampler2D _sampler_diffuse[];

layout(location = 0) in vec2 frag_tex_coord;
layout(location = 1) in flat uint texture_index;

layout(location = 0) out vec4 out_color;




void main() {
    out_color = texture(_sampler_diffuse[texture_index], frag_tex_coord);
}