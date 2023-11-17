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
        /// VertexCode
        /// </summary>
        public string VertexCodePath { get; set; }

        /// <summary>
        /// FragmentCode
        /// </summary>
        public string FragmentCodePath { get; set; }

        /// <summary>
        /// ComputeCodePath
        /// </summary>
        public string ComputeCodePath { get; set; }

        /// <summary>
        /// Information on the variables in the shader
        /// </summary>
        public Dictionary<string, ShaderVariableDefinition> VariableDefinitions = new Dictionary<string, ShaderVariableDefinition>();
    }
}
