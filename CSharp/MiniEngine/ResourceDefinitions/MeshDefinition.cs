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
        public List<SubMeshDefinition> SubMeshes = new List<SubMeshDefinition>();

    }


    /// <summary>
    /// Data for the representation of a Model (mesh)
    /// </summary>
    public class SubMeshDefinition
    {
        public Vector3[] Positions;
        public Vector2[] TexCoords;
        public Vector3[] Normals;
        public Vector3[] Colors;
        public int[] Indices;
        public int MaterialIndex;
    }
}
