
namespace MiniEngine
{
    public enum DebugLevel : int
    {
        Information = 0x1,
        Warning = 0x2,
        PerformanceWarning = 0x4,
        Error = 0x8,
        Debug = 0x10,
    }

    public enum ShaderStage : int
    {
        Vertex = 0x1,
        TessellationControl = 0x2,
        TessellationEvaluation = 0x4,
        Geometry = 0x8,
        Fragment = 0x10,
        Compute = 0x20
    }
}
