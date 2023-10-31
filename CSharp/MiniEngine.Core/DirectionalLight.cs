using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Directional light
    /// </summary>
    public class DirectionalLight: WorldTransform
    {
        /// <summary>
        /// Color of the light
        /// </summary>
        public Color3 Color = Color3.White;

        /// <summary>
        /// Intensity of the light
        /// </summary>
        public float Intensity = 1f;

        public float AttenuationConstant = 1f;

        public float AttenuationLinear = 0.2f;

        public float AttenuationExponent = 0f;
    }
}
