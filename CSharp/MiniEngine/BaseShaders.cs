using MiniEngine.ResourceDefinitions;
using MiniEngine.Ressources;
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
        /// Basic unlit material
        /// </summary>
        public static Shader Unlit
        {
            get
            {
                if (_unlit == null)
                {
                    ShaderDefinition shaderDef = new ShaderDefinition()
                    {
                        VertexCode = Resources.UnlitVert,
                        FragmentCode = Resources.UnlitFrag
                    };

                    _unlit = Context.Current.Renderer.CreateShader(shaderDef);
                }

                return _unlit;
            }
        }

    }
}
