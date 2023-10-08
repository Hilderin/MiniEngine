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
        public Color3 Color = Color3.White;
        public float Intensity = 1f;
    }
}
