using System;
using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;

namespace MiniEngine
{
    /// <summary>
    /// Maths methods
    /// </summary>
    public static class Math
    {

        /// <summary>
        /// Represents the value of pi(3.14159274).
        /// </summary>
        public const float Pi = MathF.PI;

        /// <summary>
        /// Represents the value of pi divided by two(1.57079637).
        /// </summary>
        public const float PiOver2 = MathF.PI / 2.0f;

        /// <summary>
        /// Represents the value of pi divided by four (0.7853982).
        /// </summary>
        public const float PiOver4 = MathF.PI / 4.0f;

        /// <summary>
        /// Represents the value of pi divided by minus four (-0.7853982).
        /// </summary>
        public const float MinusPiMinusOver4 = MathF.PI * -0.25f;

        /// <summary>
        /// Represents the value of pi times two (6.28318548).
        /// </summary>
        public const float TwoPi = MathF.PI * 2.0f;

        /// <summary>
        /// Represents the value of pi multiplied by 0.75 (2.3562).
        /// </summary>
        public const float PiThreeQuarter = MathF.PI * 0.75f;

        /// <summary>
        /// Represents the value of pi multiplied by minus 0.75 (-2.3562).
        /// </summary>
        public const float PiMinusThreeQuarter = MathF.PI * -0.75f;


        /// <summary>
        /// Random object
        /// </summary>
        private static Random _random = new Random();

        /// <summary>
        /// Espilon
        /// </summary>
        private static float _epsilon = GetMachineEpsilonFloat();


        /// <summary>
        /// The value we use to avoid floating point precision issues
        /// http://sandervanrossen.blogspot.com/2009/12/realtime-csg-part-1.html
        /// </summary>
        public static float Epsilon { get { return _epsilon; } }



        /// <summary>
        /// Calculate the cosine of a radian angle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float angleRad)
        {
            return MathF.Cos(angleRad);
        }

        /// <summary>
        /// Calculate the sinus of a radian angle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float angleRad)
        {
            return MathF.Sin(angleRad);
        }

        /// <summary>
        /// Calculate the tangent of a radian angle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan(float angleRad)
        {
            return MathF.Tan(angleRad);
        }

        /// <summary>
        /// Returns the angle whose cosine is the specified number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Acos(float angleRad)
        {
            return MathF.Acos(angleRad);
        }

        /// <summary>
        /// Converti un rad en degree
        /// </summary>
        public static float RadToDeg(float radians)
        {
            float degrees = (180.0f / Pi) * radians % 360.0f;
            //while (degrees > 360)
            //    degrees -= 360;
            //while (degrees < -360)
            //    degrees += 360;
            return degrees;
        }

        /// <summary>
        /// Converti un degree en rad
        /// </summary>
        public static float DegToRad(float angle)
        {
            return (Pi / 180.0f) * angle;
        }

