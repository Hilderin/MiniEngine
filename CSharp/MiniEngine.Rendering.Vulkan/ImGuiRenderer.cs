using ImGuiNET;
using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Dear ImGui renderer...
    /// </summary>
    public class ImGuiRenderer : IDisposable
    {
        private static uint _sizeOfDrawVert = (uint)Marshal.SizeOf<ImDrawVert>();
        private static uint _sizeOfIndice = sizeof(ushort);

        private IntPtr _context;
        private VkRenderer _renderer;
        private Device _device;
        private BufferWrapper _vertexBuffer;
        private BufferWrapper _indexBuffer;
        private BufferWrapper<Matrix4> _projMatrixBuffer;
        private ShaderWrapper _shader;
        private PipelineWrapper _pipeline;
        private IntPtr _fontAtlasID = (IntPtr)1;
        private VkTexture2D _fontTexture;
        private PipelineDescriptorSet _mainSet;
        private PipelineDescriptorSet _fontTextureSet;
        private PipelineDescriptorSet _textureSet;
        private Extent2D _currentExtent;
        private Stopwatch _stopWatch;
        private float _lastRenderTime = 0f;
        private CommandBuffer[] _secondaryCommandBuffers;

        public Device Device => _device;

        public ImGuiRenderer(VkRenderer renderer)
        {
            _stopWatch = Stopwatch.StartNew();

            _context = ImGui.CreateContext();
            ImGui.SetCurrentContext(_context);

            
            ImGuiIOPtr io = ImGui.GetIO();
            //io.DisplayFramebufferScale = System.Numerics.Vector2.One * 0.5f;
            io.DeltaTime = 0; // DeltaTime is in seconds.

            io.Fonts.AddFontDefault();
            io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

            _renderer = renderer;
            _device = _renderer.Device;

            _vertexBuffer = _renderer.CreateBufferWrapper(10000, BufferUsageFlags.VertexBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.HostVisible);
            _indexBuffer = _renderer.CreateBufferWrapper(2000, BufferUsageFlags.IndexBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.HostVisible);
            _projMatrixBuffer = _renderer.CreateBufferWrapper<Matrix4>(1, BufferUsageFlags.UniformBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.HostVisible);

            CreateShader();

            _pipeline = _renderer.Swapchain.CreatePipelineWrapper(_shader)
                                                .AddDynamicState(DynamicState.Scissor)
                                                .SetDepthTest(false)
                                                .Build();


            _mainSet = _pipeline.CreateDescriptorSet(0).Set("FontSampler", _renderer.DefaultSampler)
                                                       .Set("ProjectionMatrixBuffer", _projMatrixBuffer);
            _fontTextureSet = _pipeline.CreateDescriptorSet(1);
            _textureSet = _pipeline.CreateDescriptorSet(1);

            _secondaryCommandBuffers = _renderer.Swapchain.CreateSecondaryCommandBuffers();

            UpdateDisplaySize();

            RecreateFontDeviceTexture();

            ImGui.NewFrame();


        }

        /// <summary>
        /// Notification from the exterior that the window as resized
        /// </summary>
        public void NotifyWindowResized()
        {
            UpdateDisplaySize();
        }


        

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render(CommandBuffer commandBuffer)
        {
            CommandBuffer secCommandBuffer = _secondaryCommandBuffers[((RenderCommandBuffer)commandBuffer).ImageIndex];

            secCommandBuffer.Begin();

            ImGuiIOPtr io = ImGui.GetIO();
            float newTime = (float)_stopWatch.Elapsed.TotalSeconds;
            io.DeltaTime = newTime - _lastRenderTime; // DeltaTime is in seconds.
            _lastRenderTime = newTime;
            //Debug.Print(io.DeltaTime.ToString());

            ImGui.Render();

            var draw_data = ImGui.GetDrawData();



            if (draw_data.CmdListsCount > 0)
            {

                uint vertexOffsetInVertices = 0;
                uint indexOffsetInElements = 0;

                //Ensure the capacity of the buffers...
                EnsureBufferCapacity(draw_data);

                //Update vertex and indices buffer...
                for (int i = 0; i < draw_data.CmdListsCount; i++)
                {
                    ImDrawListPtr cmd_list = draw_data.CmdLists[i];

                    _vertexBuffer.Update(cmd_list.VtxBuffer.Data, vertexOffsetInVertices * _sizeOfDrawVert, (uint)(cmd_list.VtxBuffer.Size * _sizeOfDrawVert));

                    _indexBuffer.Update(cmd_list.IdxBuffer.Data, indexOffsetInElements * _sizeOfIndice, (uint)(cmd_list.IdxBuffer.Size * (int)_sizeOfIndice));

                    vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                    indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
                }

                secCommandBuffer.CmdBindVertexBuffer(0, _vertexBuffer, 0);
                secCommandBuffer.CmdBindIndexBuffer(_indexBuffer, 0, IndexType.Uint16);
                secCommandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, _pipeline.Pipeline);

                secCommandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _mainSet.DescriptorSets, null);

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
                                    secCommandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 1, _fontTextureSet.DescriptorSets, null);
                                }
                                else
                                {
                                    //TODO: Support custom texture
                                    throw new InvalidOperationException("Custom texture id is not supported");
                                }
                            }

                            //Scissor to clip what exceeds the window...
                            secCommandBuffer.CmdSetScissor(0, new Rect2D((int)pcmd.ClipRect.X, (int)pcmd.ClipRect.Y, (int)(pcmd.ClipRect.Z - pcmd.ClipRect.X), (int)(pcmd.ClipRect.W - pcmd.ClipRect.Y)));

                            secCommandBuffer.CmdDrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, (int)(pcmd.VtxOffset + vtx_offset), 0);
                        }
                    }

                    idx_offset += cmd_list.IdxBuffer.Size;
                    vtx_offset += cmd_list.VtxBuffer.Size;
                }
            }

            secCommandBuffer.End();

            commandBuffer.CmdExecuteCommand(secCommandBuffer);


            ImGui.NewFrame();
        }

        /// <summary>
        /// Update the projection matrix
        /// </summary>
        private void UpdateDisplaySize()
        {
            _currentExtent = _renderer.CurrentExtent;

            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(_currentExtent.Width, _currentExtent.Height);
            

            //Scale of fit the coord (-1, -1) on the top left and (1, 1) on the bottom right.
            //Negative 2 on Y to flip the screen upside down...
            Matrix4 scale = Matrix4.CreateScaleMatrix(2.0f / _currentExtent.Width, -2.0f / _currentExtent.Height, 1f);
            Matrix4 translate = Matrix4.CreateTranslationMatrix(-1f, 1f, 0f);
            Matrix4 mvp = translate * scale;

            _projMatrixBuffer.Update(ref mvp);
        }

        /// <summary>
        /// Ensure the capacity of the buffers
        /// </summary>
        private void EnsureBufferCapacity(ImDrawDataPtr draw_data)
        {
            uint totalVBSize = (uint)(draw_data.TotalVtxCount * _sizeOfDrawVert);
            if (totalVBSize > _vertexBuffer.Size)
            {
                _vertexBuffer.Resize((uint)(totalVBSize * 1.5f));
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * _sizeOfIndice);
            if (totalIBSize > _indexBuffer.Size)
            {
                _indexBuffer.Resize((uint)(totalIBSize * 1.5f));
            }
        }



        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            // Build
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out _);

            // Store our identifier
            io.Fonts.SetTexID(_fontAtlasID);

            _fontTexture?.Dispose();

            //var data = new Span<byte>(pixels, width * height * bytesPerPixel);
            //var dataBytes = data.ToArray();
            //File.WriteAllBytes("C:\\Projects\\Temp\\test.bin", dataBytes);
            _fontTexture = new VkTexture2D(pixels, width, height, Format.R8G8B8A8Unorm, _renderer, _renderer.ResourceFactory);
            _fontTextureSet.Set("FontTexture", _fontTexture.ImageWrapper.ImageView);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Create the shader
        /// </summary>
        private void CreateShader()
        {
            _shader = new ShaderWrapper(_renderer)
.SetVariable("in_color", new() { Format = Format.R8G8B8A8Unorm })
.SetCode(ShaderStageFlags.Vertex,
@"#version 450

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
)
.SetCode(ShaderStageFlags.Fragment
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
");

        }


        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _projMatrixBuffer?.Dispose();
            _pipeline.Dispose();
            _fontTexture.Dispose();
            _mainSet.Dispose();
            _fontTextureSet.Dispose();
            _textureSet.Dispose();

            ImGui.DestroyContext(_context);
        }
    }
}
