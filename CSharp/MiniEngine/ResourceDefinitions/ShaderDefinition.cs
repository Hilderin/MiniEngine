using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.ResourceDefinitions
{
    /// <summary>
    /// Information necessary to create a shader
    /// </summary>
    public class ShaderDefinition
    {
        /// <summary>
        /// VertexCode
        /// </summary>
        public string VertexCode { get; set; }

        /// <summary>
        /// FragmentCode
        /// </summary>
        public string FragmentCode { get; set; }

        /// <summary>
        /// Overwrides for variable formats
        /// </summary>
        public Dictionary<string, string> OverwrideVariableFormats = new Dictionary<string, string>();
    }
}
