using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Encapsulate object that can be place in the world
    /// </summary>
    public class WorldTransform
    {

        /// <summary>
        /// Location in the world
        /// </summary>
        public Vector3 Location;

        /// <summary>
        /// Scale in the world
        /// </summary>
        public Vector3 Scale = Vector3.One;

        /// <summary>
        /// Rotation in the world
        /// </summary>
        public Rotator3 Rotation;

        /// <summary>
        /// Get th forward vector
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                var q = Quaternion.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
                return Vector3.Transform(Vector3.Forward, q);
            }
        }

        /// <summary>
        /// Get the left vector
        /// </summary>
        public Vector3 Left => Vector3.Up.Cross(Forward);

        /// <summary>
        /// Get the right vector
        /// </summary>
        public Vector3 Right => Left * -1f;

        /// <summary>
        /// Get the up vector
        /// </summary>
        public Vector3 Up => Forward.Cross(Left);

        /// <summary>
        /// Get the back vector
        /// </summary>
        public Vector3 Backward => Forward * -1f;


        /// <summary>
        /// Rotate on X axis
        /// </summary>
        public void RotateX(float angleRad)
        {
            if (angleRad == 0)
                return;

            Rotation.X += angleRad;
        }

        /// <summary>
        /// Rotate on Y axis
        /// </summary>
        public void RotateY(float angleRad)
        {
            if (angleRad == 0)
                return;

            Rotation.Y += angleRad;
        }

        /// <summary>
        /// Rotate on Z axis
        /// </summary>
        public void RotateZ(float angleRad)
        {
            if (angleRad == 0)
                return;

            Rotation.Z += angleRad;
        }

        /// <summary>
        /// Move forward considering the rotation of the world transform
        /// </summary>
        public void MoveForward(float distance)
        {
            this.Location += this.Forward * -distance;
        }

        /// <summary>
        /// Move forward considering the rotation of the world transform
        /// Z < 0 = Forward
        /// Z < 0 = Backward
        /// X > 0 = Right
        /// X < 0 = Left
        /// Y > 0 = Up
        /// Y < 0 = Down
        /// </summary>
        public void MoveInDirections(float distance, Vector3 directions)
        {
            if (!Math.IsZero(directions.Z))
                this.Location += this.Forward * directions.Z * distance;
            if (!Math.IsZero(directions.X))
                this.Location += this.Left * directions.X * -distance;
            if (!Math.IsZero(directions.Y))
                this.Location += this.Up * directions.Y * distance;
            
        }

        /// <summary>
        /// Move backward considering the rotation of the world transform
        /// </summary>
        public void MoveBackward(float distance)
        {
            this.Location += this.Forward * distance;
        }

        /// <summary>
        /// Move left considering the rotation of the world transform
        /// </summary>
        public void MoveLeft(float distance)
        {
            this.Location += this.Left * -distance;
        }

        /// <summary>
        /// Move left considering the rotation of the world transform
        /// </summary>
        public void MoveRight(float distance)
        {
            this.Location += this.Left * distance;
        }

        ///// <summary>
        ///// Move in 4 directions
        ///// </summary>
        //public void Move(float distance, bool forward, bool backword, bool left, bool right)
        //{
        //    if (forward)
        //        MoveForward(distance);
        //    if (backword)
        //        MoveBackward(distance);
        //    if (left)
        //        MoveLeft(distance);
        //    if (right)
        //        MoveRight(distance);
        //}


        /// <summary>
        /// Get the world rotation matrix
        /// </summary>
        public Matrix4 GetRotationMatrix()
        {
            return Matrix4.CreateRotationMatrixX(Rotation.X) * Matrix4.CreateRotationMatrixY(Rotation.Y) * Matrix4.CreateRotationMatrixZ(Rotation.Z);
        }

        /// <summary>
        /// Get the world scale matrix
        /// </summary>
        public Matrix4 GetScaleMatrix()
        {
            return Matrix4.CreateScaleMatrix(Scale.X, Scale.Y, Scale.Z);
        }

        /// <summary>
        /// Get the world translation matrix
        /// </summary>
        public Matrix4 GetTranslationMatrix()
        {
            return Matrix4.CreateTranslationMatrix(Location.X, Location.Y, Location.Z);
        }

        /// <summary>
        /// Get the world transformation matrix
        /// </summary>
        public virtual Matrix4 GetMatrix()
        {
            Matrix4 rotationMatrix = GetRotationMatrix();
            Matrix4 scaleMatrix = GetScaleMatrix();
            Matrix4 translationMatrix = GetTranslationMatrix();

            return translationMatrix * rotationMatrix * scaleMatrix;

        }

        /// <summary>
        /// Get the revert translation matrix
        /// </summary>
        public Matrix4 GetReversedTranslationMatrix()
        {
            return Matrix4.CreateTranslationMatrix(-Location.X, -Location.Y, -Location.Z);
        }

        /// <summary>
        /// Get the revert rotation matrix
        /// </summary>
        public Matrix4 GetReversedRotationMatrix()
        {
            return Matrix4.CreateRotationMatrixXYZ(-Rotation.X, -Rotation.Y, -Rotation.Z);
        }


        /// <summary>
        /// Calculate the location position from a world transform
        /// </summary>
        public Vector3 GetLocalPosition(ref Vector3 location)
        {

            Matrix4 cameraToLocalTranslation = GetReversedTranslationMatrix();

            Matrix4 cameraToLocalRotation = GetReversedRotationMatrix();

            Matrix4 cameraToLocalTransformation = cameraToLocalRotation * cameraToLocalTranslation;

            Vector4 cameraWorldPos = new Vector4(location, 1.0f);

            Vector4 CameraLocalPos = cameraToLocalTransformation * cameraWorldPos;

            return CameraLocalPos;

        }


    }
}
