using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MiniEngine.AssetDefinitions
{
    /// <summary>
    /// Definition of a mesh asset
    /// </summary>
    public class MeshAssetDefinition
    {
        /// <summary>
        /// Full path of the mesh file
        /// </summary>
        [YamlIgnore]
        public string MeshFullPath { get; set; }

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
        /// Rotation on X axis
        /// </summary>
        public float RotationX;

        /// <summary>
        /// Rotation on Y axis
        /// </summary>
        public float RotationY;

        /// <summary>
        /// Rotation on Z axis
        /// </summary>
        public float RotationZ;

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
