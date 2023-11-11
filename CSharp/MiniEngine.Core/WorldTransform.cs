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
        /// Event when the transform moved, rotate or scale
        /// </summary>
        public event OnTransformChangedHandler OnChanged;


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
                    _location = value;
                    OnChanged?.Invoke();
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
                    _scale = value;
                    OnChanged?.Invoke();
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
                    _rotation = value;
                    OnChanged?.Invoke();
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
        /// Get the up vector
        /// </summary>
        public Vector3 Down => Up * -1f;


        /// <summary>
        /// Get the back vector
        /// </summary>
        public Vector3 Backward => Forward * -1f;


        /// <summary>
        /// Rotate on X axis
        /// </summary>
        public WorldTransform RotatePitch(float angleRad)
        {
            if (float.IsNaN(angleRad))
            {
                Debug.Warning("Rotation Pitch NaN");
            }
            else
            {
                _rotation.Pitch += angleRad;
                OnChanged?.Invoke();
            }
            return this;
        }

        /// <summary>
        /// Rotate on Y axis
        /// </summary>
        public WorldTransform RotateYaw(float angleRad)
        {
            if (float.IsNaN(angleRad))
            {
                Debug.Warning("Rotation Yaw NaN");
            }
            else
            {
                _rotation.Yaw += angleRad;
                OnChanged?.Invoke();
            }
            return this;
        }

        /// <summary>
        /// Rotate on Z axis
        /// </summary>
        public WorldTransform RotateRoll(float angleRad)
        {
            if (float.IsNaN(angleRad))
            {
                Debug.Warning("Rotation Roll NaN");
            }
            else
            {
                _rotation.Roll += angleRad;
                OnChanged?.Invoke();
            }
            return this;
        }

        /// <summary>
        /// Move to a specific location
        /// </summary>
        public WorldTransform MoveTo(Vector3 location)
        {
            _location = location;
            OnChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Move forward considering the rotation of the world transform
        /// </summary>
        public WorldTransform MoveForward(float distance)
        {
            _location += this.Forward * -distance;
            OnChanged?.Invoke();
            return this;
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
        public WorldTransform MoveInDirections(float distance, Vector3 directions)
        {
            Vector3 movement = Vector3.Zero;
            if (!Math.IsZero(directions.Z))
                movement += this.Forward * directions.Z * distance;
            if (!Math.IsZero(directions.X))
                movement += this.Left * directions.X * -distance;
            if (!Math.IsZero(directions.Y))
                movement += this.Up * directions.Y * distance;

            if (movement != Vector3.Zero)
            {
                _location += movement;
                OnChanged?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// Move backward considering the rotation of the world transform
        /// </summary>
        public WorldTransform MoveBackward(float distance)
        {
            Location += this.Forward * distance;
            return this;
        }

        /// <summary>
        /// Move left considering the rotation of the world transform
        /// </summary>
        public WorldTransform MoveLeft(float distance)
        {
            _location += this.Left * -distance;
            OnChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Move left considering the rotation of the world transform
        /// </summary>
        public WorldTransform MoveRight(float distance)
        {
            _location += this.Left * distance;
            OnChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Move up considering the rotation of the world transform
        /// </summary>
        public WorldTransform MoveUp(float distance)
        {
            _location += this.Up * distance;
            OnChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Move down considering the rotation of the world transform
        /// </summary>
        public WorldTransform MoveDown(float distance)
        {
            _location += this.Down * -distance;
            OnChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Add scale
        /// </summary>
        public WorldTransform AddScale(float scale)
        {
            _scale.X += scale;
            _scale.Y += scale;
            _scale.Z += scale;
            OnChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Set scale
        /// </summary>
        public WorldTransform SetScale(float scale)
        {
            _scale.X = scale;
            _scale.Y = scale;
            _scale.Z = scale;
            OnChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Set scale
        /// </summary>
        public WorldTransform SetScale(float scaleX, float scaleY, float scaleZ)
        {
            _scale.X = scaleX;
            _scale.Y = scaleY;
            _scale.Z = scaleZ;
            OnChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Set scale
        /// </summary>
        public WorldTransform SetScale(Vector3 scale)
        {
            _scale = scale;
            OnChanged?.Invoke();
            return this;
        }

        ///// <summary>
        ///// Move in 4 directions
        ///// </summary>
        //public WorldTransform Move(float distance, bool forward, bool backword, bool left, bool right)
        //{
        //    if (forward)
        //        MoveForward(distance);
        //    if (backword)
        //        MoveBackward(distance);
        //    if (left)
        //        MoveLeft(distance);
        //    if (right)
        //        MoveRight(distance);
        //    return this;
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
