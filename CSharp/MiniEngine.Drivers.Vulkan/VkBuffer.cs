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
    public class VkBuffer: IDisposable
    {
        public int Size;
        public Device Device;
        public Buffer Buffer;
        public DeviceMemory DeviceMemory;

        public VkBuffer(Device device, Buffer buffer, DeviceMemory deviceMemory, int length)
        {
            Device = device;
            Buffer = buffer;
            DeviceMemory = deviceMemory;
            Size = length;
        }

        public void Dispose()
        {
            if (DeviceMemory != null)
            {
                Device.FreeMemory(DeviceMemory);
                DeviceMemory = null;
            }

            if (Buffer != null)
            {
                Device.DestroyBuffer(Buffer);
                Buffer = null;
            }


        }

        /// <summary>
        /// Implicit conversion to a Buffer
        /// </summary>
        public static implicit operator Buffer(VkBuffer buffer) { return buffer.Buffer; }

        /// <summary>
        /// Implicit conversion to a DeviceMemory
        /// </summary>
        public static implicit operator DeviceMemory(VkBuffer buffer) { return buffer.DeviceMemory; }

    }
}
