using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

                switch (image.BitsPerPixel)
                {
                    case 8:
                        //8 bits per pixels... 1 byte so.. only red
                        texture.SetDataRed(image.Width, image.Height, image.GetScanLine(0));
                        break;

                    case 24:
                        //24 bits per pixels so... 3 bytes... so RGB
                        texture.SetDataRGB(image.Width, image.Height, image.GetScanLine(0));
                        break;

                    default:
                        throw new Exception($"Unsupported color type of texture '{path}', ColorType: {image.ColorType}, BitsPerPixel: {image.BitsPerPixel}");
                }

                return texture;
            }

        }

    }
}
