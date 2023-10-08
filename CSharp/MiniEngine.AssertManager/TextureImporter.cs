using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public Texture2D GetTexture2DFromFile(TextureType textureType, string path)
        {

            using (Surface image = Surface.LoadFromFile(path))
            {
                int bufferSize;
                TextureType type;

                switch (image.BitsPerPixel)
                {
                    case 8:
                        //8 bits per pixels... 1 byte so.. only red
                        type = TextureType.Red;
                        bufferSize = image.Width * image.Height;
                        break;

                    case 24:
                        //24 bits per pixels so... 3 bytes... so RGB
                        type = TextureType.RGB;
                        bufferSize = image.Width * image.Height * 3;
                        break;

                    default:
                        throw new Exception($"Unsupported color type of texture '{path}', ColorType: {image.ColorType}, BitsPerPixel: {image.BitsPerPixel}");
                }

                //Copy the data in managed array...
                byte[] data = new byte[bufferSize];
                Marshal.Copy(image.GetScanLine(0), data, 0, bufferSize);

                return new Texture2D(image.Width, image.Height, data, type);
            }

        }

    }
}
