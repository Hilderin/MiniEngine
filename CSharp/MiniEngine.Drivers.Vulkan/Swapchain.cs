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
    public class Swapchain: IDisposable
    {
        private Device _device;

        public Queue Queue;
        public Fence Fence;
        public SwapchainKhr SwapchainKhr;
        public Image[] SwapChainImages;
        public ImageView[] SwapChainImagesView;
        public Framebuffer[] Framebuffers;
        public Semaphore Semaphore;
        public SurfaceFormatKhr SurfaceFormat;

        /// <summary>
        /// Constructor
        /// </summary>
        public Swapchain(Device device, SwapchainKhr swapchainKhr, SurfaceFormatKhr surfaceFormat, PresentModeKhr presentMode)
        {
            _device = device;

            _device.Swapchains.Add(this);

            SwapchainKhr = swapchainKhr;
            SurfaceFormat = surfaceFormat;

            SwapChainImages = device.GetSwapchainImagesKHR(SwapchainKhr);
            SwapChainImagesView = device.CreateImageViews(SwapChainImages, SurfaceFormat);

            Queue = _device.GetQueue(0, 0);
            Fence = _device.CreateFence();
            Semaphore = _device.CreateSemaphore();

        }

        /// <summary>
        /// Create the render pass
        /// </summary>
        public RenderPass CreateRenderPass()
        {
            //TODO: Remettre le Depth test
            var attDesc = new AttachmentDescription
            {
                Format = SurfaceFormat.Format,
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

            RenderPass renderPass = _device.CreateRenderPass(renderPassCreateInfo);

            //Now that we have our render pass, we can init the framebuffers...
            InitFramebuffers(renderPass);

            return renderPass;
        }


        /// <summary>
        /// Create the frame buffers for a render pass
        /// </summary>
        public void InitFramebuffers(RenderPass renderPass)
        {
            Framebuffers = _device.CreateFramebuffers(renderPass, SwapChainImagesView, _device.CurrentExtent);
        }

        /// <summary>
        /// Acquire next image
        /// </summary>
        public uint AcquireNextImage()
        {
            uint nextImageIndex = _device.AcquireNextImageKHR(SwapchainKhr, ulong.MaxValue, Semaphore);
            return nextImageIndex;
        }

        /// <summary>
        /// Present the rendered image...
        /// </summary>
        public void Present(CommandBuffer commandBuffer, uint imageIndex)
        {
            //Execute the command buffer...
            // submit the commandbuffer to the queue
            // the command will begin only when the semaphore is available
            // the queue will wait until the fence is up.
            // the fence will be up when the commandbuffer is all done.
            Fence.Reset();
            var submitInfo = new SubmitInfo
            {
                WaitSemaphores = new Semaphore[] { Semaphore },
                WaitDstStageMask = new PipelineStageFlags[] { PipelineStageFlags.AllGraphics },
                CommandBuffers = new CommandBuffer[] { commandBuffer }
            };
            Queue.Submit(submitInfo, Fence);
            Queue.WaitIdle();
            Fence.Wait();


            //And we show the image on the surface...
            var presentInfo = new PresentInfoKhr
            {
                Swapchains = new SwapchainKhr[] { SwapchainKhr },
                ImageIndices = new uint[] { imageIndex },
            };
            Queue.PresentKHR(presentInfo);
        }

        /// <summary>
        /// Dispose of the SwapChain
        /// </summary>
        public void Dispose()
        {
            if (Framebuffers != null)
            {
                foreach (Framebuffer framebuffer in Framebuffers)
                    _device.DestroyFramebuffer(framebuffer);
                Framebuffers = null;
            }

            if (SwapChainImagesView != null)
            {
                foreach (ImageView imageView in SwapChainImagesView)
                    _device.DestroyImageView(imageView);
                SwapChainImagesView = null;
            }

            if (SwapchainKhr != null)
            {
                _device.DestroySwapchainKHR(SwapchainKhr);
                SwapchainKhr = null;
            }

            _device.Swapchains.Remove(this);
        }
    }
}
