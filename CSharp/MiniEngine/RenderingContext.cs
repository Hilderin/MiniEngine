

using System.Collections.Generic;

namespace MiniEngine
{
    /// <summary>
    /// Context of the rendering
    /// </summary>
    public class RenderingContext
    {
        public Matrix4 WVPMatrix;

        public Color3 AmbientColor = Color3.White;
        public float AmbientIntensity = 1f;

        public Color3 DiffuseColor = Color3.White;
        public float DiffuseIntensity = 0f;
        //public Vector3 DiffuseDirection = Vector3.Down;
        public Vector3 CalculatedDiffuseDirection = Vector3.Down;

        /// <summary>
        /// Camera local position in the local space for the current world transform of the object that is rendering
        /// </summary>
        public Vector3 CameraLocalPosition = Vector3.Zero;

        /// <summary>
        /// Point lights
        /// </summary>
        public List<PointLight> PointLights = new List<PointLight>(Renderer.MAX_POINT_LIGHTS);

        /// <summary>
        /// Spot lights
        /// </summary>
        public List<SpotLight> SpotLights = new List<SpotLight>(Renderer.MAX_POINT_LIGHTS);

        /// <summary>
        /// Position of the point light in reference to the current mesh
        /// </summary>
        public Vector3[] PointLightsCalulcatedLocalPositions = new Vector3[Renderer.MAX_POINT_LIGHTS];

        /// <summary>
        /// Position of the spot light in reference to the current mesh
        /// </summary>
        public Vector3[] SpotLightsCalulcatedLocalPositions = new Vector3[Renderer.MAX_POINT_LIGHTS];

        /// <summary>
        /// Direction of the spot light in reference to the current mesh
        /// </summary>
        public Vector3[] SpotLightsCalulcatedLocalDirections = new Vector3[Renderer.MAX_POINT_LIGHTS];


    }
}
