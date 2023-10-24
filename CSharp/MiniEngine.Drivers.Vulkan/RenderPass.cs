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

        internal RenderPass(Device device)
        {
            _device = device;

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
