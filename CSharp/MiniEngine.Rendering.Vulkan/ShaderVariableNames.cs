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
        public const string MatrixMVP = "_matrix_mvp";


        /// <summary>
        /// Current diffuse texture sampler.
        /// </summary>
        public const string SamplerDiffuse = "_sampler_diffuse";
                
        /// <summary>
        /// Index of the diffuse material in the _sampler_diffuse when _sampler_diffuse is an array.
        /// </summary>
        public const string MaterialDiffuseIndex = "_mat_diffuse_index";

        /// <summary>
        /// Index of the vertex buffer index
        /// </summary>
        public const string VertexBufferIndex = "_vertex_buffer_index";


    }
}