        /// <summary>
        /// Permet d'arroundir une valeur float
        /// </summary>
        public static int Round(float value, int digits)
        {
            return (int)MathF.Round(value, digits, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Permet d'arroundir une valeur double
        /// </summary>
        public static int Round(double value, int digits)
        {
            return (int)System.Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Permet d'arroundir une valeur float en int
        /// </summary>
        public static int RoundInt(float value)
        {
            return (int)MathF.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Permet d'arroundir une valeur double en int
        /// </summary>
        public static int RoundInt(double value)
        {
            return (int)System.Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }


        /// <summary>
        /// Permet d'arroundir à la valeur supérieur en int dans un multiple de X
        /// </summary>
        public static int RoundUp(int value, int multipleOf)
        {
            int size_difference = multipleOf - (((value - 1) % multipleOf) + 1);

            return value + size_difference;
        }

        /// <summary>
        /// Permet d'arroundir à la valeur supérieur en int dans un multiple de X
        /// </summary>
        public static uint RoundUp(uint value, uint multipleOf)
        {
            return value + (multipleOf - (((value - 1) % multipleOf) + 1));
            
        }


        // <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">
        /// The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c>
        /// will be returned.
        /// </param>
        /// <param name="max">
        /// The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c>
        /// will be returned.
        /// </param>
        /// <returns>The clamped value.</returns>
        public static float Clamp(float value, float min, float max)
        {
            // First we check to see if we're greater than the max.
            if (value > max)
                return max;

            // Then we check to see if we're less than the min.
            if (value < min)
                return min;

            // There's no check to see if min > max.
            return value;
        }

        /// <summary>
        /// Get a random int between 2 values (max is exclusive)
        /// </summary>
        public static int RandomInt(int min, int exlusiveMax)
        {
            return _random.Next(min, exlusiveMax);
        }

        /// <summary>
        /// Get a random int between 2 values (max is exclusive)
        /// </summary>
        public static float RandomFloat(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min)) + min;
        }

        /// <summary>
        /// Calculate the square value
        /// </summary>
        public static float Sqrt(float value)
        {
            return MathF.Sqrt(value);
        }

        /// <summary>
        /// Calculate the absolute value
        /// </summary>
        public static float Abs(float value)
        {
            return MathF.Abs(value);
        }

        /// <summary>
        /// Return the floor value
        /// </summary>
        public static float Floor(float value)
        {
            return MathF.Floor(value);
        }

        /// <summary>
        /// Returns the base 2 logarithm of a number
        /// </summary>
        public static float Log2(float value)
        {
            return MathF.Log2(value);
        }

        /// <summary>
        /// Return a byte from a float base 1
        /// </summary>
        public static byte Float1ToByte(float value)
        {
            value *= 255;
            if (value > 255)
                return 255;
            if (value < 0)
                return 0;
            return (byte)value;
        }

        /// <summary>
        /// Return a byte from a float base 1
        /// </summary>
        public static float ByteToFloat1(byte value)
        {
            return value / 255f;
        }

        /// <summary>
        /// Swap 2 values
        /// </summary>
        public static void SwapValues<T>(ref T valueA, ref T valueB)
        {
            (valueB, valueA) = (valueA, valueB);
        }

        /// <summary>
        /// Test if a float is the same as another float
        /// </summary>
        public static bool AreFloatsEqual(float a, float b)
        {
            float diff = a - b;

            float e = Epsilon;

            if (diff < e && diff > -e)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Clamp list indices
        /// Will even work if index is larger/smaller than listSize, so can loop multiple times
        /// </summary>
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }

        /// <summary>
		/// Linearly interpolates between two values.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <param name="amount">
		/// Value between 0 and 1 indicating the weight of value2.
		/// </param>
		/// <returns>Interpolated value.</returns>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <c>value1 + (value2 - value1) * amount</c>
		/// Passing amount a value of 0 will cause value1 to be returned, a value of 1 will
		/// cause value2 to be returned.
		/// </remarks>
		public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        /// <summary>
		/// Returns the greater of two values.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <returns>The greater value.</returns>
		public static float Max(float value1, float value2)
        {
            return value1 > value2 ? value1 : value2;
        }

        /// <summary>
		/// Returns the greater of two values.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <returns>The greater value.</returns>
		public static int Max(int value1, int value2)
        {
            return value1 > value2 ? value1 : value2;
        }

        /// <summary>
        /// Returns the lesser of two values.
        /// </summary>
        /// <param name="value1">Source value.</param>
        /// <param name="value2">Source value.</param>
        /// <returns>The lesser value.</returns>
        public static float Min(float value1, float value2)
        {
            return value1 < value2 ? value1 : value2;
        }

        /// <summary>
        /// Returns the lesser of two values.
        /// </summary>
        /// <param name="value1">Source value.</param>
        /// <param name="value2">Source value.</param>
        /// <returns>The lesser value.</returns>
        public static int Min(int value1, int value2)
        {
            return value1 < value2 ? value1 : value2;
        }


        /// <summary>
		/// Interpolates between two values using a cubic equation.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <param name="amount">Weighting value.</param>
		/// <returns>Interpolated value.</returns>
		public static float SmoothStep(float value1, float value2, float amount)
        {
            /* It is expected that 0 < amount < 1.
			 * If amount < 0, return value1.
			 * If amount > 1, return value2.
			 */
            float result = Math.Clamp(amount, 0f, 1f);
            result = Math.Hermite(value1, 0f, value2, 0f, result);

            return result;
        }

        /// <summary>
		/// Performs a Hermite spline interpolation.
		/// </summary>
		/// <param name="value1">Source position.</param>
		/// <param name="tangent1">Source tangent.</param>
		/// <param name="value2">Source position.</param>
		/// <param name="tangent2">Source tangent.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The result of the Hermite spline interpolation.</returns>
		public static float Hermite(
            float value1,
            float tangent1,
            float value2,
            float tangent2,
            float amount
        )
        {
            /* All transformed to double not to lose precision
			 * Otherwise, for high numbers of param:amount the result is NaN instead
			 * of Infinity.
			 */
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount;
            double result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (IsDiffZero(amount, 0f))
            {
                result = value1;
            }
            else if (IsDiffZero(amount, 1f))
            {
                result = value2;
            }
            else
            {
                result = (
                    ((2 * v1 - 2 * v2 + t2 + t1) * sCubed) +
                    ((3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared) +
                    (t1 * s) +
                    v1
                );
            }

            return (float)result;
        }

        /// <summary>
        /// Check if the value est within epsilon range
        /// </summary>
        public static bool IsZero(float value)
        {
            return Math.Abs(value) < Epsilon;
        }

        /// <summary>
        /// Check if the difference between 2 vectors is in epsilon range
        /// </summary>
        public static bool IsDiffZero(float floatA, float floatB)
        {
            return Math.Abs(floatA - floatB) < Epsilon;
        }


        #region Private Static Methods

        /// <summary>
        /// Find the current machine's Epsilon for the float data type.
        /// (That is, the largest float, e,  where e == 0.0f is true.)
        /// </summary>
        private static float GetMachineEpsilonFloat()
        {
            float machineEpsilon = 1.0f;
            float comparison;

            /* Keep halving the working value of machineEpsilon until we get a number that
			 * when added to 1.0f will still evaluate as equal to 1.0f.
			 */
            do
            {
                machineEpsilon *= 0.5f;
                comparison = 1.0f + machineEpsilon;
            }
            while (comparison > 1.0f);

            return machineEpsilon;
        }

        #endregion


    }
}
