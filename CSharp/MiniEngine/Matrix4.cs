
using System.Runtime.InteropServices;

namespace MiniEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4
    {
        #region Static members

        /// <summary>
        /// Identity matrix
        /// </summary>
        private static Matrix4 _identity = new Matrix4(1.0f, 0.0f, 0.0f, 0.0f, 
                                                       0.0f, 1.0f, 0.0f, 0.0f, 
                                                       0.0f, 0.0f, 1.0f, 0.0f,
                                                       0.0f, 0.0f, 0.0f, 1.0f);

        #endregion

        #region Public Fields

        /// <summary>
        /// A first row and first column value.
        /// </summary>
        public float M11;

		/// <summary>
		/// A first row and second column value.
		/// </summary>
		public float M12;

		/// <summary>
		/// A first row and third column value.
		/// </summary>
		public float M13;

		/// <summary>
		/// A first row and fourth column value.
		/// </summary>
		public float M14;

		/// <summary>
		/// A second row and first column value.
		/// </summary>
		public float M21;

		/// <summary>
		/// A second row and second column value.
		/// </summary>
		public float M22;

		/// <summary>
		/// A second row and third column value.
		/// </summary>
		public float M23;

		/// <summary>
		/// A second row and fourth column value.
		/// </summary>
		public float M24;

		/// <summary>
		/// A third row and first column value.
		/// </summary>
		public float M31;

		/// <summary>
		/// A third row and second column value.
		/// </summary>
		public float M32;

		/// <summary>
		/// A third row and third column value.
		/// </summary>
		public float M33;

		/// <summary>
		/// A third row and fourth column value.
		/// </summary>
		public float M34;

		/// <summary>
		/// A fourth row and first column value.
		/// </summary>
		public float M41;

		/// <summary>
		/// A fourth row and second column value.
		/// </summary>
		public float M42;

		/// <summary>
		/// A fourth row and third column value.
		/// </summary>
		public float M43;

		/// <summary>
		/// A fourth row and fourth column value.
		/// </summary>
		public float M44;

        #endregion

        #region Constructors

        ///// <summary>
        ///// Constructs a matrix all zeros
        ///// </summary>
        //public Matrix4()
        //{ 
        //}

        /// <summary>
		/// Constructs a matrix.
		/// </summary>
		/// <param name="m11">A first row and first column value.</param>
		/// <param name="m12">A first row and second column value.</param>
		/// <param name="m13">A first row and third column value.</param>
		/// <param name="m14">A first row and fourth column value.</param>
		/// <param name="m21">A second row and first column value.</param>
		/// <param name="m22">A second row and second column value.</param>
		/// <param name="m23">A second row and third column value.</param>
		/// <param name="m24">A second row and fourth column value.</param>
		/// <param name="m31">A third row and first column value.</param>
		/// <param name="m32">A third row and second column value.</param>
		/// <param name="m33">A third row and third column value.</param>
		/// <param name="m34">A third row and fourth column value.</param>
		/// <param name="m41">A fourth row and first column value.</param>
		/// <param name="m42">A fourth row and second column value.</param>
		/// <param name="m43">A fourth row and third column value.</param>
		/// <param name="m44">A fourth row and fourth column value.</param>
		public Matrix4(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44
        )
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        #endregion

        #region Public methods



        /// <summary>
        /// ToString of the matrix
        /// </summary>
        public override string ToString()
        {
            return $@"[{M11}, {M12}, {M13}, {M14}][{M21}, {M22}, {M23}, {M24}][{M31}, {M32}, {M33}, {M34}][{M41}, {M42}, {M43}, {M44}]";
        }


        #endregion

        #region Public static properties

        /// <summary>
        /// Gets the identity matrix.
        /// </summary>
        public static Matrix4 Identity
        {
            get
            {
                return _identity;
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Creation a translation matrix
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Matrix4 CreateTranslationMatrix(float x, float y, float z)
        {
            Matrix4 newMatrix = new Matrix4();

            newMatrix.M11 = 1;
            newMatrix.M22 = 1;
            newMatrix.M33 = 1;
            newMatrix.M14 = x;
            newMatrix.M24 = y;
            newMatrix.M34 = z;
            newMatrix.M44 = 1;

            return newMatrix;
        }

        /// <summary>
        /// Create a rotation matrix on X
        /// </summary>
        public static Matrix4 CreateRotationMatrixX(float angleRad)
        {
            Matrix4 newMatrix = new Matrix4();

            float cos = Math.Cos(angleRad);
            float sin = Math.Sin(angleRad);

            newMatrix.M11 = 1.0f;
            newMatrix.M22 = cos;
            newMatrix.M23 = Math.Sin(angleRad);
            newMatrix.M32 = -sin;
            newMatrix.M33 = cos;
            newMatrix.M44 = 1.0f;

            return newMatrix;
        }


        /// <summary>
        /// Create a rotation matrix on Y
        /// </summary>
        public static Matrix4 CreateRotationMatrixY(float angleRad)
        {
            
            Matrix4 newMatrix = new Matrix4();

            float cos = Math.Cos(angleRad);
            float sin = Math.Sin(angleRad);

            newMatrix.M11 = cos;
            newMatrix.M13 = -sin;
            newMatrix.M22 = 1.0f;
            newMatrix.M31 = sin;
            newMatrix.M33 = cos;
            newMatrix.M44 = 1.0f;

            return newMatrix;
        }


        /// <summary>
        /// Create a rotation matrix on Z
        /// </summary>
        public static Matrix4 CreateRotationMatrixZ(float angleRad)
        {
            Matrix4 newMatrix = new Matrix4();

            float cos = Math.Cos(angleRad);
            float sin = Math.Sin(angleRad);

            newMatrix.M11 = cos;
            newMatrix.M12 = sin;
            newMatrix.M21 = -sin;
            newMatrix.M22 = cos;
            newMatrix.M33 = 1.0f;
            newMatrix.M44 = 1.0f;

            return newMatrix;
        }

        /// <summary>
        /// Create a scale matrix
        /// </summary>
        public static Matrix4 CreateScaleMatrix(float x, float y, float z)
        {
            Matrix4 newMatrix = new Matrix4();

            newMatrix.M11 = x;
            newMatrix.M22 = y;
            newMatrix.M33 = z;
            newMatrix.M44 = 1.0f;

            return newMatrix;
        }

        /// <summary>
        /// Create a projection matrix
        /// </summary>
        public static Matrix4 CreateProjection(float fov, float width, float height, float nearZ, float farZ)
        {
            Matrix4 newMatrix = new Matrix4();

            float tanHalfFOV = Math.Tan(Math.DegToRad(fov / 2.0f));
            float f = 1 / tanHalfFOV;
            float aspectRatio = width / height;

            float zRange = nearZ - farZ;

            float A = (-farZ - nearZ) / zRange;
            float B = 2.0f * farZ * nearZ / zRange;

            newMatrix.M11 = f / aspectRatio;
            newMatrix.M22 = f;
            newMatrix.M33 = A;
            newMatrix.M34 = B;
            newMatrix.M43 = 1.0f;

            return newMatrix;
        }


        /// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <returns>The new <see cref="Matrix4"/> which contains the transposing result.</returns>
		public static Matrix4 Transpose(ref Matrix4 matrix)
        {
            Matrix4 ret;
            Transpose(ref matrix, out ret);
            return ret;
        }

        /// <summary>
        /// Swap the matrix rows and columns.
        /// </summary>
        /// <param name="matrix">The matrix for transposing operation.</param>
        /// <param name="result">The new <see cref="Matrix4"/> which contains the transposing result as an output parameter.</param>
        public static void Transpose(ref Matrix4 matrix, out Matrix4 result)
        {
            Matrix4 ret;

            ret.M11 = matrix.M11;
            ret.M12 = matrix.M21;
            ret.M13 = matrix.M31;
            ret.M14 = matrix.M41;

            ret.M21 = matrix.M12;
            ret.M22 = matrix.M22;
            ret.M23 = matrix.M32;
            ret.M24 = matrix.M42;

            ret.M31 = matrix.M13;
            ret.M32 = matrix.M23;
            ret.M33 = matrix.M33;
            ret.M34 = matrix.M43;

            ret.M41 = matrix.M14;
            ret.M42 = matrix.M24;
            ret.M43 = matrix.M34;
            ret.M44 = matrix.M44;

            result = ret;
        }

        #endregion


        #region Opérations

        /// <summary>
        /// Creates a new <see cref="Matrix4"/> that contains a multiplication of two matrix.
        /// </summary>
        /// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
        /// <param name="matrix2">Source <see cref="Matrix4"/>.</param>
        /// <returns>Result of the matrix multiplication.</returns>
        public static Matrix4 operator *(Matrix4 matrix1, Matrix4 matrix2)
        {
            return new Matrix4(
            (
                (matrix1.M11 * matrix2.M11) +
                (matrix1.M12 * matrix2.M21) +
                (matrix1.M13 * matrix2.M31) +
                (matrix1.M14 * matrix2.M41)
            )
            ,
            (
                (matrix1.M11 * matrix2.M12) +
                (matrix1.M12 * matrix2.M22) +
                (matrix1.M13 * matrix2.M32) +
                (matrix1.M14 * matrix2.M42)
            )
            ,
            (
                (matrix1.M11 * matrix2.M13) +
                (matrix1.M12 * matrix2.M23) +
                (matrix1.M13 * matrix2.M33) +
                (matrix1.M14 * matrix2.M43)
            )
            ,
            (
                (matrix1.M11 * matrix2.M14) +
                (matrix1.M12 * matrix2.M24) +
                (matrix1.M13 * matrix2.M34) +
                (matrix1.M14 * matrix2.M44)
            )
            ,
            (
                (matrix1.M21 * matrix2.M11) +
                (matrix1.M22 * matrix2.M21) +
                (matrix1.M23 * matrix2.M31) +
                (matrix1.M24 * matrix2.M41)
            )
            ,
            (
                (matrix1.M21 * matrix2.M12) +
                (matrix1.M22 * matrix2.M22) +
                (matrix1.M23 * matrix2.M32) +
                (matrix1.M24 * matrix2.M42)
            )
            ,
            (
                (matrix1.M21 * matrix2.M13) +
                (matrix1.M22 * matrix2.M23) +
                (matrix1.M23 * matrix2.M33) +
                (matrix1.M24 * matrix2.M43)
            )
            ,
            (
                (matrix1.M21 * matrix2.M14) +
                (matrix1.M22 * matrix2.M24) +
                (matrix1.M23 * matrix2.M34) +
                (matrix1.M24 * matrix2.M44)
            )
            ,
            (
                (matrix1.M31 * matrix2.M11) +
                (matrix1.M32 * matrix2.M21) +
                (matrix1.M33 * matrix2.M31) +
                (matrix1.M34 * matrix2.M41)
            )
            ,
            (
                (matrix1.M31 * matrix2.M12) +
                (matrix1.M32 * matrix2.M22) +
                (matrix1.M33 * matrix2.M32) +
                (matrix1.M34 * matrix2.M42)
            )
            ,
            (
                (matrix1.M31 * matrix2.M13) +
                (matrix1.M32 * matrix2.M23) +
                (matrix1.M33 * matrix2.M33) +
                (matrix1.M34 * matrix2.M43)
            )
            ,
            (
                (matrix1.M31 * matrix2.M14) +
                (matrix1.M32 * matrix2.M24) +
                (matrix1.M33 * matrix2.M34) +
                (matrix1.M34 * matrix2.M44)
            )
            ,
            (
                (matrix1.M41 * matrix2.M11) +
                (matrix1.M42 * matrix2.M21) +
                (matrix1.M43 * matrix2.M31) +
                (matrix1.M44 * matrix2.M41)
            )
            ,
            (
                (matrix1.M41 * matrix2.M12) +
                (matrix1.M42 * matrix2.M22) +
                (matrix1.M43 * matrix2.M32) +
                (matrix1.M44 * matrix2.M42)
            )
            ,
            (
                (matrix1.M41 * matrix2.M13) +
                (matrix1.M42 * matrix2.M23) +
                (matrix1.M43 * matrix2.M33) +
                (matrix1.M44 * matrix2.M43)
            )
            ,
            (
                (matrix1.M41 * matrix2.M14) +
                (matrix1.M42 * matrix2.M24) +
                (matrix1.M43 * matrix2.M34) +
                (matrix1.M44 * matrix2.M44)
            ));
        }

        /// <summary>
        /// Implicit conversion from a 3x3 matrix to a 4x4 matrix.
        /// </summary>
        /// <param name="mat">3x3 matrix</param>
        /// <returns>4x4 matrix</returns>
        public static implicit operator Matrix4(Matrix3 mat)
        {
            Matrix4 m;

            m.M11 = mat.M11;
            m.M12 = mat.M12;
            m.M13 = mat.M13;
            m.M14 = 0.0f;

            m.M21 = mat.M21;
            m.M22 = mat.M22;
            m.M23 = mat.M23;
            m.M24 = 0.0f;

            m.M31 = mat.M31;
            m.M32 = mat.M32;
            m.M33 = mat.M33;
            m.M34 = 0.0f;

            m.M41 = 0.0f;
            m.M42 = 0.0f;
            m.M43 = 0.0f;
            m.M44 = 1.0f;

            return m;
        }

        #endregion

    }
}
