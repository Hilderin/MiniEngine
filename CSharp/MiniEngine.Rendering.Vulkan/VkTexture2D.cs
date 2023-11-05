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
    public class VkTexture2D : Texture2D
    {
        private VkResourceFactory _factory;

        /// <summary>
        /// Image wrapper for vulkan
        /// </summary>
        public ImageWrapper ImageWrapper;

        /// <summary>
        /// Format
        /// </summary>
        public Format Format;

        /// <summary>
        /// Indicate if the texture is loaded and ready to use
        /// </summary>
        public bool IsLoaded { get { return ImageWrapper.IsLoaded; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public unsafe VkTexture2D(byte* pixelData, int width, int height, Format format, VkRenderer renderer, VkResourceFactory factory)
        {
            ImageWrapper = new ImageWrapper(renderer, pixelData, width, height, format);

            this.Width = width;
            this.Height = height;
            this.Format = format;


            _factory = factory;
            factory?.Add(this);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public VkTexture2D(byte[] pixelData, int width, int height, Format format, VkRenderer renderer, VkResourceFactory factory)
        {


            ImageWrapper = new ImageWrapper(renderer, pixelData, width, height, format);

            this.Width = width;
            this.Height = height;
            this.Format = format;


            _factory = factory;
            factory?.Add(this);

        }

        /// <summary>
        /// Reloading of the texture
        /// </summary>
        protected override void Reload()
        {
            
        }

        /// <summary>
        /// Destruction
        /// </summary>
        protected override void Destroy()
        {
            ImageWrapper?.Dispose();

            _factory?.Remove(this);
        }
    }
}
