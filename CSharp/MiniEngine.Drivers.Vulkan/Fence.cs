using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// A Vulkan Fence
    /// </summary>
    public partial class Fence : INonDispatchableHandleMarshalling, IDisposable
    {
        private Device _device;

        internal Fence(Device device)
        {
            _device = device;
        }

        internal UInt64 m;

        UInt64 INonDispatchableHandleMarshalling.Handle
        {
            get
            {
                return m;
            }
        }

        public void Reset()
        {
            _device.ResetFence(this);
        }

        public void Wait(UInt64 timeout)
        {
            _device.WaitForFence(this, true, timeout);
        }

        public void Wait()
        {
            _device.WaitForFence(this, true, 100000000);
        }

        public void Dispose()
        {
            if (_device != null && m != 0)
            {
                _device.DestroyFence(this);
                _device = null;
                m = 0;
            }
        }
    }
}
