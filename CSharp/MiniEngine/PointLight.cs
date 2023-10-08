using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Point light
    /// </summary>
    public class PointLight: WorldTransform
    {
        /// <summary>
        /// Color of the light
        /// </summary>
        public Color3 Color = Color3.White;

        /// <summary>
        /// Ambient intensity of the light
        /// </summary>
        public float AmbientIntensity = 0.3f;

        /// <summary>
        /// Diffuse intensity of the light
        /// </summary>
        public float DiffuseIntensity = 1f;

        public float AttenuationConstant = 1f;

        public float AttenuationLinear = 0.2f;

        public float AttenuationExponent = 0f;


    }
}
