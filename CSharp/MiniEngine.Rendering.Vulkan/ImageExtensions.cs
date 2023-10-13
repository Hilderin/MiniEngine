using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    public unsafe static class ImageExtensions
    {
        /// <summary>
        /// Create a image view
        /// </summary>
        public static ImageView CreateImageView(this VulkanInstance vi, Image image, Format format, ImageAspectFlags aspectFlags, uint mipLevels)
        {
            ImageViewCreateInfo createInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.Type2D,
                Format = format,
                //Components =
                //    {
                //        R = ComponentSwizzle.Identity,
                //        G = ComponentSwizzle.Identity,
                //        B = ComponentSwizzle.Identity,
                //        A = ComponentSwizzle.Identity,
                //    },
                SubresourceRange =
                {
                    AspectMask = aspectFlags,
                    BaseMipLevel = 0,
                    LevelCount = mipLevels,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

            };


            if (vi.Api.CreateImageView(vi.device, createInfo, null, out ImageView imageView) != Result.Success)
            {
                throw new Exception("failed to create image views!");
            }

            return imageView;
        }

        /// <summary>
        /// Create and bind an image
        /// </summary>
        public static void CreateImage(this VulkanInstance vi, uint width, uint height, uint mipLevels, SampleCountFlags numSamples, Format format, ImageTiling tiling, ImageUsageFlags usage, MemoryPropertyFlags properties, ref Image image, ref DeviceMemory imageMemory)
        {
            ImageCreateInfo imageInfo = new()
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent =
            {
                Width = width,
                Height = height,
                Depth = 1,
            },
                MipLevels = mipLevels,
                ArrayLayers = 1,
                Format = format,
                Tiling = tiling,
                InitialLayout = ImageLayout.Undefined,
                Usage = usage,
                Samples = numSamples,
                SharingMode = SharingMode.Exclusive,
            };

            fixed (Image* imagePtr = &image)
            {
                if (vi.Api.CreateImage(vi.device, imageInfo, null, imagePtr) != Result.Success)
                {
                    throw new Exception("failed to create image!");
                }
            }

            vi.Api.GetImageMemoryRequirements(vi.device, image, out MemoryRequirements memRequirements);

            MemoryAllocateInfo allocInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = vi.FindMemoryType(memRequirements.MemoryTypeBits, properties),
            };

            fixed (DeviceMemory* imageMemoryPtr = &imageMemory)
            {
                if (vi.Api.AllocateMemory(vi.device, allocInfo, null, imageMemoryPtr) != Result.Success)
                {
                    throw new Exception("failed to allocate image memory!");
                }
            }

            vi.Api.BindImageMemory(vi.device, image, imageMemory, 0);
        }

    }
}
