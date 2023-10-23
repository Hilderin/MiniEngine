using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Represent a High level reprensentation of a SwapChain
    /// </summary>
    public class VkSwapchain: IDisposable
    {
        public VkDevice Device;
        public VkQueue Queue;
        public VkSwapchainKhr SwapchainKhr;
        public VkImage[] SwapChainImages;
        public VkImageView[] SwapChainImagesView;
        public VkFramebuffer[] Framebuffers;
        public VkSemaphore Semaphore;
        public SurfaceFormatKhr SurfaceFormat;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkSwapchain(VkDevice device, VkSwapchainKhr swapchainKhr, SurfaceFormatKhr surfaceFormat, PresentModeKhr presentMode)
        {
            Device = device;

            SwapchainKhr = swapchainKhr;
            SurfaceFormat = surfaceFormat;

            SwapChainImages = device.GetSwapchainImagesKHR(SwapchainKhr);
            SwapChainImagesView = device.CreateImageViews(SwapChainImages, SurfaceFormat);

            Queue = Device.GetQueue(0, 0);
            Semaphore = Device.CreateSemaphore();

        }

        /// <summary>
        /// Create the frame buffers for a render pass
        /// </summary>
        public void InitFramebuffers(VkRenderPass renderPass)
        {
            Framebuffers = Device.CreateFramebuffers(renderPass, SwapChainImagesView, Device.CurrentExtent);
        }

        /// <summary>
        /// Acquire next image
        /// </summary>
        public uint AcquireNextImage()
        {
            uint nextImageIndex = Device.AcquireNextImageKHR(SwapchainKhr, ulong.MaxValue, Semaphore);
            return nextImageIndex;
        }

        /// <summary>
        /// Present the rendered image...
        /// </summary>
        public void Present(uint imageIndex)
        {
            
            var presentInfo = new PresentInfoKhr
            {
                Swapchains = new VkSwapchainKhr[] { SwapchainKhr },
                ImageIndices = new uint[] { imageIndex }
            };
            Queue.PresentKHR(presentInfo);
        }

        /// <summary>
        /// Dispose of the SwapChain
        /// </summary>
        public void Dispose()
        {
            if (Semaphore != null)
            {
                Device.DestroySemaphore(Semaphore);
                Semaphore = null;
            }

            if (Framebuffers != null)
            {
                foreach (VkFramebuffer framebuffer in Framebuffers)
                    Device.DestroyFramebuffer(framebuffer);
                Framebuffers = null;
            }

            if (SwapChainImagesView != null)
            {
                foreach (VkImageView imageView in SwapChainImagesView)
                    Device.DestroyImageView(imageView);
                SwapChainImagesView = null;
            }

            if (SwapchainKhr != null)
            {
                Device.DestroySwapchainKHR(SwapchainKhr);
                SwapchainKhr = null;
            }
        }
    }
}
