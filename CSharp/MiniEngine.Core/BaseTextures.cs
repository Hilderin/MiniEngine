using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Basic textures
    /// </summary>
    public static class BaseTextures
    {
        /// <summary>
        /// White
        /// </summary>
        private static Texture2D _white;

        /// <summary>
        /// Magenta
        /// </summary>
        private static Texture2D _magenta;

        /// <summary>
        /// Aqua
        /// </summary>
        private static Texture2D _aqua;

        /// <summary>
        /// Default texture
        /// </summary>
        public static Texture2D Default => White;

        /// <summary>
        /// Get a basic white texture
        /// </summary>
        public static Texture2D White
        {
            get
            {
                _white ??= CreateTexture2D(ResourceUtils.GetBytes("PixelWhite.bmp"), "White");

                return _white;
            }
        }
               

        /// <summary>
        /// Get a basic magenta texture
        /// </summary>
        public static Texture2D Magenta
        {
            get
            {
                _magenta ??= CreateTexture2D(ResourceUtils.GetBytes("PixelMagenta.bmp"), "Magenta");

                return _magenta;
            }
        }

        /// <summary>
        /// Get a basic aqua texture
        /// </summary>
        public static Texture2D Aqua
        {
            get
            {
                _aqua ??= CreateTexture2D(ResourceUtils.GetBytes("PixelAqua.bmp"), "Aqua");

                return _aqua;
            }
        }

        /// <summary>
        /// Create a Texture2D from pixelData
        /// </summary>
        private static Texture2D CreateTexture2D(byte[] pixelData, string name)
        {
            if (Renderer.Current == null)
                throw new InvalidOperationException("No current renderer.");

            Texture2D texture = Renderer.Current.CreateTexture2D(new Texture2DDefinition()
            {
                Data = pixelData
            });
            texture.Name = name;
            return texture;
        }
    }
}
