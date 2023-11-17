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
        /// Code for each stages
        /// </summary>
        public Dictionary<ShaderStage, string> StageCodes { get; set; } = new Dictionary<ShaderStage, string>();

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
