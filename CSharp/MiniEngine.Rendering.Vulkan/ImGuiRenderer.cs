using ImGuiNET;
using MiniEngine.Drivers.Vulkan;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MiniEngine.Rendering.Vulkan
{
    public class ImGuiRenderer
    {
        private static uint _sizeOfDrawVert = (uint)Marshal.SizeOf<ImDrawVert>();
        private static uint _sizeOfIndice = sizeof(ushort);

        private VkRenderer _vk;
        private Device _device;
        private BufferWrapper _vertexBuffer;
        private BufferWrapper _indexBuffer;
        private BufferWrapper _projMatrixBuffer;
        private VkShader _shader;
        private PipelineWrapper _pipeline;
        private IntPtr _fontAtlasID = (IntPtr)1;
        private VkTexture2D _fontTexture;
        private PipelineDescriptorSet _fontSet;

        public ImGuiRenderer(VkRenderer renderer)
        {
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGui.GetIO().Fonts.AddFontDefault();
            ImGui.GetIO().Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

            _vk = renderer;
            _device = _vk.Device;

            _vertexBuffer = _device.CreateBufferWrapper(10000, BufferUsageFlags.VertexBuffer);
            _indexBuffer = _device.CreateBufferWrapper(2000, BufferUsageFlags.IndexBuffer);
            _projMatrixBuffer = _device.CreateBufferWrapper((uint)Marshal.SizeOf<Matrix4>(), BufferUsageFlags.UniformBuffer);

            CreateShader();

            _pipeline = new PipelineWrapper(_device, _vk.Swapchain.RenderPass, _shader);


            //_fontSet = _pipeline.CreateDescriptorSet().Set("texSampler", mat.VkDiffuseTexture.ImageWrapper.ImageView, _vk.Sampler);


        }


        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render(CommandBuffer commandBuffer)
        {

            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData(), commandBuffer);

        }

        /// <summary>
        /// Ensure the capacity of the buffers
        /// </summary>
        private void EnsureBufferCapacity(ImDrawDataPtr draw_data)
        {
            uint totalVBSize = (uint)(draw_data.TotalVtxCount * _sizeOfDrawVert);
            if (totalVBSize > _vertexBuffer.Size)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = _device.CreateBufferWrapper((uint)(totalVBSize * 1.5f), BufferUsageFlags.VertexBuffer);
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * _sizeOfIndice);
            if (totalIBSize > _indexBuffer.Size)
            {
                _indexBuffer.Dispose();
                _indexBuffer = _device.CreateBufferWrapper((uint)(totalIBSize * 1.5f), BufferUsageFlags.IndexBuffer);
            }
        }


        private unsafe void RenderImDrawData(ImDrawDataPtr draw_data, CommandBuffer commandBuffer)
        {
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            //Ensure the capacity of the buffers...
            EnsureBufferCapacity(draw_data);



            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[i];

                commandBuffer.CmdUpdateBuffer(_vertexBuffer, 
                    vertexOffsetInVertices * _sizeOfDrawVert,
                    (uint)(cmd_list.VtxBuffer.Size * _sizeOfDrawVert),
                    cmd_list.VtxBuffer.Data
                    );

                commandBuffer.CmdUpdateBuffer(
                    _indexBuffer,
                    indexOffsetInElements * _sizeOfIndice,
                    (uint)(cmd_list.IdxBuffer.Size * _sizeOfIndice),
                    cmd_list.IdxBuffer.Data
                    );

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            {
                var io = ImGui.GetIO();

                Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                    0f,
                    io.DisplaySize.X,
                    io.DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                commandBuffer.CmdUpdateBuffer(_projMatrixBuffer, 0, ref mvp);
            }

            commandBuffer.CmdBindVertexBuffer(0, _vertexBuffer, 0);
            commandBuffer.CmdBindIndexBuffer(_indexBuffer, 0, IndexType.Uint32);
            commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, _pipeline.Pipeline);



            //if (_pipeline.des != null)
            //    commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _pipeline.DescriptorSets, null);

            //draw_data.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            //// Render command lists
            //int vtx_offset = 0;
            //int idx_offset = 0;
            //for (int n = 0; n < draw_data.CmdListsCount; n++)
            //{
            //    ImDrawListPtr cmd_list = draw_data.CmdLists[n];
            //    for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            //    {
            //        ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
            //        if (pcmd.UserCallback != IntPtr.Zero)
            //        {
            //            throw new NotImplementedException();
            //        }
            //        else
            //        {
            //            if (pcmd.TextureId != IntPtr.Zero)
            //            {
            //                if (pcmd.TextureId == _fontAtlasID)
            //                {
            //                    cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
            //                }
            //                else
            //                {
            //                    cl.SetGraphicsResourceSet(1, GetImageResourceSet(pcmd.TextureId));
            //                }
            //            }

            //            cl.SetScissorRect(
            //                0,
            //                (uint)pcmd.ClipRect.X,
            //                (uint)pcmd.ClipRect.Y,
            //                (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
            //                (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

            //            cl.DrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, (int)(pcmd.VtxOffset + vtx_offset), 0);
            //        }
            //    }

            //    idx_offset += cmd_list.IdxBuffer.Size;
            //    vtx_offset += cmd_list.VtxBuffer.Size;
            //}
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            // Build
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

            // Store our identifier
            io.Fonts.SetTexID(_fontAtlasID);

            _fontTexture?.Dispose();

            ref byte[] pixelData = ref Unsafe.AsRef<byte[]>(pixels);

            _fontTexture = new VkTexture2D(pixelData, width, height, Format.R8G8B8A8Srgb, _vk, _vk.ResourceFactory);

            //_vk.ResourceFactory.CreateTexture2D(TextureDescription.Texture2D(
            //    (uint)width,
            //    (uint)height,
            //    1,
            //    1,
            //    PixelFormat.R8_G8_B8_A8_UNorm,
            //    TextureUsage.Sampled));
            //_fontTexture.Name = "ImGui.NET Font Texture";
            //gd.UpdateTexture(
            //    _fontTexture,
            //    (IntPtr)pixels,
            //    (uint)(bytesPerPixel * width * height),
            //    0,
            //    0,
            //    0,
            //    (uint)width,
            //    (uint)height,
            //    1,
            //    0,
            //    0);

            //_fontTextureResourceSet?.Dispose();
            //_fontTextureResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _fontTexture));
            //_fontTextureResourceSet.Name = "ImGui.NET Font Texture Resource Set";

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Create the shader
        /// </summary>
        private void CreateShader()
        {
            _shader = VkShaderHelper.CreateShader(@"#version 450

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
        }
    }
}
