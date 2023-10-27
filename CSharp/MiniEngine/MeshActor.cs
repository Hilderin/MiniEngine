using System;
using System.Collections.Generic;

namespace MiniEngine
{
    /// <summary>
    /// MeshActor
    /// </summary>
    public unsafe class MeshActor: WorldTransform
    {   

        /// <summary>
        /// Materials
        /// </summary>
        private List<Material> _materials = new List<Material>(1);

        /// <summary>
        /// The mesh to render
        /// </summary>
        public Mesh Mesh;

        /// <summary>
        /// State for the renderer
        /// </summary>
        public object RendererStateObj = null;


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
        public MeshActor()
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MeshActor(Mesh mesh)
        {
            Mesh = mesh;
        }



    }




}
