using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Wrapper around a queue
    /// </summary>
    public class QueueWrapper: IDisposable
    {
        private Device _device;
        private Queue _queue;
        private CommandPool _commandPool;
        private CommandBuffer _commandBufferMainThreadWait;
        private Fence _fenceMainThreadWait;
        private SubmitInfo _submitInfoMainThreadWait;

        /// <summary>
        /// Constructor
        /// </summary>
        public QueueWrapper(Device device, uint queueFamilyIndex, uint queueIndex)
        {
            _device = device;

            _queue = _device.GetQueue(queueFamilyIndex, queueIndex);
            
            _commandPool = device.CreateCommandPool(queueFamilyIndex, CommandPoolCreateFlags.ResetCommandBuffer);

            _fenceMainThreadWait = _device.CreateFence();
            _commandBufferMainThreadWait = CreateCommandBuffer();

            _submitInfoMainThreadWait = new SubmitInfo()
            {
                CommandBufferCount = 1,
                CommandBuffers = new CommandBuffer[] { _commandBufferMainThreadWait }
            };
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

            _commandBufferMainThreadWait.Begin();

            //Populate the actions...
            commandActions(_commandBufferMainThreadWait);


            _commandBufferMainThreadWait.End();

            _fenceMainThreadWait.Reset();
            _queue.Submit(_submitInfoMainThreadWait, _fenceMainThreadWait);
            _fenceMainThreadWait.Wait();

            //DestroyCommandBuffer(commandBuffer);

        }


        /// <summary>
        /// Execute actions on command buffer
        /// </summary>
        public void ExecuteAsync(Action<CommandBuffer> commandActions, Action callback)
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
    }

}
