using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MiniEngine.Rendering.OpenGL
{
    /// <summary>
    /// Utils to native calls
    /// </summary>
    internal static class NativeUtils
    {
        #region Methods

        /// <summary>
        ///     Reads memory from the pointer until the first null byte is encountered and decodes the bytes from UTF-8 into a
        ///     managed <see cref="string" />.
        /// </summary>
        /// <param name="ptr">Pointer to the start of the string.</param>
        /// <returns>Managed string created from read UTF-8 bytes.</returns>
        public static string PtrToStringUtf8(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                var length = 0;
                while (Marshal.ReadByte(ptr, length) != 0)
                    length++;
                var buffer = new byte[length];
                Marshal.Copy(ptr, buffer, 0, length);
                return Encoding.UTF8.GetString(buffer);
            }


            return String.Empty;

        }

        /// <summary>
        ///     Reads memory from the pointer until the first null byte is encountered and decodes the bytes from UTF-8 into a
        ///     managed <see cref="string" />.
        /// </summary>
        /// <param name="ptr">Pointer to the start of the string.</param>
        /// <returns>Managed string created from read UTF-8 bytes.</returns>
        public static string PtrToStringUtf8(IntPtr ptr, int length)
        {
            if (ptr != IntPtr.Zero)
            {
                var buffer = new byte[length];
                Marshal.Copy(ptr, buffer, 0, length);
                return Encoding.UTF8.GetString(buffer);
            }

            return String.Empty;
        }


        /// <summary>
        /// Convienence method for marshaling a pointer to a structure. Only use if the type is not blittable, otherwise
        /// use the read methods for blittable types.
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="ptr">Pointer to marshal</param>
        /// <param name="value">The marshaled structure</param>
        public static void MarshalStructure<T>(IntPtr ptr, out T value) where T : struct
        {
            if (ptr == IntPtr.Zero)
                value = default(T);

            Type type = typeof(T);

            //INativeCustomMarshaler marshaler;
            //if (HasNativeCustomMarshaler(type, out marshaler))
            //{
            //    value = (T)marshaler.MarshalNativeToManaged(ptr);
            //    return;
            //}

            value = (T)Marshal.PtrToStructure(ptr, type);

        }

        #endregion

    }
}
