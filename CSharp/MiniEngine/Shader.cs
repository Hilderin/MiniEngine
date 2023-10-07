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
    public class Shader     //: IDisposable
    {

        /// <summary>
        /// OpenGL program id
        /// Static so we can reuse the same shader program
        /// </summary>
        private static uint _program = uint.MaxValue;

        /// <summary>
        /// Shaders
        /// </summary>
        private List<uint> _shaders = new List<uint>();

        /// <summary>
        /// Uniforms by name
        /// </summary>
        private Dictionary<string, int> _uniforms = new Dictionary<string, int>();

        /// <summary>
        /// Indicate if the shader is compiled
        /// </summary>
        private bool _isCompiled = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public Shader()
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Shader(string vertexShaderCode, string fragmentShaderCode)
        {
            Add(vertexShaderCode, ShaderType.Vertex);
            Add(fragmentShaderCode, ShaderType.Fragment);
        }

        /// <summary>
        /// Activate the shader
        /// </summary>
        public void Enable()
        {
            if (!_isCompiled)
                Compile();

            GL.glUseProgram(_program);
            GL.CheckError();
        }

        /// <summary>
        /// Add shader code
        /// </summary>
        public void Add(string code, ShaderType shaderType)
        {
            if (_isCompiled)
                throw new InvalidOperationException("Impossible to add additionnal shaders after it has been compiled.");

            uint shader = GL.CreateShader((uint)shaderType, code);
            GL.CheckError();

            _shaders.Add(shader);
        }

        /// <summary>
        /// Compile the material
        /// </summary>
        private void Compile()
        {
            //Create the OpenGL program...
            _program = GL.glCreateProgram();
            GL.CheckError();

            //Attach shaders...
            foreach (uint shader in _shaders)
            {
                GL.glAttachShader(_program, shader);
                GL.CheckError();
            }

            //Linking program...
            GL.glLinkProgram(_program);
            GL.CheckError();

            if (!GL.glGetProgramiv(_program, GL.GL_LINK_STATUS))
                throw new Exception($"Error linking program: {GL.glGetProgramInfoLog(_program)}");

            //Validate program....
            GL.glValidateProgram(_program);
            if (!GL.glGetProgramiv(_program, GL.GL_VALIDATE_STATUS))
                throw new Exception($"Error validating program: {GL.glGetProgramInfoLog(_program)}");

            //Delete shaders...
            foreach (uint shader in _shaders)
            {
                GL.glDeleteShader(shader);
                GL.CheckError();
            }

            _shaders.Clear();

            //It's compiled!
            _isCompiled = true;

        }

        /// <summary>
        /// Set uniform matrix
        /// </summary>
        public void SetUniform(string uniformName, Matrix4 matrix)
        {
            SetUniform(GetUniformLocation(uniformName), ref matrix);
        }

        /// <summary>
        /// Set uniform 1 int
        /// </summary>
        public void SetUniform(string uniformName, int v0)
        {
            SetUniform(GetUniformLocation(uniformName), v0);
        }

        /// <summary>
        /// Set uniform matrix
        /// </summary>
        public void SetUniform(int uniformID, ref Matrix4 matrix)
        {
            if (uniformID >= 0)
            {
                GL.glUniformMatrix4fv(uniformID, ref matrix);
                GL.CheckError();
            }
        }

        /// <summary>
        /// Set uniform 1 float
        /// </summary>
        public void SetUniform(string uniformName, float v0)
        {
            SetUniform(GetUniformLocation(uniformName), v0);
        }

        /// <summary>
        /// Set uniform 1 int
        /// </summary>
        public void SetUniform(int uniformID, int v0)
        {
            if (uniformID >= 0)
            {
                GL.glUniform1i(uniformID, v0);
                GL.CheckError();
            }
        }

        /// <summary>
        /// Set uniform 1 float
        /// </summary>
        public void SetUniform(int uniformID, float v0)
        {
            if (uniformID >= 0)
            {
                GL.glUniform1f(uniformID, v0);
                GL.CheckError();
            }
        }
        /// <summary>
        /// Set uniform 2 floats
        /// </summary>
        public void SetUniform(int uniformID, float v0, float v1, float v2)
        {
            if (uniformID >= 0)
            {
                GL.glUniform3f(uniformID, v0, v1, v2);
                GL.CheckError();
            }
        }

        /// <summary>
        /// Set uniform 3 floats
        /// </summary>
        public void SetUniform(int uniformID, float v0, float v1)
        {
            if (uniformID >= 0)
            {
                GL.glUniform2f(uniformID, v0, v1);
                GL.CheckError();
            }
        }

        /// <summary>
        /// Get the uniform id
        /// </summary>
        public int GetUniformLocation(string name)
        {
            if (!_isCompiled)
                Compile();

            int id;
            if (!_uniforms.TryGetValue(name, out id))
            {
                id = GL.glGetUniformLocation(_program, name);
                //if (id == -1)
                //    throw new Exception($"Error getting uniform location of '{name}'");

                _uniforms.Add(name, id);
            }

            return id;
        }

        ///// <summary>
        ///// Dispose the shader
        ///// </summary>
        //public void Dispose()
        //{
        //    if (_program != uint.MaxValue)
        //    {
        //        GL.glDeleteProgram(_program);
        //        GL.CheckError();

        //        _program = uint.MaxValue;
        //    }

        //}
    }
}
