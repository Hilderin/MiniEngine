using MiniEngine.Drivers.Vulkan;
using MiniEngine.Drivers.Vulkan.Windows;
using MiniEngine.ResourceDefinitions;
using System.Diagnostics;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Vulkan renderer
    /// </summary>
    public class VkRenderer : IRenderer, IDisposable
    {
        #region Internal members

        internal VkInstance vk;
        internal Device Device;
        internal SwapchainWrapper Swapchain;

        internal Matrix4 MVPMatrix;

        internal Sampler Sampler;

        #endregion

        #region Public members

        /// <summary>
        /// Indicate if we sould swap the buffer
        /// </summary>
        public bool ShouldSwapBuffer { get; set; } = true;

        /// <summary>
        /// Resource factory
        /// </summary>
        public VkResourceFactory ResourceFactory { get { return _resourceFactory; } }

        bool IRenderer.ShouldSwapBuffer => throw new NotImplementedException();

        #endregion

        #region Private members

        private List<VkMeshRenderer> _meshRenderers = new List<VkMeshRenderer>();
        private IWindow _window;
        private string _applicationName;
        private VkVersion _applicationVersion;
        private Func<VkInstance, SurfaceKhr> _surfaceCreationCallback;
        private DebugCallback _debugCallback;
        private bool _initialized = false;
        private VkResourceFactory _resourceFactory;
        private ImGuiRenderer _imGui;
        private IntPtr _windowsHandle = IntPtr.Zero;
        private Dictionary<VkShader, PipelineWrapper> _cachePipeline = new Dictionary<VkShader, PipelineWrapper>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public VkRenderer(string applicationName, string applicationVersion)
        {
            _applicationName = applicationName;
            _applicationVersion = new VkVersion(applicationVersion);
            


        }

        #endregion

        #region Public methods

        /// <summary>
        /// Enable debugging
        /// </summary>
        void IRenderer.EnableDebug(DebugCallback debugCallback)
        {
            _debugCallback = debugCallback;
        }

        /// <summary>
        /// Enable debugging
        /// </summary>
        public VkRenderer EnableDebug(DebugCallback debugCallback)
        {
            _debugCallback = debugCallback;
            return this;
        }

        /// <summary>
        /// Init ImGui
        /// </summary>
        public VkRenderer InitGui()
        {
            _imGui = new ImGuiRenderer(this);
            return this;
        }

        /// <summary>
        /// Update the input for the mouse and the keyboard to ImGui
        /// </summary>
        public void UpdateImGuiInput(InputManager input)
        {
            _imGui.UpdateImGuiInput(input);
        }


        /// <summary>
        /// Init the renderer
        /// </summary>
        public void Init()
        {
            if (_initialized)
                throw new Exception("Already initialized.");

            //If we have enabled the debug mode...
            DebugReportCallback callback = null;
            if (_debugCallback != null)
                callback = DebugReportCallback;

            vk = new VkInstance(_applicationName, _applicationVersion, CreateSurfaceCallback, callback);

            Device = vk.Device;

            Swapchain = new SwapchainWrapper(vk.Device, new Format[] { Format.B8G8R8A8Srgb }, new ColorSpaceKhr[] { ColorSpaceKhr.SrgbNonlinear }, PresentModeKhr.Mailbox);


            _resourceFactory = new VkResourceFactory(this);

            Sampler = SamplerHelper.CreateMaxAnisotropy(Device);

            _initialized = true;
        }


        /// <summary>
        /// Create a surface
        /// </summary>
        public SurfaceKhr CreateSurfaceFromWindow(VkInstance vi, IWindow window)
        {
            unsafe
            {
                SurfaceKhr surface = new SurfaceKhr();

                fixed (ulong* ptr = &surface.Handle)
                {
                    window.CreateSurface(vi.Handle, (IntPtr)ptr);
                }
                return surface;
            }
        }

        /// <summary>
        /// Creates a pipeline wrapper
        /// </summary>
        public PipelineWrapper CreatePipelineWrapper(VkShader shader)
        {
            return Swapchain.CreatePipelineWrapper(shader.ShaderData);
        }


        /// <summary>
        /// Set Window from the interface IRenderer
        /// </summary>
        void IRenderer.SetWindow(IWindow window)
        {
            SetWindow(window);
        }

        /// <summary>
        /// Set Windows handle from the interface IRenderer
        /// </summary>
        void IRenderer.SetWindow32Handle(IntPtr handle)
        {
            SetWindow32Handle(handle);
        }

        /// <summary>
        /// Pass the window to the render when it's created
        /// </summary>
        public VkRenderer SetWindow(IWindow window)
        {
            _window = window;

            _window.OnWindowResized += Window_OnWindowResized;

            return this;
        }

        /// <summary>
        /// Set the window handle for win32 (Windows)
        /// </summary>
        public VkRenderer SetWindow32Handle(IntPtr handle)
        {
            _windowsHandle = handle;

            return this;
        }

        /// <summary>
        /// Render a scene
        /// </summary>
        public void Render(Scene scene)
        {
            if (!_initialized)
                Init();

            RecalculateNextFrame(scene);

            //Render the frame...
            RenderFrame(scene);
        }


        /// <summary>
        /// Tale a screenshot
        /// </summary>
        public byte[] GetFramebufferRGBA(int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a new mesh
        /// </summary>
        public Mesh CreateMesh(MeshDefinition meshDefinition)
        {
            return _resourceFactory.CreateMesh(meshDefinition);
        }

        /// <summary>
        /// Create a Texture2D
        /// </summary>
        public Texture2D CreateTexture2D(Texture2DDefinition texDef)
        {
            return _resourceFactory.CreateTexture2D(texDef);
        }

        /// <summary>
        /// Create a Material
        /// </summary>
        public Material CreateMaterial(MaterialDefinition matDef)
        {
            return _resourceFactory.CreateMaterial(matDef);
        }

        /// <summary>
        /// Create a shader
        /// </summary>
        public Shader CreateShader(ShaderDefinition shaderDef)
        {
            return _resourceFactory.CreateShader(shaderDef);
        }


        /// <summary>
        /// Get a pipeline for mesh rendering
        /// </summary>
        public PipelineWrapper GetPipeline(VkShader shader)
        {
            if (!_cachePipeline.TryGetValue(shader, out var pipeline))
            {
                pipeline = CreatePipelineWrapper(shader)
                                .SetCullMode(CullModeFlags.Back)
                                .Build();

                _cachePipeline.Add(shader, pipeline);

            }

            return pipeline;
        }


        /// <summary>
        /// Destruction
        /// </summary>
        public void Dispose()
        {
            if (_window != null)
                _window.OnWindowResized -= Window_OnWindowResized;

            _imGui?.Dispose();
            _resourceFactory?.Dispose();

            //Disposing mesh renderer...
            foreach (VkMeshRenderer vkMeshRenderer in _meshRenderers)
                vkMeshRenderer.Dispose();

            //Disposing pipelines...
            foreach (PipelineWrapper pipeline in _cachePipeline.Values)
                pipeline.Dispose();

            Swapchain?.Dispose();

            

            vk?.Dispose();
        }


        

        #endregion


        #region Private methods

        /// <summary>
        /// Recalculate information for the next frame
        /// </summary>
        private void RecalculateNextFrame(Scene scene)
        {
            //The camera needs to be the same size has the client...
            if (Device.CurrentExtent.Width != scene.Camera.ClientSize.X || Device.CurrentExtent.Height != scene.Camera.ClientSize.Y)
                scene.Camera.ClientSize = new Vector2(Device.CurrentExtent.Width, Device.CurrentExtent.Height);

            RecalculateProjectionMatrix(scene);

            //-------------------
            //New meshes..........
            if (HistoryManager.Current.AddedMeshes.Count > 0)
            {
                foreach (var meshComponent in HistoryManager.Current.AddedMeshes)
                {
                    //Initialization of the mesh renderer...
                    VkMeshRenderer meshRenderer = new VkMeshRenderer(meshComponent, this);
                    meshComponent.RendererStateObj = meshRenderer;
                    _meshRenderers.Add(meshRenderer);


                }
                HistoryManager.Current.AddedMeshes.Clear();
            }


            //-------------------
            //Deleted meshes..........
            if (HistoryManager.Current.RemovedMeshes.Count > 0)
            {
                foreach (var meshComponent in HistoryManager.Current.RemovedMeshes)
                {
                    //Initialization of the mesh renderer...
                    VkMeshRenderer meshRenderer = meshComponent.RendererStateObj as VkMeshRenderer;

                    if (meshRenderer != null)
                    {
                        meshRenderer.Dispose();
                        _meshRenderers.Remove(meshRenderer);
                    }

                }
                HistoryManager.Current.RemovedMeshes.Clear();
            }

        }

        /// <summary>
        /// Recalculate the projection matrix
        /// </summary>
        private void RecalculateProjectionMatrix(Scene scene)
        {
           

            //Update MVP Matrix...
            Matrix4 viewMat2 = scene.Camera.GetMatrix();
            Matrix4 projMat = scene.Camera.GetProjectionMatrixVulkan();
            
            this.MVPMatrix = projMat * viewMat2;

        }

        /// <summary>
        /// Render the next frame
        /// </summary>
        private void RenderFrame(Scene scene)
        {
            RenderCommandBuffer commandBuffer = Swapchain.GetNextRenderCommandBuffer();

            commandBuffer.Begin();

            //If no camera.. then.. nothing on screen...
            if (scene.Camera != null)
            {
                foreach (var meshRenderer in _meshRenderers)
                    meshRenderer.PopulateCommandBuffers(commandBuffer);
            }

            //Rendering of ImGui...
            if (_imGui != null)
                _imGui.Render(commandBuffer);


            commandBuffer.End();


            //Execute the command buffer and show the results on surface...
            Swapchain.Present(commandBuffer);
        }


        /// <summary>
        /// Create the surface
        /// </summary>
        private SurfaceKhr CreateSurfaceCallback(VkInstance vi)
        {
            //Function to create a Surface...
            if (_window != null)
            {
                //Already have a window...
                return CreateSurfaceFromWindow(vi, _window);
            }
            else if (_windowsHandle != IntPtr.Zero)
            {
                //Windows...
                return vi.CreateWin32SurfaceKHR(
                    new Win32SurfaceCreateInfoKhr
                    {
                        Hwnd = _windowsHandle,
                        Hinstance = Process.GetCurrentProcess().Handle
                    });
            }
            else
                throw new Exception("Impossible to create the surface. No window and no window handle exists.");

        }


        /// <summary>
        /// Event when window was resized
        /// </summary>
        private void Window_OnWindowResized(Vector2 obj)
        {
            //Update the suface capability and current extend in the device...
            Device.UpdateSurfaceCapabilities();

            Swapchain?.NotifyWindowResized();

            _imGui?.NotifyWindowResized();
        }

        /// <summary>
        /// Internal debug callback
        /// </summary>
        private bool DebugReportCallback(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, int messageCode, string message)
        {
            _debugCallback((DebugLevel)flags, messageCode, message);
            return true;
        }

        #endregion

    }
}
