using System;
using System.Collections.Generic;

namespace MiniEngine
{
    /// <summary>
    /// Mesh
    /// </summary>
    public unsafe class Mesh: WorldTransform
    {   

        /// <summary>
        /// Materials
        /// </summary>
        private List<Material> _materials = new List<Material>(1);

        /// <summary>
        /// Sub meshes
        /// </summary>
        private List<SubMeshData> _subMeshes = new List<SubMeshData>();

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
        public Mesh()
        {
            
        }


        /// <summary>
        /// Set the material
        /// </summary>
        public void SetMaterial(Material material, int index)
        {
            while (_materials.Count <= index)
                _materials.Add(Material.NotFound);

            _materials[index] = material;
        }

        /// <summary>
        /// Set the mesh data...
        /// </summary>
        public void AddMeshData(Vector3[] positions,
                                Vector2[] texCoords,
                                Vector3[] normals,
                                int[] indices,
                                int materialIndex)
        {
            _subMeshes.Add(new SubMeshData()
            {
                Positions = positions,
                TexCoords = texCoords,
                Normals = normals,
                Indices = indices,
                MaterialIndex = materialIndex
            });

        }

        /// <summary>
        /// Get the internal mesh data for rendering
        /// </summary>
        public MeshData GetMeshData()
        {
            return new MeshData()
            {
                Materials = _materials,
                SubMeshes = _subMeshes
            };
        }

        /// <summary>
        /// Wrapper for the mesh data
        /// </summary>
        public class MeshData
        {
            public List<Material> Materials;
            public List<SubMeshData> SubMeshes;
        }

        /// <summary>
        /// Wrapper for the mesh data
        /// </summary>
        public class SubMeshData
        {
            public Vector3[] Positions;
            public Vector2[] TexCoords;
            public Vector3[] Normals;
            public int[] Indices;
            public int MaterialIndex;
        }



    }
}
