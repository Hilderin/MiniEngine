using System;
using System.Runtime.InteropServices;

namespace MiniEngine
{
    /// <summary>
    /// Vector2 struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2: IEquatable<Vector2>
    {
        #region Private Static Fields

        // These are NOT readonly, for weird performance reasons -flibit
        private static Vector2 _zero = new Vector2(0f, 0f);
        private static Vector2 _one = new Vector2(1f, 1f);

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Returns a <see cref="Vector2"/> with components 0, 0
        /// </summary>
        public static Vector2 Zero
        {
            get
            {
                return _zero;
            }
        }

        /// <summary>
        /// Returns a <see cref="Vector2"/> with components 1, 1
        /// </summary>
        public static Vector2 One
        {
            get
            {
                return _one;
            }
        }

        #endregion


        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Vector2"/>.
        /// </summary>
        public float X;

        /// <summary>
        /// The y coordinate of this <see cref="Vector2"/>.
        /// </summary>
        public float Y;


        #endregion


        #region Constructors

        /// <summary>
        /// Constructs a 2d vector with X and Y from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 2d-space.</param>
        /// <param name="y">The y coordinate in 2d-space.</param>
        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Constructs a 2d vector with X and Y set to the same value.
        /// </summary>
        /// <param name="value">The x and y coordinates in 3d-space.</param>
        public Vector2(float value)
        {
            this.X = value;
            this.Y = value;
        }

        #endregion


        #region Public methods

        /// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
        {
            return (obj is Vector2) && Equals((Vector2)obj);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Vector2"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vector2"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector2 other)
        {
            return (X == other.X &&
                    Y == other.Y);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Vector2"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Vector2"/>.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }


        /// <summary>
		/// Turns this <see cref="Vector2"/> to a unit vector with the same direction.
		/// </summary>
		public void Normalize()
        {
            float val = 1.0f / Math.Sqrt((X * X) + (Y * Y));
            X *= val;
            Y *= val;
        }

        /// <summary>
        /// Override the ToString
        /// </summary>
        public override string ToString()
        {
            return $"{X}, {Y}";
        }

        #endregion


        #region Public Static Operators

        /// <summary>
        /// Compares whether two <see cref="Vector2"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector2"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="Vector2"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Vector2 value1, Vector2 value2)
        {
            return (value1.X == value2.X &&
                    value1.Y == value2.Y);
        }

        /// <summary>
        /// Compares whether two <see cref="Vector2"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="Vector2"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="Vector2"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Vector2 value1, Vector2 value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="Vector2"/> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Vector2 operator +(Vector2 value1, Vector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        /// <summary>
        /// Inverts values in the specified <see cref="Vector2"/>.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2"/> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Vector2 operator -(Vector2 value)
        {
            value = new Vector2(-value.X, -value.Y);
            return value;
        }

        /// <summary>
        /// Subtracts a <see cref="Vector2"/> from a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Vector2"/> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Vector2 operator -(Vector2 value1, Vector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of two vectors by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2"/> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="Vector2"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication.</returns>
        public static Vector2 operator *(Vector2 value1, Vector2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2"/> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector2 operator *(Vector2 value, float scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="Vector2"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector2 operator *(float scaleFactor, Vector2 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector2"/> by the components of another <see cref="Vector2"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Vector2"/> on the left of the div sign.</param>
        /// <param name="value2">Divisor <see cref="Vector2"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector2 operator /(Vector2 value1, Vector2 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Vector2"/> by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Vector2"/> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector2 operator /(Vector2 value, float divider)
        {
            float factor = 1 / divider;
            value.X *= factor;
            value.Y *= factor;
            return value;
        }

        #endregion

    }
}
