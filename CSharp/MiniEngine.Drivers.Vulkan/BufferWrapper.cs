using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Encapsulate a Buffer and his BufferMemory
    /// </summary>
    public class BufferWrapper: IDisposable
    {
        private Device _device;
        
        public uint Size { get; private set; }
        public BufferUsageFlags UsageFlags { get; private set; }
        public MemoryPropertyFlags MemoryPropertyFlags { get; private set; }
        public Buffer Buffer { get; private set; }
        public DeviceMemory DeviceMemory { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BufferWrapper(Device device, uint size, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags = MemoryPropertyFlags.HostVisible)
        {
            _device = device;

            UsageFlags = usageFlags;
            Size = size;
            MemoryPropertyFlags = memoryPropertyFlags;

            CreateInternalObjects();
        }

        /// <summary>
        /// Copy data to a buffer from a pointer
        /// </summary>
        public unsafe void UpdateFrom(IntPtr srcPtr, uint size)
        {
            UpdateFrom((void*)srcPtr, 0, size);
        }

        /// <summary>
        /// Copy data to a buffer from a pointer
        /// </summary>
        public unsafe void Update(IntPtr srcPtr, uint destOffset, uint size)
        {
            UpdateFrom((void*)srcPtr, destOffset, size);
        }

        /// <summary>
        /// Copy data to a buffer from a pointer
        /// </summary>
        public unsafe void UpdateFrom(void* srcPtr, uint size)
        {
            UpdateFrom(srcPtr, 0, size);
        }

        /// <summary>
        /// Copy data to a buffer from a pointer at an offset
        /// </summary>
        public unsafe void UpdateFrom(void* srcPtr, uint destOffset, uint size)
        {
            var memPtr = _device.MapMemory(DeviceMemory, destOffset, size, 0);

            System.Buffer.MemoryCopy((void*)srcPtr, (void*)memPtr, size, size);

            _device.UnmapMemory(DeviceMemory);
        }

        /// <summary>
        /// Copy data to a buffer from values
        /// </summary>
        public unsafe void Update<T>(T[] values)
        {
            Type type = typeof(T);
            var size = System.Runtime.InteropServices.Marshal.SizeOf(type) * values.Length;

            //Copy to the memPtr location...
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            fixed (T* ptr = &values[0])
            {
                UpdateFrom((void*)ptr, (uint)size);
            }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            //values.AsSpan().CopyTo(new Span<T>((void*)memPtr, values.Length));

        }

        /// <summary>
        /// Copy data to a buffer from value
        /// </summary>
        public unsafe void Update<T>(ref T value)
        {
               
            IntPtr dataPtr = _device.MapMemory(DeviceMemory, 0, Unsafe.SizeOf<T>());
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            *((T*)dataPtr) = value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            //new Span<T>((void*)dataPtr, 1)[0] = value;
            _device.UnmapMemory(DeviceMemory);

        }

        /// <summary>
        /// Resize the vertex buffer...
        /// </summary>
        public void Resize(int size)
        {
            Resize((uint)size);
        }

        /// <summary>
        /// Resize the vertex buffer...
        /// </summary>
        public void Resize(uint size)
        {
            DestroyInternalObjects();

            Size = size;

            CreateInternalObjects();
        }

        /// <summary>
        /// Dispose the buffer
        /// </summary>
        public void Dispose()
        {
            DestroyInternalObjects();

        }

        /// <summary>
        /// Create the internal objects
        /// </summary>
        private void CreateInternalObjects()
        {
            using (var createBufferInfo = new BufferCreateInfo
            {
                Size = Size,
                Usage = UsageFlags,
                SharingMode = SharingMode.Exclusive,
                QueueFamilyIndices = new uint[] { 0 }
            })
            {
                Buffer = _device.CreateBuffer(createBufferInfo);
            }

            DeviceMemory = _device.CreateDeviceMemory(Buffer, MemoryPropertyFlags);

            _device.BindBufferMemory(Buffer, DeviceMemory, 0);
        }

        /// <summary>
        /// Destroy internal objects
        /// </summary>
        private void DestroyInternalObjects()
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
