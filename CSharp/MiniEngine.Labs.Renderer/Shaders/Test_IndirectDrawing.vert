#version 450

//push constants block
layout( push_constant ) uniform constants
{
	mat4 _matrix_vp;
};

struct object_instance_data
{
    vec3 location;
    vec3 rotation;
    vec3 scale;
    mat4 transform_matrix;
};

struct meshlet_instance_data
{
    uint object_index;
    uint meshlet_index;
    uint texture_index;
};

#include "../../MiniEngine.Core/Shaders/vertex_layout.glsl"


layout(std430, binding = 2) readonly buffer _objects {
    object_instance_data objects[];
};
layout(std430, binding = 3) readonly buffer _meshlet_instances {
    meshlet_instance_data meshlet_instances[];
};



layout(location = 0) out vec2 frag_tex_coord;
layout(location = 1) flat out uint texture_index;


void main() {
    uint object_index = meshlet_instances[gl_InstanceIndex].object_index;

    gl_Position = _matrix_vp * objects[object_index].transform_matrix * vec4(position, 1.0);
    
    frag_tex_coord = tex_coord;
    texture_index = meshlet_instances[gl_InstanceIndex].texture_index;
}