using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Pointer to an object used internally by the renderer
    /// </summary>
    public interface IRenderHandle
    {
        /// <summary>
        /// Set a shader variable
        /// </summary>
        void SetShaderVariable<T>(string name, int materialIndex, T value);
    }
}
