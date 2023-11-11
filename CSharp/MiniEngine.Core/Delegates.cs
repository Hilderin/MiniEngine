
namespace MiniEngine
{

    /// <summary>
    /// Handler when the tranform moved, rotate or scale
    /// </summary>
    public delegate void OnTransformChangedHandler();


    /// <summary>
    /// Debug delegate
    /// </summary>
    public delegate void DebugCallback(DebugLevel level, int messageCode, string message);

}
