

namespace MiniEngine
{
    /// <summary>
    /// Context of the rendering
    /// </summary>
    public class RenderingContext
    {
        public Material Material = Material.Empty;

        public Matrix4 WVPMatrix;

        public Color3 AmbientColor = Color3.White;
        public float AmbientIntensity = 1f;
        public Color3 MaterialAmbientColor = Color3.White;

        public Color3 DiffuseColor = Color3.White;
        public float DiffuseIntensity = 1f;
        public Vector3 DiffuseDirection = Vector3.Down;
        public Vector3 CalculatedDiffuseDirection = Vector3.Down;
        public Color3 MaterialDiffuseColor = Color3.White;

        /// <summary>
        /// Camera local position in the local space for the current world transform of the object that is rendering
        /// </summary>
        public Vector3 CameraLocalPosition = Vector3.Zero;





        /// <summary>
        /// Calculate diffuse ligth direction from world matrix
        /// </summary>
        /// <param name="worldMatrix"></param>
        public void CalculateDiffuseDirection(ref Matrix4 worldMatrix)
        {
            // Inverse local-to-world transformation using transpose
            // (assuming uniform scaling)
            Matrix3 WorldToLocal = Matrix4.Transpose(ref worldMatrix);


            CalculatedDiffuseDirection = WorldToLocal * DiffuseDirection;

            CalculatedDiffuseDirection.Normalize();

        }

    }
}
