using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Shaders
{
    /// <summary>
    /// Helper for the shader
    /// </summary>
    public static class ShaderCompiler
    {
        /// <summary>
        /// Analyze a shader and creates a shader binder
        /// </summary>
        public static ShaderBinder BuildBinder(Shader shader)
        {
            ShaderBinder binder = new ShaderBinder();

            binder.Shader = shader;

            return binder;
        }

        ///// <summary>
        ///// Reads shader code and returns parameters
        ///// </summary>
        //public static List<ShaderParameter> GetShaderParameters(string code)
        //{

        //}
    }
}
