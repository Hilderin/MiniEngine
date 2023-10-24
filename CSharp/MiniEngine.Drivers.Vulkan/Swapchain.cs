using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Represent a High level reprensentation of a Swapchain
    /// </summary>
    public class Swapchain: IDisposable
    {
        private Device _device;

        private Queue Queue;
        private Fence Fence;
        private SwapchainKhr SwapchainKhr;
        private Image[] SwapchainImages;
        private ImageView[] SwapchainImagesView;
        private Framebuffer[] _framebuffers;
        private Semaphore Semaphore;
        private SurfaceFormatKhr SurfaceFormat;

        private RenderPass _renderPass;
        private CommandPool CommandPool;
        private RenderCommandBuffer[] RenderCommandBuffers;

        public RenderPass RenderPass => _renderPass;
        public Framebuffer[] Framebuffers => _framebuffers;


        public int IndexNextImage;

        /// <summary>
        /// Constructor
        /// </summary>
        public Swapchain(Device device, SwapchainKhr swapchainKhr, SurfaceFormatKhr surfaceFormat, PresentModeKhr presentMode)
        {
            _device = device;

            _device.Swapchains.Add(this);

            SwapchainKhr = swapchainKhr;
            SurfaceFormat = surfaceFormat;

            SwapchainImages = device.GetSwapchainImagesKHR(SwapchainKhr);
            SwapchainImagesView = device.CreateImageViews(SwapchainImages, SurfaceFormat);

            Queue = _device.GetQueue(0, 0);
            Fence = _device.CreateFence();
            Semaphore = _device.CreateSemaphore();


            _renderPass = CreateRenderPass();

            CommandPool = device.CreateCommandPool(CommandPoolCreateFlags.ResetCommandBuffer);

            RenderCommandBuffers = CreateRenderCommandBuffers();
        }


        /// <summary>
        /// Create the render command buffer
        /// </summary>
        /// <returns></returns>
        private RenderCommandBuffer[] CreateRenderCommandBuffers()
        {
            int imageIndex = 0;
            return CommandPool.AllocateCommandBuffers(CommandBufferLevel.Primary, SwapchainImages.Length, () => new RenderCommandBuffer(_renderPass, imageIndex++));
        }

        /// <summary>
        /// Create the render pass
        /// </summary>
        protected virtual RenderPass CreateRenderPass()
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

            RenderPass renderPass = _device.CreateRenderPass(renderPassCreateInfo, this);


            //Now that we have our render pass, we can init the framebuffers...
            InitFramebuffers(renderPass);

            return renderPass;
        }


        /// <summary>
        /// Get the next render command buffer
        /// </summary>
        public RenderCommandBuffer GetNextRenderCommandBuffer()
        {
            IndexNextImage = AcquireNextImage();

            return RenderCommandBuffers[IndexNextImage];

        }

        /// <summary>
        /// Create the frame buffers for a render pass
        /// </summary>
        private void InitFramebuffers(RenderPass renderPass)
        {
            _framebuffers = _device.CreateFramebuffers(renderPass, SwapchainImagesView, _device.CurrentExtent);
        }

        /// <summary>
        /// Acquire next image
        /// </summary>
        private int AcquireNextImage()
        {
            return (int)_device.AcquireNextImageKHR(SwapchainKhr, ulong.MaxValue, Semaphore);
        }

        /// <summary>
        /// Present the rendered image...
        /// </summary>
        public void Present(CommandBuffer commandBuffer)
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
                ImageIndices = new uint[] { (uint)IndexNextImage },
            };
            Queue.PresentKHR(presentInfo);
        }

        /// <summary>
        /// Dispose of the Swapchain
        /// </summary>
        public void Dispose()
        {
            if (_framebuffers != null)
            {
                foreach (Framebuffer framebuffer in _framebuffers)
                    _device.DestroyFramebuffer(framebuffer);
                _framebuffers = null;
            }

            if (SwapchainImagesView != null)
            {
                foreach (ImageView imageView in SwapchainImagesView)
                    _device.DestroyImageView(imageView);
                SwapchainImagesView = null;
            }

            if (SwapchainKhr != null)
            {
                _device.DestroySwapchainKHR(SwapchainKhr);
                SwapchainKhr = null;
            }

            if (_renderPass != null)
            {
                _renderPass.Dispose();
                _renderPass = null;
            }


            if (CommandPool != null)
            {

                foreach (RenderCommandBuffer renderCommandBuffer in RenderCommandBuffers)
                    _device.FreeCommandBuffer(CommandPool, renderCommandBuffer);
                
                CommandPool.Dispose();
                CommandPool = null;
            }


            _device.Swapchains.Remove(this);
        }
    }
}
