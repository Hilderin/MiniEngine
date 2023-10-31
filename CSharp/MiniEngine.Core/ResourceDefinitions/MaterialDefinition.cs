using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.ResourceDefinitions
{
    /// <summary>
    /// Information necessary to create a material
    /// </summary>
    public class MaterialDefinition
    {
        /// <summary>
        /// Diffuse texture
        /// </summary>
        public Texture2D DiffuseTexture;

        /// <summary>
        /// Shader
        /// </summary>
        public Shader Shader;

    }
}
