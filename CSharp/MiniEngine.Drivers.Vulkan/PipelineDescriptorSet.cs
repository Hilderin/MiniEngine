using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Wrapper for a DescriptorSet
    /// </summary>
    public class PipelineDescriptorSet : IDisposable
    {

        private Device _device;
        private PipelineWrapper _pipeline;
        private VkShader _shader;

        public DescriptorSet[] DescriptorSets;
        public DescriptorPool DescriptorPool;

        private Dictionary<string, DescriptorSetData> _descriptorSetsPerName = new Dictionary<string, DescriptorSetData>();
        private DescriptorSetData[] _descriptorSetList;

        /// <summary>
        /// Constructor
        /// </summary>
        public PipelineDescriptorSet(Device device, PipelineWrapper pipeline)
        {
            _device = device;
            _pipeline = pipeline;
            _shader = pipeline.Shader;


            CreateDescriptorPool();

            CreateDescriptorSets();

        }

        /// <summary>
        /// Set a uniform buffer
        /// </summary>
        public PipelineDescriptorSet Set(string name, BufferWrapper uniformBuffer)
        {
            if (!_descriptorSetsPerName.TryGetValue(name, out var descriptorData))
                throw new InvalidOperationException("Descriptor name not found '{name}'");

            if (descriptorData.DescriptorType != DescriptorType.UniformBuffer)
                throw new InvalidOperationException($"Wrong DescryptorName, expected '{DescriptorType.UniformBuffer}', current type: {descriptorData.DescriptorType}");

            var uniformBufferInfo = new DescriptorBufferInfo
            {
                Buffer = uniformBuffer.Buffer,
                Offset = 0,
                Range = uniformBuffer.Size
            };

            WriteDescriptorSet writeSet = new WriteDescriptorSet
            {
                DstSet = descriptorData.DescriptorSet,
                DescriptorType = DescriptorType.UniformBuffer,
                BufferInfo = new DescriptorBufferInfo[] { uniformBufferInfo },
                DstBinding = descriptorData.Binding
            };

            _device.UpdateDescriptorSets(writeSet);

            return this;
        }

        /// <summary>
        /// Set a combined image and sampler
        /// </summary>
        public PipelineDescriptorSet Set(string name, ImageView imageView, Sampler sampler)
        {

            if (!_descriptorSetsPerName.TryGetValue(name, out var descriptorData))
                throw new InvalidOperationException("Descriptor name not found '{name}'");

            if (descriptorData.DescriptorType != DescriptorType.CombinedImageSampler)
                throw new InvalidOperationException($"Wrong DescryptorName, expected '{DescriptorType.CombinedImageSampler}', current type: {descriptorData.DescriptorType}");

            var imageInfo = new DescriptorImageInfo
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                ImageView = imageView,
                Sampler = sampler,
            };

            WriteDescriptorSet writeSet = new WriteDescriptorSet
            {
                DstSet = descriptorData.DescriptorSet,
                DescriptorType = DescriptorType.CombinedImageSampler,
                ImageInfo = new DescriptorImageInfo[] { imageInfo },
                DstBinding = descriptorData.Binding
            };

            _device.UpdateDescriptorSets(writeSet);

            return this;
        }

        /// <summary>
        /// Set a sampled image only
        /// </summary>
        public PipelineDescriptorSet Set(string name, ImageView imageView)
        {

            if (!_descriptorSetsPerName.TryGetValue(name, out var descriptorData))
                throw new InvalidOperationException("Descriptor name not found '{name}'");

            if(descriptorData.DescriptorType != DescriptorType.SampledImage)
                throw new InvalidOperationException($"Wrong DescryptorName, expected '{DescriptorType.SampledImage}', current type: {descriptorData.DescriptorType}");

            var imageInfo = new DescriptorImageInfo
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                ImageView = imageView
            };

            WriteDescriptorSet writeSet = new WriteDescriptorSet
            {
                DstSet = descriptorData.DescriptorSet,
                DescriptorType = DescriptorType.SampledImage,
                ImageInfo = new DescriptorImageInfo[] { imageInfo },
                DstBinding = descriptorData.Binding
            };

            _device.UpdateDescriptorSets(writeSet);

            return this;
        }

        /// <summary>
        /// Set a sampler only
        /// </summary>
        public PipelineDescriptorSet Set(string name, Sampler sampler)
        {

            if (!_descriptorSetsPerName.TryGetValue(name, out var descriptorData))
                throw new InvalidOperationException("Descriptor name not found '{name}'");

            if (descriptorData.DescriptorType != DescriptorType.Sampler)
                throw new InvalidOperationException($"Wrong DescryptorName, expected '{DescriptorType.Sampler}', current type: {descriptorData.DescriptorType}");

            var imageInfo = new DescriptorImageInfo
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                Sampler = sampler,
            };

            WriteDescriptorSet writeSet = new WriteDescriptorSet
            {
                DstSet = descriptorData.DescriptorSet,
                DescriptorType = DescriptorType.Sampler,
                ImageInfo = new DescriptorImageInfo[] { imageInfo },
                DstBinding = descriptorData.Binding
            };

            _device.UpdateDescriptorSets(writeSet);

            return this;
        }


        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (DescriptorPool != null)
            {
                _device.DestroyDescriptorPool(DescriptorPool);
                DescriptorPool = null;
            }

        }

        private void CreateDescriptorPool()
        {
            Dictionary<DescriptorType, int> poolSizesDict = new Dictionary<DescriptorType, int>();

            for (int iSet = 0; iSet < _shader.BindingSets.Length; iSet++)
            {
                foreach (var binding in _shader.BindingSets[iSet])
                {
                    poolSizesDict.TryGetValue(binding.DescriptorType, out int nb);
                    poolSizesDict[binding.DescriptorType] = nb + 1;
                }
            }

            List<DescriptorPoolSize> poolSizes = new List<DescriptorPoolSize>();
            foreach (var kv in poolSizesDict)
            {
                poolSizes.Add(new DescriptorPoolSize()
                {
                    Type = kv.Key,
                    DescriptorCount = (uint)kv.Value
                });
            }
            var descriptorPoolCreateInfo = new DescriptorPoolCreateInfo
            {
                PoolSizes = poolSizes.ToArray(),
                MaxSets = 1
            };

            DescriptorPool = _device.CreateDescriptorPool(descriptorPoolCreateInfo);
        }

        /// <summary>
        /// Create the descriptors...
        /// </summary>
        private void CreateDescriptorSets()
        {
            if (_shader.BindingSets.Length > 0)
            {
                var descriptorSetAllocateInfo = new DescriptorSetAllocateInfo
                {
                    SetLayouts = _pipeline.DescriptorSetLayouts,
                    DescriptorPool = DescriptorPool
                };

                DescriptorSets = _device.AllocateDescriptorSets(descriptorSetAllocateInfo);

                if (DescriptorSets.Length != _shader.BindingSets.Length)
                    throw new InvalidOperationException("Number of DescriptorSets ({DescriptorSets.Length}) different then the shader ({_shader.BindingSets.Count})");


                for (int iSet = 0; iSet < _shader.BindingSets.Length; iSet++)
                {
                    foreach (var binding in _shader.BindingSets[iSet])
                    {
                        _descriptorSetsPerName.Add(binding.Name, new DescriptorSetData()
                        {
                            DescriptorSet = DescriptorSets[iSet],
                            DescriptorType = binding.DescriptorType,
                            Binding = binding.Binding
                        });
                    }
                }
            }




        }


        ///// <summary>
        ///// Create uniform buffers....
        ///// </summary>
        //private void UpdateDescriptorSets()
        //{

        //    List<WriteDescriptorSet> writeSets = new List<WriteDescriptorSet>();

        //    foreach (DescriptorSetData descriptorData in _descriptorSetList)
        //    {
        //        if (descriptorData.DescriptorType == DescriptorType.UniformBuffer)
        //        {
        //            //UniformBuffer...
        //            if (descriptorData.UniformBuffer != null)
        //            {
        //                var uniformBufferInfo = new DescriptorBufferInfo
        //                {
        //                    Buffer = descriptorData.UniformBuffer,
        //                    Offset = 0,
        //                    Range = descriptorData.UniformBuffer.Size
        //                };

        //                writeSets.Add(new WriteDescriptorSet
        //                {
        //                    DstSet = descriptorData.DescriptorSet,
        //                    DescriptorType = DescriptorType.UniformBuffer,
        //                    BufferInfo = new DescriptorBufferInfo[] { uniformBufferInfo },
        //                    DstBinding = descriptorData.Binding
        //                });
        //            }
        //        }
        //        else if (descriptorData.DescriptorType == DescriptorType.CombinedImageSampler)
        //        {
        //            //Image...
        //            if (descriptorData.ImageView != null && descriptorData.Sampler != null)
        //            {
        //                var imageInfo = new DescriptorImageInfo
        //                {
        //                    ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
        //                    ImageView = descriptorData.ImageView,
        //                    Sampler = descriptorData.Sampler,
        //                };

        //                writeSets.Add(new WriteDescriptorSet
        //                {
        //                    DstSet = descriptorData.DescriptorSet,
        //                    DescriptorType = DescriptorType.CombinedImageSampler,
        //                    ImageInfo = new DescriptorImageInfo[] { imageInfo },
        //                    DstBinding = descriptorData.Binding
        //                });
        //            }
        //        }
        //        else
        //            throw new NotSupportedException($"Descriptor type not supported: {descriptorData.DescriptorType}");

        //    }

        //    _device.UpdateDescriptorSets(writeSets.ToArray(), null);
        //}

        private class CombinedImageSampler
        {
            public ImageView ImageView;
            public Sampler Sampler;
        }


        private class DescriptorSetData
        {
            public DescriptorSet DescriptorSet;
            public DescriptorType DescriptorType;
            //public BufferWrapper UniformBuffer;
            //public ImageView ImageView;
            //public Sampler Sampler;
            public uint Binding;
        }

    }

}
