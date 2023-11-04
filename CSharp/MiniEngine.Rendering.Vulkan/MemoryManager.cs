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
        private ConcurrentQueue<Action> _actionsQueue = new ConcurrentQueue<Action>();
        private Thread _mainThread;

        /// <summary>
        /// Constructor
        /// </summary>
        public MemoryManager(VkRenderer renderer)
        {
            _renderer = renderer;
            _device = renderer.Device;

            _mainThread = new Thread(MainThread);
            _mainThread.IsBackground = true;
            _mainThread.Start();
        }

        /// <summary>
        /// Execute actions on command buffer
        /// </summary>
        public void ExecuteOnTransferQueue(Action<CommandBuffer> commandActions)
        {
            if (Thread.CurrentThread == _mainThread)
            {
                _queue.ExecuteAndWait(commandActions);
            }
            else
            {
                //Invoke...
                bool done = false;
                _actionsQueue.Enqueue(() =>
                {
                    _queue.ExecuteAsync(commandActions, () => done = true);
                });

                while (!done)
                {
                    Thread.Sleep(1);
                }
            }
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


        /// <summary>
        /// Main thread
        /// </summary>
        private void MainThread()
        {
            try
            {
                _queue = new QueueWrapper(_device, _renderer.TransferQueueIndex, 0);
                
                while (_queue != null)
                {
                    if (_actionsQueue.TryDequeue(out var action))
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception exAction)
                        {
                            Debug.Error($"MemoryManager - Error: {exAction}");
                        }
                    }
                    else
                        Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Debug.Error($"MemoryManager.MainThread - Error: {ex}");
            }
        }


    }
}