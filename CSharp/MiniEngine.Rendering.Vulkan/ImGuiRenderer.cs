using ImGuiNET;
using MiniEngine.Drivers.Vulkan;
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
        private Extent2D _currentExtent;
        private Stopwatch _stopWatch;
        private float _lastRenderTime = 0f;

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

            _vk = renderer;
            _device = _vk.Device;

            _vertexBuffer = _device.CreateBufferWrapper(10000, BufferUsageFlags.VertexBuffer | BufferUsageFlags.TransferDst);
            _indexBuffer = _device.CreateBufferWrapper(2000, BufferUsageFlags.IndexBuffer | BufferUsageFlags.TransferDst);
            _projMatrixBuffer = _device.CreateBufferWrapper((uint)Marshal.SizeOf<Matrix4>(), BufferUsageFlags.UniformBuffer | BufferUsageFlags.TransferDst);

            CreateShader();

            _pipeline = _vk.CreatePipelineWrapper(_shader)
                                .AddDynamicState(DynamicState.Scissor)
                                .Build();


            _mainSet = _pipeline.CreateDescriptorSet(0).Set("FontSampler", _vk.Sampler)
                                                       .Set("ProjectionMatrixBuffer", _projMatrixBuffer);
            _fontTextureSet = _pipeline.CreateDescriptorSet(1);
            _textureSet = _pipeline.CreateDescriptorSet(1);

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
        /// Update the input for the mouse and the keyboard to ImGui
        /// </summary>
        public void UpdateImGuiInput(Input input)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            //io.ClearInputKeys();

            io.AddMousePosEvent(input.MousePosition.X, input.MousePosition.Y);
            io.AddMouseButtonEvent(0, input.IsMouseDown(MouseButton.Left));
            io.AddMouseButtonEvent(1, input.IsMouseDown(MouseButton.Right));
            io.AddMouseButtonEvent(2, input.IsMouseDown(MouseButton.Middle));
            io.AddMouseButtonEvent(3, input.IsMouseDown(MouseButton.Button1));
            io.AddMouseButtonEvent(4, input.IsMouseDown(MouseButton.Button2));
            io.AddMouseWheelEvent(0f, input.MouseScrollDelta.Y);

            for (int i = 0; i < input.NewlyKeyDowns.Count; i++)
                ProcessKeyState(input.NewlyKeyDowns[i], true, io);

            for (int i = 0; i < input.NewlyKeyUps.Count; i++)
                ProcessKeyState(input.NewlyKeyUps[i], false, io);


        }

        /// <summary>
        /// Process a key state...
        /// </summary>
        private void ProcessKeyState(Keys key, bool down, ImGuiIOPtr io)
        {
            
            Debug.Print("newly " + (down ? "down": "up") + ": " + key);
            

            if (TryMapKey(key, out bool isTextInput, out ImGuiKey imguikey))
            {
                io.AddKeyEvent(imguikey, down);
            }

            if(down && isTextInput)
                io.AddInputCharacter((uint)key);
        }


        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render(CommandBuffer commandBuffer)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            float newTime = (float)_stopWatch.Elapsed.TotalSeconds;
            io.DeltaTime = newTime - _lastRenderTime; // DeltaTime is in seconds.
            _lastRenderTime = newTime;
            Debug.Print(io.DeltaTime.ToString());

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
                                }
                                else
                                {
                                    //TODO: Support custom texture
                                    throw new InvalidOperationException("Custom texture id is not supported");
                                }
                            }

                            //Scissor to clip what exceeds the window...
                            commandBuffer.CmdSetScissor(0, new Rect2D((int)pcmd.ClipRect.X, (int)pcmd.ClipRect.Y, (int)(pcmd.ClipRect.Z - pcmd.ClipRect.X), (int)(pcmd.ClipRect.W - pcmd.ClipRect.Y)));

                            commandBuffer.CmdDrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, (int)(pcmd.VtxOffset + vtx_offset), 0);
                        }
                    }

                    idx_offset += cmd_list.IdxBuffer.Size;
                    vtx_offset += cmd_list.VtxBuffer.Size;
                }
            }


            ImGui.NewFrame();
        }

        /// <summary>
        /// Update the projection matrix
        /// </summary>
        private void UpdateDisplaySize()
        {
            _currentExtent = _vk.Device.CurrentExtent;

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
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

            // Store our identifier
            io.Fonts.SetTexID(_fontAtlasID);

            _fontTexture?.Dispose();

            //var data = new Span<byte>(pixels, width * height * bytesPerPixel);
            //var dataBytes = data.ToArray();
            //File.WriteAllBytes("C:\\Projects\\Temp\\test.bin", dataBytes);
            _fontTexture = new VkTexture2D(pixels, width, height, Format.R8G8B8A8Unorm, _vk, _vk.ResourceFactory);
            _fontTextureSet.Set("FontTexture", _fontTexture.ImageWrapper.ImageView);

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


        private bool TryMapKey(Keys key, out bool isTextInput, out ImGuiKey result)
        {
            ImGuiKey keyToImGuiKeyShortcut(Keys keyToConvert, Keys startKey1, ImGuiKey startKey2)
            {
                int changeFromStart1 = (int)keyToConvert - (int)startKey1;
                return startKey2 + changeFromStart1;
            }

            if (key >= Keys.F1 && key <= Keys.F12)
            {
                result = keyToImGuiKeyShortcut(key, Keys.F1, ImGuiKey.F1);
                isTextInput = false;
                return true;
            }
            else if (key >= Keys.Numpad0 && key <= Keys.Numpad9)
            {
                result = keyToImGuiKeyShortcut(key, Keys.Numpad0, ImGuiKey.Keypad0);
                isTextInput = true;
                return true;
            }
            else if (key >= Keys.A && key <= Keys.Z)
            {
                result = keyToImGuiKeyShortcut(key, Keys.A, ImGuiKey.A);
                isTextInput = true;
                return true;
            }
            else if (key >= Keys.Number0 && key <= Keys.Number9)
            {
                result = keyToImGuiKeyShortcut(key, Keys.Number0, ImGuiKey._0);
                isTextInput = true;
                return true;
            }

            switch (key)
            {
                case Keys.ShiftLeft:
                case Keys.ShiftRight:
                    result = ImGuiKey.ModShift;
                    isTextInput = false;
                    return true;
                case Keys.ControlLeft:
                case Keys.ControlRight:
                    result = ImGuiKey.ModCtrl;
                    isTextInput = false;
                    return true;
                case Keys.AltLeft:
                case Keys.AltRight:
                    result = ImGuiKey.ModAlt;
                    isTextInput = false;
                    return true;
                case Keys.LeftSuper:
                case Keys.RightSuper:
                    result = ImGuiKey.ModSuper;
                    isTextInput = false;
                    return true;
                case Keys.Menu:
                    result = ImGuiKey.Menu;
                    isTextInput = false;
                    return true;
                case Keys.Up:
                    result = ImGuiKey.UpArrow;
                    isTextInput = false;
                    return true;
                case Keys.Down:
                    result = ImGuiKey.DownArrow;
                    isTextInput = false;
                    return true;
                case Keys.Left:
                    result = ImGuiKey.LeftArrow;
                    isTextInput = false;
                    return true;
                case Keys.Right:
                    result = ImGuiKey.RightArrow;
                    isTextInput = false;
                    return true;
                case Keys.Enter:
                    result = ImGuiKey.Enter;
                    isTextInput = false;
                    return true;
                case Keys.Escape:
                    result = ImGuiKey.Escape;
                    isTextInput = false;
                    return true;
                case Keys.Space:
                    result = ImGuiKey.Space;
                    isTextInput = true;
                    return true;
                case Keys.Tab:
                    result = ImGuiKey.Tab;
                    isTextInput = false;
                    return true;
                case Keys.Backspace:
                    result = ImGuiKey.Backspace;
                    isTextInput = false;
                    return true;
                case Keys.Insert:
                    result = ImGuiKey.Insert;
                    isTextInput = false;
                    return true;
                case Keys.Delete:
                    result = ImGuiKey.Delete;
                    isTextInput = false;
                    return true;
                case Keys.PageUp:
                    result = ImGuiKey.PageUp;
                    isTextInput = false;
                    return true;
                case Keys.PageDown:
                    result = ImGuiKey.PageDown;
                    isTextInput = false;
                    return true;
                case Keys.Home:
                    result = ImGuiKey.Home;
                    isTextInput = false;
                    return true;
                case Keys.End:
                    result = ImGuiKey.End;
                    isTextInput = false;
                    return true;
                case Keys.CapsLock:
                    result = ImGuiKey.CapsLock;
                    isTextInput = false;
                    return true;
                case Keys.ScrollLock:
                    result = ImGuiKey.ScrollLock;
                    isTextInput = false;
                    return true;
                case Keys.PrintScreen:
                    result = ImGuiKey.PrintScreen;
                    isTextInput = false;
                    return true;
                case Keys.Pause:
                    result = ImGuiKey.Pause;
                    isTextInput = false;
                    return true;
                case Keys.NumLock:
                    result = ImGuiKey.NumLock;
                    isTextInput = false;
                    return true;
                case Keys.NumpadDivide:
                    result = ImGuiKey.KeypadDivide;
                    isTextInput = false;
                    return true;
                case Keys.NumpadMultiply:
                    result = ImGuiKey.KeypadMultiply;
                    isTextInput = true;
                    return true;
                case Keys.NumpadSubtract:
                    result = ImGuiKey.KeypadSubtract;
                    isTextInput = true;
                    return true;
                case Keys.NumpadAdd:
                    result = ImGuiKey.KeypadAdd;
                    isTextInput = true;
                    return true;
                case Keys.NumpadDecimal:
                    result = ImGuiKey.KeypadDecimal;
                    isTextInput = true;
                    return true;
                case Keys.NumpadEnter:
                    result = ImGuiKey.KeypadEnter;
                    isTextInput = false;
                    return true;
                case Keys.GraveAccent:
                    result = ImGuiKey.GraveAccent;
                    isTextInput = true;
                    return true;
                case Keys.Minus:
                    result = ImGuiKey.Minus;
                    isTextInput = true;
                    return true;
                case Keys.Equal:
                    result = ImGuiKey.Equal;
                    isTextInput = true;
                    return true;
                case Keys.BracketLeft:
                    result = ImGuiKey.LeftBracket;
                    isTextInput = true;
                    return true;
                case Keys.BracketRight:
                    result = ImGuiKey.RightBracket;
                    isTextInput = true;
                    return true;
                case Keys.Semicolon:
                    result = ImGuiKey.Semicolon;
                    isTextInput = true;
                    return true;
                case Keys.Apostrophe:
                    result = ImGuiKey.Apostrophe;
                    isTextInput = true;
                    return true;
                case Keys.Comma:
                    result = ImGuiKey.Comma;
                    isTextInput = true;
                    return true;
                case Keys.Period:
                    result = ImGuiKey.Period;
                    isTextInput = true;
                    return true;
                case Keys.Slash:
                    result = ImGuiKey.Slash;
                    isTextInput = true;
                    return true;
                case Keys.Backslash:
                    result = ImGuiKey.Backslash;
                    isTextInput = true;
                    return true;
                default:
                    result = ImGuiKey.None;
                    isTextInput = false;
                    return false;
            }
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
