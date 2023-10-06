using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Class for a texture 2D
    /// </summary>
    public class Texture2D
    {
        private uint _textureid;

        /// <summary>
        /// Constructor
        /// </summary>
        public Texture2D(string path)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using (Bitmap image = new Bitmap(path))
            {
                LoadImage(image);
            }
#pragma warning restore CA1416 // Validate platform compatibility
        }

        /// <summary>
        /// Bind the texture to a textureunit
        /// </summary>
        public void Bind(uint textureUnit)
        {
            GL.glActiveTexture(textureUnit);
            GL.CheckError();

            GL.glBindTexture(GL.GL_TEXTURE_2D, _textureid);
            GL.CheckError();
        }


        /// <summary>
        /// Load an image in the texture
        /// </summary>
        private void LoadImage(Bitmap image)
        {
            _textureid = GL.glGenTextures();


            GL.glBindTexture(GL.GL_TEXTURE_2D, _textureid);
            GL.CheckError();


            //  Tell OpenGL where the texture data is.
#pragma warning disable CA1416 // Validate platform compatibility
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGB, image.Width, image.Height, 0, GL.GL_RGB, GL.GL_UNSIGNED_BYTE, bitmapData.Scan0);
#pragma warning restore CA1416 // Validate platform compatibility
            GL.CheckError();

            //  Specify linear filtering.
            GL.glTexParameterf(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
            GL.CheckError();

            GL.glTexParameterf(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);
            GL.CheckError();

            GL.glTexParameterf(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_BASE_LEVEL, 0);
            GL.CheckError();

            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
            GL.CheckError();

            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);
            GL.CheckError();

            //Unbinding...
            GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
            GL.CheckError();
        }





    }
}
