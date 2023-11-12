using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    public static class ShaderVariableNames
    {

        /// <summary>
        /// Current model, view, projection matrix.
        /// </summary>
        public const string MatrixVP = "_matrix_vp";

        /// <summary>
        /// Current diffuse texture sampler.
        /// </summary>
        public const string SamplerDiffuse = "_sampler_diffuse";
                
        /// <summary>
        /// Index of the vertex buffer index
        /// </summary>
        public const string VertexBufferIndex = "_vertex_buffer_index";


    }
}
