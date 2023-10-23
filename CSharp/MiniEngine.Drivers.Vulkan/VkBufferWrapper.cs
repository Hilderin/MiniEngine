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
    public class VkBufferWrapper: IDisposable
    {
        public int Size;
        public VkDevice Device;
        public VkBuffer Buffer;
        public VkDeviceMemory DeviceMemory;

        public VkBufferWrapper(VkDevice device, VkBuffer buffer, VkDeviceMemory deviceMemory, int length)
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
        public static implicit operator VkBuffer(VkBufferWrapper buffer) { return buffer.Buffer; }

        /// <summary>
        /// Implicit conversion to a DeviceMemory
        /// </summary>
        public static implicit operator VkDeviceMemory(VkBufferWrapper buffer) { return buffer.DeviceMemory; }

    }
}
