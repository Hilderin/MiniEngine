using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Representation of a camera
    /// </summary>
    public class Camera: WorldTransform
    {
        /// <summary>
        /// Field of view
        /// </summary>
        public float FOV = 90.0f;

        ///// <summary>
        ///// Width
        ///// </summary>
        //public float Width;

        ///// <summary>
        ///// Height
        ///// </summary>
        //public float Height;

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
        public Matrix4 GetCameraMatrix()
        {
            //UVN...
            //U = Points to the right of the camera
            //V = UP
            //N = Target, where the camera is looking
            //Vector3 U = new Vector3(1.0f, 0.0f, 0.0f);
            //Vector3 V = new Vector3(0.0f, 1.0f, 0.0f);
            //Vector3 N = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 U = this.Right;
            Vector3 V = this.Up;
            Vector3 N = this.Backward;

            Matrix4 translationMatrix = Matrix4.CreateTranslationMatrix(-Location.X, -Location.Y, -Location.Z);

            Matrix4 cameraMatrix = new Matrix4(U.X, U.Y, U.Z, 0.0f,
                                                V.X, V.Y, V.Z, 0.0f,
                                                N.X, N.Y, N.Z, 0.0f,
                                                0.0f, 0.0f, 0.0f, 1.0f);

            return cameraMatrix * translationMatrix;
        }

        /// <summary>
        /// Get the projection matrix
        /// </summary>
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreateProjection(FOV, Context.Current.ClientSize.X, Context.Current.ClientSize.Y, NearZ, FarZ);
        }

        /// <summary>
        /// Get the matrix for the camera (projection * camera matrices)
        /// </summary>
        /// <returns></returns>
        public override Matrix4 GetMatrix()
        {
            return GetProjectionMatrix() * GetCameraMatrix();
        }


        /// <summary>
        /// Calculate the location position from a world transform
        /// </summary>
        public Vector3 GetLocalPositionForWorldTransform(WorldTransform worldTransform)
        {

            Matrix4 cameraToLocalTranslation = worldTransform.GetReversedTranslationMatrix();

            Matrix4 cameraToLocalRotation = worldTransform.GetReversedRotationMatrix();

            Matrix4 cameraToLocalTransformation = cameraToLocalRotation * cameraToLocalTranslation;

            Vector4 cameraWorldPos = new Vector4(this.Location, 1.0f);

            Vector4 CameraLocalPos = cameraToLocalTransformation * cameraWorldPos;

            return CameraLocalPos;

        }

    }
}
