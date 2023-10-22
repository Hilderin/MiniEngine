using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    public static class DeviceExtensions
    {
        /// <summary>
        /// Create the SwapChain
        /// </summary>
        public static SwapchainKhr CreateSwapchain(this Device device, SurfaceKhr surface, SurfaceCapabilitiesKhr surfaceCapabilities, SurfaceFormatKhr surfaceFormat, PresentModeKhr presentMode)
        {

            var presentModes = device.PhysicalDevice.GetSurfacePresentModesKHR(surface);

            if (!presentModes.Contains(presentMode))
                throw new NotSupportedException($"Present mode not supported by the surface: {presentMode}");

            var compositeAlpha = surfaceCapabilities.SupportedCompositeAlpha.HasFlag(CompositeAlphaFlagsKhr.Inherit)
                ? CompositeAlphaFlagsKhr.Inherit
                : CompositeAlphaFlagsKhr.Opaque;

            var swapchainInfo = new SwapchainCreateInfoKhr
            {
                Surface = surface,
                MinImageCount = surfaceCapabilities.MinImageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = surfaceCapabilities.CurrentExtent,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                PreTransform = surfaceCapabilities.CurrentTransform,
                ImageArrayLayers = 1,
                ImageSharingMode = SharingMode.Exclusive,
                QueueFamilyIndices = new uint[] { 0 },
                PresentMode = presentMode,
                CompositeAlpha = compositeAlpha,
                Clipped = true
            };

            return device.CreateSwapchainKHR(swapchainInfo);
        }



        /// <summary>
        /// Create ImageViews from Images
        /// </summary>
        public static ImageView[] CreateImageViews(this Device device, Image[] images, SurfaceFormatKhr surfaceFormat)
        {
            var displayViews = new ImageView[images.Length];

            for (int i = 0; i < images.Length; i++)
            {
                var viewCreateInfo = new ImageViewCreateInfo
                {
                    Image = images[i],
                    ViewType = ImageViewType.View2D,
                    Format = surfaceFormat.Format,
                    //TODO: à mettre en commentaire???
                    Components = new ComponentMapping
                    {
                        R = ComponentSwizzle.R,
                        G = ComponentSwizzle.G,
                        B = ComponentSwizzle.B,
                        A = ComponentSwizzle.A
                    },
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.Color,
                        LevelCount = 1,
                        LayerCount = 1
                    }
                };
                displayViews[i] = device.CreateImageView(viewCreateInfo);
            }

            return displayViews;
        }

        /// <summary>
        /// Create framebuffers from imageviews
        /// </summary>
        public static Framebuffer[] CreateFramebuffers(this Device device, RenderPass renderPass, ImageView[] displayViews, Extent2D extent)
        {
            var framebuffers = new Framebuffer[displayViews.Length];

            for (int i = 0; i < displayViews.Length; i++)
            {
                var frameBufferCreateInfo = new FramebufferCreateInfo
                {
                    Layers = 1,
                    RenderPass = renderPass,
                    Attachments = new ImageView[] { displayViews[i] },
                    Width = extent.Width,
                    Height = extent.Height
                };
                framebuffers[i] = device.CreateFramebuffer(frameBufferCreateInfo);
            }

            return framebuffers;
        }


        /// <summary>
        /// Create the render pass
        /// </summary>
        public static RenderPass CreateRenderPass(this Device device, SurfaceFormatKhr surfaceFormat)
        {
            //TODO: Remettre le Depth test
            var attDesc = new AttachmentDescription
            {
                Format = surfaceFormat.Format,
                Samples = SampleCountFlags.Count1,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr       //TODO: À voir
                //FinalLayout = ImageLayout.ColorAttachmentOptimal
            };
            var attRef = new AttachmentReference { Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };


            var subpassDesc = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachments = new AttachmentReference[] { attRef }
            };
            var renderPassCreateInfo = new RenderPassCreateInfo
            {
                Attachments = new AttachmentDescription[] { attDesc },
                Subpasses = new SubpassDescription[] { subpassDesc }
            };

            return device.CreateRenderPass(renderPassCreateInfo);
        }

        /// <summary>
        /// Copy data to a buffer
        /// </summary>
        public unsafe static void CopyToBuffer<T>(this Device device, VkBuffer buffer, T[] values)
        {
            Type type = typeof(T);
            var size = System.Runtime.InteropServices.Marshal.SizeOf(type) * values.Length;

            var memPtr = device.MapMemory(buffer.DeviceMemory, 0, size, 0);

            //Copy to the memPtr location...
            values.AsSpan().CopyTo(new Span<T>((void*)memPtr, values.Length));

            device.UnmapMemory(buffer.DeviceMemory);
        }

        /// <summary>
        /// Create a buffer
        /// </summary>
        public unsafe static VkBuffer CreateBuffer<T>(this Device device, T[] values, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags = MemoryPropertyFlags.HostVisible)
        {
            Type type = typeof(T);            
            var size = System.Runtime.InteropServices.Marshal.SizeOf(type) * values.Length;
                
            VkBuffer buffer = CreateBuffer(device, size, usageFlags, memoryPropertyFlags);

            CopyToBuffer(device, buffer, values);

            return buffer;
        }

        /// <summary>
        /// Create a buffer
        /// </summary>
        public unsafe static VkBuffer CreateBuffer(this Device device, int size, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags = MemoryPropertyFlags.HostVisible)
        {
            var createBufferInfo = new BufferCreateInfo
            {
                Size = size,
                Usage = usageFlags,
                SharingMode = SharingMode.Exclusive,
                QueueFamilyIndices = new uint[] { 0 }
            };
            var buffer = device.CreateBuffer(createBufferInfo);

            var deviceMemory = device.CreateDeviceMemory(buffer, memoryPropertyFlags);

            device.BindBufferMemory(buffer, deviceMemory, 0);

            return new VkBuffer(device, buffer, deviceMemory, size);
        }


        /// <summary>
        /// Allocate a DeviceMemory
        /// </summary>
        public static DeviceMemory CreateDeviceMemory(this Device device, Buffer buffer, MemoryPropertyFlags memoryPropertyFlags)
        {
            var memoryReq = device.GetBufferMemoryRequirements(buffer);

            var allocInfo = new MemoryAllocateInfo { AllocationSize = memoryReq.Size };
            var memoryProperties = device.PhysicalDevice.GetMemoryProperties();
            bool heapIndexSet = false;
            var memoryTypes = memoryProperties.MemoryTypes;

            for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if (((memoryReq.MemoryTypeBits >> (int)i) & 1) == 1 &&
                    (memoryTypes[i].PropertyFlags & MemoryPropertyFlags.HostVisible) == MemoryPropertyFlags.HostVisible)
                {
                    allocInfo.MemoryTypeIndex = i;
                    heapIndexSet = true;
                }
            }

            if (!heapIndexSet)
                allocInfo.MemoryTypeIndex = GetMemoryTypeIndex(device, memoryReq.MemoryTypeBits, memoryPropertyFlags);

            return device.AllocateMemory(allocInfo);
        }

        /// <summary>
        /// Find MemoryTypeIndex on a device for memorytype and memoryproperties
        /// </summary>
        public static uint GetMemoryTypeIndex(this Device device, UInt32 memoryTypeBits, MemoryPropertyFlags memoryPropertyFlags)
        {

            var memoryProperties = device.PhysicalDevice.GetMemoryProperties();
            var memoryTypes = memoryProperties.MemoryTypes;

            for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if (((memoryTypeBits >> (int)i) & 1) == 1 &&
                    (memoryTypes[i].PropertyFlags & memoryPropertyFlags) == memoryPropertyFlags)
                {
                    return i;
                }
            }

            //On the heap...
            return memoryProperties.MemoryTypes[0].HeapIndex;
        }
    }
}
