using MiniEngine;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.ResourceDefinitions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Factory for Vulkan resources
    /// </summary>
    public class VkResourceFactory: IDisposable
    {
        private VkRenderer _vk;

        private Dictionary<int, IDisposable> _resources = new Dictionary<int, IDisposable>();


        /// <summary>
        /// Constructor
        /// </summary>
        public VkResourceFactory(VkRenderer vk)
        {
            _vk = vk;
        }

        /// <summary>
        /// Create a Texture2D
        /// </summary>
        public VkTexture2D CreateTexture2D(Texture2DDefinition texDef)
        {
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(texDef.Data))
            {
                byte[] pixelData = new byte[image.Width * image.Height * image.PixelType.BitsPerPixel / 8];

                image.CopyPixelDataTo(pixelData);
                return new VkTexture2D(pixelData, image.Width, image.Height, Format.R8G8B8A8Srgb, _vk, this);
            }
        }

        /// <summary>
        /// Create a Material
        /// </summary>
        public VkMaterial CreateMaterial(MaterialDefinition matDef)
        {
            return new VkMaterial(matDef, this);
        }

        /// <summary>
        /// Create a Mesh
        /// </summary>
        public VkMesh CreateMesh(MeshDefinition meshDef)
        {
            return new VkMesh(meshDef, _vk, this);

        }

        /// <summary>
        /// Remove an object from the resources list
        /// </summary>
        public void Add(IDisposable resource)
        {
            _resources.Add(resource.GetHashCode(), resource);
        }

        /// <summary>
        /// Remove an object from the resources list
        /// </summary>
        public void Remove(IDisposable resource)
        {
            _resources.Remove(resource.GetHashCode());
        }

        /// <summary>
        /// Dispose the resources
        /// </summary>
        public void Dispose()
        {
            foreach (IDisposable disposable in _resources.Values)
            {
                disposable.Dispose();
            }
        }
    }
}
