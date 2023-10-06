using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Material
    /// </summary>
    public class Shader: IDisposable
    {

        /// <summary>
        /// OpenGL program id
        /// </summary>
        private uint _program = uint.MaxValue;

        /// <summary>
        /// Code for the vertex shader
        /// </summary>
        private string _vertexShaderCode;

        /// <summary>
        /// Code for the fragment shader
        /// </summary>
        private string _fragmentShaderCode;

        /// <summary>
        /// Uniforms by name
        /// </summary>
        private Dictionary<string, int> _uniforms = new Dictionary<string, int>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Shader(string vertexShaderCode, string fragmentShaderCode)
        {
            _vertexShaderCode = vertexShaderCode;
            _fragmentShaderCode = fragmentShaderCode;

            Compile();
        }


        /// <summary>
        /// Compile the material
        /// </summary>
        internal void Compile()
        {
            //Create the OpenGL program...
            _program = GL.glCreateProgram();
            GL.CheckError();

            var vertex = GL.CreateShader(GL.GL_VERTEX_SHADER, _vertexShaderCode);
            GL.CheckError();

            var fragment = GL.CreateShader(GL.GL_FRAGMENT_SHADER, _fragmentShaderCode);
            GL.CheckError();

            _program = GL.glCreateProgram();
            GL.CheckError();

            GL.glAttachShader(_program, vertex);
            GL.CheckError();

            GL.glAttachShader(_program, fragment);
            GL.CheckError();

            //Linking program...
            GL.glLinkProgram(_program);
            GL.CheckError();

            if (!GL.glGetProgramiv(_program, GL.GL_LINK_STATUS))
                throw new Exception($"Error linking program: {GL.glGetProgramInfoLog(_program)}");

            //Validate program....
            GL.glValidateProgram(_program);
            if (!GL.glGetProgramiv(_program, GL.GL_VALIDATE_STATUS))
                throw new Exception($"Error validating program: {GL.glGetProgramInfoLog(_program)}");


            GL.glDeleteShader(vertex);
            GL.CheckError();

            GL.glDeleteShader(fragment);
            GL.CheckError();

            GL.glUseProgram(_program);
            GL.CheckError();

        }

        /// <summary>
        /// Set uniform matrix
        /// </summary>
        public void SetUniform(string uniformName, Matrix4 matrix)
        {
            GL.glUniformMatrix4fv(GetUniformID(uniformName), matrix);
            GL.CheckError();
        }

        /// <summary>
        /// Set uniform 1 int
        /// </summary>
        public void SetUniform(string uniformName, int v0)
        {
            GL.glUniform1i(GetUniformID(uniformName), v0);
            GL.CheckError();
        }

        /// <summary>
        /// Set uniform matrix
        /// </summary>
        public void SetUniform(int uniformID, Matrix4 matrix)
        {
            GL.glUniformMatrix4fv(uniformID, matrix);
            GL.CheckError();
        }

        /// <summary>
        /// Get the uniform id
        /// </summary>
        public int GetUniformID(string name)
        {
            int id;
            if (!_uniforms.TryGetValue(name, out id))
            {
                id = GL.glGetUniformLocation(_program, name);
                if (id == -1)
                    throw new Exception($"Error getting uniform location of '{name}'");

                _uniforms.Add(name, id);
            }

            return id;
        }

        /// <summary>
        /// Dispose the shader
        /// </summary>
        public void Dispose()
        {
            if (_program != uint.MaxValue)
            {
                GL.glDeleteProgram(_program);
                GL.CheckError();

                _program = uint.MaxValue;
            }

        }
    }
}
