using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{

    public interface IMarshalling
    {
        IntPtr Handle { get; }
    }

    public interface INonDispatchableHandleMarshalling
    {
        UInt64 Handle { get; }
    }

}
