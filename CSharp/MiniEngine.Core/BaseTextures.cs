using MiniEngine.ResourceDefinitions;
using MiniEngine.Ressources;
using System;
using System.Collections.Generic;
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
        /// Get a basic white texture
        /// </summary>
        public static Texture2D White
        {
            get
            {
                _white ??= CreateTexture2D(Resources.PixelWhite);

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
                _magenta ??= CreateTexture2D(Resources.PixelMagenta);

                return _magenta;
            }
        }

        /// <summary>
        /// Create a Texture2D from pixelData
        /// </summary>
        private static Texture2D CreateTexture2D(byte[] pixelData)
        {
            if (Renderer.Current == null)
                throw new InvalidOperationException("No current renderer.");

            return Renderer.Current.CreateTexture2D(new Texture2DDefinition()
            {
                Data = pixelData
            });
        }
    }
}
