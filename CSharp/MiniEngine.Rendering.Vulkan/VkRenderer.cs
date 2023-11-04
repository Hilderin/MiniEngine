using MiniEngine.Drivers.Vulkan;
using MiniEngine.Drivers.Vulkan.Windows;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Vulkan renderer
    /// </summary>
    public class VkRenderer : IRenderer, IDisposable
    {
        #region Internal members


        public Matrix4 MVPMatrix;

        public Sampler Sampler;

        #endregion

        #region Public members

        /// <summary>
        /// Resource factory
        /// </summary>
        public VkResourceFactory ResourceFactory { get { return _resourceFactory; } }

        /// <summary>
        /// Get or set the current camera
        /// </summary>
        public Camera Camera { get; set; } = new Camera();

        #endregion

        #region Private members

        private List<VkMeshRenderer> _meshRenderers = new List<VkMeshRenderer>();
        private IWindow _window;
        private string _applicationName;
        private VkVersion _applicationVersion;
        private DebugCallback _debugCallback;
        private bool _initialized = false;
        private VkResourceFactory _resourceFactory;
        private ImGuiRenderer _imGui;
        private IntPtr _windowsHandle = IntPtr.Zero;
        private Dictionary<VkShader, PipelineWrapper> _cachePipeline = new Dictionary<VkShader, PipelineWrapper>();
        private bool _isDisposing;


        private VkInstance _vi;
        private Device _device;
        private PhysicalDevice _physicalDevice;
        private SurfaceKhr _surface;
        private SwapchainWrapper _swapchain;


        private uint _graphicsQueueIndex;
        private Queue _graphicsQueue;
        private uint _transferQueueIndex;
        private Queue _transferQueue;

        public Extent2D CurrentExtent;
        //public Vector2 ClientSize;
        public SurfaceCapabilitiesKhr SurfaceCapabilities;
        public List<Fence> Fences = new List<Fence>();
        public List<Semaphore> Semaphores = new List<Semaphore>();
        //public List<SwapchainWrapper> Swapchains = new List<SwapchainWrapper>();
        public List<CommandPool> CommandPools = new List<CommandPool>();
        public List<RenderPass> RenderPasses = new List<RenderPass>();
        public List<Sampler> Samplers = new List<Sampler>();
        public Dictionary<int, ShaderModule> CacheShaderModule = new Dictionary<int, ShaderModule>();

        public MemoryManager MemoryManager;
        
        public Device Device => _device;
        public SwapchainWrapper Swapchain => _swapchain;
        public uint GraphicsQueueIndex => _graphicsQueueIndex;
        public uint TransferQueueIndex => _transferQueueIndex;


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
        /// Activate Dear ImGui
        /// </summary>
        void IRenderer.EnableGui()
        {
            EnableGui();
        }

        /// <summary>
        /// Init ImGui
        /// </summary>
        public VkRenderer EnableGui()
        {
            _imGui ??= new ImGuiRenderer(this);
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

            Renderer.Current = this;

            //If we have enabled the debug mode...
            DebugReportCallback callback = null;
            if (_debugCallback != null)
                callback = DebugReportCallback;

            _vi = new VkInstance(_applicationName, _applicationVersion, callback);


            //Surface creation...
            _surface = CreateSurfaceCallback();

            //Physical device...
            _physicalDevice = PickPhysicalDevice();

            _graphicsQueueIndex = GetGraphicsQueueFamilyIndex(_surface);
            _transferQueueIndex = GetTransferQueueFamilyIndex();


            //And we can create a device...
            _device = _physicalDevice.CreateDevice(_surface, new[] { _graphicsQueueIndex, _transferQueueIndex });

            UpdateSurfaceCapabilities();

            MemoryManager = new MemoryManager(this);

            _swapchain = new SwapchainWrapper(this, new Format[] { Format.B8G8R8A8Srgb }, new ColorSpaceKhr[] { ColorSpaceKhr.SrgbNonlinear }, PresentModeKhr.Mailbox, true);

            _resourceFactory = new VkResourceFactory(this);

            Sampler = SamplerHelper.CreateMaxAnisotropy(_device);

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
            return _swapchain.CreatePipelineWrapper(shader.ShaderData);
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
        /// Add a mesh on the screen
        /// </summary>
        public IRenderHandle AddMesh(Mesh mesh, List<Material> materials, WorldTransform transform)
        {
            VkMeshRenderer meshRenderer = new VkMeshRenderer(mesh, materials, transform, this);
            _meshRenderers.Add(meshRenderer);
            return meshRenderer;
        }

        /// <summary>
        /// Remove a mesh from the screen
        /// </summary>
        public void RemoveMesh(IRenderHandle handle)
        {
            VkMeshRenderer meshRenderer = handle as VkMeshRenderer;

            if (meshRenderer != null)
            {
                meshRenderer.Dispose();
                _meshRenderers.Remove(meshRenderer);
            }

        }

        /// <summary>
        /// Render a scene
        /// </summary>
        public void Render()
        {
            if (!_initialized)
                Init();

            //If no camera... nothing to render...
            if (Camera != null)
            {
                RecalculateProjectionMatrix();
            }


            //Render the frame...
            RenderFrame();
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
                                .SetDepthTest(true)
                                .Build();

                _cachePipeline.Add(shader, pipeline);

            }

            return pipeline;
        }


        /// <summary>
        /// Create a shader module
        /// </summary>
        public ShaderModule CreateShaderModule(byte[] shaderCode, uint flags = 0, AllocationCallbacks allocator = null)
        {

            int key = BytesHelper.CombineHash(BytesHelper.GetHashCodeBytes(shaderCode), (int)flags);

            //Already in the cache??
            if (CacheShaderModule.TryGetValue(key, out ShaderModule shaderModule))
                return shaderModule;

            using (ShaderModuleCreateInfo createInfo = new ShaderModuleCreateInfo
            {
                CodeBytes = shaderCode,
                Flags = flags
            })
            {
                shaderModule = _device.CreateShaderModule(createInfo, allocator);
            }

            CacheShaderModule[key] = shaderModule;

            return shaderModule;

        }

        /// <summary>
        /// Update the current surface capabilities
        /// </summary>
        public void UpdateSurfaceCapabilities()
        {
            SurfaceCapabilities = _physicalDevice.GetSurfaceCapabilitiesKHR(_surface);

            CurrentExtent = SurfaceCapabilities.CurrentExtent;
        }

        /// <summary>
        /// Create a swapchain
        /// </summary>
        public SwapchainWrapper CreateSwapchainWrapper(Format[] expectedFormats, ColorSpaceKhr[] expectedColorSpaces, PresentModeKhr presentMode, bool depthTest)
        {
            return new SwapchainWrapper(this, expectedFormats, expectedColorSpaces, presentMode, depthTest);
        }



        public Queue GetGraphicsQueue()
        {
            _graphicsQueue ??= _device.GetQueue(GraphicsQueueIndex, 0);
            return _graphicsQueue;
        }

        

        public Queue GetTransferQueue()
        {
            _transferQueue ??= _device.GetQueue(TransferQueueIndex, 0);
            return _transferQueue;
        }


        public CommandPool CreateGraphicsCommandPool()
        {
            return _device.CreateCommandPool(GraphicsQueueIndex, CommandPoolCreateFlags.ResetCommandBuffer);
        }

        public CommandPool CreateTransferCommandPool()
        {
            return _device.CreateCommandPool(TransferQueueIndex, CommandPoolCreateFlags.ResetCommandBuffer);
        }



        /// <summary>
        /// Create a buffer
        /// </summary>
        public unsafe BufferWrapper CreateBufferWrapper<T>(T[] values, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags = MemoryPropertyFlags.HostVisible)
        {
            Type type = typeof(T);
            var size = System.Runtime.InteropServices.Marshal.SizeOf(type) * values.Length;

            BufferWrapper buffer = CreateBufferWrapper((uint)size, usageFlags, memoryPropertyFlags);

            buffer.Update(values);

            return buffer;
        }

        /// <summary>
        /// Create a buffer
        /// </summary>
        public unsafe BufferWrapper CreateBufferWrapper(uint size, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags = MemoryPropertyFlags.HostVisible)
        {
            return new BufferWrapper(this, size, usageFlags, memoryPropertyFlags);
        }



        /// <summary>
        /// Destruction
        /// </summary>
        public void Dispose()
        {
            if (_isDisposing)
                return;

            _isDisposing = true;


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


            _swapchain?.Dispose();

            if (_surface != null)
            {
                _vi.DestroySurfaceKHR(_surface);
                _surface = null;
            }



            foreach (Semaphore semaphore in Semaphores)
                _device.DestroySemaphore(semaphore);
            Semaphores.Clear();

            foreach (Fence fence in Fences)
                _device.DestroyFence(fence);
            Fences.Clear();

            //foreach (SwapchainWrapper swapchain in Swapchains.ToArray())
            //    swapchain.Dispose();
            //Swapchains.Clear();


            if (MemoryManager != null)
            {
                MemoryManager.Dispose();
                MemoryManager = null;
            }


            foreach (RenderPass renderPass in RenderPasses)
                _device.DestroyRenderPass(renderPass);
            RenderPasses.Clear();

            foreach (CommandPool commandPool in CommandPools)
                _device.DestroyCommandPool(commandPool);
            CommandPools.Clear();

            foreach (Sampler sampler in Samplers)
                _device.DestroySampler(sampler);
            Samplers.Clear();



            //We don't need it anymore...
            foreach (ShaderModule shaderModule in CacheShaderModule.Values)
                _device.DestroyShaderModule(shaderModule);


            _vi?.Dispose();

            if (Renderer.Current == this)
                Renderer.Current = null;
        }




        #endregion


        #region Private methods


        /// <summary>
        /// Get the right Physical device
        /// </summary>
        private PhysicalDevice PickPhysicalDevice()
        {
            //TODO: Check the physical device suitable for our project
            return _vi.EnumeratePhysicalDevices()[0];
        }

        /// <summary>
        /// Recalculate the projection matrix
        /// </summary>
        private void RecalculateProjectionMatrix()
        {


            //Update MVP Matrix...
            Matrix4 viewMat2 = Camera.GetViewMatrix();
            Matrix4 projMat = Camera.GetProjectionMatrixVulkan((int)CurrentExtent.Width, (int)CurrentExtent.Height);

            this.MVPMatrix = projMat * viewMat2;

        }

        /// <summary>
        /// Render the next frame
        /// </summary>
        private void RenderFrame()
        {
            RenderCommandBuffer commandBuffer = _swapchain.GetNextRenderCommandBuffer();

            commandBuffer.Begin();

            //If no camera, nothing to render... in 3D
            if (Camera != null)
            {
                foreach (var meshRenderer in _meshRenderers)
                    meshRenderer.PopulateCommandBuffers(commandBuffer);
            }

            //Rendering of ImGui...
            _imGui?.Render(commandBuffer);


            commandBuffer.End();


            //Execute the command buffer and show the results on surface...
            _swapchain.Present();
        }


        /// <summary>
        /// Create the surface
        /// </summary>
        private SurfaceKhr CreateSurfaceCallback()
        {
            //Function to create a Surface...
            if (_window != null)
            {
                //Already have a window...
                return CreateSurfaceFromWindow(_vi, _window);
            }
            else if (_windowsHandle != IntPtr.Zero)
            {
                //Windows...
                using (var createInfo = new Win32SurfaceCreateInfoKhr
                {
                    Hwnd = _windowsHandle,
                    Hinstance = System.Diagnostics.Process.GetCurrentProcess().Handle
                })
                {
                    return _vi.CreateWin32SurfaceKHR(createInfo);
                }
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
            UpdateSurfaceCapabilities();

            _swapchain?.NotifyWindowResized();

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


        /// <summary>
        /// Get the best queue index for transfer data
        /// </summary>
        private int GetQueueFamilyPriorityForTransfer(QueueFamilyProperties queueFamProp)
        {
            if (queueFamProp.QueueFlags.HasFlag(QueueFlags.Transfer))
            {
                //Important...
                if (!queueFamProp.QueueFlags.HasFlag(QueueFlags.Graphics) && !queueFamProp.QueueFlags.HasFlag(QueueFlags.Compute))
                    //Perfect, a queue specific to transfers...
                    return 0;

                if (!queueFamProp.QueueFlags.HasFlag(QueueFlags.Graphics))
                    //Perfect, a queue almost specific to transfers...
                    return 1;

                return 3;

            }
            else
                //Not good...
                return Int32.MaxValue;
        }


        /// <summary>
        /// Get the graphic queue family index
        /// </summary>
        public uint GetGraphicsQueueFamilyIndex(SurfaceKhr surface)
        {
            var queueFamilyProperties = _physicalDevice.GetQueueFamilyProperties();

            for (uint graphicsQueueIndex = 0; graphicsQueueIndex < queueFamilyProperties.Length; ++graphicsQueueIndex)
            {
                if (!_physicalDevice.GetSurfaceSupportKHR(graphicsQueueIndex, surface))
                    //This queue does not support SurfaceKHR...
                    continue;

                if (queueFamilyProperties[graphicsQueueIndex].QueueFlags.HasFlag(QueueFlags.Graphics))
                    //Found it! Should be good
                    return graphicsQueueIndex;
            }

            throw new Exception("Not device found for graphics queue.");
        }

        /// <summary>
        /// Get the transferer queue famility index
        /// </summary>
        public uint GetTransferQueueFamilyIndex()
        {
            bool found = false;
            uint transfersQueueIndex = 0;
            int bestPriority = Int32.MaxValue;
            var queueFamProps = _physicalDevice.GetQueueFamilyProperties();
            for (int i = 0; i < queueFamProps.Length; i++)
            {
                if (queueFamProps[i].QueueFlags.HasFlag(QueueFlags.Transfer))
                {
                    int priority = GetQueueFamilyPriorityForTransfer(queueFamProps[i]);
                    if (priority < bestPriority)
                    {
                        //This one is better...
                        transfersQueueIndex = (uint)i;
                        bestPriority = priority;
                        found = true;
                    }
                }
            }

            if(!found)
                throw new Exception("Not device found for transfer queue.");

            return transfersQueueIndex;
        }

        #endregion

    }
}
