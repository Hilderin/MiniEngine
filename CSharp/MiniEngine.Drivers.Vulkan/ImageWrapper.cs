using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Advanced;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Wrapper for images
    /// </summary>
    public class ImageWrapper : IDisposable
    {
        private Device _device;


        public int Width;
        public int Height;
        public Format Format;


        public Image Image;
        public ImageView ImageView;
        public DeviceMemory DeviceMemory;

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageWrapper(Device device, byte[] data, int width, int height, Format format)
        {
            _device = device;

            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(@"C:\Projects\SilkVulkanTutorial\Assets\texture.jpg"))
            {
                data = new byte[image.Width * image.Height * image.PixelType.BitsPerPixel / 8];
                image.CopyPixelDataTo(data);

                width = image.Width;
                height = image.Height;
            }

            Width = width;
            Height = height;
            Format = format;

            Init(data);

        }

        /// <summary>
        /// Create the image
        /// </summary>
        private void Init(byte[] data)
        {
            using (BufferWrapper bufferStaging = _device.CreateBufferWrapper(data, BufferUsageFlags.TransferSrc))
            {

                CreateImage(Format, ImageUsageFlags.TransferDst | ImageUsageFlags.Sampled, MemoryPropertyFlags.DeviceLocal);

                TransitionImageLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

                CopyBufferToImage(bufferStaging.Buffer);

                TransitionImageLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);

            }

            //We need to create the image view now...
            CreateImageView();
        }

        /// <summary>
        /// Create image
        /// </summary>
        private void CreateImage(Format format, ImageUsageFlags usage, MemoryPropertyFlags properties)
        {
            ImageCreateInfo imageInfo = new()
            {
                ImageType = ImageType.Image2D,
                Extent = new()
                {
                    Width = (uint)Width,
                    Height = (uint)Height,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Format = format,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = usage,
                Samples = SampleCountFlags.Count1,
                SharingMode = SharingMode.Exclusive,
            };

            Image = _device.CreateImage(imageInfo);


            var memRequirements = _device.GetImageMemoryRequirements(Image);

            MemoryAllocateInfo allocInfo = new()
            {
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = _device.GetMemoryTypeIndex(memRequirements.MemoryTypeBits, properties),
            };

            DeviceMemory = _device.AllocateMemory(allocInfo);

            _device.BindImageMemory(Image, DeviceMemory, 0);
        }


        /// <summary>
        /// Transition the image layout
        /// </summary>
        private void TransitionImageLayout(ImageLayout oldLayout, ImageLayout newLayout)
        {
            _device.MemoryManager.ExecuteCommandBuffer(commandBuffer =>
            {
                ImageMemoryBarrier barrier = new()
                {
                    OldLayout = oldLayout,
                    NewLayout = newLayout,
                    SrcQueueFamilyIndex = uint.MaxValue,
                    DstQueueFamilyIndex = uint.MaxValue,
                    Image = Image,
                    SubresourceRange = new ()
                    {
                        AspectMask = ImageAspectFlags.Color,
                        BaseMipLevel = 0,
                        LevelCount = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1,
                    }
                };

                PipelineStageFlags sourceStage;
                PipelineStageFlags destinationStage;

                if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
                {
                    barrier.SrcAccessMask = 0;
                    barrier.DstAccessMask = AccessFlags.TransferWrite;

                    sourceStage = PipelineStageFlags.TopOfPipe;
                    destinationStage = PipelineStageFlags.Transfer;
                }
                else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
                {
                    barrier.SrcAccessMask = AccessFlags.TransferWrite;
                    barrier.DstAccessMask = AccessFlags.ShaderRead;

                    sourceStage = PipelineStageFlags.Transfer;
                    destinationStage = PipelineStageFlags.FragmentShader;
                }
                else
                {
                    throw new Exception("unsupported layout transition!");
                }

                commandBuffer.CmdPipelineBarrier(sourceStage, destinationStage, DependencyFlags.ByRegion, null, null, barrier);
            });

        }


        private void CreateImageView()
        {
            ImageViewCreateInfo createInfo = new()
            {
                Image = Image,
                ViewType = ImageViewType.View2D,
                Format = Format,
                //Components =
                //    {
                //        R = ComponentSwizzle.Identity,
                //        G = ComponentSwizzle.Identity,
                //        B = ComponentSwizzle.Identity,
                //        A = ComponentSwizzle.Identity,
                //    },
                SubresourceRange = new()
                {
                    AspectMask = ImageAspectFlags.Color,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

            };

            ImageView = _device.CreateImageView(createInfo);
        }

        /// <summary>
        /// Copy buffer to image
        /// </summary>
        private void CopyBufferToImage(Buffer buffer)
        {
            _device.MemoryManager.ExecuteCommandBuffer(commandBuffer =>
            {
                BufferImageCopy region = new()
                {
                    BufferOffset = 0,
                    BufferRowLength = 0,
                    BufferImageHeight = 0,
                    ImageSubresource =
                    {
                        AspectMask = ImageAspectFlags.Color,
                        MipLevel = 0,
                        BaseArrayLayer = 0,
                        LayerCount = 1,
                    },
                    ImageOffset = new Offset3D(),
                    ImageExtent = new Extent3D()
                    {
                        Width = (uint)Width,
                        Height = (uint)Height,
                        Depth = 1
                    }
                };

                commandBuffer.CmdCopyBufferToImage(buffer, Image, ImageLayout.TransferDstOptimal, region);

            });
        }



        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (ImageView != null)
            {
                _device.DestroyImageView(ImageView);
                ImageView = null;
            }

            if (DeviceMemory != null)
            {
                _device.FreeMemory(DeviceMemory);
                DeviceMemory = null;
            }

            if (Image != null)
            {
                _device.DestroyImage(Image);
                Image = null;
            }
                
        //    public Image Image;
        //public ImageView ImageView;
        //public DeviceMemory DeviceMemory;
    }
    }
}
