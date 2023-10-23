using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MiniEngine.Assets;
using Silk.NET.Assimp;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Instance for Vulkan engine
    /// </summary>
    public unsafe class VulkanInstance : IDisposable
    {
        #region Constants

        const string MODEL_PATH = @"Assets\viking_room.obj";
        const string TEXTURE_PATH = @"Assets\viking_room.png";

        const int MAX_FRAMES_IN_FLIGHT = 2;

       

        

        #endregion

        #region Public members

        public readonly string[] validationLayers = new[]
        {
            "VK_LAYER_KHRONOS_validation"
        };

        public readonly string[] deviceExtensions = new[]
        {
            KhrSwapchain.ExtensionName
        };

        public Vk Api;

        public Instance Instance;

        public bool EnableValidationLayers = true;

        public string ApplicationName;

        public KhrSurface khrSurface = null;
        public SurfaceKHR surface;

        public Window Window;

        public PhysicalDevice physicalDevice;

        public SampleCountFlags msaaSamples = SampleCountFlags.Count1Bit;

        public Device device;

        public Queue graphicsQueue;
        public Queue presentQueue;

        public KhrSwapchain khrSwapChain = null;

        public CommandPool commandPool;

        
        //public Vertex[] vertices = null;

        //public uint[] indices = null;


        public DescriptorSetLayout descriptorSetLayout;

        #endregion

        #region Private members

        private VulkanInitializer _initializer = null;

        private VulkanSwapChain _swapChain = null;


        public VulkanTextureBinder _texture = null;
        public VulkanMeshRenderer _meshRenderer = null;

        private Dictionary<uint, VulkanSampler> _samplers = new Dictionary<uint, VulkanSampler>();
        


        private Semaphore[] imageAvailableSemaphores = null;
        private Semaphore[] renderFinishedSemaphores = null;
        private Fence[] inFlightFences = null;
        private Fence[] imagesInFlight = null;
        private int currentFrame = 0;

        private bool frameBufferResized = false;


        

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="applicationName"></param>
        public VulkanInstance(string applicationName, bool enableValidationLayers)
        {
            ApplicationName = applicationName;
            EnableValidationLayers = enableValidationLayers;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Set the current window
        /// </summary>
        /// <param name="window"></param>
        public void SetWindow(Window window)
        {
            this.Window = window;
            window.OnWindowResized += FramebufferResizeCallback;
        }

        #endregion

        /// <summary>
        /// Init the vulkan engine
        /// </summary>
        public void Init()
        {
            _initializer = new VulkanInitializer(this);

            _initializer.Init();

            AssetManager assetManager = new AssetManager();
            var texture = assetManager.GetTexture2DFromFile(TEXTURE_PATH);
            _texture = new VulkanTextureBinder(texture, this);
            _texture.Init();

            var mesh = assetManager.GetMeshFromFile(MODEL_PATH, new MeshImportationParameters() { InverseFaces = true });
            _meshRenderer = new VulkanMeshRenderer(mesh, this);
            _meshRenderer.Init();

            
            CreateDescriptorSetLayout();

            _swapChain = new VulkanSwapChain(_meshRenderer, this);

            _swapChain.Init();


            
            CreateSyncObjects();
        }



        private void FramebufferResizeCallback(Vector2 newSize)
        {
            frameBufferResized = true;
        }

        /// <summary>
        /// Get the sampler for a mipLevels
        /// </summary>
        public VulkanSampler GetSampler(uint mipLevels)
        {
            if (!_samplers.TryGetValue(mipLevels, out VulkanSampler sampler))
            {
                sampler = new VulkanSampler(this, mipLevels);
                sampler.Init();
                _samplers.Add(mipLevels, sampler);
            }

            return sampler;
        }

        /// <summary>
        /// Get an extension
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public T GetInstanceExtension<T>() where T : NativeExtension<Vk>
        {
            T extension;
            if (!Api.TryGetInstanceExtension<T>(Instance, out extension))
            {
                throw new NotSupportedException(typeof(T).Name + " extension not found.");
            }

            return extension;
        }


        /// <summary>
        /// Returns the list of physical devices
        /// </summary>
        public PhysicalDevice[] GetPhysicalDevices()
        {
            uint devicedCount = 0;
            Api.EnumeratePhysicalDevices(Instance, ref devicedCount, null);

            if (devicedCount == 0)
            {
                throw new Exception("failed to find GPUs with Vulkan support!");
            }

            var devices = new PhysicalDevice[devicedCount];
            fixed (PhysicalDevice* devicesPtr = devices)
            {
                Api.EnumeratePhysicalDevices(Instance, ref devicedCount, devicesPtr);
            }

            return devices;

        }



        public void Dispose()
        {
            //Wait everything that is in progress to be done...
            Api.DeviceWaitIdle(device);

            _swapChain?.Dispose();

            _texture?.Dispose();
            _meshRenderer?.Dispose();

            foreach (VulkanSampler sampler in _samplers.Values)
                sampler.Dispose();
            _samplers.Clear();

            Api.DestroyDescriptorSetLayout(device, descriptorSetLayout, null);

            
            for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                Api.DestroySemaphore(device, renderFinishedSemaphores[i], null);
                Api.DestroySemaphore(device, imageAvailableSemaphores[i], null);
                Api.DestroyFence(device, inFlightFences[i], null);
            }

            


            

            _initializer.Dispose();

        }

        private void RecreateSwapChain()
        {
            Vector2 framebufferSize = Window.FramebufferSize;
            //Vector2D<int> framebufferSize = window.FramebufferSize;

            if (framebufferSize.X == 0 || framebufferSize.Y == 0)
                return;
            //while (framebufferSize.X == 0 || framebufferSize.Y == 0)
            //{
            //    framebufferSize = window.FramebufferSize;
            //    window.DoEvents();
            //}

            Api.DeviceWaitIdle(device);

            _swapChain.Dispose();


            _swapChain = new VulkanSwapChain(_meshRenderer, this);
            _swapChain.Init();

            //CreateImageViews();
            //CreateRenderPass();
            //CreateGraphicsPipeline();
            //CreateColorResources();
            //CreateDepthResources();
            //CreateFramebuffers();
            //CreateUniformBuffers();
            //CreateDescriptorPool();
            //CreateDescriptorSets();
            //CreateCommandBuffers();

            imagesInFlight = new Fence[_swapChain.NbSwapChainImages];
        }




        private void CreateSyncObjects()
        {
            imageAvailableSemaphores = new Semaphore[MAX_FRAMES_IN_FLIGHT];
            renderFinishedSemaphores = new Semaphore[MAX_FRAMES_IN_FLIGHT];
            inFlightFences = new Fence[MAX_FRAMES_IN_FLIGHT];
            imagesInFlight = new Fence[_swapChain.NbSwapChainImages];

            SemaphoreCreateInfo semaphoreInfo = new()
            {
                SType = StructureType.SemaphoreCreateInfo,
            };

            FenceCreateInfo fenceInfo = new()
            {
                SType = StructureType.FenceCreateInfo,
                Flags = FenceCreateFlags.SignaledBit,
            };

            for (var i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                if (Api.CreateSemaphore(device, semaphoreInfo, null, out imageAvailableSemaphores[i]) != Result.Success ||
                    Api.CreateSemaphore(device, semaphoreInfo, null, out renderFinishedSemaphores[i]) != Result.Success ||
                    Api.CreateFence(device, fenceInfo, null, out inFlightFences[i]) != Result.Success)
                {
                    throw new Exception("failed to create synchronization objects for a frame!");
                }
            }
        }



        private void CreateDescriptorSetLayout()
        {
            DescriptorSetLayoutBinding uboLayoutBinding = new()
            {
                Binding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.VertexBit,
            };

            DescriptorSetLayoutBinding samplerLayoutBinding = new()
            {
                Binding = 1,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.FragmentBit,
            };

            var bindings = new DescriptorSetLayoutBinding[] { uboLayoutBinding, samplerLayoutBinding };

            fixed (DescriptorSetLayoutBinding* bindingsPtr = bindings)
            fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &descriptorSetLayout)
            {
                DescriptorSetLayoutCreateInfo layoutInfo = new()
                {
                    SType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = (uint)bindings.Length,
                    PBindings = bindingsPtr,
                };

                if (Api.CreateDescriptorSetLayout(device, layoutInfo, null, descriptorSetLayoutPtr) != Result.Success)
                {
                    throw new Exception("failed to create descriptor set layout!");
                }
            }
        }










        public void DrawFrame(double time)
        {
            Api.WaitForFences(device, 1, inFlightFences[currentFrame], true, ulong.MaxValue);

            uint imageIndex = 0;
            var result = khrSwapChain.AcquireNextImage(device, _swapChain.swapChain, ulong.MaxValue, imageAvailableSemaphores[currentFrame], default, ref imageIndex);

            if (result == Result.ErrorOutOfDateKhr)
            {
                RecreateSwapChain();
                return;
            }
            else if (result != Result.Success && result != Result.SuboptimalKhr)
            {
                throw new Exception("failed to acquire swap chain image!");
            }

            _swapChain.UpdateUniformBuffer(imageIndex);

            if (imagesInFlight[imageIndex].Handle != default)
            {
                Api.WaitForFences(device, 1, imagesInFlight[imageIndex], true, ulong.MaxValue);
            }
            imagesInFlight[imageIndex] = inFlightFences[currentFrame];

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
            };

            var waitSemaphores = stackalloc[] { imageAvailableSemaphores[currentFrame] };
            var waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };

            var buffer = _swapChain.commandBuffers[imageIndex];

            submitInfo = submitInfo with
            {
                WaitSemaphoreCount = 1,
                PWaitSemaphores = waitSemaphores,
                PWaitDstStageMask = waitStages,

                CommandBufferCount = 1,
                PCommandBuffers = &buffer
            };

            var signalSemaphores = stackalloc[] { renderFinishedSemaphores[currentFrame] };
            submitInfo = submitInfo with
            {
                SignalSemaphoreCount = 1,
                PSignalSemaphores = signalSemaphores,
            };

            Api.ResetFences(device, 1, inFlightFences[currentFrame]);

            if (Api.QueueSubmit(graphicsQueue, 1, submitInfo, inFlightFences[currentFrame]) != Result.Success)
            {
                throw new Exception("failed to submit draw command buffer!");
            }

            var swapChains = stackalloc[] { _swapChain.swapChain };
            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = signalSemaphores,

                SwapchainCount = 1,
                PSwapchains = swapChains,

                PImageIndices = &imageIndex
            };

            result = khrSwapChain.QueuePresent(presentQueue, presentInfo);

            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || frameBufferResized)
            {
                frameBufferResized = false;
                RecreateSwapChain();
            }
            else if (result != Result.Success)
            {
                throw new Exception("failed to present swap chain image!");
            }

            currentFrame = (currentFrame + 1) % MAX_FRAMES_IN_FLIGHT;

        }



        public SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice)
        {
            var details = new SwapChainSupportDetails();

            khrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out details.Capabilities);

            uint formatCount = 0;
            khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, null);

            if (formatCount != 0)
            {
                details.Formats = new SurfaceFormatKHR[formatCount];
                fixed (SurfaceFormatKHR* formatsPtr = details.Formats)
                {
                    khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, formatsPtr);
                }
            }
            else
            {
                details.Formats = Array.Empty<SurfaceFormatKHR>();
            }

            uint presentModeCount = 0;
            khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null);

            if (presentModeCount != 0)
            {
                details.PresentModes = new PresentModeKHR[presentModeCount];
                fixed (PresentModeKHR* formatsPtr = details.PresentModes)
                {
                    khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, formatsPtr);
                }

            }
            else
            {
                details.PresentModes = Array.Empty<PresentModeKHR>();
            }

            return details;
        }


        ///// <summary>
        ///// Implicit conversion from VulkanInstance to Instance
        ///// </summary>
        //public static implicit operator Instance(VulkanInstance vi)
        //{
        //    return vi.Instance;
        //}

    }
}