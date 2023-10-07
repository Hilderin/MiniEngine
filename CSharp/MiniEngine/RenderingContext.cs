using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Context of the rendering
    /// </summary>
    public class RenderingContext
    {
        public Material Material = Material.Empty;

        public Matrix4 WVPMatrix;

        public Color3 AmbiantColor = Color3.White;

        public float AmbientIntensity = 1f;

        public Color3 MaterialAmbientColor = Color3.White;
    }
}
