using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public class Shader
    {
        /// <summary>
        /// Code for vertex shader
        /// </summary>
        public string VertexCode;

        /// <summary>
        /// Code for fragment shader
        /// </summary>
        public string FragmentCode;

        /// <summary>
        /// Constructor
        /// </summary>
        public Shader(string vertexCode, string fragmentCode)
        {
            VertexCode = vertexCode;
            FragmentCode = fragmentCode;
        }
    }
}
