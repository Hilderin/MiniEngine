#version 450    //!

//-----------------------
//Structs
struct scene_data
{
    mat4 matrix_vp;
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
    uint draw_calls_buffer_index;
    uint draw_call_index;
    uint visible;
};
