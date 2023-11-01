using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Representation of a camera
    /// </summary>
    public class Camera: ICamera
    {
        /// <summary>
        /// Transform of the camera
        /// </summary>
        public WorldTransform Transform = new WorldTransform();

        /// <summary>
        /// Field of view
        /// </summary>
        public float FOV = 60.0f;

        /// <summary>
        /// Nearest Z
        /// </summary>
        public float NearZ = 0.1f;

        /// <summary>
        /// Farest Z
        /// </summary>
        public float FarZ = 100.0f;

        /// <summary>
        /// Get the Camera matrix
        /// </summary>
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.GetViewMatrix(Transform.Location, Transform.Backward, Transform.Up);
        }

        /// <summary>
        /// Get the projection matrix
        /// </summary>
        public Matrix4 GetProjectionMatrixOpenGL(int clientSizeX, int clientSizeY)
        {
            //Vector2 clientSize = Context.Current.ClientSize;
            return Matrix4.CreateProjectionOpenGL(FOV, clientSizeX, clientSizeY, NearZ, FarZ);
        }

        /// <summary>
        /// Get the projection matrix
        /// </summary>
        public Matrix4 GetProjectionMatrixVulkan(int clientSizeX, int clientSizeY)
        {
            //Vector2 clientSize = Context.Current.ClientSize;
            return Matrix4.CreatePerspectiveVulkan(FOV, clientSizeX, clientSizeY, NearZ, FarZ);
        }


    }
}
