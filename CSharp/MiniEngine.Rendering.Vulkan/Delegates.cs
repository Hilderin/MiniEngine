using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Debug delegate
    /// </summary>
    public delegate void VkDebugReportCallback(DebugReportLevel level, int messageCode, string message);

}
