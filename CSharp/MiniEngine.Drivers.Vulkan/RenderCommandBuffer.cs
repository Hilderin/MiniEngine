using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    public class RenderCommandBuffer: CommandBuffer
    {
        private RenderPass _renderPass;
        private int _imageIndex;

        private RenderPassBeginInfo _renderPassBeginInfo;

        public RenderCommandBuffer(RenderPass renderPass, int imageIndex)
        {
            _renderPass = renderPass;
            _imageIndex = imageIndex;

            _renderPassBeginInfo = new RenderPassBeginInfo
            {
                Framebuffer = _renderPass.Swapchain.Framebuffers[imageIndex],
                RenderPass = renderPass,
                //ClearValues = new ClearValue[] { new ClearValue { Color = new ClearColorValue(new float[] { DateTime.Now.Millisecond % 100f / 100f, 0.87f, 0.75f, 1.0f }) } },
                ClearValues = new ClearValue[] { new ClearValue { Color = new ClearColorValue(new float[] { 0f, 0.87f, 0.75f, 1.0f }) } },
                RenderArea = new Rect2D { Extent = renderPass.Device.CurrentExtent }
            };
        }

        /// <summary>
        /// Beginning of the render
        /// </summary>
        public override void Begin()
        {
            base.Begin();


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
