using System;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Vulkan DeviceMemory
    /// </summary>
    public partial class VkDeviceMemory : INonDispatchableHandleMarshalling
    {
        internal VkDeviceMemory() { }

        internal UInt64 m;

        UInt64 INonDispatchableHandleMarshalling.Handle
        {
            get
            {
                return m;
            }
        }
    }

}
