using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Vulkan Material
    /// </summary>
    public class VkMaterial : IDisposable
    {
        /// <summary>
        /// Diffused Image
        /// </summary>
        public ImageWrapper Diffuse;


        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (Diffuse != null)
            {
                Diffuse.Dispose();
                Diffuse = null;
            }
        }
    }
}
