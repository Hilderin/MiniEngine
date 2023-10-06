
using System;
using System.Runtime.InteropServices;

namespace MiniEngine
{
    /// <summary>
    /// Vector3 struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3: IEquatable<Vector3>
    {
        #region Private Static Fields

        // These are NOT readonly, for weird performance reasons -flibit
        private static Vector3 _zero = new Vector3(0f, 0f, 0f);
        private static Vector3 _one = new Vector3(1f, 1f, 1f);
        private static Vector3 _unitX = new Vector3(1f, 0f, 0f);
        private static Vector3 _unitY = new Vector3(0f, 1f, 0f);
        private static Vector3 _unitZ = new Vector3(0f, 0f, 1f);
        private static Vector3 _up = new Vector3(0f, 1f, 0f);
        private static Vector3 _down = new Vector3(0f, -1f, 0f);
        private static Vector3 _right = new Vector3(1f, 0f, 0f);
        private static Vector3 _left = new Vector3(-1f, 0f, 0f);
        private static Vector3 _forward = new Vector3(0f, 0f, -1f);
        private static Vector3 _backward = new Vector3(0f, 0f, 1f);

        #endregion



        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Vector3"/>.
        /// </summary>
        public float X;

        /// <summary>
        /// The y coordinate of this <see cref="Vector3"/>.
        /// </summary>
        public float Y;

        /// <summary>
        /// The z coordinate of this <see cref="Vector3"/>.
        /// </summary>
        public float Z;


        #endregion

        #region Public Static Properties

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 0, 0, 0.
        /// </summary>
        public static Vector3 Zero
        {
            get
            {
                return _zero;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 1, 1, 1.
        /// </summary>
        public static Vector3 One
        {
            get
            {
                return _one;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 1, 0, 0.
        /// </summary>
        public static Vector3 UnitX
        {
            get
            {
                return _unitX;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 0, 1, 0.
        /// </summary>
        public static Vector3 UnitY
        {
            get
            {
                return _unitY;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 0, 0, 1.
        /// </summary>
        public static Vector3 UnitZ
        {
            get
            {
                return _unitZ;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 0, 1, 0.
        /// </summary>
        public static Vector3 Up
        {
            get
            {
                return _up;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 0, -1, 0.
        /// </summary>
        public static Vector3 Down
        {
            get
            {
                return _down;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 1, 0, 0.
        /// </summary>
        public static Vector3 Right
        {
            get
            {
                return _right;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components -1, 0, 0.
        /// </summary>
        public static Vector3 Left
        {
            get
            {
                return _left;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 0, 0, -1.
        /// </summary>
        public static Vector3 Forward
        {
            get
            {
                return _forward;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> with components 0, 0, 1.
        /// </summary>
        public static Vector3 Backward
        {
            get
            {
                return _backward;
            }
        }

        #endregion



        #region Constructors

        /// <summary>
        /// Constructs a 3d vector with X, Y and Z from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 3d-space.</param>
        /// <param name="y">The y coordinate in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Constructs a 3d vector with X, Y and Z set to the same value.
        /// </summary>
        /// <param name="value">The x, y and z coordinates in 3d-space.</param>
        public Vector3(float value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        public Vector3 Cross(Vector3 vector)
        {
            float x = this.Y * vector.Z - vector.Y * this.Z;
            float y = -(this.X * vector.Z - vector.X * this.Z);
            float z = this.X * vector.Y - vector.X * this.Y;

            return new Vector3(x, y, z);
        }

        /// <summary>
		/// Turns this <see cref="Vector3"/> to a unit vector with the same direction.
		/// </summary>
		public void Normalize()
        {
            float factor = 1.0f / Math.Sqrt(
                (X * X) +
                (Y * Y) +
                (Z * Z)
            );
            X *= factor;
            Y *= factor;
            Z *= factor;
        }

        /// <summary>
        /// Invert the vector (multiplies it by -1)
        /// </summary>
        public void Invert()
        {
            Multiply(-1f);
        }

        /// <summary>
        /// Multiply the vector by a factor
        /// </summary>
        public void Multiply(float factor)
        {
            this.X *= -factor;
            this.Y *= -factor;
            this.Z *= -factor;
        }


        /// <summary>
        /// Gets or sets the component value at the specified zero-based index
        /// in the order of XYZ (index 0 access X, 1 access Y, etc). If
        /// the index is not in range, a value of zero is returned.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <returns>The component value</returns>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Override the ToString
        /// </summary>
        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }



        /// <summary>
        /// Transform the current vector by the specified <see cref="Quaternion"/>, representing the rotation.
        /// </summary>
        /// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
        public void Transform(Quaternion rotation)
        {
            float x = 2 * (rotation.Y * this.Z - rotation.Z * this.Y);
            float y = 2 * (rotation.Z * this.X - rotation.X * this.Z);
            float z = 2 * (rotation.X * this.Y - rotation.Y * this.X);

            this.X = this.X + x * rotation.W + (rotation.Y * z - rotation.Z * y);
            this.Y = this.Y + y * rotation.W + (rotation.Z * x - rotation.X * z);
            this.X = this.Z + z * rotation.W + (rotation.X * y - rotation.Y * x);
        }

        /// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a transformation of 3d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/>.</param>
		/// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
		/// <returns>Transformed <see cref="Vector3"/>.</returns>
		public static Vector3 Transform(Vector3 value, Quaternion rotation)
        {
            Vector3 result;
            Transform(ref value, ref rotation, out result);
            return result;
        }

        /// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a transformation of 3d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/>.</param>
		/// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
		/// <param name="result">Transformed <see cref="Vector3"/> as an output parameter.</param>
		public static void Transform(
            ref Vector3 value,
            ref Quaternion rotation,
            out Vector3 result
        )
        {
            float x = 2 * (rotation.Y * value.Z - rotation.Z * value.Y);
            float y = 2 * (rotation.Z * value.X - rotation.X * value.Z);
            float z = 2 * (rotation.X * value.Y - rotation.Y * value.X);

            result.X = value.X + x * rotation.W + (rotation.Y * z - rotation.Z * y);
            result.Y = value.Y + y * rotation.W + (rotation.Z * x - rotation.X * z);
            result.Z = value.Z + z * rotation.W + (rotation.X * y - rotation.Y * x);
        }

        #endregion


        #region Public static methods

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <param name="result">The cross product of two vectors as an output parameter.</param>
        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            float x = vector1.Y * vector2.Z - vector2.Y * vector1.Z;
            float y = -(vector1.X * vector2.Z - vector2.X * vector1.Z);
            float z = vector1.X * vector2.Y - vector2.X * vector1.Y;

            return new Vector3(x, y, z);
        }

        /// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a normalized values from another vector.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/>.</param>
		/// <param name="result">Unit vector as an output parameter.</param>
		public static Vector3 Normalize(Vector3 value)
        {
            float factor = 1.0f / (float)Math.Sqrt(
                (value.X * value.X) +
                (value.Y * value.Y) +
                (value.Z * value.Z)
            );

            return new Vector3(value.X * factor, value.Y * factor, value.Z * factor);
        }

        /// <summary>
        /// Returns a dot product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The dot product of two vectors.</returns>
        public static float Dot(Vector3 vector1, Vector3 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }


        /// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
        {
            return (obj is Vector3) && Equals((Vector3)obj);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Vector3"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vector3"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector3 other)
        {
            return (X == other.X &&
                    Y == other.Y &&
                    Z == other.Z);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Vector3"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Vector3"/>.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }

        #endregion

        #region Public Static Operators

        /// <summary>
        /// Compares whether two <see cref="Vector3"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector3"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="Vector3"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Vector3 value1, Vector3 value2)
        {
            return (value1.X == value2.X &&
                    value1.Y == value2.Y &&
                    value1.Z == value2.Z);
        }

        /// <summary>
        /// Compares whether two <see cref="Vector3"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector3"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="Vector3"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Vector3 value1, Vector3 value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="Vector3"/> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        /// <summary>
        /// Inverts values in the specified <see cref="Vector3"/>.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3"/> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Vector3 operator -(Vector3 value)
        {
            value = new Vector3(-value.X, -value.Y, -value.Z);
            return value;
        }

        /// <summary>
        /// Subtracts a <see cref="Vector3"/> from a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Vector3"/> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of two vectors by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3"/> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="Vector3"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication.</returns>
        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3"/> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector3 operator *(Vector3 value, float scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="Vector3"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector3 operator *(float scaleFactor, Vector3 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector3"/> by the components of another <see cref="Vector3"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector3"/> on the left of the div sign.</param>
        /// <param name="value2">Divisor <see cref="Vector3"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector3"/> by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Vector3"/> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector3 operator /(Vector3 value, float divider)
        {
            float factor = 1 / divider;
            value.X *= factor;
            value.Y *= factor;
            value.Z *= factor;
            return value;
        }

        #endregion

    }
}
