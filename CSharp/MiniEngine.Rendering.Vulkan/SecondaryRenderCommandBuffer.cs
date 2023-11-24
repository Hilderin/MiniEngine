using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    public class SecondaryRenderCommandBuffer : CommandBuffer
    {
        private SwapchainWrapper _swapchain;
        private int _imageIndex;
        private CommandBufferBeginInfo _beginInfo;
        /// <summary>
        /// Constructor
        /// </summary>
        public SecondaryRenderCommandBuffer(SwapchainWrapper swapchain, int imageIndex, CommandPool commandPool): base(commandPool)
        {
            _swapchain = swapchain;
            _imageIndex = imageIndex;

            _beginInfo = new CommandBufferBeginInfo()
            {
                InheritanceInfo = new CommandBufferInheritanceInfo()
                {
                    RenderPass = _swapchain.RenderPass,
                    Subpass = 0,
                    Framebuffer = _swapchain.Framebuffers[_imageIndex]
                },
                Flags = CommandBufferUsageFlags.RenderPassContinue
            };
        }

        /// <summary>
        /// Beginning of the render
        /// </summary>
        public override void Begin()
        {
            Begin(_beginInfo);
        }


        /// <summary>
        /// End
        /// </summary>
        public override void End()
        {
            base.End();

        }
    }
}
