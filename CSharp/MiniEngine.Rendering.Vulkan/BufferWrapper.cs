using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        private uint _length;
        
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
            uint size = VkSizeOfHelper.SizeOf<T>(1) * (uint)values.Length;

            BufferWrapper buffer = new BufferWrapper(renderer, size, usageFlags, memoryPropertyFlags);

            buffer.Update(values);

            return buffer;
        }


        /// <summary>
        /// Create a buffer with data
        /// </summary>
        public static BufferWrapper Create<T>(VkRenderer renderer, int count, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            uint size = VkSizeOfHelper.SizeOf<T>() * (uint)count;

            BufferWrapper buffer = new BufferWrapper(renderer, size, usageFlags, memoryPropertyFlags);

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

            if (_length < destOffset + size)
                _length = destOffset + size;
        }

        /// <summary>
        /// Copy data to a buffer from values
        /// </summary>
        public unsafe void Update<T>(T[] values, uint offset = 0)
        {
            Type type = typeof(T);
            var size = SizeOf<T>() * values.Length;

            //Copy to the memPtr location...
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            fixed (T* ptr = &values[0])
            {
                Update((void*)ptr, offset, (uint)size);
            }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

        }


        /// <summary>
        /// Copy data to a buffer from value
        /// </summary>
        public unsafe void Update<T>(ref T value, uint offset = 0)
        {
            uint size = SizeOf<T>();

            if (IsOnGPU)
            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                fixed (T* ptr = &value)
                {
                    CopyValuesToGPU(ptr, offset, size);
                }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            }
            else
            {
                lock (DeviceMemory)
                {
                    IntPtr dataPtr = _device.MapMemory(DeviceMemory, offset, size);
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    *((T*)dataPtr) = value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    //new Span<T>((void*)dataPtr, 1)[0] = value;
                    _device.UnmapMemory(DeviceMemory);
                }

            }

            if (_length < offset + (uint)size)
                _length = offset + (uint)size;

        }

        /// <summary>
        /// Append data to the buffer and return the start index
        /// </summary>
        public unsafe uint Append<T>(T[] values)
        {
            uint size = SizeOf<T>() * (uint)values.Length;

            uint startIndex;
            lock (this)
            {
                startIndex = _length;
                _length += size;
            }

            Update(values, startIndex);

            return startIndex;

        }


        /// <summary>
        /// Append data to the buffer and return the start index
        /// </summary>
        public unsafe uint Append<T>(ref T value)
        {
            uint size = SizeOf<T>();

            uint startIndex;
            lock (this)
            {
                startIndex = _length;
                _length += size;
            }

            Update(ref value, startIndex);

            return startIndex;

        }

        /// <summary>
        /// Type the size of a value aligned in the buffer
        /// </summary>
        public uint SizeOf<T>()
        {
            return VkSizeOfHelper.SizeOf<T>();
        }


        /// <summary>
        /// Copy the buffer to an array
        /// </summary>
        public unsafe void CopyTo<T>(T[] values, uint srcOffset = 0)
        {
            uint size = SizeOf<T>() * (uint)values.Length;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            fixed (T* destPtr = &values[0])
            {
                CopyTo(destPtr, srcOffset, size);
            }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            
        }

        /// <summary>
        /// Copy the buffer to local memory
        /// </summary>
        public unsafe void CopyTo(void* destPtr, uint srcOffset, uint size)
        {
            if (IsOnGPU)
            {
                //We must copy from GPU...
                CopyValuesFromGPU(destPtr, srcOffset, size);
            }
            else
            {
                //Copy directly from memory...
                lock (DeviceMemory)
                {
                    var memPtr = _device.MapMemory(DeviceMemory, srcOffset, size, 0);

                    System.Buffer.MemoryCopy((void*)memPtr, (void*)destPtr, size, size);

                    _device.UnmapMemory(DeviceMemory);
                }
            }

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
        /// Create data from the GPU to local memory
        /// </summary>
        private unsafe void CopyValuesFromGPU(void* destPtr, uint srcOffset, uint size)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (BufferWrapper stagingBuffer = new BufferWrapper(_renderer, size, BufferUsageFlags.TransferDst, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                _renderer.MemoryManager.ExecuteOnTransferQueue(commandBuffer =>
                {

                    BufferCopy copyRegion = new()
                    {
                        SrcOffset = 0,
                        DstOffset = srcOffset,
                        Size = stagingBuffer.Size,
                    };

                    //Copy from GPU buffer to staging buffer...
                    commandBuffer.CmdCopyBuffer(this.Buffer, stagingBuffer.Buffer, copyRegion);

                });

                //Copy from host to local...
                stagingBuffer.CopyTo(destPtr, 0, size);
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
        private void ValidateBufferFlags(BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            if (memoryPropertyFlags.HasFlag(MemoryPropertyFlags.DeviceLocal) && !usageFlags.HasFlag(BufferUsageFlags.TransferDst))
                throw new InvalidOperationException("Impossible to create a buffer DeviceLocal (on GPU) without the usageFlags TransferDst. The buffer needs to be TransferDst to transfert data from Host memory to Device Memory.");
        }

        /// <summary>
        /// Calculate the alignment of bytes when we append to the buffer.
        /// Uniform buffer and Storage buffer has a 16 bytes alignment which means each "row" must have a length dividable by 16
        /// </summary>
        private static uint GetAlignment(BufferUsageFlags usageFlags)
        {
            //if (usageFlags.HasFlag(BufferUsageFlags.UniformBuffer) || usageFlags.HasFlag(BufferUsageFlags.StorageBuffer))
            //    return 16;
            //else
                return 1;
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
