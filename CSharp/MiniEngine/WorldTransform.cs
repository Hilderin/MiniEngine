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
        public Vector3 Scale;

        /// <summary>
        /// Rotation in the world
        /// </summary>
        public Vector3 Rotation;

        /// <summary>
        /// Get th forward vector
        /// </summary>
        public Vector3 Forward = Vector3.Forward;       //Default -Z (0, 0, -1)

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
            var q = Quaternion.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            //var q = Quaternion.CreateFromAxisAngle(Vector3.Up, -Rotation.Y);      //Adding Pi/2 because angle 0 = normal but the up vector is up, we need to compensate.
            Forward = Vector3.Transform(Vector3.Forward, q);
            //Forward.Transform2(q);
            //Forward.Normalize();

            //Vector3 v = Vector3.Right;
            //v.RotateY(angleRad);

            //var q = Quaternion.CreateFromAxisAngle(Vector3.Up, angleRad);
            //Vector3 v = Vector3.Right;
            //var v2 = Vector3.Transform(v, q);
            //v2.Normalize();

            //v.Transform(q);
            //Forward.Normalize();
        }

        /// <summary>
        /// Rotate on Y axis
        /// </summary>
        public void RotateY(float angleRad)
        {
            if (angleRad == 0)
                return;

            Rotation.Y += angleRad;
            var q = Quaternion.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            //var q = Quaternion.CreateFromAxisAngle(Vector3.Up, -Rotation.Y);      //Adding Pi/2 because angle 0 = normal but the up vector is up, we need to compensate.
            Forward = Vector3.Transform(Vector3.Forward, q);
            //Forward.Transform2(q);
            //Forward.Normalize();

            //Vector3 v = Vector3.Right;
            //v.RotateY(angleRad);

            //var q = Quaternion.CreateFromAxisAngle(Vector3.Up, angleRad);
            //Vector3 v = Vector3.Right;
            //var v2 = Vector3.Transform(v, q);
            //v2.Normalize();

            //v.Transform(q);
            //Forward.Normalize();
        }



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



    }
}
