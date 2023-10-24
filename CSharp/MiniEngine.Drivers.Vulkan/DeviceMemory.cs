using System;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Vulkan DeviceMemory
    /// </summary>
    public partial class DeviceMemory : INonDispatchableHandleMarshalling
    {
        internal DeviceMemory() { }

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
