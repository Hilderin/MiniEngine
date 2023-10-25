using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Presentations.Glfw
{
    /// <summary>
    /// Utils to native calls
    /// </summary>
    internal static class NativeUtils
    {
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

    }
}
