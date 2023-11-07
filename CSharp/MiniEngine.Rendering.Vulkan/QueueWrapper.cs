using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Wrapper around a queue
    /// </summary>
    public class QueueWrapper : IDisposable
    {
        private Device _device;
        private Queue _queue;
        private CommandPool _commandPool;
        private CommandBuffer _commandBufferMainThreadWait;
        private Fence _fenceMainThreadWait;
        private SubmitInfo _submitInfoMainThreadWait;
        private bool _supportMultiThreading;
        private uint _queueFamilyIndex;
        private uint _queueIndex;

        private ConcurrentQueue<Action> _actionsQueue = new ConcurrentQueue<Action>();
        private Thread _mainThread;

        /// <summary>
        /// Constructor
        /// </summary>
        public QueueWrapper(Device device, uint queueFamilyIndex, uint queueIndex, bool supportMultiThreading)
        {
            _device = device;

            _queueFamilyIndex = queueFamilyIndex;
            _queueIndex = queueIndex;
            _supportMultiThreading = supportMultiThreading;

            _commandPool = device.CreateCommandPool(queueFamilyIndex, CommandPoolCreateFlags.ResetCommandBuffer);

            _fenceMainThreadWait = _device.CreateFence();
            _commandBufferMainThreadWait = CreateCommandBuffer();

            _submitInfoMainThreadWait = new SubmitInfo()
            {
                CommandBufferCount = 1,
                CommandBuffers = new CommandBuffer[] { _commandBufferMainThreadWait }
            };

            //Start main thread if needed
            if (_supportMultiThreading)
            {
                _mainThread = new Thread(MainThread);
                _mainThread.IsBackground = true;
                _mainThread.Start();
            }
            else
            {
                //We can create the queue already...
                _queue = _device.GetQueue(queueFamilyIndex, queueIndex);
            }
        }

        /// <summary>
        /// Dispose queue
        /// </summary>
        public void Dispose()
        {

            if (_commandBufferMainThreadWait != null)
            {
                _commandBufferMainThreadWait.Dispose();
                _commandBufferMainThreadWait = null;
            }

            if (_fenceMainThreadWait != null)
            {
                _fenceMainThreadWait.Dispose();
                _fenceMainThreadWait = null;
            }

            if (_commandPool != null)
            {
                _commandPool.Dispose();
                _commandPool = null;
            }
        }

        /// <summary>
        /// Create a commandbuffer
        /// </summary>
        public CommandBuffer CreateCommandBuffer()
        {
            return _device.AllocateCommandBuffer(_commandPool);
        }

        /// <summary>
        /// Create multiple command buffers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <param name="count"></param>
        /// <param name="createNewBufferFunc"></param>
        /// <returns></returns>
        public T[] CreateCommandBuffers<T>(CommandBufferLevel level, int count, Func<T> createNewBufferFunc) where T : CommandBuffer
        {
            return _commandPool.AllocateCommandBuffers(level, count, createNewBufferFunc);
        }

        ///// <summary>
        ///// Destroy a commandbuffer
        ///// </summary>
        //public void DestroyCommandBuffer(CommandBuffer commandBuffer)
        //{
        //    _device.FreeCommandBuffer(_commandPool, commandBuffer);
        //}

        /// <summary>
        /// Execute actions on command buffer
        /// </summary>
        public void ExecuteAndWait(Action<CommandBuffer> commandActions)
        {
            if (_commandBufferMainThreadWait == null)
                //disposed...
                return;

            if (_supportMultiThreading && Thread.CurrentThread != _mainThread)
            {
                InvokeOnMainThread(commandActions);
            }
            else
            {
                //Execution on the current thread...
                _commandBufferMainThreadWait.Begin();

                //Populate the actions...
                commandActions(_commandBufferMainThreadWait);


                _commandBufferMainThreadWait.End();

                _fenceMainThreadWait.Reset();
                _queue.Submit(_submitInfoMainThreadWait, _fenceMainThreadWait);
                _fenceMainThreadWait.Wait();

                //DestroyCommandBuffer(commandBuffer);
            }

        }


        /// <summary>
        /// Execute actions on mainthread
        /// </summary>
        private void InvokeOnMainThread(Action<CommandBuffer> commandActions)
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            _actionsQueue.Enqueue(() =>
            {
                ExecuteAndWait(commandActions);

                waitHandle.Set();
            });

            waitHandle.WaitOne();

        }

        /// <summary>
        /// Execute actions on command buffer
        /// </summary>
        private void ExecuteAsync(Action<CommandBuffer> commandActions, Action callback)
        {
            using (Fence fence = _device.CreateFence())
            {
                using (var commandBuffer = CreateCommandBuffer())
                {
                    commandBuffer.Begin(CommandBufferUsageFlags.OneTimeSubmit);

                    //Populate the actions...
                    commandActions(commandBuffer);


                    commandBuffer.End();

                    fence.Reset();
                    _queue.Submit(commandBuffer, fence);
                    fence.Wait();

                    callback();
                }
            }

        }


        /// <summary>
        /// Main thread
        /// </summary>
        private void MainThread()
        {
            try
            {
                //Now that the thread is running, we will create the thread...
                _queue = _device.GetQueue(_queueFamilyIndex, _queueIndex);

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
                            Debug.Error($"QueueWrapper - Error: {exAction}");
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
                Debug.Error($"QueueWrapper.MainThread - Error: {ex}");
            }
        }

    }

}
