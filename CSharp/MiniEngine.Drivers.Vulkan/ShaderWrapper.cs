using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Information on a shader for Vulkan
    /// </summary>
    public class ShaderWrapper
    {
        private Device _device;

        /// <summary>
        /// Compiled code for vertex shader
        /// </summary>
        public byte[] VertexSpirv;

        /// <summary>
        /// Compiled code for vertex shader
        /// </summary>
        public byte[] FragmentSpirv;

        /// <summary>
        /// Vertex Entry point name
        /// </summary>
        public string VertexEntryPoint;

        /// <summary>
        /// Fragment Entry point name
        /// </summary>
        public string FragmentEntryPoint;


        /// <summary>
        /// Constants
        /// </summary>
        public PushConstantRange[] Constants;

        /// <summary>
        /// Bindings
        /// </summary>
        public DescriptorSetLayoutBinding[][] BindingSets;

        /// <summary>
        /// Bindings for the vertex buffer
        /// </summary>
        public VertexInputBindingDescription[] VertexBindings;

        /// <summary>
        /// Attributes from vertex buffer
        /// </summary>
        public VertexInputAttributeDescription[] VertexInputAttributes;

        public ShaderModule VertexShaderModule;

        public ShaderModule FragmentShaderModule;

        /// <summary>
        /// Constructor
        /// </summary>
        public ShaderWrapper(Device device, byte[] vertexSpirv, byte[] fragmentSpirv, Dictionary<string, Format> overwrideVariableFormats = null)
        {
            _device = device;

            this.VertexSpirv = vertexSpirv;
            this.FragmentSpirv = fragmentSpirv;

            SpirvParser.ParseUpdateShader(this, overwrideVariableFormats);

            if (_device != null)
                CreateModules();

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ShaderWrapper(Device device, string vertexCode, string fragmentCode, Dictionary<string, Format> overwrideVariableFormats = null)
        {
            _device = device;

            this.VertexSpirv = VkShaderCompiler.Compile(vertexCode, ShaderStageFlags.Vertex);
            this.FragmentSpirv = VkShaderCompiler.Compile(fragmentCode, ShaderStageFlags.Fragment);

            SpirvParser.ParseUpdateShader(this, overwrideVariableFormats);

            if (_device != null)
                CreateModules();
        }


        /// <summary>
        /// Create the modules for the shader
        /// </summary>
        public void CreateModules()
        {
            VertexShaderModule = _device.CreateShaderModule(VertexSpirv);
            FragmentShaderModule = _device.CreateShaderModule(FragmentSpirv);
        }
    }
}
