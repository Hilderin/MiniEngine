using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Shaders
{
    /// <summary>
    /// Shader parameter (uniform)
    /// </summary>
    public class ShaderParameter
    {
        public int Location;
        public string Name;
        public Type Type;
        public ShaderStage Stage;
        
    }
}
