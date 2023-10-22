using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Shaders
{
    [Flags]
    public enum ShaderStage : int
    {
        Vertex = 0x1,
        TessellationControl = 0x2,
        TessellationEvaluation = 0x4,
        Geometry = 0x8,
        Fragment = 0x10,
        Compute = 0x20,
        AllGraphics = 0x0000001F,
        All = 0x7FFFFFFF,
    }
}
