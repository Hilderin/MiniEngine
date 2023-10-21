using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// A shader
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// Code for the vertex shader
        /// </summary>
        public string VertexCode;

        /// <summary>
        /// Code for the fragment shader
        /// </summary>
        public string FragmentCode;

        /// <summary>
        /// Constructor
        /// </summary>
        public Shader(string vertexCode, string fragmentCode)
        {
            this.VertexCode = vertexCode;
            this.FragmentCode = fragmentCode;
        }
    }
}
