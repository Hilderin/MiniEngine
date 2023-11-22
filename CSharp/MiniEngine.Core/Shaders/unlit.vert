#version 450

#include "render_structs.glsl"



//-----------------------
//Bindings
layout(binding = 1) readonly uniform _scene {
    scene_data scene;
};
layout(std430, binding = 2) readonly buffer _objects {
    object_instance_data objects[];
};
layout(std430, binding = 3) readonly buffer _meshlet_instances {
    meshlet_instance_data meshlet_instances[];
};



//-----------------------
//Inputs Vertex attributes
#include "vertex_layout.glsl"


//-----------------------
//Outs variables
layout(location = 0) out vec2 frag_tex_coord;
layout(location = 1) flat out uint texture_index;
layout(location = 2) flat out uint instance_index;


//-----------------------
//Main
void main() {
    uint object_index = meshlet_instances[gl_InstanceIndex].object_index;

    gl_Position = scene.matrix_vp * objects[object_index].transform_matrix * vec4(position, 1.0);
        
    frag_tex_coord = tex_coord;
    texture_index = meshlet_instances[gl_InstanceIndex].texture_index;
    instance_index = gl_InstanceIndex;
}
