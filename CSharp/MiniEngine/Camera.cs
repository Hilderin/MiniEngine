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
        public Vector2 ClientSize;

        /// <summary>
        /// Field of view
        /// </summary>
        public float FOV = 60.0f;

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
        public Matrix4 GetViewMatrix()
        {
            //public static Matrix4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
            //{
            //    Vector3 vector3D = Vector3.Normalize(cameraPosition - cameraTarget);
            //    Vector3 vector3D2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector3D));
            //    Vector3 vector = Vector3.Cross(vector3D, vector3D2);
            //    Matrix4 identity = Matrix4.Identity;
            //    identity.M11 = vector3D2.X;
            //    identity.M12 = vector.X;
            //    identity.M13 = vector3D.X;
            //    identity.M21 = vector3D2.Y;
            //    identity.M22 = vector.Y;
            //    identity.M23 = vector3D.Y;
            //    identity.M31 = vector3D2.Z;
            //    identity.M32 = vector.Z;
            //    identity.M33 = vector3D.Z;
            //    identity.M41 = -Vector3.Dot(vector3D2, cameraPosition);
            //    identity.M42 = -Vector3.Dot(vector, cameraPosition);
            //    identity.M43 = -Vector3.Dot(vector3D, cameraPosition);

            //    return identity;
            //}

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
        public Matrix4 GetProjectionMatrixOpenGL()
        {
            //Vector2 clientSize = Context.Current.ClientSize;
            return Matrix4.CreateProjectionOpenGL(FOV, ClientSize.X, ClientSize.Y, NearZ, FarZ);
        }

        /// <summary>
        /// Get the projection matrix
        /// </summary>
        public Matrix4 GetProjectionMatrixVulkan()
        {
            //Vector2 clientSize = Context.Current.ClientSize;
            return Matrix4.CreatePerspectiveVulkan(FOV, ClientSize.X, ClientSize.Y, NearZ, FarZ);
        }

        /// <summary>
        /// Get the matrix for the camera (projection * camera matrices)
        /// </summary>
        /// <returns></returns>
        public override Matrix4 GetMatrix()
        {
            return GetViewMatrix();
        }


        ///// <summary>
        ///// Calculate the location position from a world transform
        ///// </summary>
        //public Vector3 GetLocalPositionForWorldTransform(WorldTransform worldTransform)
        //{

        //    Matrix4 cameraToLocalTranslation = worldTransform.GetReversedTranslationMatrix();

        //    Matrix4 cameraToLocalRotation = worldTransform.GetReversedRotationMatrix();

        //    Matrix4 cameraToLocalTransformation = cameraToLocalRotation * cameraToLocalTranslation;

        //    Vector4 cameraWorldPos = new Vector4(this.Location, 1.0f);

        //    Vector4 CameraLocalPos = cameraToLocalTransformation * cameraWorldPos;

        //    return CameraLocalPos;

        //}

    }
}
