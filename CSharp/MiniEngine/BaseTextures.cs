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
        private static Texture2D _white;

        /// <summary>
        /// Get pixel data for white texture
        /// </summary>
        public static byte[] WhitePixelData => Resources.PixelWhite;

        /// <summary>
        /// Get a basic white textre
        /// </summary>
        public static Texture2D White
        {
            get
            {
                if (_white == null)
                {
                    _white = Context.Current.CreateTexture2D(new Texture2DDefinition()
                    {
                        Data = WhitePixelData
                    });
                }
                return _white;
            }
        }

    }
}
