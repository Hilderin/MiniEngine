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
    public class Texture2D
    {
        /// <summary>
        /// Empty texture
        /// </summary>
        private static Texture2D _emptyTexture = new Texture2D(1, 1, Ressources.Resources.PixelWhite, TextureType.RGB);

        /// <summary>
        /// Empty texture
        /// </summary>
        public static Texture2D Empty { get { return _emptyTexture; } }

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
        /// Data of the texture
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// State object for the renderer
        /// </summary>
        public object RendererStateObj;

        /// <summary>
        /// Constructor
        /// </summary>
        public Texture2D()
        {

        }


        /// <summary>
        /// Constructor
        /// </summary>
        public Texture2D(int width, int height, byte[] data, TextureType type)
        {
            this.Width = width;
            this.Height = height;
            this.Data = data;
            this.Type = type;
        }


    }
}
