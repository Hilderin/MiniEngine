using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Memory manager
    /// </summary>
    public class MemoryManager: IDisposable
    {
        private Device _device;
        private CommandPool _commandPool;
        private ConcurrentQueue<ExecutionThreadElement> _commandQueue = new ConcurrentQueue<ExecutionThreadElement>();
        private Thread _mainThread;

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryManager(Device device)
        {
            _device = device;

            _commandPool = device.CreateTransferCommandPool();


            _mainThread = new Thread(MainThread);
            _mainThread.IsBackground = true;
            _mainThread.Start();

        }

        /// <summary>
        /// Execute actions on command buffer
        /// </summary>
        public void ExecuteCommandBuffer(Action<CommandBuffer> commandActions)
        {
            var commandBuffer = _device.AllocateCommandBuffer(_commandPool);

            
            commandBuffer.Begin(CommandBufferUsageFlags.OneTimeSubmit);

            //Populate the actions...
            commandActions(commandBuffer);


            commandBuffer.End();

            var element = new ExecutionThreadElement()
            {
                CommandBuffer = commandBuffer,
            };
            _commandQueue.Enqueue(element);

            element.waitHandle.WaitOne();

            //using (Fence fence = _device.CreateFence())
            //{

            //    commandBuffer.Begin(CommandBufferUsageFlags.OneTimeSubmit);

            //    //Populate the actions...
            //    commandActions(commandBuffer);


            //    commandBuffer.End();

            //    fence.Reset();
            //    _commandQueue.Enqueue(commandBuffer);
            //    fence.Wait();
            //}

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

        /// <summary>
        /// Main thread
        /// </summary>
        private void MainThread()
        {
            try
            {
                Queue queue = _device.GetTransferQueue();
                while (true)
                {
                    if (_commandQueue.TryDequeue(out var element))
                    {
                        try
                        {
                            ExecuteCommandBuffer(queue, element.CommandBuffer);
                        }
                        catch(Exception exIn)
                        {
                            Debug.Print("MemoryManager.MainThread - Erreur: " + exIn.ToString());
                        }

                        //All done!
                        element.waitHandle.Set();
                    }
                    else
                        System.Threading.Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Debug.Print("MemoryManager.MainThread - Erreur: " + ex.ToString());
            }
        }

        private void ExecuteCommandBuffer(Queue queue, CommandBuffer commandBuffer)
        {
            using (Fence fence = _device.CreateFence())
            {
                fence.Reset();
                queue.Submit(commandBuffer);
                fence.Wait();
            }
            
        }

        private class ExecutionThreadElement
        {
            public EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            public CommandBuffer CommandBuffer;
        }

    }
}
