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
                _white ??= AssetManager.Current.Get<Texture2D>("Resources/PixelWhite.bmp");

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
                _magenta ??= AssetManager.Current.Get<Texture2D>("Resources/PixelMagenta.bmp");

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
                _aqua ??= AssetManager.Current.Get<Texture2D>("Resources/PixelAqua.bmp");

                return _aqua;
            }
        }

    }
}
