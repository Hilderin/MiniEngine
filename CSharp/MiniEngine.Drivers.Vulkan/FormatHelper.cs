using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    public static class FormatHelper
    {
        /// <summary>
        /// Get the number of bytes for a format
        /// </summary>
        public static int GetFormatSizeBytes(Format format)
        {
            return GetFormatSizeBits(format) / 8;
        }

        /// <summary>
        /// Get the number of bits total for a format
        /// </summary>
        public static int GetFormatSizeBits(Format format)
        {
            switch (format)
            {
                case Format.R32Sfloat: return 32;
                case Format.R32G32Sfloat: return 64;
                case Format.R32G32B32Sfloat: return 96;
                case Format.R32G32B32A32Sfloat: return 128;
                case Format.R8G8B8A8Snorm: return 32;
                case Format.R8G8B8A8Srgb: return 32;
                case Format.R8G8B8A8Unorm:return 32;                    
                default:

                    string formatStr = format.ToString();
                    if (formatStr.StartsWith("R8G8B8A8"))
                        return 32;
                    if (formatStr.StartsWith("R16G16B16A16"))
                        return 64;
                    if (formatStr.StartsWith("R32G32B32A32"))
                        return 128;
                    if (formatStr.StartsWith("R64G64B64A64"))
                        return 256;

                    throw new NotSupportedException($"Format not supported: {format}");
            }

        }


    }
}
