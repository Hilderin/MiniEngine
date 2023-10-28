using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    internal static class BytesHelper
    {
        /// <summary>
        /// Calculate the hash from a byte array
        /// </summary>
        public static int GetHashCodeBytes(byte[] data)
        {
            int hash = 1;
            for (int iv = data.Length; --iv >= 0;)
            {
                hash = CombineHash(hash, (int)data[iv]);
            }
            return hash;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CombineHash(int n1, int n2)
        {
            return (int)CombineHash((uint)n1, (uint)n2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CombineHash(uint u1, uint u2)
        {
            return ((u1 << 7) | (u1 >> 25)) ^ u2;
        }

    }


}
