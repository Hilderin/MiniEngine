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
    public class BufferWrapper<T>: BufferWrapper
    {
        /// <summary>
        /// Size of a value
        /// </summary>
        public uint ElementSize { get; private set; }

        /// <summary>
        /// Number of elements
        /// </summary>
        public uint Count { get { return _position / ElementSize; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public BufferWrapper(VkRenderer renderer, uint size, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags): base(renderer, size, usageFlags, memoryPropertyFlags)
        {
            ElementSize = VkSizeOfHelper.SizeOf<T>();
        }

        /// <summary>
        /// Create a buffer with data
        /// </summary>
        public static BufferWrapper<T> Create(VkRenderer renderer, T[] values, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            uint size = VkSizeOfHelper.SizeOf<T>(1) * (uint)values.Length;

            BufferWrapper<T> buffer = new BufferWrapper<T>(renderer, size, usageFlags, memoryPropertyFlags);

            buffer.Update(values);

            return buffer;
        }


        /// <summary>
        /// Create a buffer with data
        /// </summary>
        public static BufferWrapper<T> Create(VkRenderer renderer, int count, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags)
        {
            uint size = VkSizeOfHelper.SizeOf<T>() * (uint)count;

            BufferWrapper<T> buffer = new BufferWrapper<T>(renderer, size, usageFlags, memoryPropertyFlags);

            return buffer;
        }

        /// <summary>
        /// Copy data to a buffer from values
        /// </summary>
        public unsafe void Update(T[] values, uint offset = 0)
        {
            if (values.Length == 0)
                return;

            Type type = typeof(T);
            var size = ElementSize * values.Length;

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
        public unsafe void Update(ref T value, uint offset = 0)
        {
            if (IsOnGPU)
            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                fixed (T* ptr = &value)
                {
                    CopyValuesToGPU(ptr, offset, ElementSize);
                }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            }
            else
            {
                lock (DeviceMemory)
                {
                    IntPtr dataPtr = _device.MapMemory(DeviceMemory, offset, ElementSize);
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    *((T*)dataPtr) = value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    //new Span<T>((void*)dataPtr, 1)[0] = value;
                    _device.UnmapMemory(DeviceMemory);
                }

            }

            if (_position < offset + ElementSize)
                _position = offset + ElementSize;

        }

        /// <summary>
        /// Append data to the buffer and return the start index
        /// </summary>
        public unsafe uint Append(T[] values)
        {
            uint size = ElementSize * (uint)values.Length;

            uint startIndex;
            lock (this)
            {
                startIndex = _position;
                _position += size;
            }

            Update(values, startIndex);

            

            return startIndex;

        }

        /// <summary>
        /// Append data to the buffer and return the start offset and the start element Index
        /// </summary>
        public unsafe uint Append(T[] values, out uint startElementIndex)
        {
            uint startIndex = Append(values);

            startElementIndex = startIndex / ElementSize;

            return startIndex;

        }


        /// <summary>
        /// Append data to the buffer and return the start index
        /// </summary>
        public unsafe uint Append(ref T value)
        {
            uint startIndex;
            lock (this)
            {
                startIndex = _position;
                _position += ElementSize;
            }

            Update(ref value, startIndex);

            return startIndex;

        }

        /// <summary>
        /// Append data to the buffer and return the start offset and and element index
        /// </summary>
        public unsafe uint Append(ref T value, out uint elementIndex)
        {
            uint startIndex = Append(ref value);

            elementIndex = startIndex / ElementSize;

            return startIndex;

        }



        /// <summary>
        /// Reserve space into the buffer for a struct and return the start offset
        /// </summary>
        public unsafe uint Reserve()
        {
            uint startIndex;
            lock (this)
            {
                startIndex = _position;
                _position += ElementSize;
            }

            return startIndex;

        }

        /// <summary>
        /// Reserve space into the buffer for a struct and return the start offset and and element index
        /// </summary>
        public unsafe uint Reserve(out uint elementIndex)
        {
            uint startIndex = Reserve();
            
            elementIndex = startIndex / ElementSize;

            return startIndex;

        }


        /// <summary>
        /// Copy the buffer to an array
        /// </summary>
        public unsafe void CopyTo(T[] values, uint srcOffset = 0)
        {
            uint size = ElementSize * (uint)values.Length;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            fixed (T* destPtr = &values[0])
            {
                CopyTo(destPtr, srcOffset, size);
            }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

        }


    }
}
