using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Types of textures
    /// </summary>
    public enum TextureType
    {
        /// <summary>
        /// RGB texture (3 colors)
        /// </summary>
        RGB,

        /// <summary>
        /// RGB texture (4 colors)
        /// </summary>
        RGBA,

        /// <summary>
        /// Only red channel
        /// </summary>
        Red
    }
}
