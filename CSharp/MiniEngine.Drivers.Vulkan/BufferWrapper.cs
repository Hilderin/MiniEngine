using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public uint Size;        
        public Buffer Buffer;
        public DeviceMemory DeviceMemory;

        public BufferWrapper(Device device, Buffer buffer, DeviceMemory deviceMemory, uint size)
        {
            _device = device;
            Buffer = buffer;
            DeviceMemory = deviceMemory;
            Size = size;
        }

        /// <summary>
        /// Copy data to a buffer from values
        /// </summary>
        public unsafe void UpdateFrom<T>(T[] values)
        {
            Type type = typeof(T);
            var size = System.Runtime.InteropServices.Marshal.SizeOf(type) * values.Length;

            var memPtr = _device.MapMemory(DeviceMemory, 0, size, 0);

            //Copy to the memPtr location...
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            fixed (T* ptr = &values[0])
            {
                System.Buffer.MemoryCopy((void*)ptr, (void*)memPtr, size, size);
            }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            //values.AsSpan().CopyTo(new Span<T>((void*)memPtr, values.Length));

            _device.UnmapMemory(DeviceMemory);
        }

        /// <summary>
        /// Copy data to a buffer from value
        /// </summary>
        public unsafe void UpdateFrom<T>(T value)
        {
               
            IntPtr dataPtr = _device.MapMemory(DeviceMemory, 0, Unsafe.SizeOf<T>());
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            *((T*)dataPtr) = value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            //new Span<T>((void*)dataPtr, 1)[0] = value;
            _device.UnmapMemory(DeviceMemory);

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
