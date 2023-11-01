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


        public MeshComponent SetMesh(string assetName)
        {
            return SetMesh(Context.Asset.Get<Mesh>(assetName));
        }

        public MeshComponent SetMesh(Mesh mesh)
        {
            this.Mesh = mesh;

            //Adding missing materials...
            while (Materials.Count < mesh.Materials.Length)
                Materials.Add(mesh.Materials[Materials.Count]);

            //Removing additionnal materials...
            while (Materials.Count > mesh.Materials.Length)
                Materials.RemoveAt(Materials.Count - 1);

            return this;
        }

        public MeshComponent SetMaterial(string assetName, int matIndex)
        {
            return SetMaterial(Context.Asset.Get<Material>(assetName), matIndex);
        }

        public MeshComponent SetMaterial(Material mat, int matIndex)
        {
            if (Mesh == null)
                throw new Exception("Mesh not set. You must set a Mesh beforce the materials.");

            if (matIndex < 0 || matIndex >= Mesh.Materials.Length)
                throw new ArgumentOutOfRangeException($"Invalid {nameof(matIndex)}.");

            while (Materials.Count <= matIndex)
                Materials.Add(null);

            Materials[matIndex] = mat;
            return this;
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
