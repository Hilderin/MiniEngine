using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.ResourceDefinitions
{
    /// <summary>
    /// Information necessary to create a Mesh
    /// </summary>
    public class MeshDefinition
    {
        /// <summary>
        /// Default materials. Represents the slots for the materials for this mesh
        /// </summary>
        public List<Material> Materials { get; private set; } = new List<Material>();

        /// <summary>
        /// Submeshes
        /// </summary>
        public List<SubMeshDefinition> SubMeshes = new List<SubMeshDefinition>();

    }


    /// <summary>
    /// Data for the representation of a Model (mesh)
    /// </summary>
    public class SubMeshDefinition
    {
        public Vertex[] Vertices;
        public uint[] Indices;
        public uint MaterialIndex;
    }

}
