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
    /// Vulkan Material
    /// </summary>
    public class VkMaterial : Material, IDisposable
    {
        private VkResourceFactory _factory;

        public VkTexture2D VkDiffuseTexture;

        public VkShader Shader;
        

        /// <summary>
        /// Constructor
        /// </summary>
        public VkMaterial(MaterialDefinition matdef, VkResourceFactory factory)
        {

            VkDiffuseTexture = (VkTexture2D)matdef.DiffuseTexture;
            this.Diffuse = VkDiffuseTexture;

            Shader = (VkShader)matdef.Shader;

            _factory = factory;
            factory?.Add(this);

        }

        /// <summary>
        /// Destruction
        /// </summary>
        protected override void Destroy()
        {
            _factory?.Remove(this);
        }
    }
}
