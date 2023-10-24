using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{

    /// <summary>
    /// A Vulkan Semaphore
    /// </summary>
    public partial class Semaphore : IDisposable, INonDispatchableHandleMarshalling
    {
        private Device _device;

        internal Semaphore(Device device)
        {
            _device = device;

            if (device != null)
                device.Semaphores.Add(this);
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
                _device.DestroySemaphore(this);
                _device = null;
                m = 0;
            }
        }
    }
}
