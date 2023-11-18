using MiniEngine.Drivers.Vulkan;
using MiniEngine.Rendering.Vulkan;

namespace MiniEngine.Tests.Drivers.Vulkan
{
    [TestClass]
    public class SpirvParserTests
    {
        

        /// <summary>
        /// Parse ImGUI
        /// </summary>
        [TestMethod]
        public void SpirvParserImGuiTest()
        {

            var shader = new ShaderWrapper(null)
.SetCode(ShaderStageFlags.Vertex,
@"#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout (location = 0) in vec2 vsin_position;
layout (location = 1) in vec2 vsin_texCoord;
layout (location = 2) in vec4 vsin_color;

layout (binding = 0) uniform Projection
{
    mat4 projection;
};

layout (location = 0) out vec4 vsout_color;
layout (location = 1) out vec2 vsout_texCoord;

layout (constant_id = 0) const bool IsClipSpaceYInverted = true;
layout (constant_id = 1) const bool UseLegacyColorSpaceHandling = false;

out gl_PerVertex 
{
    vec4 gl_Position;
};

vec3 SrgbToLinear(vec3 srgb)
{
    return srgb * (srgb * (srgb * 0.305306011 + 0.682171111) + 0.012522878);
}

void main() 
{
    gl_Position = projection * vec4(vsin_position, 0, 1);
    vsout_color = vsin_color;
    if (!UseLegacyColorSpaceHandling)
    {
        vsout_color.rgb = SrgbToLinear(vsin_color.rgb);
    }
    vsout_texCoord = vsin_texCoord;
    if (IsClipSpaceYInverted)
    {
        gl_Position.y = -gl_Position.y;
    }
}
")
.SetCode(ShaderStageFlags.Fragment
,@"#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(set = 1, binding = 0) uniform texture2D FontTexture;
layout(set = 0, binding = 1) uniform sampler FontSampler;

layout (location = 0) in vec4 color;
layout (location = 1) in vec2 texCoord;
layout (location = 0) out vec4 outputColor;

void main()
{
    outputColor = color * texture(sampler2D(FontTexture, FontSampler), texCoord);
}
");


            Assert.AreEqual(2, shader.BindingSets.Length);
            Assert.AreEqual(2, shader.BindingSets[0].Length);
            Assert.AreEqual(1, shader.BindingSets[1].Length);

            Assert.AreEqual("Projection", shader.BindingSets[0][0].Name);
            Assert.AreEqual(DescriptorType.UniformBuffer, shader.BindingSets[0][0].DescriptorType);
            Assert.AreEqual("FontSampler", shader.BindingSets[0][1].Name);
            Assert.AreEqual(DescriptorType.Sampler, shader.BindingSets[0][1].DescriptorType);
            Assert.AreEqual("FontTexture", shader.BindingSets[1][0].Name);
            Assert.AreEqual(DescriptorType.SampledImage, shader.BindingSets[1][0].DescriptorType);

        }


        /// <summary>
        /// Parse ComputeDynamicWorkGroupSize
        /// </summary>
        [TestMethod]
        public void SpirvParserComputeDynamicWorkGroupSizeTest()
        {

            var shader = new ShaderWrapper(null)
.SetCode(ShaderStageFlags.Compute,
@"#version 450



//-----------------------
//Compute config
layout (local_size_x_id = 0, local_size_y = 1, local_size_z = 1) in;



//-----------------------
//Main
void main()
{
	
}
");


            Assert.AreEqual(0, shader.BindingSets.Length);
            Assert.AreEqual(1, shader.SpecializationConstants.Length);
            Assert.AreEqual("gl_WorkgroupSize.x", shader.SpecializationConstants[0].Name);
            Assert.AreEqual((uint)4, shader.SpecializationConstants[0].Size);

        }


        /// <summary>
        /// Parse PushConstant
        /// </summary>
        [TestMethod]
        public void SpirvParserPushConstantTest()
        {

            var shader = new ShaderWrapper(null)
.SetCode(ShaderStageFlags.Vertex,
@"#version 450


layout( push_constant ) uniform constants
{
	mat4 _matrix_vp;
};

layout(location = 3) in vec3 position;

//-----------------------
//Main
void main()
{
	gl_Position = _matrix_vp * vec4(position, 1.0);
}
");


            Assert.AreEqual(0, shader.BindingSets.Length);
            
            Assert.AreEqual(1, shader.VertexBindings.Length);
            Assert.AreEqual((uint)0, shader.VertexBindings[0].Binding);
            Assert.AreEqual((uint)12, shader.VertexBindings[0].Stride);
            
            Assert.AreEqual(1, shader.VertexInputAttributes.Length);
            Assert.AreEqual((uint)0, shader.VertexInputAttributes[0].Binding);
            Assert.AreEqual((uint)3, shader.VertexInputAttributes[0].Location);
            Assert.AreEqual(Format.R32G32B32Sfloat, shader.VertexInputAttributes[0].Format);


            Assert.AreEqual(0, shader.SpecializationConstants.Length);

            Assert.AreEqual(1, shader.Constants.Length);
            Assert.AreEqual("_matrix_vp", shader.Constants[0].Name);
            Assert.AreEqual((uint)0, shader.Constants[0].Offset);
            Assert.AreEqual((uint)64, shader.Constants[0].Size);
            Assert.AreEqual(ShaderStageFlags.Vertex, shader.Constants[0].Stage);

        }

        /// <summary>
        /// Parse PushConstant
        /// </summary>
        [TestMethod]
        public void SpirvParserPushConstant2Test()
        {

            var shader = new ShaderWrapper(null)
.SetCode(ShaderStageFlags.Vertex,
@"#version 450


layout( push_constant ) uniform constants
{
	mat4 _matrix_vp;
    layout(offset = 92) float _f1;
};

layout(location = 3) in vec3 position;

//-----------------------
//Main
void main()
{
	gl_Position = _matrix_vp * vec4(position, 1.0);
}
");


            Assert.AreEqual(0, shader.BindingSets.Length);

            Assert.AreEqual(1, shader.VertexBindings.Length);
            Assert.AreEqual((uint)0, shader.VertexBindings[0].Binding);
            Assert.AreEqual((uint)12, shader.VertexBindings[0].Stride);

            Assert.AreEqual(1, shader.VertexInputAttributes.Length);
            Assert.AreEqual((uint)0, shader.VertexInputAttributes[0].Binding);
            Assert.AreEqual((uint)3, shader.VertexInputAttributes[0].Location);
            Assert.AreEqual(Format.R32G32B32Sfloat, shader.VertexInputAttributes[0].Format);


            Assert.AreEqual(0, shader.SpecializationConstants.Length);

            Assert.AreEqual(2, shader.Constants.Length);
            Assert.AreEqual("_matrix_vp", shader.Constants[0].Name);
            Assert.AreEqual((uint)0, shader.Constants[0].Offset);
            Assert.AreEqual((uint)64, shader.Constants[0].Size);
            Assert.AreEqual(ShaderStageFlags.Vertex, shader.Constants[0].Stage);

            Assert.AreEqual("_f1", shader.Constants[1].Name);
            Assert.AreEqual((uint)92, shader.Constants[1].Offset);
            Assert.AreEqual((uint)4, shader.Constants[1].Size);
            Assert.AreEqual(ShaderStageFlags.Vertex, shader.Constants[1].Stage);

        }


        /// <summary>
        /// Parse SpecializationConstant
        /// </summary>
        [TestMethod]
        public void SpirvParserSpecializationConstantTest()
        {

            var shader = new ShaderWrapper(null)
.SetCode(ShaderStageFlags.Vertex,
@"#version 450


layout (constant_id = 2) const float C1 = 0.1f;

layout(location = 3) in vec3 position;

//-----------------------
//Main
void main()
{
	gl_Position = C1 * vec4(position, 1.0);
}
");


            Assert.AreEqual(1, shader.SpecializationConstants.Length);
            Assert.AreEqual("C1", shader.SpecializationConstants[0].Name);
            Assert.AreEqual((uint)4, shader.SpecializationConstants[0].Size);
            Assert.AreEqual((uint)2, shader.SpecializationConstants[0].ConstantId);

        }

        /// <summary>
        /// Parse SpecializationConstant2
        /// </summary>
        [TestMethod]
        public void SpirvParserSpecializationConstant2Test()
        {

            var shader = new ShaderWrapper(null)
.SetCode(ShaderStageFlags.Vertex,
@"#version 450


layout (constant_id = 0) const float C0 = 0.1f;
layout (constant_id = 1) const float C1 = 0.1f;

layout(location = 3) in vec3 position;

//-----------------------
//Main
void main()
{
	gl_Position = C1 * vec4(position, 1.0);
}
");


            Assert.AreEqual(2, shader.SpecializationConstants.Length);
            Assert.AreEqual("C0", shader.SpecializationConstants[0].Name);
            Assert.AreEqual((uint)4, shader.SpecializationConstants[0].Size);
            Assert.AreEqual((uint)0, shader.SpecializationConstants[0].ConstantId);

            Assert.AreEqual("C1", shader.SpecializationConstants[1].Name);
            Assert.AreEqual((uint)4, shader.SpecializationConstants[1].Size);
            Assert.AreEqual((uint)1, shader.SpecializationConstants[1].ConstantId);

        }
    }
}