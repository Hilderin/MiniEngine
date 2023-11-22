using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Helper for sizes of objects
    /// </summary>
    public unsafe static class VkSizeOfHelper
    {

        /// <summary>
        /// Base type sizes
        /// </summary>
        private static Dictionary<Type, uint> _cacheTypeSizes = new Dictionary<Type, uint>();

        /// <summary>
        /// Get the size of a type on glsl vulkan shaders with an alignement
        /// </summary>
        private static uint SizeOf(Type type)
        {
            if (_cacheTypeSizes.TryGetValue(type, out uint size))
                return size;

            MethodInfo metSum = typeof(Unsafe).GetMethod("SizeOf");
            MethodInfo genSizeOf = metSum.MakeGenericMethod(type);
            size = Convert.ToUInt32(genSizeOf.Invoke(null, null));

            _cacheTypeSizes[type] = size;

            return size;
        }

        /// <summary>
        /// Get the size of a type on glsl vulkan shaders with an alignement
        /// </summary>
        public static uint SizeOf(Type type, uint alignment)
        {
            if (alignment > 1)
            {
                return Math.RoundUp(SizeOf(type), alignment);
            }
            else
            {
                //For vextex and index buffer... the data are not aligned...
                return SizeOf(type);
            }
            
        }

        /// <summary>
        /// Get the size of a type on glsl vulkan shaders
        /// </summary>
        public static uint SizeOf<T>()
        {
            return (uint)Unsafe.SizeOf<T>();

        }

        /// <summary>
        /// Get the size of a type on glsl vulkan shaders
        /// </summary>
        public static uint SizeOf<T>(uint alignment)
        {
            if (alignment > 1)
            {
                return Math.RoundUp((uint)Unsafe.SizeOf<T>(), alignment);
            }
            else
            {
                //For vextex and index buffer... the data are not aligned...
                return (uint)Unsafe.SizeOf<T>();
            }

        }

    }
}
