//? #version 450

//-----------------------
//Structs
struct scene_data
{
    mat4 matrix_vp;
    vec3 camera_location;
    uint nb_meshlet_instances;
};

// Information on an instance on the GPU buffer
struct object_instance_data
{
    vec3 location;
    vec3 rotation;
    vec3 scale;
    mat4 transform_matrix;
};

// Information on a meshlet instance on the scene in the GPU buffer
struct meshlet_instance_data
{
    uint object_index;
    uint meshlet_index;
    uint texture_index;
    uint draw_calls_buffer_index;
    uint draw_call_index;
    uint visible;
};

// Information on MeshLet in the GPU buffer
struct meshlet_data
{
    uint vertices_buffer_index;
    uint indices_buffer_index;
    uint nb_indices;
};

// Indirect draw call (VkDrawIndexedIndirectCommand)
struct draw_call
{
    uint index_count;
    uint instance_count;
    uint first_index;
    uint vertex_offset;
    uint first_instance;
};
