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
        /// Lock object
        /// </summary>
        private static object _lockObj = new object();

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
                if (_unlit == null)
                {
                    lock (_lockObj)
                    {
                        if (_unlit == null)
                        {
                            string vertexCode = GlslHelper.Expand(ResourceUtils.GetString("MiniEngine.Resources.Shaders.unlit.vert"), String.Empty);
                            string fragmentCode = GlslHelper.Expand(ResourceUtils.GetString("MiniEngine.Resources.Shaders.unlit.frag"), String.Empty);

                            _unlit = CreateShader(vertexCode, fragmentCode);
                        }
                    }
                }

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
