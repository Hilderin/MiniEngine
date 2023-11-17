using MiniEngine.Drivers.Vulkan;
using MiniEngine.ResourceDefinitions;
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

        public ShaderWrapper ShaderWrapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkShader(ShaderWrapper shaderWrapper)
        {
            ShaderWrapper = shaderWrapper;
        }

        /// <summary>
        /// Create a shader
        /// </summary>
        public override VkShader Load(ShaderDefinition shaderDef)
        {
            ShaderWrapper.Load(shaderDef);
            return this;
        }

        /// <summary>
        /// Nothing to destroy
        /// </summary>
        protected override void Destroy()
        {
        }
    }
}
