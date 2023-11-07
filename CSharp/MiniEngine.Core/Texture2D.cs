using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Class for a texture 2D
    /// </summary>
    public abstract class Texture2D: IDisposable
    {
        /// <summary>
        /// Name of the texture
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Texture type
        /// </summary>
        public TextureType Type { get; set; } = TextureType.RGB;

        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }

        


        /// <summary>
        /// Destruction of the Material
        /// </summary>
        protected abstract void Destroy();

        /// <summary>
        /// Reload the texture
        /// </summary>
        protected abstract void Reload();

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Destroy();
        }


    }

}
