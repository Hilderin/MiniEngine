using ImGuiNET;
using MiniEngine.Drivers.Vulkan;
using System.Diagnostics;
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
        private PipelineDescriptorSet _mainSet;
        private PipelineDescriptorSet _fontTextureSet;
        private PipelineDescriptorSet _textureSet;

        private ImDrawDataPtr draw_data;

        private Stopwatch _stopWatch;

        public ImGuiRenderer(VkRenderer renderer)
        {
            _stopWatch = Stopwatch.StartNew();

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(renderer.Device.CurrentExtent.Width, renderer.Device.CurrentExtent.Height);
            //io.DisplayFramebufferScale = System.Numerics.Vector2.One * 0.5f;
            io.DeltaTime = (float)_stopWatch.Elapsed.TotalSeconds; // DeltaTime is in seconds.

            io.Fonts.AddFontDefault();
            io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

            _vk = renderer;
            _device = _vk.Device;

            _vertexBuffer = _device.CreateBufferWrapper(10000, BufferUsageFlags.VertexBuffer | BufferUsageFlags.TransferDst);
            _indexBuffer = _device.CreateBufferWrapper(2000, BufferUsageFlags.IndexBuffer | BufferUsageFlags.TransferDst);
            _projMatrixBuffer = _device.CreateBufferWrapper((uint)Marshal.SizeOf<Matrix4>(), BufferUsageFlags.UniformBuffer | BufferUsageFlags.TransferDst);

            CreateShader();

            _pipeline = new PipelineWrapper(_device, _vk.Swapchain.RenderPass, _shader, CullModeFlags.None);


            _mainSet = _pipeline.CreateDescriptorSet(0).Set("FontSampler", _vk.Sampler)
                                                       .Set("ProjectionMatrixBuffer", _projMatrixBuffer);
            _fontTextureSet = _pipeline.CreateDescriptorSet(1);
            _textureSet = _pipeline.CreateDescriptorSet(1);


            RecreateFontDeviceTexture();


            

        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void PreRender(CommandBuffer commandBuffer)
        {
            
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(_vk.Device.CurrentExtent.Width, _vk.Device.CurrentExtent.Height);
            io.DisplayFramebufferScale = System.Numerics.Vector2.One;
            io.DeltaTime = (float)_stopWatch.Elapsed.TotalSeconds; // DeltaTime is in seconds.
            

            ImGui.NewFrame();
            ImGui.ShowDemoWindow();
            //ImGui.Begin("Demo window");
            //ImGui.Button("Hello!");
            //ImGui.End();

            ImGui.Render();

            draw_data = ImGui.GetDrawData();

            if (draw_data.CmdListsCount == 0)
                return;

            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;


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
                    (uint)RoundUp(cmd_list.IdxBuffer.Size * (int)_sizeOfIndice, 4),
                    cmd_list.IdxBuffer.Data
                    );

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            //var io = ImGui.GetIO();

            //Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
            //    0f,
            //    io.DisplaySize.X,
            //    io.DisplaySize.Y,
            //    0.0f,
            //    -1.0f,
            //    1.0f);
            Matrix4 scale = Matrix4.CreateScaleMatrix(2.0f / io.DisplaySize.X, -2.0f / io.DisplaySize.Y, 1f);
            Matrix4 translate = Matrix4.CreateTranslationMatrix(-1f, 1f, 0f);
            Matrix4 mvp = translate * scale;

            commandBuffer.CmdUpdateBuffer(_projMatrixBuffer, 0, ref mvp);
            
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render(CommandBuffer commandBuffer)
        {
            if (draw_data.CmdListsCount == 0)
                return;

            commandBuffer.CmdBindVertexBuffer(0, _vertexBuffer, 0);
            commandBuffer.CmdBindIndexBuffer(_indexBuffer, 0, IndexType.Uint16);
            commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, _pipeline.Pipeline);

            commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _mainSet.DescriptorSets, null);

            //draw_data.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (pcmd.TextureId != IntPtr.Zero)
                        {
                            if (pcmd.TextureId == _fontAtlasID)
                            {
                                commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 1, _fontTextureSet.DescriptorSets, null);
                                //cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                            }
                            else
                            {
                                throw new InvalidOperationException("Not supported custom texture id");
                                //cl.SetGraphicsResourceSet(1, GetImageResourceSet(pcmd.TextureId));
                            }
                        }

                        //commandBuffer.CmdSetScissor(0, new Rect2D((int)pcmd.ClipRect.X, (int)pcmd.ClipRect.Y, (int)(pcmd.ClipRect.Z - pcmd.ClipRect.X), (int)(pcmd.ClipRect.W - pcmd.ClipRect.Y)));


                        commandBuffer.CmdDrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, (int)(pcmd.VtxOffset + vtx_offset), 0);
                    }
                }

                idx_offset += cmd_list.IdxBuffer.Size;
                vtx_offset += cmd_list.VtxBuffer.Size;
            }


            ImGui.NewFrame();
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
                _vertexBuffer = _device.CreateBufferWrapper((uint)(totalVBSize * 10f), BufferUsageFlags.VertexBuffer | BufferUsageFlags.TransferDst);
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * _sizeOfIndice);
            if (totalIBSize > _indexBuffer.Size)
            {
                _indexBuffer.Dispose();
                _indexBuffer = _device.CreateBufferWrapper((uint)(totalIBSize * 10f), BufferUsageFlags.IndexBuffer | BufferUsageFlags.TransferDst);
            }
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

            //var data = new Span<byte>(pixels, width * height * bytesPerPixel);
            //var dataBytes = data.ToArray();
            //File.WriteAllBytes("C:\\Projects\\Temp\\test.bin", dataBytes);
            _fontTexture = new VkTexture2D(pixels, width, height, Format.R8G8B8A8Unorm, _vk, _vk.ResourceFactory);
            _fontTextureSet.Set("FontTexture", _fontTexture.ImageWrapper.ImageView);
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

layout (location = 0) in vec2 in_position;
layout (location = 1) in vec2 in_texCoord;
layout (location = 2) in vec4 in_color;

layout (binding = 0) uniform ProjectionMatrixBuffer
{
    mat4 projection_matrix;
};

layout (location = 0) out vec4 color;
layout (location = 1) out vec2 texCoord;

out gl_PerVertex
{
    vec4 gl_Position;
};

void main() 
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
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
    //outputColor = color * texture(sampler2D(FontTexture, FontSampler), texCoord);
    outputColor = color * texture(sampler2D(FontTexture, FontSampler), texCoord);
    //outputColor = color;
}
"
, new Dictionary<string, Format>()
{
    { "in_color", Format.R8G8B8A8Unorm }
}
);
        }



        /// <summary>
        /// Permet d'arroundir à la valeur supérieur en int dans un multiple de X
        /// </summary>
        public static int RoundUp(int value, int multipleOf)
        {
            int size_difference = multipleOf - (value % 4);

            return value + size_difference;
        }

    }
}
