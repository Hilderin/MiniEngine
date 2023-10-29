using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// A Vulkan RenderPass
    /// </summary>
    public partial class RenderPass : IDisposable, INonDispatchableHandleMarshalling
    {
        private Device _device;
        private SwapchainWrapper _swapChain;

        public Device Device => _device;
        public SwapchainWrapper Swapchain => _swapChain;

        internal RenderPass(Device device, SwapchainWrapper swapChain)
        {
            _device = device;
            _swapChain = swapChain;

            if (device != null)
                device.RenderPasses.Add(this);

        }

        internal UInt64 m;

        UInt64 INonDispatchableHandleMarshalling.Handle
        {
            get
            {
                return m;
            }
        }

        public void Dispose()
        {
            if (_device != null && m != 0)
            {
                _device.DestroyRenderPass(this);
                _device = null;
                m = 0;
            }
        }
    }
}
