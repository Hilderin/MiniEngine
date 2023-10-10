using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.OpenGL
{
    /// <summary>
    /// Texture binder for OpenGL
    /// </summary>
    public class OpenGLTextureBinder
    {
       
        /// <summary>
        /// Texture id OpenGL
        /// </summary>
        private uint _textureid = uint.MaxValue;

        /// <summary>
        /// Texture that uses this binder
        /// </summary>
        private Texture2D _texture = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public OpenGLTextureBinder(Texture2D texture)
        {
            _texture = texture;

            uint internalFormat;
            uint format;

            switch (texture.Type)
            {
                case TextureType.RGB:
                    //RGB...
                    internalFormat = GL.GL_RGB;
                    format = GL.GL_BGR;
                    break;

                case TextureType.Red:
                    //Red...
                    internalFormat = GL.GL_RED;
                    format = GL.GL_RED;
                    break;


                default:
                    throw new InvalidDataException($"Invalid texture type: {texture.Type}");
            }


            PrepareOpenGL();

            //  Tell OpenGL where the texture data is.
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, internalFormat, texture.Width, texture.Height, 0, format, GL.GL_UNSIGNED_BYTE, texture.Data);
            GL.CheckError();

            FinalizeOpenGL();
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
        /// Dispose the texture
        /// </summary>
        public void Dispose()
        {
            if (_textureid != uint.MaxValue)
            {
                GL.glDeleteTextures(_textureid);
                GL.CheckError();

                _textureid = uint.MaxValue;

                _texture.RendererStateObj = null;
                _texture = null;
            }
        }


        /// <summary>
        /// Prepare before loading texture
        /// </summary>
        private void PrepareOpenGL()
        {
            _textureid = GL.glGenTextures();
            GL.CheckError();


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
