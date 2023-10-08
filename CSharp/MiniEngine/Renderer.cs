using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Renderer
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// The maximum number of point lights
        /// </summary>
        public const int MAX_POINT_LIGHTS = 2;

        /// <summary>
        /// Context
        /// </summary>
        private Context _context;

        /// <summary>
        /// Rendering context
        /// </summary>
        private RenderingContext _renderingContext = new RenderingContext();

        /// <summary>
        /// Meshes to render
        /// </summary>
        private List<Mesh> _meshes = new List<Mesh>();

        /// <summary>
        /// Point lights
        /// </summary>
        private List<PointLight> _pointLights = new List<PointLight>();

        
        /// <summary>
        /// Directional light
        /// </summary>
        public DirectionalLight DirectionalLight = null;

        /// <summary>
        /// Default ambient light
        /// </summary>
        public AmbientLight AmbientLight = AmbientLight.Default;

        /// <summary>
        /// Current camera
        /// </summary>
        public Camera Camera = new Camera();

        /// <summary>
        /// Constructor
        /// </summary>
        public Renderer(Context context)
        {
            _context = context;

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
        /// Add a mesh to render
        /// </summary>
        public Mesh AddMesh(Mesh mesh)
        {
            _meshes.Add(mesh);
            return mesh;
        }

        /// <summary>
        /// Add a point light to render
        /// </summary>
        public void AddPointLight(PointLight pointLight)
        {
            _pointLights.Add(pointLight);
        }

        /// <summary>
        /// Remove a point light to render
        /// </summary>
        public void RemovePointLight(PointLight pointLight)
        {
            _pointLights.Remove(pointLight);
        }


        /// <summary>
        /// Render the scene
        /// </summary>
        public void Render()
        {
            //No camera = nothing to render...
            if (this.Camera == null)
                return;

            //Setup the ambient light...
            if (AmbientLight != null)
            {
                _renderingContext.AmbientColor = AmbientLight.Color;
                _renderingContext.AmbientIntensity = AmbientLight.Intensity;
            }
            else
            {
                _renderingContext.AmbientIntensity = 0f;
            }


            //Get the camera matrix for the render call...
            Matrix4 cameraMatrix = this.Camera.GetMatrix();

            for (int iMesh = 0; iMesh < _meshes.Count; iMesh++)
            {
                Mesh mesh = _meshes[iMesh];

                Matrix4 worldMatrix = mesh.GetMatrix();

                //Setup camera in reference to the mesh...
                _renderingContext.WVPMatrix = cameraMatrix * worldMatrix;
                _renderingContext.CameraLocalPosition = mesh.GetLocalPosition(ref this.Camera.Location);


                if (DirectionalLight != null)
                {
                    _renderingContext.DiffuseColor = DirectionalLight.Color;
                    _renderingContext.DiffuseIntensity = DirectionalLight.Intensity;
                    _renderingContext.DiffuseDirection = DirectionalLight.Backward;
                    _renderingContext.CalculateDiffuseDirection(ref worldMatrix);
                }
                else
                {
                    //No directionnal light...
                    _renderingContext.DiffuseIntensity = 0f;
                }


                //Calculate the point lights in reference to the mesh
                _renderingContext.PointLights = _pointLights;
                for (int i = 0; i < _pointLights.Count; i++)
                    _renderingContext.PointLightsCalulcatedLocalPositions[i] = mesh.GetLocalPosition(ref _pointLights[i].Location);
                

                //And we render the mesh...
                mesh.Render(_renderingContext);

            }


        }


    }
}
