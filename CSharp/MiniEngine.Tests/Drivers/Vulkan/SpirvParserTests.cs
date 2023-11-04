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

            var shader = new ShaderWrapper(null, 
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
"
, @"#version 450

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

    }
}