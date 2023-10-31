using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Component to render a mesh
    /// </summary>
    public class MeshComponent : GameComponent
    {
        /// <summary>
        /// Materials
        /// </summary>
        private List<Material> _materials = new List<Material>();

        /// <summary>
        /// Mesh
        /// </summary>
        private Mesh _mesh;

        /// <summary>
        /// The mesh to render
        /// </summary>
        public Mesh Mesh
        { 
            get { return _mesh; } 
            set
            {
                if(RendererHandle != null)
                    Renderer.Current.RemoveMesh(RendererHandle);

                _mesh = value;

                if (value != null)
                    RendererHandle = Renderer.Current.AddMesh(Mesh, _materials, this.Parent.Transform);
            } 
        }

        /// <summary>
        /// State for the renderer
        /// </summary>
        public IRenderHandle RendererHandle = null;


        /// <summary>
        /// Materials
        /// </summary>
        public List<Material> Materials
        {
            get { return _materials; }
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public MeshComponent()
        {

        }


        /// <summary>
        /// Destruction of the mesh
        /// </summary>
        protected override void OnDestroy()
        {
            if (RendererHandle != null)
                Renderer.Current.RemoveMesh(RendererHandle);
        }

    }
}
