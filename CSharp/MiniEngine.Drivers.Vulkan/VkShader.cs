using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// A shader for Vulkan
    /// </summary>
    public class VkShader
    {
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
        public List<PushConstantRange> Constants { get; private set; } = new List<PushConstantRange>();

        /// <summary>
        /// Bindings
        /// </summary>
        public List<DescriptorSetLayoutBinding> Bindings { get; private set; } = new List<DescriptorSetLayoutBinding>();

        /// <summary>
        /// Bindings for the vertex buffer
        /// </summary>
        public List<VertexInputBindingDescription> VertexBindings { get; private set; } = new List<VertexInputBindingDescription>();

        /// <summary>
        /// Attributes from vertex buffer
        /// </summary>
        public List<VertexInputAttributeDescription> VertexInputAttributes { get; private set; } = new List<VertexInputAttributeDescription>();

        /// <summary>
        /// Constructor
        /// </summary>
        internal VkShader(byte[] vertexSpirv, byte[] fragmentSpirv)
        {
            this.VertexSpirv = vertexSpirv;
            this.FragmentSpirv = fragmentSpirv;
        }
    }
}
