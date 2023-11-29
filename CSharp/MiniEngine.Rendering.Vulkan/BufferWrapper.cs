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
        protected VkRenderer _renderer;
        protected Device _device;
        private uint _position;

        public uint Position => _position;

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

            if (_position < destOffset + size)
                SetPosition(destOffset + size);
        }

        

        /// <summary>
        /// Type the size of a value aligned in the buffer
        /// </summary>
        protected uint SizeOf<T>()
        {
            return VkSizeOfHelper.SizeOf<T>();
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
        /// Reserve space into the buffer for a struct and return the start offset
        /// </summary>
        public uint Reserve(uint size)
        {
            uint startIndex;
            lock (this)
            {
                startIndex = _position;
                SetPosition(_position + size);
            }

            return startIndex;

        }

        /// <summary>
        /// Dispose the buffer
        /// </summary>
        public void Dispose()
        {
            DestroyInternalObjects();

        }

        /// <summary>
        /// Set the current position
        /// </summary>
        protected virtual void SetPosition(uint position)
        {

            if (_position < position)
                _position = position;
        }

        /// <summary>
        /// Create a buffer on the GPU
        /// </summary>
        protected unsafe void CopyValuesToGPU(void* srcPtr, uint destOffset, uint size)
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
        protected unsafe void CopyValuesFromGPU(void* destPtr, uint srcOffset, uint size)
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
        /// Implicit conversion to a Buffer
        /// </summary>
        public static implicit operator Buffer(BufferWrapper buffer) { return buffer.Buffer; }

        /// <summary>
        /// Implicit conversion to a DeviceMemory
        /// </summary>
        public static implicit operator DeviceMemory(BufferWrapper buffer) { return buffer.DeviceMemory; }

    }
}
