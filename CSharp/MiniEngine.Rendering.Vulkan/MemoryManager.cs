using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Memory manager
    /// </summary>
    public class MemoryManager : IDisposable
    {
        private Device _device;
        private VkRenderer _renderer;
        private QueueWrapper _queue;

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryManager(VkRenderer renderer)
        {
            _renderer = renderer;
            _device = renderer.Device;

            _queue = new QueueWrapper(_device, _renderer.TransferQueueIndex, 0, true);
        }

        /// <summary>
        /// Execute actions on command buffer
        /// </summary>
        public void ExecuteOnTransferQueue(Action<CommandBuffer> commandActions)
        {
            _queue.ExecuteAndWait(commandActions);
        }


        /// <summary>
        /// Create a buffer on the GPU
        /// </summary>
        public BufferWrapper CreateBufferOnGPU<T>(T[] values, BufferUsageFlags usageFlags)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (BufferWrapper stagingBuffer = _renderer.CreateBufferWrapper(values, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                //Create a buffer on the GPU..
                BufferWrapper gpuBuffer = _renderer.CreateBufferWrapper(stagingBuffer.Size, BufferUsageFlags.TransferDst | usageFlags, MemoryPropertyFlags.DeviceLocal);

                //Copy the data to the GPU...
                CopyBuffer(stagingBuffer, gpuBuffer);

                return gpuBuffer;
            }
        }

        /// <summary>
        /// Copy a buffer
        /// </summary>
        public void CopyBuffer(BufferWrapper bufferSource, BufferWrapper bufferDest)
        {
            ExecuteOnTransferQueue(commandBuffer =>
            {

                BufferCopy copyRegion = new()
                {
                    Size = bufferSource.Size,
                };

                commandBuffer.CmdCopyBuffer(bufferSource.Buffer, bufferDest.Buffer, copyRegion);

            });
        }

        public void Dispose()
        {
            if (_queue != null)
            {
                _queue.Dispose();
                _queue = null;
            }
        }

    }
}