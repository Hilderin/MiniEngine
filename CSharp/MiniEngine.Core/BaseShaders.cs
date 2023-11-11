using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Base shaders
    /// </summary>
    public class BaseShaders
    {
        /// <summary>
        /// Unlit shader
        /// </summary>
        public static Shader _unlit;

        /// <summary>
        /// Default shader
        /// </summary>
        public static Shader Default => Unlit;


        /// <summary>
        /// Basic unlit shader
        /// </summary>
        public static Shader Unlit
        {
            get
            {
                _unlit ??= CreateShader(ResourceUtils.GetString("Resources.Shaders.Unlit.vert"), ResourceUtils.GetString("Resources.Shaders.Unlit.frag"));

                return _unlit;
            }
        }

        /// <summary>
        /// Create a shader
        /// </summary>
        private static Shader CreateShader(string vertexCode, string fragmentCode)
        {
            if (Renderer.Current == null)
                throw new InvalidOperationException("No current renderer.");

            ShaderDefinition shaderDef = new ShaderDefinition()
            {
                VertexCode = vertexCode,
                FragmentCode = fragmentCode
            };

            return Renderer.Current.CreateShader(shaderDef);
        }

    }
}
