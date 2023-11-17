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
        /// Current camera location
        /// </summary>
        public const string CameraLocation = "_camera_location";

        /// <summary>
        /// Number of meshlet instance
        /// </summary>
        public const string MeshLetInstanceCount = "_meshlet_instance_count";
        

        /// <summary>
        /// Current diffuse texture sampler.
        /// </summary>
        public const string SamplerDiffuse = "_sampler_diffuse";

        /// <summary>
        /// Buffer of objects
        /// </summary>
        public const string Objects = "_objects";

        /// <summary>
        /// Buffer of meshlet
        /// </summary>
        public const string Meshlets = "_meshlets";

        /// <summary>
        /// Buffer of meshlet instances
        /// </summary>
        public const string MeshletInstances = "_meshlet_instances";


        /// <summary>
        /// Buffer of draw calls buffers
        /// </summary>
        public const string DrawCallsBuffers = "_draw_calls_buffers";

        /// <summary>
        /// Buffer of draw call counts
        /// </summary>
        public const string DrawCallCounts = "_draw_calls_counts";

        /// <summary>
        /// SceneData
        /// </summary>
        public const string Scene = "_scene";


    }
}
