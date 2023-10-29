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
    public class SwapchainWrapper: IDisposable
    {
        private Device _device;

        private Queue _queue;
        private Fence _fence;
        private SwapchainKhr _swapchainKhr;
        private Image[] _swapchainImages;
        private ImageView[] _swapchainImagesView;
        private Framebuffer[] _framebuffers;
        private Semaphore _semaphore;
        private SurfaceFormatKhr _surfaceFormat;
        private PresentModeKhr _presentMode;
        private RenderPass _renderPass;
        private CommandPool _commandPool;
        private RenderCommandBuffer[] _renderCommandBuffers;
        private int _indexNextImage;
        private List<PipelineWrapper> _pipelines = new List<PipelineWrapper>();
        private bool _windowSizeChanged = false;
        private Extent2D _currentExtent;

        

        public Device Device => _device;
        public RenderPass RenderPass => _renderPass;
        public Framebuffer[] Framebuffers => _framebuffers;
        public Extent2D CurrentExtent => _currentExtent;

        public int IndexNextImage => _indexNextImage;


        /// <summary>
        /// Create a swapchain
        /// </summary>
        public SwapchainWrapper(Device device, Format[] expectedFormats, ColorSpaceKhr[] expectedColorSpaces, PresentModeKhr presentMode)
        {
            _device = device;

            _surfaceFormat = _device.PhysicalDevice.GetSurfaceFormat(_device.Surface, expectedFormats, expectedColorSpaces);

            _presentMode = presentMode;

            Init();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SwapchainWrapper(Device device, SurfaceFormatKhr surfaceFormat, PresentModeKhr presentMode)
        {
            _device = device;
                        
            _surfaceFormat = surfaceFormat;
            _presentMode = presentMode;

            Init();


        }

        /// <summary>
        /// Notification from the exterior that the window as resized
        /// </summary>
        public void NotifyWindowResized()
        {
            _windowSizeChanged = true;
        }


        /// <summary>
        /// Creates a pipeline wrapper
        /// </summary>
        public PipelineWrapper CreatePipelineWrapper(ShaderWrapper shader)
        {
            PipelineWrapper pipelineWrapper = new PipelineWrapper(_device, this, shader);

            _pipelines.Add(pipelineWrapper);

            return pipelineWrapper;
        }

        /// <summary>
        /// Get the next render command buffer
        /// </summary>
        public RenderCommandBuffer GetNextRenderCommandBuffer()
        {
            _indexNextImage = AcquireNextImage();

            return _renderCommandBuffers[_indexNextImage];

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
            _fence.Reset();
            var submitInfo = new SubmitInfo
            {
                WaitSemaphores = new Semaphore[] { _semaphore },
                WaitDstStageMask = new PipelineStageFlags[] { PipelineStageFlags.AllGraphics },
                CommandBuffers = new CommandBuffer[] { commandBuffer }
            };
            _queue.Submit(submitInfo, _fence);
            _queue.WaitIdle();
            _fence.Wait();


            //And we show the image on the surface...
            var presentInfo = new PresentInfoKhr
            {
                Swapchains = new SwapchainKhr[] { _swapchainKhr },
                ImageIndices = new uint[] { (uint)_indexNextImage },
            };

            var result = _queue.PresentKHRReturnsResult(presentInfo);
            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || _windowSizeChanged)
            {
                //The screen was resized...
                //Update the suface capability and current extend in the device...
                _device.UpdateSurfaceCapabilities();

                //And now that we have the new screensize in memory... let's recreate the swapchain
                Rebuild();

                _windowSizeChanged = false;
            }

        }

        /// <summary>
        /// Recreate the swapchain
        /// </summary>
        public void Rebuild()
        {
            DestroySwapChainObjects();
            CreateSwapChainObjects();

            //Rebuilding pipelines...
            foreach (var pipeline in _pipelines)
                pipeline.Rebuild();

        }

        /// <summary>
        /// Dispose of the Swapchain
        /// </summary>
        public void Dispose()
        {

            DestroySwapChainObjects();


            if (_renderPass != null)
            {
                _renderPass.Dispose();
                _renderPass = null;
            }


            if (_commandPool != null)
            {

                foreach (RenderCommandBuffer renderCommandBuffer in _renderCommandBuffers)
                    _device.FreeCommandBuffer(_commandPool, renderCommandBuffer);

                _commandPool.Dispose();
                _commandPool = null;
            }

        }

        /// <summary>
        /// Remove the pipeline from the pipeline that we are watching
        /// </summary>
        internal void RemovePipelineWrapper(PipelineWrapper pipelineWrapper)
        {
            _pipelines.Remove(pipelineWrapper);
        }


        /// <summary>
        /// Initialize the swapchain
        /// </summary>
        private void Init()
        {
            var presentModes = _device.PhysicalDevice.GetSurfacePresentModesKHR(_device.Surface);

            if (!presentModes.Contains(_presentMode))
                throw new NotSupportedException($"Present mode not supported by the surface: {_presentMode}");

            _queue = _device.GetQueue(0, 0);
            _fence = _device.CreateFence();
            _semaphore = _device.CreateSemaphore();

            CreateRenderPass();

            _commandPool = _device.CreateCommandPool(CommandPoolCreateFlags.ResetCommandBuffer);

            CreateSwapChainObjects();

            //Now that we kwon the number of images...
            _renderCommandBuffers = CreateRenderCommandBuffers();
        }


        /// <summary>
        /// Create the internal objects
        /// </summary>
        private void CreateSwapChainObjects()
        {
            _currentExtent = _device.SurfaceCapabilities.CurrentExtent;

            var compositeAlpha = _device.SurfaceCapabilities.SupportedCompositeAlpha.HasFlag(CompositeAlphaFlagsKhr.Inherit)
                ? CompositeAlphaFlagsKhr.Inherit
                : CompositeAlphaFlagsKhr.Opaque;

            var swapchainInfo = new SwapchainCreateInfoKhr
            {
                Surface = _device.Surface,
                MinImageCount = _device.SurfaceCapabilities.MinImageCount,
                ImageFormat = _surfaceFormat.Format,
                ImageColorSpace = _surfaceFormat.ColorSpace,
                ImageExtent = _device.SurfaceCapabilities.CurrentExtent,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                PreTransform = _device.SurfaceCapabilities.CurrentTransform,
                ImageArrayLayers = 1,
                ImageSharingMode = SharingMode.Exclusive,
                QueueFamilyIndices = new uint[] { 0 },
                PresentMode = _presentMode,
                CompositeAlpha = compositeAlpha,
                Clipped = true
            };

            _swapchainKhr = _device.CreateSwapchainKHR(swapchainInfo);

            _swapchainImages = _device.GetSwapchainImagesKHR(_swapchainKhr);
            _swapchainImagesView = _device.CreateImageViews(_swapchainImages, _surfaceFormat);

            CreateFramebuffers();



        }



        /// <summary>
        /// Create the render command buffer
        /// </summary>
        /// <returns></returns>
        private RenderCommandBuffer[] CreateRenderCommandBuffers()
        {
            int imageIndex = 0;
            return _commandPool.AllocateCommandBuffers(CommandBufferLevel.Primary, _swapchainImages.Length, () => new RenderCommandBuffer(this, imageIndex++));
        }

        /// <summary>
        /// Create the render pass
        /// </summary>
        private void CreateRenderPass()
        {
            //TODO: Remettre le Depth test
            var attDesc = new AttachmentDescription
            {
                Format = _surfaceFormat.Format,
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

            _renderPass = _device.CreateRenderPass(renderPassCreateInfo, this);
        }



        /// <summary>
        /// Create the frame buffers for a render pass
        /// </summary>
        private void CreateFramebuffers()
        {
            _framebuffers = _device.CreateFramebuffers(_renderPass, _swapchainImagesView, _currentExtent);
        }

        /// <summary>
        /// Acquire next image
        /// </summary>
        private int AcquireNextImage()
        {
            return (int)_device.AcquireNextImageKHR(_swapchainKhr, ulong.MaxValue, _semaphore);
        }

        /// <summary>
        /// Destroy internal objects
        /// </summary>
        private void DestroySwapChainObjects()
        {
            if (_framebuffers != null)
            {
                foreach (Framebuffer framebuffer in _framebuffers)
                    _device.DestroyFramebuffer(framebuffer);
                _framebuffers = null;
            }

            if (_swapchainImagesView != null)
            {
                foreach (ImageView imageView in _swapchainImagesView)
                    _device.DestroyImageView(imageView);
                _swapchainImagesView = null;
            }

            if (_swapchainKhr != null)
            {
                _device.DestroySwapchainKHR(_swapchainKhr);
                _swapchainKhr = null;
            }

           
        }
    }
}
