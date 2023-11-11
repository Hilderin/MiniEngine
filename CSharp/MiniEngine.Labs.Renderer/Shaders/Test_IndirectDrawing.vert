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

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inColor;
layout(location = 2) in vec2 inTexCoord;

layout(std430, binding = 2) readonly buffer _objects {
    object_instance_data objects[];
};
layout(std430, binding = 3) readonly buffer _meshlet_instances {
    meshlet_instance_data meshlet_instances[];
};



layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec2 fragTexCoord;
layout(location = 2) flat out uint textureIndex;


void main() {
    uint object_index = meshlet_instances[gl_InstanceIndex].object_index;

    gl_Position = _matrix_vp * objects[object_index].transform_matrix * vec4(inPosition, 1.0);
    
    fragColor = inColor;
    fragTexCoord = inTexCoord;
    textureIndex = meshlet_instances[gl_InstanceIndex].texture_index;
}