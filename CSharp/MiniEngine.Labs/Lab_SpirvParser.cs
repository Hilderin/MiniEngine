using MiniEngine.Drivers.Vulkan;
using MiniEngine.Rendering.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Labs
{
    internal class Lab_SpirvParser
    {
        public void Test()
        {

            Shader shader = new Shader(@"#version 450

layout(binding = 0) uniform UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

//push constants block
layout( push_constant ) uniform constants
{
	mat4 render_matrix;
    mat4 render_matrix2;
} PushConstants;

layout(set = 0, location = 0) in vec3 inPosition;
layout(set = 0, location = 1) in vec3 inColor;
layout(set = 0, location = 2) in vec2 inTexCoord;
layout(set = 0, location = 3) in float inFloat;
layout(set = 1, location = 4) in vec3 inPosition2;

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec2 fragTexCoord;

void main() {
    gl_Position = PushConstants.render_matrix * vec4(inPosition, 1.0);
    
    fragColor = inColor;
    fragTexCoord = inTexCoord;
}
", @"#version 450

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragTexCoord;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(fragColor, 1.0);
}
");


            byte[] vertexBytes = VkShaderHelper.Compile(shader.VertexCode, ShaderStageFlags.Vertex);
            byte[] fragmentBytes = VkShaderHelper.Compile(shader.FragmentCode, ShaderStageFlags.Fragment);

            VkShader vkShader = SpirvParser.Parse(vertexBytes, fragmentBytes);




        }
    }
}
