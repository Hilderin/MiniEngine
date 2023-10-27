using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// A Texture 2D for Vulkan
    /// </summary>
    public class VkTexture2D: Texture2D
    {
        private VkResourceFactory _factory;

        /// <summary>
        /// Image wrapper for vulkan
        /// </summary>
        public VkImageWrapper ImageWrapper;

        /// <summary>
        /// Format
        /// </summary>
        public Format Format;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkTexture2D(byte[] pixelData, int width, int height, Format format, VkRenderer vk, VkResourceFactory factory)
        {
            

            ImageWrapper = new VkImageWrapper(vk.Device, pixelData, width, height, format);

            this.Width = width;
            this.Height = height;
            this.Format = format;


            _factory = factory;
            if (factory != null)
                factory.Add(this);

        }

        /// <summary>
        /// Destruction
        /// </summary>
        protected override void Destroy()
        {
            if (ImageWrapper != null)
                ImageWrapper.Dispose();

            if (_factory != null)
                _factory.Remove(this);
        }
    }
}
