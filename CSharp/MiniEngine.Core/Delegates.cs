
namespace MiniEngine
{

    /// <summary>
    /// Handler when the tranform moved
    /// </summary>
    public delegate void OnLocationChangedHandler(Vector3 oldLocation, Vector3 newLocation);

    /// <summary>
    /// Handler when the rotation changed
    /// </summary>
    public delegate void OnRotationChangedHandler(Rotator3 oldRotation, Rotator3 newRotation);

    /// <summary>
    /// Handler when the scale changed
    /// </summary>
    public delegate void OnScaleChangedHandler(Vector3 oldScale, Vector3 newScale);


    /// <summary>
    /// Debug delegate
    /// </summary>
    public delegate void DebugCallback(DebugLevel level, int messageCode, string message);

}
