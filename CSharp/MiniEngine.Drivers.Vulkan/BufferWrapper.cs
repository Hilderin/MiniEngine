using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Encapsulate a buffer
    /// </summary>
    public class BufferWrapper: IDisposable
    {
        private Device _device;

        public int Size;        
        public Buffer Buffer;
        public DeviceMemory DeviceMemory;

        public BufferWrapper(Device device, Buffer buffer, DeviceMemory deviceMemory, int length)
        {
            _device = device;
            Buffer = buffer;
            DeviceMemory = deviceMemory;
            Size = length;
        }

        public void Dispose()
        {
            if (DeviceMemory != null)
            {
                _device.FreeMemory(DeviceMemory);
                DeviceMemory = null;
            }

            if (Buffer != null)
            {
                _device.DestroyBuffer(Buffer);
                Buffer = null;
            }


        }

        /// <summary>
        /// Implicit conversion to a Buffer
        /// </summary>
        public static implicit operator Buffer(BufferWrapper buffer) { return buffer.Buffer; }

        /// <summary>
        /// Implicit conversion to a DeviceMemory
        /// </summary>
        public static implicit operator DeviceMemory(BufferWrapper buffer) { return buffer.DeviceMemory; }

    }
}
