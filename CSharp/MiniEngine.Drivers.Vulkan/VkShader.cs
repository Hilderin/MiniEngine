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
