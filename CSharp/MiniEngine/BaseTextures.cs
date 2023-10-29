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
        /// Get pixel data for white texture
        /// </summary>
        public static byte[] WhitePixelData => Resources.PixelWhite;

        /// <summary>
        /// Get pixel data for magenta texture
        /// </summary>
        public static byte[] MagentaPixelData => Resources.PixelMagenta;

        /// <summary>
        /// Get a basic white texture
        /// </summary>
        public static Texture2D White
        {
            get
            {
                if (_white == null)
                {
                    _white = Context.Current.Renderer.CreateTexture2D(new Texture2DDefinition()
                    {
                        Data = WhitePixelData
                    });
                }
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
                if (_magenta == null)
                {
                    _magenta = Context.Current.Renderer.CreateTexture2D(new Texture2DDefinition()
                    {
                        Data = MagentaPixelData
                    });
                }
                return _magenta;
            }
        }

    }
}
