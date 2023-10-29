using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    public enum DebugReportLevel : int
    {
        Information = 0x1,
        Warning = 0x2,
        PerformanceWarning = 0x4,
        Error = 0x8,
        Debug = 0x10,
    }
}
