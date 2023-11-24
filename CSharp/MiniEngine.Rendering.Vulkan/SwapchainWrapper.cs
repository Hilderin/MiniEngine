using MiniEngine.Drivers.Vulkan;
using MiniEngine.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Represent a High level reprensentation of a Swapchain
    /// </summary>
    public class SwapchainWrapper: IDisposable
    {
        private VkRenderer _renderer;
        private Device _device;

        private Queue _queue;
        private Fence _fence;
        private SwapchainKhr _swapchainKhr;
        private Image[] _swapchainImages;
        private ImageView[] _swapchainImagesView;
        private ImageWrapper _depthImage;
        private Framebuffer[] _framebuffers;
        private Semaphore _semaphore;
        private SurfaceFormatKhr _surfaceFormat;
        private PresentModeKhr _presentMode;
        private bool _depthTest;
        private RenderPass _renderPass;
        private CommandPool _commandPool;
        private RenderCommandBuffer[] _renderCommandBuffers;
        private int _indexNextImage;
        private List<PipelineWrapper> _pipelines = new List<PipelineWrapper>();
        private bool _windowSizeChanged = false;
        private Extent2D _currentExtent;

        private SubmitInfo[] _submitInfos;
        private PresentInfoKhr[] _presentInfos;


        private ProfilerStep _queueSubmitStep;
        private ProfilerStep _presentStep;


        public Device Device => _device;
        public RenderPass RenderPass => _renderPass;
        public Framebuffer[] Framebuffers => _framebuffers;
        public Extent2D CurrentExtent => _currentExtent;
        public bool DepthTest => _depthTest;

        public int IndexNextImage => _indexNextImage;
        

        /// <summary>
        /// Create a swapchain
        /// </summary>
        public SwapchainWrapper(VkRenderer renderer, Format[] expectedFormats, ColorSpaceKhr[] expectedColorSpaces, PresentModeKhr presentMode, bool depthTest)
        {
            _renderer = renderer;
            _device = renderer.Device;

            _surfaceFormat = _device.PhysicalDevice.GetSurfaceFormat(_device.Surface, expectedFormats, expectedColorSpaces);
            

            _presentMode = presentMode;
            _depthTest = depthTest;


            Init();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SwapchainWrapper(Device device, SurfaceFormatKhr surfaceFormat, PresentModeKhr presentMode, bool depthTest)
        {
            _device = device;
                        
            _surfaceFormat = surfaceFormat;
            _presentMode = presentMode;
            _depthTest = depthTest;

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
            PipelineWrapper pipelineWrapper = new PipelineWrapper(_renderer, this, shader);

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
        public void Present()
        {
            //Execute the command buffer...
            // submit the commandbuffer to the queue
            // the command will begin only when the semaphore is available
            // the queue will wait until the fence is up.
            // the fence will be up when the commandbuffer is all done.
            _queueSubmitStep?.Begin();
            _fence.Reset();
            _queue.Submit(_submitInfos[_indexNextImage], _fence);
            _queue.WaitIdle();
            _fence.Wait();
            _queueSubmitStep?.End();

            //And we show the image on the surface...
            _presentStep.Begin();
            var result = _queue.PresentKHRReturnsResult(_presentInfos[_indexNextImage]);
            
            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || _windowSizeChanged)
            {
                //The screen was resized...
                //Update the suface capability and current extend in the device...
                _renderer.UpdateSurfaceCapabilities();

                //And now that we have the new screensize in memory... let's recreate the swapchain
                Rebuild();

                _windowSizeChanged = false;
            }

            _presentStep.End();
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

            _queue = _renderer.Device.GetQueue(_renderer.GraphicsQueueIndex, 0);
            _fence = _device.CreateFence();
            _semaphore = _device.CreateSemaphore();


            CreateRenderPass();

            _commandPool = _renderer.CreateGraphicsCommandPool();

            CreateSwapChainObjects();


            _queueSubmitStep = _renderer.FrameProfiler?.AddStep($"{nameof(VkRenderer)}.QueueSubmit");
            _presentStep = _renderer.FrameProfiler?.AddStep($"{nameof(VkRenderer)}.Present");



        }


        /// <summary>
        /// Create the internal objects
        /// </summary>
        private void CreateSwapChainObjects()
        {
            _currentExtent = _renderer.SurfaceCapabilities.CurrentExtent;

            var compositeAlpha = _renderer.SurfaceCapabilities.SupportedCompositeAlpha.HasFlag(CompositeAlphaFlagsKhr.Inherit)
                ? CompositeAlphaFlagsKhr.Inherit
                : CompositeAlphaFlagsKhr.Opaque;

            using (var swapchainInfo = new SwapchainCreateInfoKhr
            {
                Surface = _device.Surface,
                MinImageCount = _renderer.SurfaceCapabilities.MinImageCount,
                ImageFormat = _surfaceFormat.Format,
                ImageColorSpace = _surfaceFormat.ColorSpace,
                ImageExtent = _renderer.SurfaceCapabilities.CurrentExtent,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                PreTransform = _renderer.SurfaceCapabilities.CurrentTransform,
                ImageArrayLayers = 1,
                ImageSharingMode = SharingMode.Exclusive,
                QueueFamilyIndices = new uint[] { 0 },
                PresentMode = _presentMode,
                CompositeAlpha = compositeAlpha,
                Clipped = true
            })
            {
                _swapchainKhr = _device.CreateSwapchainKHR(swapchainInfo);
            }

            _swapchainImages = _device.GetSwapchainImagesKHR(_swapchainKhr);
            _swapchainImagesView = _device.CreateImageViews(_swapchainImages, _surfaceFormat);

            CreateDepthResources();

            CreateFramebuffers();


            //Now that we kwon the number of images...
#pragma warning disable IDE0074 // Use compound assignment
            if (_renderCommandBuffers == null)
                _renderCommandBuffers = CreateRenderCommandBuffers();


                //Creation of the submitinfos and presentinfos so we don't recreate them at each frame...
            _submitInfos = new SubmitInfo[_renderCommandBuffers.Length];
            _presentInfos = new PresentInfoKhr[_renderCommandBuffers.Length];
            for (int i = 0; i < _submitInfos.Length; i++)
            {
                _submitInfos[i] = new SubmitInfo
                {
                    WaitSemaphores = new Semaphore[] { _semaphore },
                    WaitDstStageMask = new PipelineStageFlags[] { PipelineStageFlags.AllGraphics },
                    CommandBuffers = new CommandBuffer[] { _renderCommandBuffers[i] }
                };

                _presentInfos[i] = new PresentInfoKhr
                {
                    Swapchains = new SwapchainKhr[] { _swapchainKhr },
                    ImageIndices = new uint[] { (uint)i },
                };
            }


        }



        /// <summary>
        /// Create the render command buffer
        /// </summary>
        /// <returns></returns>
        private RenderCommandBuffer[] CreateRenderCommandBuffers()
        {
            int imageIndex = 0;
            return _commandPool.AllocateCommandBuffers(CommandBufferLevel.Primary, _swapchainImages.Length, () => new RenderCommandBuffer(this, imageIndex++, _commandPool));
        }

        /// <summary>
        /// Create the render pass
        /// </summary>
        private void CreateRenderPass()
        {

            List<AttachmentDescription> attachementDescs = new List<AttachmentDescription>();
            List<SubpassDependency> subpasseDependencies = new List<SubpassDependency>();


            var colorAttachementDesc = new AttachmentDescription
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
            var colorAttRef = new AttachmentReference { Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };
            attachementDescs.Add(colorAttachementDesc);


            var subpassDesc = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachments = new AttachmentReference[] { colorAttRef }
            };


            //For Depth test...
            if (_depthTest)
            {
                AttachmentDescription depthAttachmentDesc = new()
                {
                    Format = _device.PhysicalDevice.FindDepthFormat(),
                    Samples = SampleCountFlags.Count1,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.DontCare,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.Undefined,
                    FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
                };
                subpassDesc.DepthStencilAttachment = new AttachmentReference { Attachment = 1, Layout = ImageLayout.DepthStencilAttachmentOptimal };


                SubpassDependency dependency = new()
                {
                    SrcSubpass = uint.MaxValue,
                    DstSubpass = 0,
                    SrcStageMask = PipelineStageFlags.ColorAttachmentOutput | PipelineStageFlags.EarlyFragmentTests,
                    SrcAccessMask = 0,
                    DstStageMask = PipelineStageFlags.ColorAttachmentOutput | PipelineStageFlags.EarlyFragmentTests,
                    DstAccessMask = AccessFlags.ColorAttachmentWrite | AccessFlags.DepthStencilAttachmentWrite
                };


                
                attachementDescs.Add(depthAttachmentDesc);
                subpasseDependencies.Add(dependency);
            }

            


            using (var renderPassCreateInfo = new RenderPassCreateInfo
            {
                Attachments = attachementDescs.ToArray(),
                Subpasses = new[] { subpassDesc },
                Dependencies = subpasseDependencies.ToArray()
            })
            {
                _renderPass = _device.CreateRenderPass(renderPassCreateInfo);
            }
        }



        /// <summary>
        /// Create the frame buffers for a render pass
        /// </summary>
        private void CreateFramebuffers()
        {
            _framebuffers = _device.CreateFramebuffers(_renderPass, _swapchainImagesView, _currentExtent, _depthImage?.ImageView);
        }


        /// <summary>
        /// Creation depth resource
        /// </summary>
        private void CreateDepthResources()
        {
            if (_depthTest)
            {
                Format depthFormat = _device.PhysicalDevice.FindDepthFormat();

                _depthImage = new ImageWrapper(_renderer, (int)_currentExtent.Width, (int)_currentExtent.Height, depthFormat, ImageUsageFlags.DepthStencilAttachment, ImageAspectFlags.Depth);
            }

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
            foreach (var submitInfo in _submitInfos)
                submitInfo.Dispose();
            foreach (var presentInfo in _presentInfos)
                presentInfo.Dispose();

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

            if (_depthImage != null)
            {
                _depthImage.Dispose();
                _depthImage = null;
            }

            if (_swapchainKhr != null)
            {
                _device.DestroySwapchainKHR(_swapchainKhr);
                _swapchainKhr = null;
            }

           
        }
    }
}
