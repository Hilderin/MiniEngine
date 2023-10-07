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
    public class Texture2D : IDisposable
    {
        /// <summary>
        /// Empty texture
        /// </summary>
        private static Texture2D _emptyTexture = new Texture2D();

        /// <summary>
        /// Empty texture
        /// </summary>
        public static Texture2D Empty { get { return _emptyTexture; } }

        /// <summary>
        /// Texture id OpenGL
        /// </summary>
        private uint _textureid = uint.MaxValue;

        /// <summary>
        /// Constructor
        /// </summary>
        public Texture2D()
        {
        }

        /// <summary>
        /// Bind the texture to a textureunit
        /// </summary>
        public void Bind(uint textureUnit)
        {
            if (_textureid == uint.MaxValue)
            {
                //We dont have any texture, we will create an empty one...
                SetDataRGB(1, 1, Ressources.Resources.PixelWhite);
            }

            GL.glActiveTexture(textureUnit);
            GL.CheckError();

            GL.glBindTexture(GL.GL_TEXTURE_2D, _textureid);
            GL.CheckError();
        }

        /// <summary>
        /// Dispose the texture
        /// </summary>
        public void Dispose()
        {
            if (_textureid != uint.MaxValue)
            {
                GL.glDeleteTextures(_textureid);
                GL.CheckError();

                _textureid = uint.MaxValue;
            }
        }


        /// <summary>
        /// Load an image in the texture
        /// </summary>
        public void SetDataRGB(int width, int height, IntPtr data)
        {
            PrepareOpenGL();

            //  Tell OpenGL where the texture data is.
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGB, width, height, 0, GL.GL_BGR, GL.GL_UNSIGNED_BYTE, data);
            GL.CheckError();

            FinalizeOpenGL();


        }

        /// <summary>
        /// Load an RGB image in the texture
        /// </summary>
        public void SetDataRGB(int width, int height, byte[] data)
        {
            PrepareOpenGL();

            //  Tell OpenGL where the texture data is.
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGB, width, height, 0, GL.GL_BGR, GL.GL_UNSIGNED_BYTE, data);
            GL.CheckError();

            FinalizeOpenGL();


        }

        /// <summary>
        /// Load a red mask in the texture
        /// </summary>
        public void SetDataRed(int width, int height, IntPtr data)
        {
            PrepareOpenGL();

            //  Tell OpenGL where the texture data is.
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RED, width, height, 0, GL.GL_RED, GL.GL_UNSIGNED_BYTE, data);
            GL.CheckError();

            FinalizeOpenGL();


        }

        /// <summary>
        /// Load a red mask in the texture
        /// </summary>
        public void SetDataRed(int width, int height, byte[] data)
        {
            PrepareOpenGL();

            //  Tell OpenGL where the texture data is.
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RED, width, height, 0, GL.GL_BGR, GL.GL_UNSIGNED_BYTE, data);
            GL.CheckError();

            FinalizeOpenGL();


        }


        /// <summary>
        /// Prepare before loading texture
        /// </summary>
        private void PrepareOpenGL()
        {
            if (_textureid == uint.MaxValue)
            {
                _textureid = GL.glGenTextures();
                GL.CheckError();
            }


            GL.glBindTexture(GL.GL_TEXTURE_2D, _textureid);
            GL.CheckError();


        }


        /// <summary>
        /// Finalize loading texture
        /// </summary>
        private void FinalizeOpenGL()
        {
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
