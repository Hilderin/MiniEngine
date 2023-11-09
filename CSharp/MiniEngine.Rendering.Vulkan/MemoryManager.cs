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