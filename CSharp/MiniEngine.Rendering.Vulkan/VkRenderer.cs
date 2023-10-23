using MiniEngine.GLFW;
using MiniEngine.Drivers.Vulkan;
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
        //internal PhysicalDevice PhysicalDevice;
        internal VkDevice Device;
        internal VkQueue Queue;
        internal VkFence Fence;
        internal VkRenderPass RenderPass;
        internal VkSwapchain SwapChain;

        //internal SwapchainKhr Swapchain;
        //internal Image[] SwapChainImages;
        //internal ImageView[] SwapChainImagesView;
        //internal Framebuffer[] Framebuffers;
        //internal Fence Fence;
        //internal Semaphore Semaphore;
        //internal Extent2D CurrentExtent;
        //internal Vector2 ClientSize;
        internal VkCommandPool CommandPool;
        internal VkCommandBuffer[] CommandBuffers;

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
        private VkVersion _applicationVersion;
        private Func<VkInstance, VkSurfaceKhr> _surfaceCreationCallback;
        private DebugReportCallback _debugCallback;
        private bool _initialized = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public VkRenderer(string applicationName, VkVersion applicationVersion, Func<VkInstance, VkSurfaceKhr> surfaceCreationCallback = null, DebugReportCallback debugCallback = null)
        {
            _applicationName = applicationName;
            _applicationVersion = applicationVersion;
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

            vk = new VkInstance(_applicationName, _applicationVersion, (i) =>
            {
                //Function to create a Surface...
                if (_surfaceCreationCallback != null)
                    return _surfaceCreationCallback(i);
                else if (_window != null)
                    return i.CreateSurfaceFromWindow(_window);
                else
                    throw new Exception("Impossible to create the surface. No window and no surfaceCreationCallback exist.");

            }
            , _debugCallback);


            VkPhysicalDevice physicalDevice = vk.PickPhysicalDevice();

            Device = physicalDevice.CreateDevice(vk.Surface);

            Queue = Device.GetQueue(0, 0);
            Fence = Device.CreateFence();

            SwapChain = Device.CreateSwapchain(new Format[] { Format.B8G8R8A8Srgb }, new ColorSpaceKhr[] { ColorSpaceKhr.SrgbNonlinear }, PresentModeKhr.Mailbox);

            RenderPass = Device.CreateRenderPass(SwapChain.SurfaceFormat);

            SwapChain.InitFramebuffers(RenderPass);

            

            var createPoolInfo = new CommandPoolCreateInfo { Flags = CommandPoolCreateFlags.ResetCommandBuffer };
            CommandPool = Device.CreateCommandPool(createPoolInfo);

            var commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                Level = CommandBufferLevel.Primary,
                CommandPool = CommandPool,
                CommandBufferCount = (uint)SwapChain.SwapChainImages.Length
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
            scene.Camera.ClientSize = Device.ClientSize;

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
        public VkBufferWrapper CreateBufferOnGPU<T>(T[] values, BufferUsageFlags usageFlags)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (VkBufferWrapper stagingBuffer = Device.CreateBuffer(values, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                //Create a buffer on the GPU..
                VkBufferWrapper gpuBuffer = Device.CreateBuffer(stagingBuffer.Size, BufferUsageFlags.TransferDst | usageFlags, MemoryPropertyFlags.DeviceLocal);

                //Copy the data to the GPU...
                CopyBuffer(stagingBuffer, gpuBuffer);

                return gpuBuffer;
            }


        }

        /// <summary>
        /// Copy a buffer
        /// </summary>
        public void CopyBuffer(VkBufferWrapper bufferSource, VkBufferWrapper bufferDest)
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
            uint nextImageIndex = SwapChain.AcquireNextImage();


            VkCommandBuffer commandBuffer = CommandBuffers[nextImageIndex];

            commandBuffer.Begin();
            var renderPassBeginInfo = new RenderPassBeginInfo
            {
                Framebuffer = SwapChain.Framebuffers[nextImageIndex],
                RenderPass = RenderPass,
                //ClearValues = new ClearValue[] { new ClearValue { Color = new ClearColorValue(new float[] { DateTime.Now.Millisecond % 100f / 100f, 0.87f, 0.75f, 1.0f }) } },
                ClearValues = new ClearValue[] { new ClearValue { Color = new ClearColorValue(new float[] { 0f, 0.87f, 0.75f, 1.0f }) } },
                RenderArea = new Rect2D { Extent = Device.CurrentExtent }
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



            Device.ResetFence(Fence);
            var submitInfo = new SubmitInfo
            {
                WaitSemaphores = new VkSemaphore[] { SwapChain.Semaphore },
                WaitDstStageMask = new PipelineStageFlags[] { PipelineStageFlags.AllGraphics },
                CommandBuffers = new VkCommandBuffer[] { commandBuffer }
            };
            Queue.Submit(submitInfo, Fence);
            Queue.WaitIdle();
            Device.WaitForFence(Fence, true, 100000000);

            SwapChain.Present(nextImageIndex);
        }

        
        #endregion

    }
}
