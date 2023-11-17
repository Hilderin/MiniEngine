using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Base class for shaders
    /// </summary>
    public abstract class Shader: IDisposable
    {
        /// <summary>
        /// Destruction of the Material
        /// </summary>
        protected abstract void Destroy();

        /// <summary>
        /// Load the shader
        /// </summary>
        public abstract Shader Load(ShaderDefinition shaderDef);

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Destroy();
        }
    }
}
