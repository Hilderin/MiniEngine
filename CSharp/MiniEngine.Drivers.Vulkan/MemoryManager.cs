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
        private CommandPool _commandPool;

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryManager(Device device)
        {
            _device = device;

            _commandPool = device.CreateCommandPool(CommandPoolCreateFlags.ResetCommandBuffer);

            Queue = _device.GetQueue(0, 0);
        }

        /// <summary>
        /// Execute actions on command buffer
        /// </summary>
        public void ExecuteCommandBuffer(Action<CommandBuffer> commandActions)
        {
            var commandBuffer = _device.AllocateCommandBuffer(_commandPool);

            using (Fence fence = _device.CreateFence())
            {

                commandBuffer.Begin(CommandBufferUsageFlags.OneTimeSubmit);

                //Populate the actions...
                commandActions(commandBuffer);


                commandBuffer.End();

                fence.Reset();
                Queue.Submit(commandBuffer, fence);
                fence.Wait();
            }

            _device.FreeCommandBuffer(_commandPool, commandBuffer);

        }

        /// <summary>
        /// Create a buffer on the GPU
        /// </summary>
        public BufferWrapper CreateBufferOnGPU<T>(T[] values, BufferUsageFlags usageFlags)
        {
            //Create a stating buffer available from the CPU... so we can copy values into it...
            using (BufferWrapper stagingBuffer = _device.CreateBufferWrapper(values, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent))
            {
                //Create a buffer on the GPU..
                BufferWrapper gpuBuffer = _device.CreateBufferWrapper(stagingBuffer.Size, BufferUsageFlags.TransferDst | usageFlags, MemoryPropertyFlags.DeviceLocal);

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
            ExecuteCommandBuffer(commandBuffer =>
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
            if (_commandPool != null)
            {
                _commandPool.Dispose();
                _commandPool = null;
            }
        }
    }
}
