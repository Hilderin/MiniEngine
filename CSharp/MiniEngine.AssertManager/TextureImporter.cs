using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeximpNet;

namespace MiniEngine.AssertManager
{
    /// <summary>
    /// Texture importer
    /// </summary>
    public class TextureImporter
    {

        /// <summary>
        /// Load a texture from disk
        /// </summary>
        public Texture2D GetTexture2DFromFile(string path)
        {

            using (Surface image = Surface.LoadFromFile(path))
            {
                Texture2D texture = new Texture2D();

                texture.SetDataRGB(image.Width, image.Height, image.GetScanLine(0));

                return texture;
            }

        }

    }
}
