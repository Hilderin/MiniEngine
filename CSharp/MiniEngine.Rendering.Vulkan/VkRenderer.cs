using MiniEngine.Drivers.Vulkan;
using MiniEngine.Drivers.Vulkan.Windows;
using MiniEngine.Profiling;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Vulkan renderer
    /// </summary>
    public class VkRenderer : IRenderer, IDisposable
    {
        /// <summary>
        /// Max number of vertices max
        /// </summary>
        private const int NB_VERTICES_MAX = 1024 * 1024;

        /// <summary>
        /// Max number of meshlets
        /// </summary>
        private const int NB_MESHLET_MAX = 10000;

        /// <summary>
        /// Max number of objects
        /// </summary>
        private const int NB_OBJECTS_MAX = 1024 * 512;

        /// <summary>
        /// Max number of meshlets instances
        /// </summary>
        private const int NB_MESHLET_INSTANCES_MAX = 1024 * 1024;


        #region Public members

        /// <summary>
        /// Data in the scene buffer
        /// </summary>
        public SceneData SceneData;

        /// <summary>
        /// Default sampler
        /// </summary>
        public Sampler DefaultSampler;
        
        /// <summary>
        /// Get or set the current camera
        /// </summary>
        public Camera Camera { get; set; } = new Camera();

        

        /// <summary>
        /// Max compute wordgroupe size (index 0 = x, 1 = y, 2 = z).
        /// 128 is the minimum for this maximum value
        /// </summary>
        public uint[] MaxComputeWorkgroupSize { get; private set; } = new uint[] { 128, 128, 128 };

        /// <summary>
        /// Get if the renderer supports async asset loading
        /// </summary>
        public bool SupportAsyncAssetLoading => true;

        public Device Device => _device;
        public SwapchainWrapper Swapchain => _swapchain;
        public Profiler FrameProfiler => _frameProfiler;
        public bool IsDisposing => _isDisposing;
        public uint GraphicsQueueIndex => _graphicsQueueIndex;
        public QueueWrapper GraphicsQueue => _graphicsQueue;
        public uint TransferQueueIndex => _transferQueueIndex;
        public uint ComputeQueueIndex => _computeQueueIndex;
        public Extent2D CurrentExtent => _currentExtent;
        public SurfaceCapabilitiesKhr SurfaceCapabilities => _surfaceCapabilities;
        public MemoryManager MemoryManager => _memoryManager;
        public VkResourceFactory ResourceFactory => _resourceFactory;
        public BufferWrapper<SceneData> SceneBuffer => _sceneBuffer;
        public BufferWrapper<Vertex> VerticesBuffer => _verticesBuffer;
        public BufferWrapper<ushort> IndicesBuffer => _indicesBuffer;
        public BufferWrapper<MeshletData> MeshLetsBuffer => _meshLetsBuffer;
        public BufferWrapper<ObjectInstanceData> ObjectsBuffer => _objectsBuffer;
        public BufferWrapper<MeshLetInstanceData> MeshLetInstancesBuffer => _meshLetInstancesBuffer;
        public List<VkMeshRenderer> MeshRenderers => _meshRenderers;
        public List<BufferWrapper> DrawCallsBuffers => _drawCallsBuffers;
        public BufferWrapper<uint> DrawCallsCountsBuffer => _drawCallsCountsBuffer;
        public BufferWrapper<int> LastMaxDrawBufferIndex => _lastMaxDrawBufferIndex;
        



        #endregion

        #region Private members

        private List<VkMeshInstance> _meshInstances = new List<VkMeshInstance>();
        private List<VkMeshRenderer> _meshRenderers = new List<VkMeshRenderer>();
        private List<BufferWrapper> _drawCallsBuffers = new List<BufferWrapper>();
        private IWindow _window;
        private bool _headless;
        private string _applicationName;
        private VkVersion _applicationVersion;
        private DebugCallback _debugCallback;
        private bool _printDebug = true;
        private bool _initialized = false;
        private VkResourceFactory _resourceFactory;
        private ImGuiRenderer _imGui;
        private IntPtr _windowsHandle = IntPtr.Zero;
        private Dictionary<VkShader, PipelineWrapper> _cachePipeline = new Dictionary<VkShader, PipelineWrapper>();
        private Dictionary<VkShader, VkMeshRenderer> _cacheMeshRenderer = new Dictionary<VkShader, VkMeshRenderer>();
        private Dictionary<int, ShaderModule> _cacheShaderModule = new Dictionary<int, ShaderModule>();
        private bool _isDisposing;

        private VkInstance _vi;
        private Device _device;
        private PhysicalDevice _physicalDevice;
        private SurfaceKhr _surface;
        private SwapchainWrapper _swapchain;
        private Extent2D _currentExtent;
        private SurfaceCapabilitiesKhr _surfaceCapabilities;
        private MemoryManager _memoryManager;

        private List<Action> _actionsBeforeEachFrame = new List<Action>();
        private ConcurrentQueue<Action> _actionsBeforeNextFrame = new ConcurrentQueue<Action>();
        private ConcurrentQueue<Action> _actionsBeforeNextFrameAsync = new ConcurrentQueue<Action>();

        private BufferWrapper<SceneData> _sceneBuffer;
        private BufferWrapper<Vertex> _verticesBuffer;
        private BufferWrapper<ushort> _indicesBuffer;
        private BufferWrapper<MeshletData> _meshLetsBuffer;
        private BufferWrapper<ObjectInstanceData> _objectsBuffer;
        private BufferWrapper<MeshLetInstanceData> _meshLetInstancesBuffer;
        private BufferWrapper<uint> _drawCallsCountsBuffer;
        private BufferWrapper<int> _lastMaxDrawBufferIndex;


        private uint _graphicsQueueIndex;
        private QueueWrapper _graphicsQueue;
        private uint _transferQueueIndex;
        private uint _computeQueueIndex;
        //private Queue _transferQueue;

        private List<IRendererExtension> _extensions = new List<IRendererExtension>();


        //Profiler...
        private Profiler _frameProfiler;
        private ProfilerStep _updateSceneDataProfilerStep;
        private ProfilerStep _beforeEachFrameProfilerStep;
        private ProfilerStep _createCommandBuffersProfilerStep;
        private ProfilerStep _imGuiRenderProfilerStep;
        private ProfilerInfo _meshletCountProfilerInfo;



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
            EnableDebug(debugCallback);
        }

        /// <summary>
        /// Enable debugging
        /// </summary>
        public VkRenderer EnableDebug(DebugCallback debugCallback = null)
        {
            if (debugCallback == null)
                debugCallback = DefaultDebugCallback;

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
        /// Init the renderer
        /// </summary>
        void IRenderer.Init()
        {
            Init();
        }

        /// <summary>
        /// Init the renderer
        /// </summary>
        public VkRenderer Init()
        {
            if (_initialized)
                throw new Exception("Already initialized.");

            Renderer.Current = this;

            List<string> extensions = new List<string>();

            if (!_headless)
            {
                extensions.Add("VK_KHR_surface");
                extensions.Add("VK_KHR_win32_surface");
            }

            //If we have enabled the debug mode...
            DebugReportCallback debugCallback = null;
            if (_debugCallback != null)
            {
                debugCallback = DebugReportCallback;
                extensions.Add("VK_EXT_debug_report");
            }

            _vi = new VkInstance(_applicationName, _applicationVersion, extensions.ToArray(), debugCallback);


            //Surface creation...
            if(!_headless)
                _surface = CreateSurfaceCallback();

            //Physical device...
            _physicalDevice = PickPhysicalDevice();

            InitDeviceProperties();

            _graphicsQueueIndex = GetGraphicsQueueFamilyIndex(_surface);
            _transferQueueIndex = GetTransferQueueFamilyIndex();
            _computeQueueIndex = GetComputeQueueFamilyIndex();

            _device = CreateDevice();

            UpdateSurfaceCapabilities();

            _graphicsQueue = new QueueWrapper(_device, _graphicsQueueIndex, 0, false);

            _memoryManager = new MemoryManager(this);

            //No swapchain in headless mode...
            if(!_headless)
                _swapchain = new SwapchainWrapper(this, new Format[] { Format.B8G8R8A8Srgb }, new ColorSpaceKhr[] { ColorSpaceKhr.SrgbNonlinear }, PresentModeKhr.Mailbox, true);

            _resourceFactory = new VkResourceFactory(this);

            DefaultSampler = SamplerHelper.CreateMaxAnisotropy(_device);

            //TODO: Dynamiccaly calculate best size
            _sceneBuffer = CreateBufferWrapper<SceneData>(1, BufferUsageFlags.UniformBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.HostVisible);
            _verticesBuffer = CreateBufferWrapper<Vertex>(NB_VERTICES_MAX, BufferUsageFlags.VertexBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);
            _indicesBuffer = CreateBufferWrapper<ushort>(NB_VERTICES_MAX, BufferUsageFlags.IndexBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);
            _meshLetsBuffer = CreateBufferWrapper<MeshletData>(NB_MESHLET_MAX, BufferUsageFlags.StorageBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);
            _objectsBuffer = CreateBufferWrapper<ObjectInstanceData>(NB_OBJECTS_MAX, BufferUsageFlags.StorageBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.HostVisible);
            _meshLetInstancesBuffer = CreateBufferWrapper<MeshLetInstanceData>(NB_MESHLET_INSTANCES_MAX, BufferUsageFlags.StorageBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);
            _drawCallsCountsBuffer = CreateBufferWrapper<uint>(NB_MESHLET_INSTANCES_MAX, BufferUsageFlags.IndirectBuffer | BufferUsageFlags.StorageBuffer | BufferUsageFlags.TransferDst | BufferUsageFlags.TransferSrc, MemoryPropertyFlags.DeviceLocal);
            _lastMaxDrawBufferIndex = CreateBufferWrapper<int>((int)MaxComputeWorkgroupSize[0], BufferUsageFlags.StorageBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);

            //Initialize the extension...
            foreach (IRendererExtension extension in _extensions)
                extension.Init(this);


            _initialized = true;

            return this;
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
            if(_headless)
                //No swapchain to track the pipelines... and anyway, there will be not rebuilding pipeline because of no resizing of the screen... no screen at all!
                return new PipelineWrapper(this, null, shader.ShaderWrapper);
            else
                return _swapchain.CreatePipelineWrapper(shader.ShaderWrapper);
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
        /// Enable headless
        /// </summary>
        public VkRenderer EnableHeadless()
        {
            _headless = true;
            return this;
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
        /// Set the current frame profiler
        /// </summary>
        public void SetFrameProfiler(Profiler profiler)
        {
            _frameProfiler = profiler;
            _updateSceneDataProfilerStep = profiler.AddStep($"{nameof(VkRenderer)}.{nameof(UpdateSceneData)}");
            _beforeEachFrameProfilerStep = profiler.AddStep($"{nameof(VkRenderer)}.BeforeEachFrame");
            _createCommandBuffersProfilerStep = profiler.AddStep($"{nameof(VkRenderer)}.CreateCommandBuffers");
            _imGuiRenderProfilerStep = profiler.AddStep($"{nameof(VkRenderer)}.ImGuiRender");
            _meshletCountProfilerInfo = profiler.AddInfo($"Meshlets Count");
            

        }

        /// <summary>
        /// Add a mesh on the screen
        /// </summary>
        public IRenderHandle AddMesh(Mesh mesh, List<Material> materials, WorldTransform transform)
        {
            VkMeshInstance meshInstance = new VkMeshInstance((VkMesh)mesh, materials, transform, this);
            _meshInstances.Add(meshInstance);
            return meshInstance;
        }

        /// <summary>
        /// Remove a mesh from the screen
        /// </summary>
        public void RemoveMesh(IRenderHandle handle)
        {
            VkMeshInstance meshInstance = handle as VkMeshInstance;

            if (meshInstance != null)
            {
                meshInstance.Dispose();
                _meshInstances.Remove(meshInstance);
            }

        }

        /// <summary>
        /// Render a scene
        /// </summary>
        public void Render()
        {
            if (!_initialized)
                Init();

            _meshletCountProfilerInfo?.Update(_meshLetInstancesBuffer.Count.ToString());

            //If no camera... nothing to render...
            _updateSceneDataProfilerStep?.Begin();
            if (Camera != null)
            {
                UpdateSceneData();
            }
            _updateSceneDataProfilerStep?.End();


            _beforeEachFrameProfilerStep?.Begin();

            //Action before each frame...
            for (int i = 0; i < _actionsBeforeEachFrame.Count; i++)
                _actionsBeforeEachFrame[i]?.Invoke();

            //Transfert the images that was loaded in the transfer queue...
            int cpt = 0;
            while (_actionsBeforeNextFrameAsync.TryDequeue(out var action))
            {
                ThreadPool.QueueUserWorkItem(a =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Debug.Error(ex);
                    }
                });

                cpt++;
                if (cpt == 10)
                    break;
            }


            //Transfert the images that was loaded in the transfer queue...
            while (_actionsBeforeNextFrame.TryDequeue(out var action))
                action();

            _beforeEachFrameProfilerStep?.End();

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
        Mesh IRenderer.CreateMesh()
        {
            return _resourceFactory.CreateMesh();
        }

        /// <summary>
        /// Create a new mesh
        /// </summary>
        public Mesh CreateMesh()
        {
            return _resourceFactory.CreateMesh();
        }
        
        /// <summary>
        /// Create a Texture2D
        /// </summary>
        Texture2D IRenderer.CreateTexture2D(Texture2DDefinition texDef)
        {
            return _resourceFactory.CreateTexture2D(texDef);
        }

        /// <summary>
        /// Create a Texture2D
        /// </summary>
        public VkTexture2D CreateTexture2D(Texture2DDefinition texDef)
        {
            return _resourceFactory.CreateTexture2D(texDef);
        }

        /// <summary>
        /// Create a Material
        /// </summary>
        Material IRenderer.CreateMaterial(MaterialDefinition matDef)
        {
            return _resourceFactory.CreateMaterial(matDef);
        }

        /// <summary>
        /// Create a Material
        /// </summary>
        public VkMaterial CreateMaterial(MaterialDefinition matDef)
        {
            return _resourceFactory.CreateMaterial(matDef);
        }

        /// <summary>
        /// Create a shader
        /// </summary>
        Shader IRenderer.CreateShader()
        {
            return _resourceFactory.CreateShader();
        }

        /// <summary>
        /// Create a shader
        /// </summary>
        public VkShader CreateShader()
        {
            return _resourceFactory.CreateShader();
        }


        /// <summary>
        /// Get the mesh renderer for a shader
        /// </summary>
        public VkMeshRenderer GetMeshRenderer(VkShader shader)
        {
            lock (_cacheMeshRenderer)
            {
                if (!_cacheMeshRenderer.TryGetValue(shader, out var meshRenderer))
                {
                    meshRenderer = new VkMeshRenderer(shader, this);

                    _cacheMeshRenderer.Add(shader, meshRenderer);

                    _meshRenderers.Add(meshRenderer);

                }

                return meshRenderer;
            }
        }

        /// <summary>
        /// Get a pipeline for mesh rendering
        /// </summary>
        public PipelineWrapper GetPipeline(VkShader shader)
        {
            lock (_cachePipeline)
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

            
        }


        /// <summary>
        /// Create a shader module
        /// </summary>
        public ShaderModule CreateShaderModule(byte[] shaderCode, uint flags = 0, AllocationCallbacks allocator = null)
        {

            int key = BytesHelper.CombineHash(BytesHelper.GetHashCodeBytes(shaderCode), (int)flags);

            lock (_cacheShaderModule)
            {
                //Already in the cache??
                if (_cacheShaderModule.TryGetValue(key, out ShaderModule shaderModule))
                    return shaderModule;

                using (ShaderModuleCreateInfo createInfo = new ShaderModuleCreateInfo
                {
                    CodeBytes = shaderCode,
                    Flags = flags
                })
                {
                    shaderModule = _device.CreateShaderModule(createInfo, allocator);
                }

                _cacheShaderModule[key] = shaderModule;

                return shaderModule;
            }

        }

        /// <summary>
        /// Create a draw call buffer
        /// </summary>
        public BufferWrapper<DrawIndexedIndirectCommand> CreateDrawCallsBuffer(uint nbDrawCallsToReserve, out uint drawCallIndex, out uint drawCallsCountsOffset)
        {
            //TODO: To allocate base on some graphic memory
            var buffer = CreateBufferWrapper<DrawIndexedIndirectCommand>(NB_MESHLET_INSTANCES_MAX, BufferUsageFlags.IndirectBuffer | BufferUsageFlags.TransferDst | BufferUsageFlags.StorageBuffer, MemoryPropertyFlags.DeviceLocal);

            lock (_drawCallsBuffers)
            {
                drawCallIndex = (uint)_drawCallsBuffers.Count;
                _drawCallsBuffers.Add(buffer);
                drawCallsCountsOffset = _drawCallsCountsBuffer.Reserve(_drawCallsCountsBuffer.ElementSize * nbDrawCallsToReserve);
            }

            return buffer;

        }

        /// <summary>
        /// Create a draw call buffer but not the draw calls counts
        /// </summary>
        public BufferWrapper<DrawIndexedIndirectCommand> CreateDrawCallsBuffer(out uint drawCallIndex)
        {
            //TODO: To allocate base on some graphic memory
            var buffer = CreateBufferWrapper<DrawIndexedIndirectCommand>(NB_MESHLET_INSTANCES_MAX, BufferUsageFlags.IndirectBuffer | BufferUsageFlags.TransferDst | BufferUsageFlags.StorageBuffer, MemoryPropertyFlags.DeviceLocal);

            lock (_drawCallsBuffers)
            {
                drawCallIndex = (uint)_drawCallsBuffers.Count;
                _drawCallsBuffers.Add(buffer);
            }

            return buffer;

        }

        /// <summary>
        /// Update the current surface capabilities
        /// </summary>
        public void UpdateSurfaceCapabilities()
        {
            if (_surface == null)
                return;

            _surfaceCapabilities = _physicalDevice.GetSurfaceCapabilitiesKHR(_surface);

            _currentExtent = _surfaceCapabilities.CurrentExtent;
        }

        /// <summary>
        /// Create a swapchain
        /// </summary>
        public SwapchainWrapper CreateSwapchainWrapper(Format[] expectedFormats, ColorSpaceKhr[] expectedColorSpaces, PresentModeKhr presentMode, bool depthTest)
        {
            return new SwapchainWrapper(this, expectedFormats, expectedColorSpaces, presentMode, depthTest);
        }

        public CommandPool CreateGraphicsCommandPool()
        {
            return _device.CreateCommandPool(GraphicsQueueIndex, CommandPoolCreateFlags.ResetCommandBuffer);
        }


        /// <summary>
        /// Create a buffer
        /// </summary>
        public unsafe BufferWrapper<T> CreateBufferWrapper<T>(T[] values, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            return BufferWrapper<T>.Create(this, values, usageFlags, memoryPropertyFlags);
        }

        /// <summary>
        /// Create a buffer
        /// </summary>
        public unsafe BufferWrapper<T> CreateBufferWrapper<T>(int count, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            return BufferWrapper<T>.Create(this, count, usageFlags, memoryPropertyFlags);
        }

        /// <summary>
        /// Create a buffer
        /// </summary>
        public unsafe BufferWrapper CreateBufferWrapper(uint size, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            return new BufferWrapper(this, size, usageFlags, memoryPropertyFlags);
        }

        /// <summary>
        /// Register an action to execute before each frame
        /// </summary>
        public void AddActionsBeforeEachFrame(Action updateAction)
        {
            lock(_actionsBeforeEachFrame)
                _actionsBeforeEachFrame.Add(updateAction);
        }

        /// <summary>
        /// Add an action to execute before next frame
        /// </summary>
        public void AddActionsBeforeNextFrame(Action action)
        {
            _actionsBeforeNextFrame.Enqueue(action);
        }

        /// <summary>
        /// Add an action to execute before next frame
        /// </summary>
        public void AddActionsBeforeNextFrameAsync(Action action)
        {
            _actionsBeforeNextFrameAsync.Enqueue(action);
        }

        /// <summary>
        /// Add an extension
        /// </summary>
        public void AddExtension(IRendererExtension extension)
        {
            if (_initialized)
                throw new InvalidOperationException("Impossible to add an extension after the renderer has been initialized.");

            _extensions.Add(extension);
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

            ////Disposing mesh instances...
            //foreach (VkMeshInstance vkMeshInstances in _meshInstances)
            //    vkMeshInstances.Dispose();

            //Disposing mesh renderer...
            foreach (VkMeshRenderer vkMeshRenderer in _meshRenderers)
                vkMeshRenderer.Dispose();

            //Disposing pipelines...
            foreach (PipelineWrapper pipeline in _cachePipeline.Values)
                pipeline.Dispose();

            //Disposing drawcall buffers...
            foreach (BufferWrapper drawCallsBuffer in _drawCallsBuffers)
                drawCallsBuffer.Dispose();

            _sceneBuffer?.Dispose();
            _objectsBuffer?.Dispose();
            _meshLetInstancesBuffer?.Dispose();
            _verticesBuffer?.Dispose();
            _indicesBuffer?.Dispose();
            _meshLetsBuffer?.Dispose();
            _drawCallsCountsBuffer?.Dispose();
            _lastMaxDrawBufferIndex?.Dispose();
            _swapchain?.Dispose();

            if (_surface != null)
            {
                _vi.DestroySurfaceKHR(_surface);
                _surface = null;
            }


            //foreach (SwapchainWrapper swapchain in Swapchains.ToArray())
            //    swapchain.Dispose();
            //Swapchains.Clear();


            if (_memoryManager != null)
            {
                _memoryManager.Dispose();
                _memoryManager = null;
            }

            //We don't need it anymore...
            foreach (ShaderModule shaderModule in _cacheShaderModule.Values)
                _device.DestroyShaderModule(shaderModule);


            _vi?.Dispose();

            if (Renderer.Current == this)
                Renderer.Current = null;
        }




        #endregion


        #region Private methods

        /// <summary>
        /// Create the device
        /// </summary>
        private unsafe Device CreateDevice()
        {
            PhysicalDeviceVulkan11Features vulkan11Features = new PhysicalDeviceVulkan11Features();
            PhysicalDeviceVulkan12Features vulkan12Features = new PhysicalDeviceVulkan12Features();
            //PhysicalDeviceIndexTypeUint8FeaturesEXT indexTypeUint8Features = new PhysicalDeviceIndexTypeUint8FeaturesEXT();

            var features12 = _physicalDevice.GetFeatures2(vulkan11Features, vulkan12Features);
            //var features12 = _physicalDevice.GetFeatures2Vulkan12(out var vulkan12Features);
            //var featuresIndexTypeInt8 = _physicalDevice.GetFeaturesIndexTypeUint8EXT(out var indexTypeUint8Features);
            //features12.Next = 
            //Get the PhysicalDeviceDescriptorIndexingFeatures feature....
            //var featuresIndexing = _physicalDevice.GetFeatures2Indexing(out var indexingFeatures);

            if (!vulkan11Features.StorageBuffer16BitAccess)
                throw new NotSupportedException("Your graphic card does not support StorageBuffer16BitAccess.");

            //Check if the graphic card is compatible with bindless rendering...
            if (!vulkan12Features.DescriptorBindingPartiallyBound)
                throw new NotSupportedException("Your graphic card does not support DescriptorBindingPartiallyBound.");
            if (!vulkan12Features.RuntimeDescriptorArray)
                throw new NotSupportedException("Your graphic card does not support RuntimeDescriptorArray.");

            //Check if the graphic card is compatible with bindless rendering...
            if (!vulkan12Features.DrawIndirectCount)
                throw new NotSupportedException("Your graphic card does not support DrawIndirectCount.");

            List<DeviceQueueCreateInfo> queueCreateInfos = new List<DeviceQueueCreateInfo>
            {
                //Graphics queue...
                new DeviceQueueCreateInfo
                {
                    QueuePriorities = new float[] { 1.0f },
                    QueueFamilyIndex = _graphicsQueueIndex,
                },

                //Transfer queue... 
                new DeviceQueueCreateInfo
                {
                    QueuePriorities = new float[] { 1.0f },
                    QueueFamilyIndex = _transferQueueIndex,
                },

                //Compute queue... 
                new DeviceQueueCreateInfo
                {
                    QueuePriorities = new float[] { 1.0f },
                    QueueFamilyIndex = _computeQueueIndex,
                }
            };

            List<string> extensions = new List<string>();

            //No swapshain in headless...
            if (!_headless)
                extensions.Add("VK_KHR_swapchain");

            using (var deviceInfo = new DeviceCreateInfo
            {
                EnabledExtensionNames = extensions.ToArray(),
                QueueCreateInfos = queueCreateInfos.ToArray(),
                Next = features12.Handle
            })
            {
                try
                {
                    //I'm tired of the debug text from vulkan when i start the application...
                    _printDebug = false;
                    return _physicalDevice.CreateDevice(deviceInfo, _surface, null);
                }
                finally
                {
                    _printDebug = true;
                }

            }
            


        }

        /// <summary>
        /// Get the right Physical device
        /// </summary>
        private PhysicalDevice PickPhysicalDevice()
        {
            //TODO: Check the physical device suitable for our project
            return _vi.EnumeratePhysicalDevices()[0];
        }

        /// <summary>
        /// Recalculate the scene data
        /// </summary>
        private void UpdateSceneData()
        {

            //Update MVP Matrix...
            Matrix4 viewMat2 = Camera.GetViewMatrix();
            Matrix4 projMat = Camera.GetProjectionMatrixVulkan((int)_currentExtent.Width, (int)_currentExtent.Height);

            SceneData.ViewProjectionMatrix = projMat * viewMat2;
            SceneData.CameraLocation = Camera.Transform.Location;
            SceneData.NbMeshletInstances = _meshLetInstancesBuffer.Count;


            var frustumX = Vector4.NormalizePlane(SceneData.ViewProjectionMatrix.Row(3) + SceneData.ViewProjectionMatrix.Row(0)); // x + w < 0
            var frustumY = Vector4.NormalizePlane(SceneData.ViewProjectionMatrix.Row(3) + SceneData.ViewProjectionMatrix.Row(1)); // y + w < 0

            //Matrix4 projMatTransposed = Matrix4.Transpose(ref SceneData.ViewProjectionMatrix);
            //var frustumX3 = Vector4.NormalizePlane(projMatTransposed.Column(3) + projMatTransposed.Column(0)); // x + w < 0
            //var frustumY3 = Vector4.NormalizePlane(projMatTransposed.Column(3) + projMatTransposed.Column(1)); // y + w < 0

            //cullData.P00 = projMat.M11;
            //cullData.P11 = projMat.M22;
            SceneData.NearZ = Camera.NearZ;
            SceneData.FarZ = Camera.FarZ;
            SceneData.FrustumLeft = frustumX.X;
            SceneData.FrustumRight = frustumX.Z;
            SceneData.FrustumTop = frustumY.Y;
            SceneData.FrustumBottom = frustumY.Z;

            //Update the buffer so the GPU can access it...
            _sceneBuffer.Update(ref SceneData);

            //if (_meshInstances.Count > 0)
            //{
            //    var center4 = SceneData.ViewProjectionMatrix * new Vector4(_meshInstances[0].Transform.Location, 1);
            //    var center = new Vector3(center4) / center4.W;
            //    Debug.Info(Camera.Transform.Location + " => " + center.ToString());
            //}



        }

        /// <summary>
        /// Render the next frame
        /// </summary>
        private void RenderFrame()
        {
            _createCommandBuffersProfilerStep?.Begin();

            RenderCommandBuffer commandBuffer = _swapchain.GetNextRenderCommandBuffer();

            commandBuffer.Begin();


            //If no camera, nothing to render... in 3D
            if (Camera != null)
            {
                //commandBuffer.CmdBindVertexBuffer(0, _verticesBuffer, 0);
                //commandBuffer.CmdBindIndexBuffer(_indicesBuffer, 0, IndexType.Uint16);

                foreach (var meshRenderer in _meshRenderers)
                    meshRenderer.PopulateCommandBuffers(commandBuffer);
            }

            _createCommandBuffersProfilerStep?.End();

            _imGuiRenderProfilerStep?.Begin();

            //Rendering of ImGui...
            _imGui?.Render(commandBuffer);

            _imGuiRenderProfilerStep?.End();


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
                //Headless...
                throw new Exception("Impossible to create the surface. No window and no window handle exists. If you want to execute headless, enable Headless mode with EnableHeadless.");

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
            if(_printDebug || flags != DebugReportFlagsExt.Information)
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
        /// Get the best queue index for compute data
        /// </summary>
        private int GetQueueFamilyPriorityForCompute(QueueFamilyProperties queueFamProp)
        {
            if (queueFamProp.QueueFlags.HasFlag(QueueFlags.Compute))
            {
                //Important...
                if (!queueFamProp.QueueFlags.HasFlag(QueueFlags.Graphics) && !queueFamProp.QueueFlags.HasFlag(QueueFlags.Transfer))
                    //Perfect, a queue specific to compute...
                    return 0;

                if (!queueFamProp.QueueFlags.HasFlag(QueueFlags.Graphics))
                    //Perfect, a queue almost specific to compute...
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
                //Support for headless...
                if (surface != null)
                {
                    if (!_physicalDevice.GetSurfaceSupportKHR(graphicsQueueIndex, surface))
                        //This queue does not support SurfaceKHR...
                        continue;
                }

                if (queueFamilyProperties[graphicsQueueIndex].QueueFlags.HasFlag(QueueFlags.Graphics))
                    //Found it! Should be good
                    return graphicsQueueIndex;
            }

            throw new Exception("Not device found for graphics queue.");
        }

        /// <summary>
        /// Get the transfer queue family index
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

            if (!found)
                throw new Exception("Not device found for transfer queue.");

            return transfersQueueIndex;
        }


        /// <summary>
        /// Get the comptue queue family index
        /// </summary>
        public uint GetComputeQueueFamilyIndex()
        {
            bool found = false;
            uint computeQueueIndex = 0;
            int bestPriority = Int32.MaxValue;
            var queueFamProps = _physicalDevice.GetQueueFamilyProperties();
            for (int i = 0; i < queueFamProps.Length; i++)
            {
                if (queueFamProps[i].QueueFlags.HasFlag(QueueFlags.Compute))
                {
                    int priority = GetQueueFamilyPriorityForCompute(queueFamProps[i]);
                    if (priority < bestPriority)
                    {
                        //This one is better...
                        computeQueueIndex = (uint)i;
                        bestPriority = priority;
                        found = true;
                    }
                }
            }

            if (!found)
                throw new Exception("Not device found for compute queue.");

            return computeQueueIndex;
        }


        /// <summary>
        /// Default callback in debug mode
        /// </summary>
        private void DefaultDebugCallback(DebugLevel level, int messageCode, string message)
        {
            if (level == DebugLevel.Error)
            {
                throw new Exception($"Renderer error: {message}");
            }
            else
            {
                System.Diagnostics.Debug.Print($"{level}: {message}");
            }
        }

        /// <summary>
        /// Init device properties
        /// </summary>
        private void InitDeviceProperties()
        {

            var deviceProp = _physicalDevice.GetProperties();


            MaxComputeWorkgroupSize = deviceProp.Limits.MaxComputeWorkGroupSize;

        }


        #endregion

    }
}
