using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// A Vulkan sampler
    /// </summary>
    public unsafe class VulkanSampler : IDisposable
    {

        /// <summary>
        /// Vulkan instance
        /// </summary>
        private VulkanInstance _vi;

        public uint mipLevels;

        /// <summary>
        /// Link to the sampler
        /// </summary>
        public Sampler textureSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public VulkanSampler(VulkanInstance vi, uint mipLevels)
        {
            _vi = vi;
            this.mipLevels = mipLevels;
        }

        public void Init()
        {
            CreateTextureSampler();
        }


        /// <summary>
        /// Dispose the texture
        /// </summary>
        public void Dispose()
        {
            _vi.Api.DestroySampler(_vi.device, textureSampler, null);

        }

        private void CreateTextureSampler()
        {
            _vi.Api.GetPhysicalDeviceProperties(_vi.physicalDevice, out PhysicalDeviceProperties properties);

            SamplerCreateInfo samplerInfo = new()
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Linear,
                MinFilter = Filter.Linear,
                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                AnisotropyEnable = true,
                MaxAnisotropy = properties.Limits.MaxSamplerAnisotropy,
                BorderColor = BorderColor.IntOpaqueBlack,
                UnnormalizedCoordinates = false,
                CompareEnable = false,
                CompareOp = CompareOp.Always,
                MipmapMode = SamplerMipmapMode.Linear,
                MinLod = 0,
                MaxLod = mipLevels,
                MipLodBias = 0,
            };

            fixed (Sampler* textureSamplerPtr = &textureSampler)
            {
                if (_vi.Api.CreateSampler(_vi.device, samplerInfo, null, textureSamplerPtr) != Result.Success)
                {
                    throw new Exception("failed to create texture sampler!");
                }
            }
        }
    }
}
