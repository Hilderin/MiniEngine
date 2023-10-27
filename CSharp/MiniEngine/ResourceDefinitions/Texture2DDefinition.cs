using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.ResourceDefinitions
{
    /// <summary>
    /// Information necessary to create a Texture2D
    /// </summary>
    public class Texture2DDefinition
    {
        /// <summary>
        /// Texture type
        /// </summary>
        public TextureType Type { get; set; } = TextureType.RGBA;

        /// <summary>
        /// Data
        /// </summary>
        public byte[] Data { get; set; }
    }
}
