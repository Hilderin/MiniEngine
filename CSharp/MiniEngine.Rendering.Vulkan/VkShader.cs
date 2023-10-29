using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// VkShader
    /// </summary>
    public class VkShader: Shader
    {

        public ShaderWrapper ShaderData;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkShader(ShaderWrapper shaderData)
        {
            ShaderData = shaderData;
        }

        /// <summary>
        /// Nothing to destroy
        /// </summary>
        protected override void Destroy()
        {
            
        }
    }
}
