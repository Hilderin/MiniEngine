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
        internal Device Device;
        internal Queue Queue;
        internal Fence Fence;
        internal RenderPass RenderPass;
        internal Swapchain SwapChain;
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
        private VkVersion _applicationVersion;
        private Func<VkInstance, SurfaceKhr> _surfaceCreationCallback;
        private DebugReportCallback _debugCallback;
        private bool _initialized = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public VkRenderer(string applicationName, VkVersion applicationVersion, Func<VkInstance, SurfaceKhr> surfaceCreationCallback = null, DebugReportCallback debugCallback = null)
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

            vk = new VkInstance(_applicationName, _applicationVersion, CreateSurface, _debugCallback);

            Device = vk.Device;

            Queue = Device.GetQueue(0, 0);
            Fence = Device.CreateFence();

            SwapChain = Device.CreateSwapchain(new Format[] { Format.B8G8R8A8Srgb }, new ColorSpaceKhr[] { ColorSpaceKhr.SrgbNonlinear }, PresentModeKhr.Mailbox);

            RenderPass = SwapChain.CreateRenderPass();

            CommandPool = Device.CreateCommandPool(CommandPoolCreateFlags.ResetCommandBuffer);

            CommandBuffers = CommandPool.AllocateCommandBuffers(CommandBufferLevel.Primary, SwapChain.SwapChainImages.Length);

            _initialized = true;
        }

        /// <summary>
        /// Create the surface
        /// </summary>
        /// <param name="vi"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private SurfaceKhr CreateSurface(VkInstance vi)
        {
            //Function to create a Surface...
            if (_surfaceCreationCallback != null)
                return _surfaceCreationCallback(vi);
            else if (_window != null)
                return vi.CreateSurfaceFromWindow(_window);
            else
                throw new Exception("Impossible to create the surface. No window and no surfaceCreationCallback exist.");

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
        /// Create a buffer on the GPU
        /// </summary>
        public BufferWrapper CreateBufferOnGPU<T>(T[] values, BufferUsageFlags usageFlags)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (BufferWrapper stagingBuffer = Device.CreateBuffer(values, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                //Create a buffer on the GPU..
                BufferWrapper gpuBuffer = Device.CreateBuffer(stagingBuffer.Size, BufferUsageFlags.TransferDst | usageFlags, MemoryPropertyFlags.DeviceLocal);

                //Copy the data to the GPU...
                CopyBuffer(stagingBuffer, gpuBuffer);

                return gpuBuffer;
            }


        }

        /// <summary>
        /// Copy a buffer
        /// </summary>
        public void CopyBuffer(BufferWrapper bufferSource, BufferWrapper bufferDest)
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
        /// Recalculate information for the next frame
        /// </summary>
        private void RecalculateNextFrame(Scene scene)
        {

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
            //Debug.Print("MVPMatrix: " + this.MVPMatrix.ToString());

            List<Mesh> meshes = scene.Meshes;
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
        }

        /// <summary>
        /// Render the next frame
        /// </summary>
        private void RenderFrame(Scene scene)
        {
            uint nextImageIndex = SwapChain.AcquireNextImage();


            CommandBuffer commandBuffer = CommandBuffers[nextImageIndex];

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


            //Execute the command buffer and show the result on surface...
            SwapChain.Present(commandBuffer, nextImageIndex);
        }

        
        #endregion

    }
}
