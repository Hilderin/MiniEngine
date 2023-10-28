using MiniEngine.Drivers.Vulkan;
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
        internal Swapchain Swapchain;

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

        #endregion

        #region Private members

        private List<VkMeshRenderer> _meshRenderers = new List<VkMeshRenderer>();
        private IWindow _window;
        private string _applicationName;
        private VkVersion _applicationVersion;
        private Func<VkInstance, SurfaceKhr> _surfaceCreationCallback;
        private DebugReportCallback _debugCallback;
        private bool _initialized = false;
        private VkResourceFactory _resourceFactory;

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
        /// Init the renderer
        /// </summary>
        public void Init()
        {
            if (_initialized)
                throw new Exception("Already initialized.");

            vk = new VkInstance(_applicationName, _applicationVersion, CreateSurface, _debugCallback);

            Device = vk.Device;

            Swapchain = Device.CreateSwapchain(new Format[] { Format.B8G8R8A8Srgb }, new ColorSpaceKhr[] { ColorSpaceKhr.SrgbNonlinear }, PresentModeKhr.Mailbox);


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
        /// Pass the window to the render when it's created
        /// </summary>
        public void SetWindow(IWindow window)
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
        /// Destruction
        /// </summary>
        public void Dispose()
        {
            if (vk == null)
                return;

            if (_resourceFactory != null)
                _resourceFactory.Dispose();

            //Disposing mesh renderer...
            foreach (VkMeshRenderer vkMeshRenderer in _meshRenderers)
                vkMeshRenderer.Dispose();

            vk.Dispose();
            vk = null;
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

            List<MeshActor> meshes = scene.Meshes;
            for (int iMesh = 0; iMesh < meshes.Count; iMesh++)
            {
                MeshActor mesh = meshes[iMesh];

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
            RenderCommandBuffer commandBuffer = Swapchain.GetNextRenderCommandBuffer();

            commandBuffer.Begin();
            

            //If no camera.. then.. nothing on screen...
            if (scene.Camera != null)
            {
                foreach (var meshRenderer in _meshRenderers)
                    meshRenderer.PopulateCommandBuffers(commandBuffer);
            }

            
            commandBuffer.End();


            //Execute the command buffer and show the results on surface...
            Swapchain.Present(commandBuffer);
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
                return CreateSurfaceFromWindow(vi, _window);
            else
                throw new Exception("Impossible to create the surface. No window and no surfaceCreationCallback exist.");

        }



        #endregion

    }
}
