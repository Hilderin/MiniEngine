using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Assets
{
    /// <summary>
    /// Mesh importation parameters
    /// </summary>
    public class MeshImportationParameters
    {
        /// <summary>
        /// Default parameters
        /// </summary>
        private static MeshImportationParameters _default = new MeshImportationParameters();

        /// <summary>
        /// Default parameters
        /// </summary>
        public static MeshImportationParameters Default { get { return _default; } }

        /// <summary>
        /// Invert face (default = false)
        /// </summary>
        public bool InverseFaces;

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
