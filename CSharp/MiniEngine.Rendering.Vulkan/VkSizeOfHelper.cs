using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        /// Static constructor
        /// </summary>
        unsafe static VkSizeOfHelper()
        {
            _cacheTypeSizes[typeof(bool)] = sizeof(bool);
            _cacheTypeSizes[typeof(byte)] = sizeof(byte);
            _cacheTypeSizes[typeof(sbyte)] = sizeof(sbyte);
            _cacheTypeSizes[typeof(char)] = sizeof(char);
            _cacheTypeSizes[typeof(decimal)] = sizeof(decimal);
            _cacheTypeSizes[typeof(double)] = sizeof(double);
            _cacheTypeSizes[typeof(float)] = sizeof(float);
            _cacheTypeSizes[typeof(int)] = sizeof(int);
            _cacheTypeSizes[typeof(uint)] = sizeof(uint);
            _cacheTypeSizes[typeof(nint)] = (uint)sizeof(nint);
            _cacheTypeSizes[typeof(nuint)] = (uint)sizeof(nuint);
            _cacheTypeSizes[typeof(long)] = sizeof(long);
            _cacheTypeSizes[typeof(ulong)] = sizeof(ulong);
            _cacheTypeSizes[typeof(short)] = sizeof(short);
            _cacheTypeSizes[typeof(ushort)] = sizeof(ushort);
            _cacheTypeSizes[typeof(Vector3)] = sizeof(float) * 3;        //A three- or four-component vector has a base alignment equal to four times its scalar alignment.
            _cacheTypeSizes[typeof(Vector4)] = sizeof(float) * 4;        //A three- or four-component vector has a base alignment equal to four times its scalar alignment.
            _cacheTypeSizes[typeof(Matrix3)] = sizeof(float) * 9;        //A column-major matrix with C columns and R rows is equivalent to a C element array of vectors with R components.
            _cacheTypeSizes[typeof(Matrix4)] = sizeof(float) * 16;       //A column-major matrix with C columns and R rows is equivalent to a C element array of vectors with R components.
        }


        /// <summary>
        /// Get the size of a type on glsl vulkan shaders
        /// Alignment Requirements
        ///There are different alignment requirements depending on the specific resources and on the features enabled on the device.
        ///Matrix types are defined in terms of arrays as follows:
        ///- A column-major matrix with C columns and R rows is equivalent to a C element array of vectors with R components.
        ///- A row-major matrix with C columns and R rows is equivalent to an R element array of vectors with C components.
        ///
        ///The scalar alignment of the type of an OpTypeStruct member is defined recursively as follows:
        ///- A scalar of size N has a scalar alignment of N.
        ///- A vector type has a scalar alignment equal to that of its component type.
        ///- An array type has a scalar alignment equal to that of its element type.
        ///- A structure has a scalar alignment equal to the largest scalar alignment of any of its members.
        ///- A matrix type inherits scalar alignment from the equivalent array declaration.
        ///
        ///The base alignment of the type of an OpTypeStruct member is defined recursively as follows:
        ///- A scalar has a base alignment equal to its scalar alignment.
        ///- A two-component vector has a base alignment equal to twice its scalar alignment.
        ///- A three- or four-component vector has a base alignment equal to four times its scalar alignment.
        ///- An array has a base alignment equal to the base alignment of its element type.
        ///- A structure has a base alignment equal to the largest base alignment of any of its members. An empty structure has a base alignment equal to the size of the smallest scalar type permitted by the capabilities declared in the SPIR-V module. (e.g., for a 1 byte aligned empty struct in the StorageBuffer storage class, StorageBuffer8BitAccess or UniformAndStorageBuffer8BitAccess must be declared in the SPIR-V module.)
        ///- A matrix type inherits base alignment from the equivalent array declaration.
        ///
        ///The extended alignment of the type of an OpTypeStruct member is similarly defined as follows:
        ///- A scalar or vector type has an extended alignment equal to its base alignment.
        ///- An array or structure type has an extended alignment equal to the largest extended alignment of any of its members, rounded up to a multiple of 16.
        ///- A matrix type inherits extended alignment from the equivalent array declaration.
        ///
        ///A member is defined to improperly straddle if either of the following are true:
        ///- It is a vector with total size less than or equal to 16 bytes, and has Offset decorations placing its first byte at F and its last byte at L, where floor(F / 16) != floor(L / 16).
        ///- It is a vector with total size greater than 16 bytes and has its Offset decorations placing its first byte at a non-integer multiple of 16.
        /// </summary>
        private static uint SizeOf(Type type)
        {
            if (_cacheTypeSizes.TryGetValue(type, out uint value))
                return value;

            uint size = 0;

            var structLayoutAttrib = type.GetCustomAttribute<StructLayoutAttribute>();
            if (structLayoutAttrib != null && structLayoutAttrib.Size > 0)
            {
                size = (uint)structLayoutAttrib.Size;
            }
            else
            {
                FieldInfo[] fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);


                for (int i = 0; i < fields.Length; i++)
                {
                    FieldOffsetAttribute foa = fields[i].GetCustomAttribute<FieldOffsetAttribute>();
                    if (foa != null)
                    {
                        if (size < foa.Value)
                            size = (uint)foa.Value;
                    }


                    size += SizeOf(fields[i].FieldType);

                }
            }

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
                return (uint)Marshal.SizeOf(type);
            }
            
        }

        /// <summary>
        /// Get the size of a type on glsl vulkan shaders
        /// </summary>
        public static uint SizeOf<T>()
        {
            return SizeOf(typeof(T));
        }

        /// <summary>
        /// Get the size of a type on glsl vulkan shaders with an alignement
        /// </summary>
        public static uint SizeOf<T>(uint alignment)
        {
            return SizeOf(typeof(T), alignment);
        }
    }
}
