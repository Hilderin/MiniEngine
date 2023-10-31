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
        /// Overwrides for variable formats
        /// </summary>
        public Dictionary<string, string> OverwrideVariableFormats = new Dictionary<string, string>();
    }
}
