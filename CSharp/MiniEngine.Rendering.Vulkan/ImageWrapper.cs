using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Advanced;
using MiniEngine.Drivers.Vulkan;
using Buffer = MiniEngine.Drivers.Vulkan.Buffer;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Wrapper for images
    /// </summary>
    public class ImageWrapper : IDisposable
    {
        private VkRenderer _renderer;
        private Device _device;


        public int Width;
        public int Height;
        public Format Format;


        public Image Image;
        public ImageView ImageView;
        public DeviceMemory DeviceMemory;

        /// <summary>
        /// Indicate if the image is loaded and ready to use
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public unsafe ImageWrapper(VkRenderer renderer, byte* data, int width, int height, Format format)
        {
            _renderer = renderer;
            _device = renderer.Device;
            
            Width = width;
            Height = height;
            Format = format;


            uint size = (uint)(Width * Height * FormatHelper.GetFormatSizeBytes(Format));

            Init(data, size);

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageWrapper(VkRenderer renderer, byte[] data, int width, int height, Format format)
        {
            _renderer = renderer;
            _device = renderer.Device;

            Width = width;
            Height = height;
            Format = format;

            unsafe
            {
                fixed (byte* ptrData = &data[0])
                {
                    Init(ptrData, (uint)data.Length);
                }
            }

        }

        /// <summary>
        /// Constructor for an empty image
        /// </summary>
        public unsafe ImageWrapper(VkRenderer renderer, int width, int height, Format format, ImageUsageFlags usageFlags, ImageAspectFlags aspectFlags)
        {
            _renderer = renderer;
            _device = renderer.Device;

            Width = width;
            Height = height;
            Format = format;


            CreateImage(Format, usageFlags, MemoryPropertyFlags.DeviceLocal);


            //We need to create the image view now...
            CreateImageView(aspectFlags);

        }

        /// <summary>
        /// Transferer laytout to graphics queue
        /// </summary>
        public void CmdTransferToGraphicsQueue(CommandBuffer commandBuffer)
        {
            CmdTransferImageLayout(
                   srcAccessMask: 0,
                   dstAccessMask: AccessFlags.ShaderRead, 
                   oldLayout: ImageLayout.Undefined,
                   newLayout: ImageLayout.ShaderReadOnlyOptimal,
                   srcQueueFamilyIndex: _renderer.TransferQueueIndex,
                   dstQueueFamilyIndex: _renderer.GraphicsQueueIndex,                   
                   srcStage: PipelineStageFlags.TopOfPipe,
                   dstStage: PipelineStageFlags.FragmentShader,
                   commandBuffer
                   );
        }

        /// <summary>
        /// Create the image
        /// </summary>
        private unsafe void Init(byte* data, uint size)
        {
            using (BufferWrapper bufferStaging = _renderer.CreateBufferWrapper(size, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible))
            {
                bufferStaging.Update(data, size);

                CreateImage(Format, ImageUsageFlags.TransferDst | ImageUsageFlags.Sampled, MemoryPropertyFlags.DeviceLocal);

                //Making the image ready to be copied to the gpu...
                TransitionImageLayout(
                    oldLayout: ImageLayout.Undefined, 
                    newLayout: ImageLayout.TransferDstOptimal,
                    srcQueueFamilyIndex: uint.MaxValue,
                    dstQueueFamilyIndex: uint.MaxValue,
                    srcAccessMask: 0,
                    dstAccessMask: AccessFlags.TransferWrite,
                    srcStage: PipelineStageFlags.TopOfPipe,
                    dstStage: PipelineStageFlags.Transfer
                    );

                //Copy the image on the GPU...
                CopyBufferToImage(bufferStaging.Buffer);

                //The texture is now on the GPU, but it is still not usable from the main queue.
                //That is why we need another memory barrier that will also transfer ownership from transfer queue to graphics queue
                TransitionImageLayout(
                    oldLayout: ImageLayout.TransferDstOptimal,
                    newLayout: ImageLayout.ShaderReadOnlyOptimal,
                    srcQueueFamilyIndex: _renderer.TransferQueueIndex,
                    dstQueueFamilyIndex: _renderer.GraphicsQueueIndex,
                    srcAccessMask: 0,
                    dstAccessMask: AccessFlags.TransferWrite,
                    srcStage: PipelineStageFlags.Transfer,
                    dstStage: PipelineStageFlags.BottomOfPipe
                    );


                //The image still needed to change layout in the graphics queue...
                IsLoaded = false;
                _renderer.AddActionsBeforeNextFrame(() =>
                {
                    _renderer.GraphicsQueue.ExecuteAndWait(this.CmdTransferToGraphicsQueue);

                    //Now it's ready!
                    IsLoaded = true;
                });
            }

            //We need to create the image view now...
            CreateImageView();
        }


        /// <summary>
        /// Create image
        /// </summary>
        private void CreateImage(Format format, ImageUsageFlags usage, MemoryPropertyFlags properties)
        {
            using (ImageCreateInfo imageInfo = new()
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
            })
            {
                Image = _device.CreateImage(imageInfo);
            }

            DeviceMemory = _device.CreateDeviceMemory(Image, properties);

            _device.BindImageMemory(Image, DeviceMemory, 0);
        }


        /// <summary>
        /// Transition the image layout
        /// </summary>
        private void TransitionImageLayout(
            ImageLayout oldLayout, 
            ImageLayout newLayout, 
            uint srcQueueFamilyIndex,
            uint dstQueueFamilyIndex,
            AccessFlags srcAccessMask,
            AccessFlags dstAccessMask,
            PipelineStageFlags srcStage,
            PipelineStageFlags dstStage)
        {
            _renderer.MemoryManager.ExecuteOnTransferQueue(commandBuffer =>
            {
                CmdTransferImageLayout(srcAccessMask, dstAccessMask, oldLayout, newLayout, srcQueueFamilyIndex, dstQueueFamilyIndex, srcStage, dstStage, commandBuffer);
            });

        }

        /// <summary>
        /// Create the transfert layout command
        /// </summary>
        private void CmdTransferImageLayout(AccessFlags srcAccessMask, AccessFlags dstAccessMask, ImageLayout oldLayout, ImageLayout newLayout, uint srcQueueFamilyIndex, uint dstQueueFamilyIndex, PipelineStageFlags srcStage, PipelineStageFlags dstStage, CommandBuffer commandBuffer)
        {
            using (ImageMemoryBarrier barrier = new()
            {
                SrcAccessMask = srcAccessMask,
                DstAccessMask = dstAccessMask,
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = srcQueueFamilyIndex,
                DstQueueFamilyIndex = dstQueueFamilyIndex,
                Image = Image,
                SubresourceRange = new()
                {
                    AspectMask = ImageAspectFlags.Color,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            })
            {
                commandBuffer.CmdPipelineBarrier(srcStage, dstStage, DependencyFlags.ByRegion, null, null, barrier);
            }
        }

        private void CreateImageView(ImageAspectFlags aspectFlags = ImageAspectFlags.Color)
        {
            using (ImageViewCreateInfo createInfo = new()
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
                    AspectMask = aspectFlags,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

            })
            {
                ImageView = _device.CreateImageView(createInfo);
            }
        }

        /// <summary>
        /// Copy buffer to image
        /// </summary>
        private void CopyBufferToImage(Buffer buffer)
        {
            _renderer.MemoryManager.ExecuteOnTransferQueue(commandBuffer =>
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
        }

        
    }
}
