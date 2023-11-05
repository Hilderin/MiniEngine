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
        /// Information on the variables in the shader
        /// </summary>
        public Dictionary<string, ShaderVariableDefinition> VariableDefinitions = new Dictionary<string, ShaderVariableDefinition>();
    }


    /// <summary>
    /// Information on a variable in the shader
    /// </summary>
    public class ShaderVariableDefinition
    {
        public string Format = String.Empty;
        public int Count = 1;
        public bool Bindless = false;
    }
}
