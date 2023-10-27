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
        ///// <summary>
        ///// Empty texture
        ///// </summary>
        //private static Texture2D _emptyTexture = new Texture2D(1, 1, Ressources.Resources.PixelWhite, TextureType.RGB, Ressources.Resources.PixelWhite);

        ///// <summary>
        ///// Empty texture
        ///// </summary>
        //public static Texture2D Empty { get { return _emptyTexture; } }

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
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Destroy();
        }

        ///// <summary>
        ///// Data of the texture
        ///// </summary>
        //public byte[] Data { get; set; }

        ///// <summary>
        ///// Source for the data of the texture (original file)
        ///// </summary>
        //public byte[] SourceData { get; set; }

        ///// <summary>
        ///// State object for the renderer
        ///// </summary>
        //public object RendererStateObj;

        ///// <summary>
        ///// Returns the number of bytes per pixels
        ///// </summary>
        //public int BitsPerPixel
        //{
        //    get
        //    {
        //        switch (Type)
        //        {
        //            case TextureType.RGB: return 3;
        //            case TextureType.RGBA: return 4;
        //            case TextureType.Red: return 1;
        //            default: throw new NotSupportedException($"BitsPerPixel - TextureType not supported: {Type}");
        //        }
        //    }
        //}

        ///// <summary>
        ///// Constructor
        ///// </summary>
        //public Texture2D()
        //{

        //}


        ///// <summary>
        ///// Constructor
        ///// </summary>
        //public Texture2D(int width, int height, byte[] data, TextureType type, byte[] sourceData)
        //{
        //    this.Width = width;
        //    this.Height = height;
        //    this.Data = data;
        //    this.Type = type;
        //    this.SourceData = sourceData;
        //}


    }

}
