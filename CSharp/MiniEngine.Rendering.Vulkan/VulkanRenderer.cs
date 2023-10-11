using MiniEngine.GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Renderer
    /// </summary>
    public class VulkanRenderer : IRenderer
    {

        /// <summary>
        /// Vulkan instance
        /// </summary>
        private VkInstance _vk = null;

        /// <summary>
        /// A reference to the window
        /// </summary>
        private Window _window = null;

        public Camera Camera;

        public Matrix4 WVPMatrix;

        public Color3 AmbientColor = Color3.White;
        public float AmbientIntensity = 1f;

        public Color3 DiffuseColor = Color3.White;
        public float DiffuseIntensity = 0f;
        //public Vector3 DiffuseDirection = Vector3.Down;
        public Vector3 CalculatedDiffuseDirection = Vector3.Down;

        /// <summary>
        /// Camera local position in the local space for the current world transform of the object that is rendering
        /// </summary>
        public Vector3 CameraLocalPosition = Vector3.Zero;

        /// <summary>
        /// Point lights
        /// </summary>
        public List<PointLight> PointLights = new List<PointLight>(Context.MAX_POINT_LIGHTS);

        /// <summary>
        /// Spot lights
        /// </summary>
        public List<SpotLight> SpotLights = new List<SpotLight>(Context.MAX_POINT_LIGHTS);

        /// <summary>
        /// Position of the point light in reference to the current mesh
        /// </summary>
        public Vector3[] PointLightsCalulcatedLocalPositions = new Vector3[Context.MAX_POINT_LIGHTS];

        /// <summary>
        /// Position of the spot light in reference to the current mesh
        /// </summary>
        public Vector3[] SpotLightsCalulcatedLocalPositions = new Vector3[Context.MAX_POINT_LIGHTS];

        /// <summary>
        /// Direction of the spot light in reference to the current mesh
        /// </summary>
        public Vector3[] SpotLightsCalulcatedLocalDirections = new Vector3[Context.MAX_POINT_LIGHTS];

        /// <summary>
        /// Application name
        /// </summary>
        public string ApplicationName;

        /// <summary>
        /// Indicate if we when to add addionnals validations (for development purpose)
        /// </summary>
        public bool AddValidationLayers;

        /// <summary>
        /// Indicate we the buffer sould be swapped each frame
        /// </summary>
        public bool ShouldSwapBuffer { get { return false; } }

        ///// <summary>
        ///// List of mesh renderers
        ///// </summary>
        //private List<OpenGLMeshRenderer> _meshRenderers = new List<OpenGLMeshRenderer>();

        ///// <summary>
        ///// List of texture binders
        ///// </summary>
        //private List<OpenGLTextureBinder> _textureBinders = new List<OpenGLTextureBinder>();

        /// <summary>
        /// Constructor
        /// </summary>
        public VulkanRenderer(string applicationName, bool addValidationLayers)
        {
            this.ApplicationName = applicationName;
            this.AddValidationLayers = addValidationLayers;
        }

        /// <summary>
        /// Clear the buffer for another render
        /// </summary>
        public void Clear()
        {
            
        }


        /// <summary>
        /// Returns the framebuffer un RGBA
        /// </summary>
        public byte[] GetFramebufferRGBA(int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update the window options specific to the engine
        /// </summary>
        public void PreInitGlfw()
        {
            // No API
            Glfw.WindowHint(Hint.ClientApi, ClientApi.None);
            Glfw.WindowHint(Hint.Resizable, false);


        }

        /// <summary>
        /// Initialize the renderer
        /// </summary>
        public void Init()
        {
            _vk = new VkInstance();

            _vk.CreateInstance();

            _vk.SetupDebugMessenger();

            _vk.CreateSurface(_window);

            _vk.PickPhysicalDevice();

            _vk.CreateLogicalDevice();

        }

        /// <summary>
        /// Pass the window to the render when it's created
        /// </summary>
        public void SetWindow(Window window)
        {
            _window = window;
        }


        /// <summary>
        /// Render the scene
        /// </summary>
        public void Render(Context context)
        {
            

        }


        /// <summary>
        /// Dispose of the renderer
        /// </summary>
        public void Dispose()
        {
            if (_vk != null)
            {
                _vk.Dispose();
                _vk = null;
            }
        }


    }
}
