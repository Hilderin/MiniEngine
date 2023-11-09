using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Buffer = MiniEngine.Drivers.Vulkan.Buffer;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Encapsulate a Buffer and his BufferMemory
    /// </summary>
    public class BufferWrapper : IDisposable
    {
        private VkRenderer _renderer;
        private Device _device;
        private uint _position;

        public uint Size { get; private set; }
        public BufferUsageFlags UsageFlags { get; private set; }
        public MemoryPropertyFlags MemoryPropertyFlags { get; private set; }
        public Buffer Buffer { get; private set; }
        public DeviceMemory DeviceMemory { get; private set; }
        public bool IsOnGPU { get { return MemoryPropertyFlags.HasFlag(MemoryPropertyFlags.DeviceLocal); } }

        /// <summary>
        /// Constructor
        /// </summary>
        public BufferWrapper(VkRenderer renderer, uint size, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            ValidateBufferFlags(usageFlags, memoryPropertyFlags);

            _renderer = renderer;
            _device = renderer.Device;

            UsageFlags = usageFlags;
            Size = size;
            MemoryPropertyFlags = memoryPropertyFlags;

            CreateInternalObjects();
        }

        /// <summary>
        /// Create a buffer with data
        /// </summary>
        public static BufferWrapper Create<T>(VkRenderer renderer, T[] values, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            ValidateBufferFlags(usageFlags, memoryPropertyFlags);

            Type type = typeof(T);
            var size = System.Runtime.InteropServices.Marshal.SizeOf(type) * values.Length;

            BufferWrapper buffer = new BufferWrapper(renderer, (uint)size, usageFlags, memoryPropertyFlags);

            buffer.Update(values);

            return buffer;
        }

        /// <summary>
        /// Copy data to a buffer from a pointer
        /// </summary>
        public unsafe void Update(IntPtr srcPtr, uint size)
        {
            Update((void*)srcPtr, 0, size);
        }

        /// <summary>
        /// Copy data to a buffer from a pointer
        /// </summary>
        public unsafe void Update(IntPtr srcPtr, uint destOffset, uint size)
        {
            Update((void*)srcPtr, destOffset, size);
        }

        /// <summary>
        /// Copy data to a buffer from a pointer
        /// </summary>
        public unsafe void Update(void* srcPtr, uint size)
        {
            Update(srcPtr, 0, size);
        }

        /// <summary>
        /// Copy data to a buffer from a pointer at an offset
        /// </summary>
        public unsafe void Update(void* srcPtr, uint destOffset, uint size)
        {
            if (IsOnGPU)
            {
                //Copy to GPU...
                CopyValuesToGPU(srcPtr, destOffset, size);
            }
            else
            {
                var memPtr = _device.MapMemory(DeviceMemory, destOffset, size, 0);

                System.Buffer.MemoryCopy((void*)srcPtr, (void*)memPtr, size, size);

                _device.UnmapMemory(DeviceMemory);
            }

            _position = destOffset + size;
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
                Update((void*)ptr, (uint)size);
            }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            //values.AsSpan().CopyTo(new Span<T>((void*)memPtr, values.Length));

            _position = (uint)size;
        }

        /// <summary>
        /// Copy data to a buffer from value
        /// </summary>
        public unsafe void Update<T>(ref T value)
        {
            if (IsOnGPU)
                throw new NotSupportedException("Update of one value on device local buffer (to GPU) is not supported");

            int size = Unsafe.SizeOf<T>();

            IntPtr dataPtr = _device.MapMemory(DeviceMemory, 0, size);
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            *((T*)dataPtr) = value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            //new Span<T>((void*)dataPtr, 1)[0] = value;
            _device.UnmapMemory(DeviceMemory);

            _position = (uint)size;

        }

        /// <summary>
        /// Append data to the buffer and return the start index
        /// </summary>
        public unsafe uint Append<T>(T[] values)
        {
            Type type = typeof(T);
            var size = System.Runtime.InteropServices.Marshal.SizeOf(type) * values.Length;

            uint startIndex;
            lock (this)
            {
                startIndex = _position;
                _position += (uint)size;
            }

            //Copy to the memPtr location...
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            fixed (T* ptr = &values[0])
            {
                Update(ptr, startIndex, (uint)size);
            }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

            return startIndex;

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
        /// Create a buffer on the GPU
        /// </summary>
        private unsafe void CopyValuesToGPU(void* srcPtr, uint destOffset, uint size)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (BufferWrapper stagingBuffer = new BufferWrapper(_renderer, size, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                stagingBuffer.Update(srcPtr, 0, size);

                _renderer.MemoryManager.ExecuteOnTransferQueue(commandBuffer =>
                {

                    BufferCopy copyRegion = new()
                    {
                        SrcOffset = 0,
                        DstOffset = destOffset,
                        Size = stagingBuffer.Size,
                    };

                    commandBuffer.CmdCopyBuffer(stagingBuffer.Buffer, this.Buffer, copyRegion);

                });

            }
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
        /// Validate buffer flags
        /// </summary>
        private static void ValidateBufferFlags(BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            if (memoryPropertyFlags.HasFlag(MemoryPropertyFlags.DeviceLocal) && !usageFlags.HasFlag(BufferUsageFlags.TransferDst))
                throw new InvalidOperationException("Impossible to create a buffer DeviceLocal (on GPU) without the usageFlags TransferDst. The buffer needs to be TransferDst to transfert data from Host memory to Device Memory.");
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
