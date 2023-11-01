using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.AssetDefinitions
{
    /// <summary>
    /// Definition of a mesh asset
    /// </summary>
    public class MeshAssetDefinition
    {
        /// <summary>
        /// Path of the mesh file
        /// </summary>
        public string MeshPath { get; set; }

        /// <summary>
        /// Material names
        /// </summary>
        public List<string> MaterialNames = new List<string>();


        /// <summary>
        /// Invert face (default = false)
        /// </summary>
        public bool InverseFaces = false;

        /// <summary>
        /// Scale
        /// </summary>
        public float Scale = 1f;

        /// <summary>
        /// Flip Y
        /// </summary>
        public bool FlipY = false;

        /// <summary>
        /// Smooth normals
        /// </summary>
        public bool SmoothNormals = true;

        /// <summary>
        /// Reset de material ambient color on materials
        /// </summary>
        public bool ResetMaterialAmbientColor = false;

    }
}
