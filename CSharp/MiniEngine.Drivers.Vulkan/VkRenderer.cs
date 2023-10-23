using MiniEngine.GLFW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MiniEngine.Drivers.Vulkan.VkInstance;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Vulkan renderer
    /// </summary>
    public class VkRenderer : IRenderer, IDisposable
    {
        #region Internal members

        internal VkInstance vk;
        internal SurfaceKhr Surface;
        internal PhysicalDevice PhysicalDevice;
        internal Device Device;
        internal Queue Queue;
        internal SwapchainKhr Swapchain;
        internal Image[] SwapChainImages;
        internal ImageView[] SwapChainImagesView;
        internal RenderPass RenderPass;
        internal Framebuffer[] Framebuffers;
        internal Fence Fence;
        internal Semaphore Semaphore;
        internal Extent2D CurrentExtent;
        internal Vector2 ClientSize;
        internal CommandPool CommandPool;
        internal CommandBuffer[] CommandBuffers;

        internal Matrix4 MVPMatrix;

        #endregion

        #region Public members

        /// <summary>
        /// Indicate if we sould swap the buffer
        /// </summary>
        public bool ShouldSwapBuffer { get; set; } = true;

        #endregion

        #region Private members

        private List<VkMeshRenderer> _meshRenderers = new List<VkMeshRenderer>();
        private Window _window;
        private string _applicationName;
        private Func<VkInstance, SurfaceKhr> _surfaceCreationCallback;
        private DebugReportCallback _debugCallback;
        private bool _initialized = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public VkRenderer(string applicationName, Func<VkInstance, SurfaceKhr> surfaceCreationCallback = null, DebugReportCallback debugCallback = null)
        {
            _applicationName = applicationName;
            _surfaceCreationCallback = surfaceCreationCallback;
            _debugCallback = debugCallback;

        }

        #endregion

        #region Public methods
        /// <summary>
        /// Destruction
        /// </summary>
        public void Dispose()
        {
            if (vk == null)
                return;

            //Disposing mesh renderer...
            foreach (VkMeshRenderer vkMeshRenderer in _meshRenderers)
                vkMeshRenderer.Dispose();

            if (CommandPool != null)
            {
                Device.DestroyCommandPool(CommandPool);
                CommandPool = null;
            }

            if (Fence != null)
            {
                Device.DestroyFence(Fence);
                Fence = null;
            }

            if (Semaphore != null)
            {
                Device.DestroySemaphore(Semaphore);
                Semaphore = null;
            }

            if (Framebuffers != null)
            {
                foreach (Framebuffer framebuffer in Framebuffers)
                    Device.DestroyFramebuffer(framebuffer);
                Framebuffers = null;
            }

            if (RenderPass != null)
            {
                Device.DestroyRenderPass(RenderPass);
                RenderPass = null;
            }

            if (SwapChainImagesView != null)
            {
                foreach (ImageView imageView in SwapChainImagesView)
                    Device.DestroyImageView(imageView);
                SwapChainImagesView = null;
            }

            if (Swapchain != null)
            {
                Device.DestroySwapchainKHR(Swapchain);
                Swapchain = null;
            }

            if (Device != null)
            {
                Device.Destroy();
                Device = null;
            }

            if (Surface != null)
            {
                vk.DestroySurfaceKHR(Surface);
                Surface = null;
            }

            vk.Dispose();
            vk = null;
        }


        /// <summary>
        /// Update the window options specific to the engine
        /// </summary>
        public void PreInitGlfw()
        {
            // No API
            Glfw.WindowHint(Hint.ClientApi, ClientApi.None);

        }

        /// <summary>
        /// Init the renderer
        /// </summary>
        public void Init()
        {
            if (_initialized)
                throw new Exception("Already initialized.");

            vk = VkBootstrapper.CreateInstance(_applicationName, _debugCallback);

            if (_surfaceCreationCallback != null)
                Surface = _surfaceCreationCallback(vk);
            else if (_window != null)
                Surface = CreateSurfaceFromWindow(_window);
            else
                throw new Exception("Impossible to create the surface. No window and no surfaceCreationCallback exist.");


            PhysicalDevice = VkBootstrapper.PickPhysicalDevice(vk);

            var surfaceCapabilities = PhysicalDevice.GetSurfaceCapabilitiesKHR(Surface);
            CurrentExtent = surfaceCapabilities.CurrentExtent;
            ClientSize = new Vector2(CurrentExtent.Width, CurrentExtent.Height);

            Device = VkBootstrapper.CreateDevice(PhysicalDevice, Surface);

            Queue = Device.GetQueue(0, 0);

            var surfaceFormat = PhysicalDevice.GetSurfaceFormat(Surface, new Format[] { Format.B8G8R8A8Srgb }, new ColorSpaceKhr[] { ColorSpaceKhr.SrgbNonlinear });
            Swapchain = Device.CreateSwapchain(Surface, surfaceCapabilities, surfaceFormat, PresentModeKhr.Mailbox);
            SwapChainImages = Device.GetSwapchainImagesKHR(Swapchain);
            SwapChainImagesView = Device.CreateImageViews(SwapChainImages, surfaceFormat);

            RenderPass = Device.CreateRenderPass(surfaceFormat);
            Framebuffers = Device.CreateFramebuffers(RenderPass, SwapChainImagesView, CurrentExtent);

            Fence = Device.CreateFence();
            Semaphore = Device.CreateSemaphore();

            var createPoolInfo = new CommandPoolCreateInfo { Flags = CommandPoolCreateFlags.ResetCommandBuffer };
            CommandPool = Device.CreateCommandPool(createPoolInfo);

            var commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                Level = CommandBufferLevel.Primary,
                CommandPool = CommandPool,
                CommandBufferCount = (uint)SwapChainImages.Length
            };

            CommandBuffers = Device.AllocateCommandBuffers(commandBufferAllocateInfo);

            _initialized = true;
        }

        /// <summary>
        /// Pass the window to the render when it's created
        /// </summary>
        public void SetWindow(Window window)
        {
            _window = window;
        }

        /// <summary>
        /// Render a scene
        /// </summary>
        public void Render(Scene scene)
        {
            if (!_initialized)
                Init();

            //The camera needs to be the same size has the client...
            scene.Camera.ClientSize = ClientSize;

            //Update MVP Matrix...
            Matrix4 viewMat2 = scene.Camera.GetMatrix();
            //Matrix4 viewMat = Matrix4.CreateLookAt(scene.Camera.Location, scene.Camera.Forward, scene.Camera.Up);
            //Matrix4 viewMat = Matrix4.CreateLookAt(scene.Camera.Location, new Vector3(0, 0, -1f), new Vector3(0, 1f, 0));
            Matrix4 projMat = scene.Camera.GetProjectionMatrixVulkan();


            //Matrix4 model = Matrix4.Identity * Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), Math.DegToRad(90.0f));
            //Matrix4 view = Matrix4.CreateLookAt(new Vector3(2, 2, 2), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
            //Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(Math.DegToRad(45.0f), (float)CurrentExtent.Width / CurrentExtent.Height, 0.1f, 10.0f);

            //Inverse because coords are inverted on Y in vulkan
            this.MVPMatrix = projMat * viewMat2;
            //this.MVPMatrix.M22 *= -1;
            Debug.Print("MVPMatrix: " + this.MVPMatrix.ToString());

            List <Mesh> meshes = scene.Meshes;
            for (int iMesh = 0; iMesh < meshes.Count; iMesh++)
            {
                Mesh mesh = meshes[iMesh];

                if (meshes[iMesh].RendererStateObj == null)
                {
                    //Initialization of the mesh renderer...
                    VkMeshRenderer meshRenderer = new VkMeshRenderer(mesh, this);
                    mesh.RendererStateObj = meshRenderer;
                    _meshRenderers.Add(meshRenderer);

                    //Initialisation of the materials...
                    //PrepareMaterials(meshRenderer.Materials);
                }

            }




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
        /// Create a buffer on the GPU
        /// </summary>
        public VkBuffer CreateBufferOnGPU<T>(T[] values, BufferUsageFlags usageFlags)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (VkBuffer stagingBuffer = Device.CreateBuffer(values, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                //Create a buffer on the GPU..
                VkBuffer gpuBuffer = Device.CreateBuffer(stagingBuffer.Size, BufferUsageFlags.TransferDst | usageFlags, MemoryPropertyFlags.DeviceLocal);

                //Copy the data to the GPU...
                CopyBuffer(stagingBuffer, gpuBuffer);

                return gpuBuffer;
            }


        }

        /// <summary>
        /// Copy a buffer
        /// </summary>
        public void CopyBuffer(VkBuffer bufferSource, VkBuffer bufferDest)
        {

            var commandBuffer = Device.AllocateCommandBuffer(CommandPool);

            commandBuffer.Begin(CommandBufferUsageFlags.OneTimeSubmit);

            BufferCopy copyRegion = new()
            {
                Size = bufferSource.Size,
            };

            commandBuffer.CmdCopyBuffer(bufferSource.Buffer, bufferDest.Buffer, copyRegion);

            commandBuffer.End();

            Queue.Submit(commandBuffer);
            Queue.WaitIdle();


            Device.FreeCommandBuffer(CommandPool, commandBuffer);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Render the next frame
        /// </summary>
        private void RenderFrame(Scene scene)
        {
            uint nextImageIndex = Device.AcquireNextImageKHR(Swapchain, ulong.MaxValue, Semaphore);
            Device.ResetFence(Fence);



            CommandBuffer commandBuffer = CommandBuffers[nextImageIndex];

            commandBuffer.Begin();
            var renderPassBeginInfo = new RenderPassBeginInfo
            {
                Framebuffer = Framebuffers[nextImageIndex],
                RenderPass = RenderPass,
                //ClearValues = new ClearValue[] { new ClearValue { Color = new ClearColorValue(new float[] { DateTime.Now.Millisecond % 100f / 100f, 0.87f, 0.75f, 1.0f }) } },
                ClearValues = new ClearValue[] { new ClearValue { Color = new ClearColorValue(new float[] { 0f, 0.87f, 0.75f, 1.0f }) } },
                RenderArea = new Rect2D { Extent = CurrentExtent }
            };
            commandBuffer.CmdBeginRenderPass(renderPassBeginInfo, SubpassContents.Inline);

            //If no camera.. then.. nothing on screen...
            if (scene.Camera != null)
            {
                foreach (var meshRenderer in _meshRenderers)
                    meshRenderer.PopulateCommandBuffers(commandBuffer);
            }

            commandBuffer.CmdEndRenderPass();
            commandBuffer.End();

            var submitInfo = new SubmitInfo
            {
                WaitSemaphores = new Semaphore[] { Semaphore },
                WaitDstStageMask = new PipelineStageFlags[] { PipelineStageFlags.AllGraphics },
                CommandBuffers = new CommandBuffer[] { commandBuffer }
            };
            Queue.Submit(submitInfo, Fence);
            Device.WaitForFence(Fence, true, 100000000);
            var presentInfo = new PresentInfoKhr
            {
                Swapchains = new SwapchainKhr[] { Swapchain },
                ImageIndices = new uint[] { nextImageIndex }
            };
            Queue.PresentKHR(presentInfo);
        }


        /// <summary>
        /// Create a surface
        /// </summary>
        private SurfaceKhr CreateSurfaceFromWindow(Window window)
        {
            unsafe
            {
                SurfaceKhr surface = new SurfaceKhr();

                fixed (ulong* ptr = &surface.m)
                {
                    _window.CreateSurface(vk.m, (IntPtr)ptr);
                }
                return surface;
            }
        }

        #endregion

    }
}
