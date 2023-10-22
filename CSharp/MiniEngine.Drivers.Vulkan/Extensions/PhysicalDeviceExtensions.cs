using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    public static class PhysicalDeviceExtensions
    {

        /// <summary>
        /// Return the format supported by the surface
        /// </summary>
        public static SurfaceFormatKhr GetSurfaceFormat(this PhysicalDevice physicalDevice, SurfaceKhr surface, Format[] expectedFormats, ColorSpaceKhr[] expectedColorSpaces)
        {
            foreach (var f in physicalDevice.GetSurfaceFormatsKHR(surface))
            {
                if (expectedFormats.Contains(f.Format) && expectedColorSpaces.Contains(f.ColorSpace))
                    return f;
            }

            throw new System.Exception("didn't find the expected formats and colorspaces.");
        }

    }
}
