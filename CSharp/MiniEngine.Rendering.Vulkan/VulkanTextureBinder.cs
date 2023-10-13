using Silk.NET.Vulkan;
using Image = Silk.NET.Vulkan.Image;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Texture binder for Vulkan
    /// </summary>
    public unsafe class VulkanTextureBinder: IDisposable
    {
        public uint mipLevels;
        private Image textureImage;
        private DeviceMemory textureImageMemory;
        public ImageView textureImageView;
        public Format Format = Format.R8G8B8A8Srgb;

        /// <summary>
        /// Texture that uses this binder
        /// </summary>
        private Texture2D _texture = null;

        /// <summary>
        /// Vulkan instance
        /// </summary>
        private VulkanInstance _vi;

        public VulkanSampler Sampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public VulkanTextureBinder(Texture2D texture, VulkanInstance vi)
        {
            _vi = vi;
            _texture = texture;
        }

        public void Init()
        {
            CreateTextureImage();
            CreateTextureImageView();

            Sampler = _vi.GetSampler(mipLevels);
        }

        /// <summary>
        /// Bind the texture to a textureunit
        /// </summary>
        public void Bind(uint textureUnit)
        {

        }

        /// <summary>
        /// Dispose the texture
        /// </summary>
        public void Dispose()
        {
            if (_texture != null)
            {
               
                _vi.Api.DestroyImageView(_vi.device, textureImageView, null);

                _vi.Api.DestroyImage(_vi.device, textureImage, null);
                _vi.Api.FreeMemory(_vi.device, textureImageMemory, null);

                _texture.RendererStateObj = null;
                _texture = null;
            }
        }


        /// <summary>
        /// Prepare before loading texture
        /// </summary>
        private void CreateTextureImage()
        {
            //using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(@"Assets\viking_room.png");
            //ulong imageSize2 = (ulong)(img.Width * img.Height * img.PixelType.BitsPerPixel / 8);
            //byte[] data = _texture.Data;


            //FormatProperties prop;
            //_vi.Api.GetPhysicalDeviceFormatProperties(_vi.physicalDevice, Format.R8G8B8Srgb, out prop);

            //FormatProperties prop2;
            //_vi.Api.GetPhysicalDeviceFormatProperties(_vi.physicalDevice, Format.R8G8B8A8Srgb, out prop2);

            ulong imageSize = (ulong)(_texture.Width * _texture.Height * 4);
            mipLevels = (uint)(Math.Floor(Math.Log2(Math.Max(_texture.Width, _texture.Height))) + 1);

            Buffer stagingBuffer = default;
            DeviceMemory stagingBufferMemory = default;
            _vi.CreateBuffer(imageSize, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, ref stagingBuffer, ref stagingBufferMemory);

            //fixed (byte* ptrData = &data[0])
            //{
            //    _vi.Api.MapMemory(_vi.device, stagingBufferMemory, 0, imageSize, 0, (void**)ptrData);
            //}

            void* data;
            _vi.Api.MapMemory(_vi.device, stagingBufferMemory, 0, imageSize, 0, &data);
            using (MemoryStream ms = new MemoryStream(_texture.SourceData))
            {
                using (var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(ms))
                {
                    image.CopyPixelDataTo(new Span<byte>(data, (int)imageSize));
                }
            }

            _vi.Api.UnmapMemory(_vi.device, stagingBufferMemory);

            //_vi.CreateImage((uint)_texture.Width, (uint)_texture.Height, mipLevels, SampleCountFlags.Count1Bit, this.Format, ImageTiling.Optimal, ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit, MemoryPropertyFlags.DeviceLocalBit, ref textureImage, ref textureImageMemory);
            _vi.CreateImage((uint)_texture.Width, (uint)_texture.Height, mipLevels, SampleCountFlags.Count1Bit, this.Format, ImageTiling.Optimal, ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit, MemoryPropertyFlags.DeviceLocalBit, ref textureImage, ref textureImageMemory);

            TransitionImageLayout(textureImage, this.Format, ImageLayout.Undefined, ImageLayout.TransferDstOptimal, mipLevels);
            CopyBufferToImage(stagingBuffer, textureImage, (uint)_texture.Width, (uint)_texture.Height);
            //Transitioned to VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL while generating mipmaps

            _vi.Api.DestroyBuffer(_vi.device, stagingBuffer, null);
            _vi.Api.FreeMemory(_vi.device, stagingBufferMemory, null);

            GenerateMipMaps(textureImage, this.Format, (uint)_texture.Width, (uint)_texture.Height, mipLevels);
        }


        private void TransitionImageLayout(Image image, Format format, ImageLayout oldLayout, ImageLayout newLayout, uint mipLevels)
        {
            CommandBuffer commandBuffer = _vi.BeginSingleTimeCommands();

            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = image,
                SubresourceRange =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = mipLevels,
                BaseArrayLayer = 0,
                LayerCount = 1,
            }
            };

            PipelineStageFlags sourceStage;
            PipelineStageFlags destinationStage;

            if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;

                sourceStage = PipelineStageFlags.TopOfPipeBit;
                destinationStage = PipelineStageFlags.TransferBit;
            }
            else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;

                sourceStage = PipelineStageFlags.TransferBit;
                destinationStage = PipelineStageFlags.FragmentShaderBit;
            }
            else
            {
                throw new Exception("unsupported layout transition!");
            }

            _vi.Api.CmdPipelineBarrier(commandBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, barrier);

            _vi.EndSingleTimeCommands(commandBuffer);

        }


        private void CopyBufferToImage(Buffer buffer, Image image, uint width, uint height)
        {
            CommandBuffer commandBuffer = _vi.BeginSingleTimeCommands();

            BufferImageCopy region = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1,
            },
                ImageOffset = new Offset3D(0, 0, 0),
                ImageExtent = new Extent3D(width, height, 1),

            };

            _vi.Api.CmdCopyBufferToImage(commandBuffer, buffer, image, ImageLayout.TransferDstOptimal, 1, region);

            _vi.EndSingleTimeCommands(commandBuffer);
        }


        private void GenerateMipMaps(Image image, Format imageFormat, uint width, uint height, uint mipLevels)
        {
            _vi.Api.GetPhysicalDeviceFormatProperties(_vi.physicalDevice, imageFormat, out var formatProperties);

            if ((formatProperties.OptimalTilingFeatures & FormatFeatureFlags.SampledImageFilterLinearBit) == 0)
            {
                throw new Exception("texture image format does not support linear blitting!");
            }

            var commandBuffer = _vi.BeginSingleTimeCommands();

            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                Image = image,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                SubresourceRange =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseArrayLayer = 0,
                LayerCount = 1,
                LevelCount = 1,
            }
            };

            var mipWidth = width;
            var mipHeight = height;

            for (uint i = 1; i < mipLevels; i++)
            {
                barrier.SubresourceRange.BaseMipLevel = i - 1;
                barrier.OldLayout = ImageLayout.TransferDstOptimal;
                barrier.NewLayout = ImageLayout.TransferSrcOptimal;
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;

                _vi.Api.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.TransferBit, 0,
                    0, null,
                    0, null,
                    1, barrier);

                ImageBlit blit = new()
                {
                    SrcOffsets =
                {
                    Element0 = new Offset3D(0,0,0),
                    Element1 = new Offset3D((int)mipWidth, (int)mipHeight, 1),
                },
                    SrcSubresource =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    MipLevel = i - 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
                    DstOffsets =
                {
                    Element0 = new Offset3D(0,0,0),
                    Element1 = new Offset3D((int)(mipWidth > 1 ? mipWidth / 2 : 1), (int)(mipHeight > 1 ? mipHeight / 2 : 1),1),
                },
                    DstSubresource =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    MipLevel = i,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },

                };

                _vi.Api.CmdBlitImage(commandBuffer,
                    image, ImageLayout.TransferSrcOptimal,
                    image, ImageLayout.TransferDstOptimal,
                    1, blit,
                    Filter.Linear);

                barrier.OldLayout = ImageLayout.TransferSrcOptimal;
                barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;

                _vi.Api.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit, 0,
                    0, null,
                    0, null,
                    1, barrier);

                if (mipWidth > 1) mipWidth /= 2;
                if (mipHeight > 1) mipHeight /= 2;
            }

            barrier.SubresourceRange.BaseMipLevel = mipLevels - 1;
            barrier.OldLayout = ImageLayout.TransferDstOptimal;
            barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;

            _vi.Api.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit, 0,
                0, null,
                0, null,
                1, barrier);

            _vi.EndSingleTimeCommands(commandBuffer);
        }



        private void CreateTextureImageView()
        {
            textureImageView = CreateImageView(textureImage, this.Format, ImageAspectFlags.ColorBit, mipLevels);
        }


        private ImageView CreateImageView(Image image, Format format, ImageAspectFlags aspectFlags, uint mipLevels)
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


            if (_vi.Api.CreateImageView(_vi.device, createInfo, null, out ImageView imageView) != Result.Success)
            {
                throw new Exception("failed to create image views!");
            }

            return imageView;
        }



    }
}
