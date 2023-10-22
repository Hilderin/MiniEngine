using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Shaders
{
    /// <summary>
    /// Shader binder
    /// </summary>
    public class ShaderBinder
    {
        /// <summary>
        /// Shader
        /// </summary>
        public Shader Shader;

        /// <summary>
        /// List of parameters
        /// </summary>
        public List<ShaderParameter> Parameters { get; private set; } = new List<ShaderParameter>();

    }
}
