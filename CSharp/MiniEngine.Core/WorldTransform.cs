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
        /// Event when the transform moved
        /// </summary>
        public event OnLocationChangedHandler OnLocationChanged;

        /// <summary>
        /// Event when the rotation changed
        /// </summary>
        public event OnRotationChangedHandler OnRotationChanged;

        /// <summary>
        /// Event when the scale changed
        /// </summary>
        public event OnScaleChangedHandler OnScaleChanged;


        private Vector3 _location;

        private Rotator3 _rotation;

        private Vector3 _scale = Vector3.One;

        /// <summary>
        /// Location in the world
        /// </summary>
        public Vector3 Location
        {
            get { return _location; }
            set
            {
                if (_location != value)
                {
                    if (OnLocationChanged != null)
                    {
                        Vector3 oldLocation = _location;
                        _location = value;
                        OnLocationChanged(oldLocation, _location);
                    }
                    else
                    {
                        _location = value;
                    }
                }
            }
        }

        /// <summary>
        /// Scale in the world
        /// </summary>
        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    if (OnScaleChanged != null)
                    {
                        Vector3 oldScale = _scale;
                        _scale = value;
                        OnScaleChanged(oldScale, _scale);
                    }
                    else
                    {
                        _scale = value;
                    }
                }
            }
        }

        /// <summary>
        /// Rotation in the world
        /// </summary>
        public Rotator3 Rotation
        {
            get { return _rotation; }
            set
            {
                if (_rotation != value)
                {
                    if (OnRotationChanged != null)
                    {
                        Rotator3 oldRotation = _rotation;
                        _rotation = value;
                        OnRotationChanged(oldRotation, _rotation);
                    }
                    else
                    {
                        _rotation = value;
                    }
                }
            }
        }

        /// <summary>
        /// Get th forward vector
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                var q = Quaternion.CreateFromYawPitchRoll(Rotation.Yaw, -Rotation.Pitch, Rotation.Roll);
                return Vector3.Transform(Vector3.Forward, q);
            }
        }

        /// <summary>
        /// Get the left vector
        /// </summary>
        public Vector3 Left
        {
            get
            {
                //Vector3.Up.Cross(Forward);
                var q = Quaternion.CreateFromYawPitchRoll(Rotation.Yaw, -Rotation.Pitch, Rotation.Roll);
                return Vector3.Transform(Vector3.Left, q);
            }
        }

        /// <summary>
        /// Get the right vector
        /// </summary>
        public Vector3 Right => Left * -1f;

        /// <summary>
        /// Get the up vector
        /// </summary>
        public Vector3 Up
        {
            get
            {
                //                Forward.Cross(Left);
                var q = Quaternion.CreateFromYawPitchRoll(Rotation.Yaw, -Rotation.Pitch, Rotation.Roll);
                return Vector3.Transform(Vector3.Up, q);
            }
        }


        /// <summary>
        /// Get the back vector
        /// </summary>
        public Vector3 Backward => Forward * -1f;


        /// <summary>
        /// Rotate on X axis
        /// </summary>
        public void RotatePitch(float angleRad)
        {
            if (angleRad == 0)
                return;

            _rotation.Pitch += angleRad;
        }

        /// <summary>
        /// Rotate on Y axis
        /// </summary>
        public void RotateYaw(float angleRad)
        {
            if (angleRad == 0)
                return;

            _rotation.Yaw += angleRad;
        }

        /// <summary>
        /// Rotate on Z axis
        /// </summary>
        public void RotateRoll(float angleRad)
        {
            if (angleRad == 0)
                return;

            _rotation.Roll += angleRad;
        }

        /// <summary>
        /// Move to a specific location
        /// </summary>
        public void MoveTo(Vector3 location)
        {
            _location = location;
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
            return Matrix4.CreateRotationMatrixPitch(Rotation.Pitch) * Matrix4.CreateRotationMatrixYaw(Rotation.Yaw) * Matrix4.CreateRotationMatrixRoll(Rotation.Roll);
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
            return Matrix4.CreateRotationMatrixPitchYawRoll(-Rotation.Pitch, -Rotation.Yaw, -Rotation.Roll);
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


        /// <summary>
        /// Overwride the string display
        /// </summary>
        public override string ToString()
        {
            return $"Loc: {_location}; Rot: {_rotation}; Scale: {_scale}";
        }


    }
}
