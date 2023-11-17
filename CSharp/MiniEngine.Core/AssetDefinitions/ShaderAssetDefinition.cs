using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.AssetDefinitions
{
    /// <summary>
    /// Definition of a shader asset
    /// </summary>
    public class ShaderAssetDefinition
    {
        /// <summary>
        /// Paths for each stages
        /// </summary>
        public Dictionary<ShaderStage, string> StagePaths { get; set; } = new Dictionary<ShaderStage, string>();

        /// <summary>
        /// Information on the variables in the shader
        /// </summary>
        public Dictionary<string, ShaderVariableDefinition> VariableDefinitions = new Dictionary<string, ShaderVariableDefinition>();
    }
}
