using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Vulkan CommandPool
    /// </summary>
    public partial class CommandPool : IDisposable, INonDispatchableHandleMarshalling
    {
        private Device _device;

        internal CommandPool(Device device)
        {
            _device = device;

            if (device != null)
                device.CommandPools.Add(this);

        }

        internal UInt64 m;

        UInt64 INonDispatchableHandleMarshalling.Handle
        {
            get
            {
                return m;
            }
        }

        /// <summary>
        /// Allocate command buffers
        /// </summary>
        public CommandBuffer[] AllocateCommandBuffers(CommandBufferLevel level, int count)
        {
            using (var commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                Level = level,
                CommandPool = this,
                CommandBufferCount = (uint)count
            })
            {
                return _device.AllocateCommandBuffers(commandBufferAllocateInfo);
            }
        }

        /// <summary>
        /// Allocate command buffers
        /// </summary>
        public T[] AllocateCommandBuffers<T>(CommandBufferLevel level, int count, Func<T> createNewBufferFunc) where T : CommandBuffer
        {
            using (var commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                Level = level,
                CommandPool = this,
                CommandBufferCount = (uint)count
            })
            {
                return _device.AllocateCommandBuffers<T>(commandBufferAllocateInfo, createNewBufferFunc);
            }
        }

        public void Dispose()
        {
            if (_device != null && m != 0)
            {
                _device.DestroyCommandPool(this);
                _device = null;
                m = 0;
            }
        }
    }
}
