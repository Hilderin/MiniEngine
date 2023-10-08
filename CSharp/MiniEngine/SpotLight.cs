using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// A Spotlight
    /// </summary>
    public class SpotLight: WorldTransform
    {
        public Color3 Color = Color3.White;
        public float Intensity = 1f;

        public float AttenuationConstant = 1f;

        public float AttenuationLinear = 0.2f;

        public float AttenuationExponent = 0f;

        public float Cutoff = 20.0f;

    }
}
