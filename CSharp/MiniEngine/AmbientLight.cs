using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Ambient light
    /// </summary>
    public class AmbientLight
    {
        /// <summary>
        /// Default ambient light
        /// </summary>
        private static AmbientLight _default = new AmbientLight();

        /// <summary>
        /// Return the default ambient light
        /// </summary>
        public static AmbientLight Default { get { return _default; } }


        public Color3 Color = Color3.White;

        public float Intensity = 1f;

    }
}
