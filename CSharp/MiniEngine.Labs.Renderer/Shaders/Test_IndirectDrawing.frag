#version 450
#extension GL_EXT_nonuniform_qualifier : enable

layout(binding = 1) uniform sampler2D _sampler_diffuse[];

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragTexCoord;
layout(location = 2) in flat uint textureIndex;

layout(location = 0) out vec4 outColor;




void main() {
    //outColor = vec4(fragColor, 1.0);
    outColor = texture(_sampler_diffuse[textureIndex], fragTexCoord);
}