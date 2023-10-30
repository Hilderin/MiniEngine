using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    public static class SamplerHelper
    {

        /// <summary>
        /// Create a sampler with max anisotrophy
        /// </summary>
        /// <param name="device"></param>
        public static Sampler CreateMaxAnisotropy(Device device)
        {
            using (var deviceProp = device.PhysicalDevice.GetProperties())
            {
                using (SamplerCreateInfo samplerInfo = new()
                {
                    MagFilter = Filter.Linear,
                    MinFilter = Filter.Linear,
                    AddressModeU = SamplerAddressMode.Repeat,
                    AddressModeV = SamplerAddressMode.Repeat,
                    AddressModeW = SamplerAddressMode.Repeat,
                    AnisotropyEnable = true,
                    MaxAnisotropy = deviceProp.Limits.MaxSamplerAnisotropy,
                    BorderColor = BorderColor.IntOpaqueBlack,
                    UnnormalizedCoordinates = false,
                    CompareEnable = false,
                    CompareOp = CompareOp.Always,
                    MipmapMode = SamplerMipmapMode.Linear,
                })
                {
                    return device.CreateSampler(samplerInfo);
                }
            }
        }
    }
}
