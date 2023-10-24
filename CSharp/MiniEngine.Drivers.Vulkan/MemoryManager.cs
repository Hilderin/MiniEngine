using System;
namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Memory manager
    /// </summary>
    public class MemoryManager: IDisposable
    {
        private Device _device;
        private Queue Queue;
        private Fence Fence;
        private CommandPool _commandPool;

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryManager(Device device)
        {
            _device = device;

            _commandPool = device.CreateCommandPool(CommandPoolCreateFlags.ResetCommandBuffer);

            Queue = _device.GetQueue(0, 0);
            Fence = _device.CreateFence();
        }



        /// <summary>
        /// Create a buffer on the GPU
        /// </summary>
        public BufferWrapper CreateBufferOnGPU<T>(T[] values, BufferUsageFlags usageFlags)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (BufferWrapper stagingBuffer = _device.CreateBuffer(values, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                //Create a buffer on the GPU..
                BufferWrapper gpuBuffer = _device.CreateBuffer(stagingBuffer.Size, BufferUsageFlags.TransferDst | usageFlags, MemoryPropertyFlags.DeviceLocal);

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
            var commandBuffer = _device.AllocateCommandBuffer(_commandPool);

            commandBuffer.Begin(CommandBufferUsageFlags.OneTimeSubmit);

            BufferCopy copyRegion = new()
            {
                Size = bufferSource.Size,
            };

            commandBuffer.CmdCopyBuffer(bufferSource.Buffer, bufferDest.Buffer, copyRegion);

            commandBuffer.End();

            Queue.Submit(commandBuffer);
            Queue.WaitIdle();


            _device.FreeCommandBuffer(_commandPool, commandBuffer);
        }

        public void Dispose()
        {

            if (Fence != null)
            {
                Fence.Dispose();
                Fence = null;
            }

            if (_commandPool != null)
            {
                _commandPool.Dispose();
                _commandPool = null;
            }
        }
    }
}
