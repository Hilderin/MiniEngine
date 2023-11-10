using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    public static class IntPtrHelper
    {

        /// <summary>
        /// Write an int into ptrDest
        /// </summary>
        public unsafe static void Write(int value, IntPtr ptrDest, int offsetDest)
        {
            int* ptrInt = (int*)(ptrDest + offsetDest);
            *ptrInt = value;
        }

        /// <summary>
        /// Write an int into ptrDest
        /// </summary>
        public unsafe static void Write(uint value, IntPtr ptrDest, int offsetDest)
        {
            uint* ptrInt = (uint*)(ptrDest + offsetDest);
            *ptrInt = value;
        }

        /// <summary>
        /// Write a float into ptrDest
        /// </summary>
        public unsafe static void Write(float value, IntPtr ptrDest, int offsetDest)
        {
            float* ptrInt = (float*)(ptrDest + offsetDest);
            *ptrInt = value;
        }

    }
}
