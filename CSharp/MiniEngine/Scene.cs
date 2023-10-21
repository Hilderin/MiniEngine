using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Scene that contains objects to render
    /// </summary>
    public class Scene
    {

        /// <summary>
        /// Meshes to render
        /// </summary>
        private List<Mesh> _meshes = new List<Mesh>();


        /// <summary>
        /// List of meshes
        /// </summary>
        public List<Mesh> Meshes { get { return _meshes; } }


        /// <summary>
        /// Current camera
        /// </summary>
        public Camera Camera = new Camera();


        /// <summary>
        /// Directional light
        /// </summary>
        public DirectionalLight DirectionalLight = null;

        /// <summary>
        /// Default ambient light
        /// </summary>
        public AmbientLight AmbientLight = AmbientLight.Default;

        /// <summary>
        /// Point lights
        /// </summary>
        public List<PointLight> PointLights = new List<PointLight>();

        /// <summary>
        /// Spot lights
        /// </summary>
        public List<SpotLight> SpotLights = new List<SpotLight>();



        /// <summary>
        /// Add a mesh to render
        /// </summary>
        public void Add(Mesh mesh)
        {
            _meshes.Add(mesh);
        }

        /// <summary>
        /// Remove a mesh
        /// </summary>
        public void Remove(Mesh mesh)
        {
            _meshes.Remove(mesh);
        }

        /// <summary>
        /// Add a point light to render
        /// </summary>
        public void Add(PointLight pointLight)
        {
            this.PointLights.Add(pointLight);
        }

        /// <summary>
        /// Remove a point light to render
        /// </summary>
        public void Remove(PointLight pointLight)
        {
            this.PointLights.Remove(pointLight);
        }

        /// <summary>
        /// Add a spot light to render
        /// </summary>
        public void Add(SpotLight SpotLight)
        {
            this.SpotLights.Add(SpotLight);
        }

        /// <summary>
        /// Remove a spot light to render
        /// </summary>
        public void Remove(SpotLight SpotLight)
        {
            this.SpotLights.Remove(SpotLight);
        }



    }
}
