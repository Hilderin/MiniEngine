using System;
using System.Runtime.InteropServices;

namespace MiniEngine
{
    /// <summary>
    /// Vector that reprensents a rotation
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rotator3 : IEquatable<Rotator3>
    {

        #region Private Static Fields

        // These are NOT readonly, for weird performance reasons -flibit
        private static Rotator3 _zero = new Rotator3(0f, 0f, 0f);

        #endregion

        #region Public Fields

        /// <summary>
        /// Rotation on X in radians
        /// Negative angles = downward Pitch rotations. (Down)
        /// Positive angles = upward Pitch rotations. (Up)
        /// </summary>
        public float Pitch;

        /// <summary>
        /// Rotation on Y in radians
        /// Negative angles = leftward Yaw (Left)
        /// Positive angles = rightward Yaw (Right)
        /// </summary>
        public float Yaw;

        /// <summary>
        /// Rotation on Z in radians
        /// Negative angles = counterclockwise Roll
        /// Positive angles = clockwise Roll
        /// </summary>
        public float Roll;

        /// <summary>
        /// Rotation on X in degrees
        /// </summary>
        public float PitchDeg
        {
            get { return Math.RadToDeg(Pitch); }
            set { Pitch = Math.RadToDeg(value); }
        }

        /// <summary>
        /// Rotation on Y in degrees
        /// </summary>
        public float YawDeg
        {
            get { return Math.RadToDeg(Yaw); }
            set { Yaw = Math.RadToDeg(value); }
        }

        /// <summarZ>
        /// Rotation on Z in degrees
        /// </summarZ>
        public float RollDeg
        {
            get { return Math.RadToDeg(Roll); }
            set { Roll = Math.RadToDeg(value); }
        }

        #endregion



        #region Public Static Properties

        /// <summary>
        /// Returns a <see cref="Rotator3"/> with components 0, 0, 0.
        /// </summary>
        public static Rotator3 Zero
        {
            get
            {
                return _zero;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a 3d rotator with X, Y, Z in radians
        /// </summary>
        /// <param name="pitch">The pitch coordinate in 3d-space.</param>
        /// <param name="yaw">The yaw coordinate in 3d-space.</param>
        /// <param name="roll">The roll coordinate in 3d-space.</param>
        public Rotator3(float pitch, float yaw, float roll)
        {
            this.Pitch = pitch;
            this.Yaw = yaw;
            this.Roll = roll;
        }


        #endregion


        #region Public methods

        /// <summary>
		/// Turns this <see cref="Vector3"/> to a unit vector with the same direction.
		/// </summary>
		public void Normalize()
        {
            float factor = 1.0f / Math.Sqrt(
                (Pitch * Pitch) +
                (Yaw * Yaw) +
                (Roll * Roll)
            );
            Pitch *= factor;
            Yaw *= factor;
            Roll *= factor;
        }

        /// <summary>
        /// Invert the vector (multiplies it by -1)
        /// </summary>
        public void Invert()
        {
            this.Pitch = -this.Pitch;
            this.Yaw = -this.Yaw;
            this.Roll = -this.Roll;
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
                        return Pitch;
                    case 1:
                        return Yaw;
                    case 2:
                        return Roll;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Pitch = value;
                        break;
                    case 1:
                        Yaw = value;
                        break;
                    case 2:
                        Roll = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Override the ToString
        /// </summary>
        public override string ToString()
        {
            return $"{PitchDeg.ToString("0.###")}°, {YawDeg.ToString("0.###")}°, {RollDeg.ToString("0.###")}° ({Pitch}, {Yaw}, {Roll})";
        }


        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Rotator3) && Equals((Rotator3)obj);
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Rotator3"/>.
        /// </summary>
        /// <param name="other">The <see cref="Rotator3"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Rotator3 other)
        {
            return (Pitch == other.Pitch &&
                    Yaw == other.Yaw &&
                    Roll == other.Roll);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Rotator3"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Rotator3"/>.</returns>
        public override int GetHashCode()
        {
            return Pitch.GetHashCode() + Yaw.GetHashCode() + Roll.GetHashCode();
        }


        #endregion

        #region Public static methods

        /// <summary>
        /// Create a Rotator from degrees
        /// </summary>
        public static Rotator3 FromDegrees(float xDegrees, float yDegrees, float zDegrees)
        {
            return new Rotator3(Math.DegToRad(xDegrees), Math.DegToRad(yDegrees), Math.DegToRad(zDegrees));
        }


        #endregion

        #region Public Static Operators

        /// <summary>
        /// Compares whether two <see cref="Rotator3"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="Rotator3"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="Rotator3"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Rotator3 value1, Rotator3 value2)
        {
            return (value1.Pitch == value2.Pitch &&
                    value1.Yaw == value2.Yaw &&
                    value1.Roll == value2.Roll);
        }

        /// <summary>
        /// Compares whether two <see cref="Rotator3"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="Rotator3"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="Rotator3"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Rotator3 value1, Rotator3 value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="Rotator3"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="Rotator3"/> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Rotator3 operator +(Rotator3 value1, Rotator3 value2)
        {
            value1.Pitch += value2.Pitch;
            value1.Yaw += value2.Yaw;
            value1.Roll += value2.Roll;
            return value1;
        }

        /// <summary>
        /// Inverts values in the specified <see cref="Rotator3"/>.
        /// </summary>
        /// <param name="value">Source <see cref="Rotator3"/> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Rotator3 operator -(Rotator3 value)
        {
            value = new Rotator3(-value.Pitch, -value.Yaw, -value.Roll);
            return value;
        }

        /// <summary>
        /// Subtracts a <see cref="Rotator3"/> from a <see cref="Rotator3"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Rotator3"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Rotator3"/> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Rotator3 operator -(Rotator3 value1, Rotator3 value2)
        {
            value1.Pitch -= value2.Pitch;
            value1.Yaw -= value2.Yaw;
            value1.Roll -= value2.Roll;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of two vectors by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="Rotator3"/> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="Rotator3"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication.</returns>
        public static Rotator3 operator *(Rotator3 value1, Rotator3 value2)
        {
            value1.Pitch *= value2.Pitch;
            value1.Yaw *= value2.Yaw;
            value1.Roll *= value2.Roll;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Rotator3"/> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Rotator3 operator *(Rotator3 value, float scaleFactor)
        {
            value.Pitch *= scaleFactor;
            value.Yaw *= scaleFactor;
            value.Roll *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Multiplies the components of vector by a scalar.
        /// </summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="Rotator3"/> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Rotator3 operator *(float scaleFactor, Rotator3 value)
        {
            value.Pitch *= scaleFactor;
            value.Yaw *= scaleFactor;
            value.Roll *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="Rotator3"/> by the components of another <see cref="Rotator3"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Rotator3"/> on the left of the div sign.</param>
        /// <param name="value2">Divisor <see cref="Rotator3"/> on the right of the div sign.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Rotator3 operator /(Rotator3 value1, Rotator3 value2)
        {
            value1.Pitch /= value2.Pitch;
            value1.Yaw /= value2.Yaw;
            value1.Roll /= value2.Roll;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="Rotator3"/> by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Rotator3"/> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Rotator3 operator /(Rotator3 value, float divider)
        {
            float factor = 1 / divider;
            value.Pitch *= factor;
            value.Yaw *= factor;
            value.Roll *= factor;
            return value;
        }


        #endregion

    }
}
