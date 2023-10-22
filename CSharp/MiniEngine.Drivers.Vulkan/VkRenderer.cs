using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MiniEngine.Drivers.Vulkan.VkInstance;

namespace MiniEngine.Drivers.Vulkan
{
    public class VkRenderer : IDisposable
    {
        public VkInstance vk;
        public SurfaceKhr Surface;
        public PhysicalDevice PhysicalDevice;
        public Device Device;
        public Queue Queue;
        public SwapchainKhr Swapchain;
        public Image[] SwapChainImages;
        public ImageView[] SwapChainImagesView;
        public RenderPass RenderPass;
        public Framebuffer[] Framebuffers;
        public Fence Fence;
        public Semaphore Semaphore;
        public Extent2D CurrentExtent;
        public CommandPool CommandPool;
        public CommandBuffer[] CommandBuffers;


        public List<VkMeshRenderer> MeshRenderers = new List<VkMeshRenderer>();

        /// <summary>
        /// Constructor
        /// </summary>
        public VkRenderer(string applicationName, Func<VkInstance, SurfaceKhr> surfaceCreationCallback, DebugReportCallback debugCallback)
        {
            vk = VkBootstrapper.CreateInstance(applicationName, debugCallback);

            Surface = surfaceCreationCallback(vk);

            PhysicalDevice = VkBootstrapper.PickPhysicalDevice(vk);

            var surfaceCapabilities = PhysicalDevice.GetSurfaceCapabilitiesKHR(Surface);
            CurrentExtent = surfaceCapabilities.CurrentExtent;

            Device = VkBootstrapper.CreateDevice(PhysicalDevice, Surface);

            Queue = Device.GetQueue(0, 0);

            var surfaceFormat = PhysicalDevice.GetSurfaceFormat(Surface, new Format[] { Format.B8G8R8A8Srgb }, new ColorSpaceKhr[] { ColorSpaceKhr.SrgbNonlinear });
            Swapchain = Device.CreateSwapchain(Surface, surfaceCapabilities, surfaceFormat, PresentModeKhr.Mailbox);
            SwapChainImages = Device.GetSwapchainImagesKHR(Swapchain);
            SwapChainImagesView = Device.CreateImageViews(SwapChainImages, surfaceFormat);

            RenderPass = Device.CreateRenderPass(surfaceFormat);
            Framebuffers = Device.CreateFramebuffers(RenderPass, SwapChainImagesView, CurrentExtent);

            Fence = Device.CreateFence();
            Semaphore = Device.CreateSemaphore();

            var createPoolInfo = new CommandPoolCreateInfo { Flags = CommandPoolCreateFlags.ResetCommandBuffer };
            CommandPool = Device.CreateCommandPool(createPoolInfo);

            var commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                Level = CommandBufferLevel.Primary,
                CommandPool = CommandPool,
                CommandBufferCount = (uint)SwapChainImages.Length
            };

            CommandBuffers = Device.AllocateCommandBuffers(commandBufferAllocateInfo);

        }

        /// <summary>
        /// Destruction
        /// </summary>
        public void Dispose()
        {
            if (vk == null)
                return;

            if (CommandPool != null)
            {
                Device.DestroyCommandPool(CommandPool);
                CommandPool = null;
            }

            if (Fence != null)
            {
                Device.DestroyFence(Fence);
                Fence = null;
            }

            if (Semaphore != null)
            {
                Device.DestroySemaphore(Semaphore);
                Semaphore = null;
            }

            if (Framebuffers != null)
            {
                foreach (Framebuffer framebuffer in Framebuffers)
                    Device.DestroyFramebuffer(framebuffer);
                Framebuffers = null;
            }

            if (RenderPass != null)
            {
                Device.DestroyRenderPass(RenderPass);
                RenderPass = null;
            }

            if (SwapChainImagesView != null)
            {
                foreach (ImageView imageView in SwapChainImagesView)
                    Device.DestroyImageView(imageView);
                SwapChainImagesView = null;
            }

            if (Swapchain != null)
            {
                Device.DestroySwapchainKHR(Swapchain);
                Swapchain = null;
            }

            if (Device != null)
            {
                Device.Destroy();
                Device = null;
            }

            if (Surface != null)
            {
                vk.DestroySurfaceKHR(Surface);
                Surface = null;
            }

            vk.Dispose();
            vk = null;
        }

        public void DrawFrame()
        {
            uint nextImageIndex = Device.AcquireNextImageKHR(Swapchain, ulong.MaxValue, Semaphore);
            Device.ResetFence(Fence);



            CommandBuffer commandBuffer = CommandBuffers[nextImageIndex];

            commandBuffer.Begin();
            var renderPassBeginInfo = new RenderPassBeginInfo
            {
                Framebuffer = Framebuffers[nextImageIndex],
                RenderPass = RenderPass,
                //ClearValues = new ClearValue[] { new ClearValue { Color = new ClearColorValue(new float[] { DateTime.Now.Millisecond % 100f / 100f, 0.87f, 0.75f, 1.0f }) } },
                ClearValues = new ClearValue[] { new ClearValue { Color = new ClearColorValue(new float[] { 0f, 0.87f, 0.75f, 1.0f }) } },
                RenderArea = new Rect2D { Extent = CurrentExtent }
            };
            commandBuffer.CmdBeginRenderPass(renderPassBeginInfo, SubpassContents.Inline);


            foreach (var meshRenderer in MeshRenderers)
                meshRenderer.PopulateCommandBuffers(commandBuffer);

            commandBuffer.CmdEndRenderPass();
            commandBuffer.End();

            var submitInfo = new SubmitInfo
            {
                WaitSemaphores = new Semaphore[] { Semaphore },
                WaitDstStageMask = new PipelineStageFlags[] { PipelineStageFlags.AllGraphics },
                CommandBuffers = new CommandBuffer[] { commandBuffer }
            };
            Queue.Submit(submitInfo, Fence);
            Device.WaitForFence(Fence, true, 100000000);
            var presentInfo = new PresentInfoKhr
            {
                Swapchains = new SwapchainKhr[] { Swapchain },
                ImageIndices = new uint[] { nextImageIndex }
            };
            Queue.PresentKHR(presentInfo);
        }

        /// <summary>
        /// Create a buffer on the GPU
        /// </summary>
        public VkBuffer CreateBufferOnGPU<T>(T[] values, BufferUsageFlags usageFlags)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (VkBuffer stagingBuffer = Device.CreateBuffer(values, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                //Create a buffer on the GPU..
                VkBuffer gpuBuffer = Device.CreateBuffer(stagingBuffer.Size, BufferUsageFlags.TransferDst | usageFlags, MemoryPropertyFlags.DeviceLocal);

                //Copy the data to the GPU...
                CopyBuffer(stagingBuffer, gpuBuffer);

                return gpuBuffer;
            }


        }

        /// <summary>
        /// Copy a buffer
        /// </summary>
        public void CopyBuffer(VkBuffer bufferSource, VkBuffer bufferDest)
        {
            
            var commandBuffer = Device.AllocateCommandBuffer(CommandPool);

            commandBuffer.Begin(CommandBufferUsageFlags.OneTimeSubmit);

            BufferCopy copyRegion = new()
            {
                Size = bufferSource.Size,
            };

            commandBuffer.CmdCopyBuffer(bufferSource.Buffer, bufferDest.Buffer, copyRegion);

            commandBuffer.End();

            Queue.Submit(commandBuffer);
            Queue.WaitIdle();


            Device.FreeCommandBuffer(CommandPool, commandBuffer);
        }


    }
}
