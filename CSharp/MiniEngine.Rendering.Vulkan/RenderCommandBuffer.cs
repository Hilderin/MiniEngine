using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    public class RenderCommandBuffer: CommandBuffer
    {
        private SwapchainWrapper _swapchain;
        private int _imageIndex;

        private RenderPassBeginInfo _renderPassBeginInfo;


        /// <summary>
        /// Constructor
        /// </summary>
        public RenderCommandBuffer(SwapchainWrapper swapchain, int imageIndex)
        {
            _swapchain = swapchain;
            _imageIndex = imageIndex;


            List<ClearValue> clearValues = new List<ClearValue>
            {
                //Background...
                new ClearValue
                {
                    Color = new ClearColorValue(new float[] { 0f, 0f, 0f, 1.0f })
                }
            };

            if (swapchain.DepthTest)
            {
                //Reset the depth test texture...
                clearValues.Add(new ClearValue
                {
                    DepthStencil = new ClearDepthStencilValue() { Depth = 1f, Stencil = 0 }
                });
            }

            _renderPassBeginInfo = new RenderPassBeginInfo
            {
                RenderPass = _swapchain.RenderPass,
                ClearValues = clearValues.ToArray()
            };
        }

        /// <summary>
        /// Beginning of the render
        /// </summary>
        public override void Begin()
        {
            base.Begin();

            _renderPassBeginInfo.Framebuffer = _swapchain.Framebuffers[_imageIndex];
            _renderPassBeginInfo.RenderArea = new Rect2D { Extent = _swapchain.CurrentExtent };

            CmdBeginRenderPass(_renderPassBeginInfo, SubpassContents.Inline);
        }


        /// <summary>
        /// End the rendering
        /// </summary>
        public override void End()
        {
            CmdEndRenderPass();

            base.End();

        }
    }
}
