using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    public static class ShaderConverter
    {

        /// <summary>
        /// Convert a shader to a vulkan shader
        /// </summary>
        public static VkShader ConvertToVulkanShader(Shader shader)
        {
            VkShader newShader = VkShaderHelper.CreateShader(shader.VertexCode, shader.FragmentCode);

            ////Bindings for a uniformbuffer...
            //newShader.Bindings.Add(new()
            //{
            //    DescriptorType = DescriptorType.UniformBuffer,
            //    DescriptorCount = 1,
            //    StageFlags = ShaderStageFlags.Vertex
            //});

            //newShader.VertexBindings.Add(new()
            //{
            //    Binding = 0,
            //    Stride = (uint)Unsafe.SizeOf<Vertex>(),
            //    InputRate = VertexInputRate.Vertex,
            //});

            //newShader.VertexInputAttributes.Add(new()
            //{
            //    Binding = 0,
            //    Location = 0,
            //    Format = Format.R32G32B32Sfloat,
            //    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.Pos)),
            //});

            //newShader.VertexInputAttributes.Add(new()
            //{
            //    Binding = 0,
            //    Location = 1,
            //    Format = Format.R32G32B32Sfloat,
            //    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.Color)),
            //});

            //newShader.VertexInputAttributes.Add(new()
            //{
            //    Binding = 0,
            //    Location = 2,
            //    Format = Format.R32G32Sfloat,
            //    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord)),
            //});


            ////Constant form de mvp matrix...
            //newShader.Constants.Add(new()
            //{
            //    Size = (uint)Marshal.SizeOf<Matrix4>(),
            //    StageFlags = ShaderStageFlags.Vertex
            //});


            return newShader;
        }

    }
}
