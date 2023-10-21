using MiniEngine.GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.OpenGL
{
    /// <summary>
    /// Renderer
    /// </summary>
    public class OpenGLRenderer : IRenderer
    {
        private Window _window;

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
        public List<PointLight> PointLights = new List<PointLight>();

        /// <summary>
        /// Spot lights
        /// </summary>
        public List<SpotLight> SpotLights = new List<SpotLight>();

        /// <summary>
        /// Position of the point light in reference to the current mesh
        /// </summary>
        public Vector3[] PointLightsCalulcatedLocalPositions = new Vector3[0];

        /// <summary>
        /// Position of the spot light in reference to the current mesh
        /// </summary>
        public Vector3[] SpotLightsCalulcatedLocalPositions = new Vector3[0];

        /// <summary>
        /// Direction of the spot light in reference to the current mesh
        /// </summary>
        public Vector3[] SpotLightsCalulcatedLocalDirections = new Vector3[0];


        /// <summary>
        /// List of mesh renderers
        /// </summary>
        private List<OpenGLMeshRenderer> _meshRenderers = new List<OpenGLMeshRenderer>();

        /// <summary>
        /// List of texture binders
        /// </summary>
        private List<OpenGLTextureBinder> _textureBinders = new List<OpenGLTextureBinder>();

        /// <summary>
        /// Indicate we the buffer sould be swapped each frame
        /// </summary>
        public bool ShouldSwapBuffer { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public OpenGLRenderer()
        {

        }

        /// <summary>
        /// Clear the buffer for another render
        /// </summary>
        public void Clear()
        {
            GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
        }


        /// <summary>
        /// Returns the framebuffer un RGBA
        /// </summary>
        public byte[] GetFramebufferRGBA(int x, int y, int width, int height)
        {
            byte[] buffer = new byte[width * height * 4];

            GL.glReadPixels(x, y, width, height, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, buffer);
            GL.CheckError();

            return buffer;
        }

        /// <summary>
        /// Update the window options specific to the engine
        /// </summary>
        public void PreInitGlfw()
        {
            // Set some common hints for the OpenGL profile creation
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);
            Glfw.WindowHint(Hint.OpenglForwardCompatible, true);
            Glfw.WindowHint(Hint.DepthBits, true);                  //Depth test
        }

        /// <summary>
        /// Initialize the renderer
        /// </summary>
        public void Init()
        {
            GL.glEnable(GL.GL_CULL_FACE);
            GL.glFrontFace(GL.GL_CW);
            GL.glCullFace(GL.GL_BACK);
            GL.glEnable(GL.GL_DEPTH_TEST);


            //Set the clear color to black... just because, black is nice!
            GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
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
        public void Render(Scene scene)
        {

            Clear();


            //No camera = nothing to render...
            if (scene.Camera == null)
                return;

            this.Camera = scene.Camera;

            //Setup the ambient light...
            if (scene.AmbientLight != null)
            {
                this.AmbientColor = scene.AmbientLight.Color;
                this.AmbientIntensity = scene.AmbientLight.Intensity;
            }
            else
            {
                this.AmbientIntensity = 0f;
            }


            //Get the camera matrix for the render call...
            Matrix4 cameraMatrix = scene.Camera.GetMatrix();

            List<Mesh> meshes = scene.Meshes;
            for (int iMesh = 0; iMesh < meshes.Count; iMesh++)
            {
                Mesh mesh = meshes[iMesh];

                OpenGLMeshRenderer meshRenderer;
                if (mesh.RendererStateObj == null)
                {
                    //Initialization of the mesh renderer...
                    meshRenderer = new OpenGLMeshRenderer(mesh);
                    mesh.RendererStateObj = meshRenderer;
                    _meshRenderers.Add(meshRenderer);

                    //Initialisation of the materials...
                    PrepareMaterials(meshRenderer.Materials);
                }
                else
                {
                    meshRenderer = (OpenGLMeshRenderer)mesh.RendererStateObj;
                }

                Matrix4 worldMatrix = mesh.GetMatrix();

                //Setup camera in reference to the mesh...
                this.WVPMatrix = cameraMatrix * worldMatrix;
                this.CameraLocalPosition = mesh.GetLocalPosition(ref this.Camera.Location);


                if (scene.DirectionalLight != null)
                {
                    this.DiffuseColor = scene.DirectionalLight.Color;
                    this.DiffuseIntensity = scene.DirectionalLight.Intensity;
                    this.CalculatedDiffuseDirection = Vector3.CalculateLocalDirection(ref worldMatrix, scene.DirectionalLight.Backward);
                }
                else
                {
                    //No directionnal light...
                    this.DiffuseIntensity = 0f;
                }


                //Calculate the point lights in reference to the mesh
                this.PointLights = scene.PointLights;
                if (this.PointLightsCalulcatedLocalPositions.Length < scene.PointLights.Count)
                    this.PointLightsCalulcatedLocalPositions = new Vector3[scene.PointLights.Count];
                for (int i = 0; i < this.PointLights.Count; i++)
                    this.PointLightsCalulcatedLocalPositions[i] = mesh.GetLocalPosition(ref this.PointLights[i].Location);

                //Calculate the spot lights in reference to the mesh
                this.SpotLights = scene.SpotLights;
                if (this.SpotLightsCalulcatedLocalPositions.Length < scene.SpotLights.Count)
                    this.SpotLightsCalulcatedLocalPositions = new Vector3[scene.SpotLights.Count];
                if (this.SpotLightsCalulcatedLocalDirections.Length < scene.SpotLights.Count)
                    this.SpotLightsCalulcatedLocalDirections = new Vector3[scene.SpotLights.Count];
                for (int i = 0; i < this.SpotLights.Count; i++)
                {
                    this.SpotLightsCalulcatedLocalPositions[i] = mesh.GetLocalPosition(ref this.SpotLights[i].Location);

                    this.SpotLightsCalulcatedLocalDirections[i] = Vector3.CalculateLocalDirection(ref worldMatrix, this.SpotLights[i].Backward);

                }


                //And we render the mesh...
                meshRenderer.Render(this);

            }


            //Swapping buffer...
            if (ShouldSwapBuffer && _window != null)
                _window.SwapBuffers();
        }


        /// <summary>
        /// Dispose of the renderer
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _meshRenderers.Count; i++)
                _meshRenderers[i].Dispose();
            _meshRenderers.Clear();

            for (int i = 0; i < _textureBinders.Count; i++)
                _textureBinders[i].Dispose();
            _textureBinders.Clear();
        }

        /// <summary>
        /// Prepare materials
        /// </summary>
        private void PrepareMaterials(Material[] materials)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].Diffuse != null)
                    PrepareTexture(materials[i].Diffuse);

                if (materials[i].Specular != null)
                    PrepareTexture(materials[i].Specular);
            }
        }


        /// <summary>
        /// Prepare a texture
        /// </summary>
        public void PrepareTexture(Texture2D texture)
        {
            OpenGLTextureBinder binder;
            if (texture.RendererStateObj == null)
            {
                //We need to create a new binder...
                binder = new OpenGLTextureBinder(texture);
                texture.RendererStateObj = binder;
                _textureBinders.Add(binder);
            }
        }



        ///// <summary>
        ///// Bind a texture
        ///// </summary>
        //public void BindTexture(Texture2D texture, uint textureUnit)
        //{
        //    OpenGLTextureBinder binder;
        //    if (texture.Binder == null)
        //    {
        //        //We need to create a new binder...
        //        binder = new OpenGLTextureBinder(texture);
        //        _textureBinders.Add(binder);
        //    }
        //    else
        //    {
        //        binder = (OpenGLTextureBinder)texture.Binder;
        //    }

        //    binder.Bind(textureUnit);
        //}



    }
}
