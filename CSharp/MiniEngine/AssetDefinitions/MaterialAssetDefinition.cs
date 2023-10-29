using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.AssetDefinitions
{
    /// <summary>
    /// Definition of a material asset
    /// </summary>
    public class MaterialAssetDefinition
    {
        /// <summary>
        /// Name for the texture
        /// </summary>
        public string DiffuseTexture { get; set; }

        /// <summary>
        /// Name of the shader
        /// </summary>
        public string Shader { get; set; }
    }
}
