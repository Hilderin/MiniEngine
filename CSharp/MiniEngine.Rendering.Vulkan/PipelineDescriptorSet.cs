using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Wrapper for a DescriptorSet
    /// </summary>
    public class PipelineDescriptorSet : IDisposable
    {
        private VkRenderer _renderer;
        private Device _device;


        private DescriptorSetLayoutBinding[][] _bindingSets;
        private DescriptorSetLayout[] _descriptorSetLayouts;
        
        public DescriptorSet[] DescriptorSets;
        public DescriptorPool DescriptorPool;

        private Dictionary<string, DescriptorSetData> _descriptorSetsPerName = new Dictionary<string, DescriptorSetData>();


        /// <summary>
        /// Constructor
        /// </summary>
        public PipelineDescriptorSet(VkRenderer renderer, PipelineWrapper pipeline, int setIndex = -1)
        {
            _renderer = renderer;
            _device = renderer.Device;
            //_shader = pipeline.Shader;

            if (setIndex == -1)
            {
                //We take all...
                _bindingSets = pipeline.Shader.BindingSets;
                _descriptorSetLayouts = pipeline.DescriptorSetLayouts;
            }
            else
            {
                //We take only one...
                _bindingSets = new DescriptorSetLayoutBinding[1][];
                _bindingSets[0] = pipeline.Shader.BindingSets[setIndex];

                _descriptorSetLayouts = new DescriptorSetLayout[1];
                _descriptorSetLayouts[0] = pipeline.DescriptorSetLayouts[setIndex];
            }
        
            CreateDescriptorPool();

            CreateDescriptorSets();

        }

        /// <summary>
        /// Return true if the descriptorset contains a variable
        /// </summary>
        public bool Contains(string name)
        {
            return _descriptorSetsPerName.ContainsKey(name);
        }

        /// <summary>
        /// Get all the descriptor names
        /// </summary>
        public IEnumerable<string> GetNames()
        {
            return _descriptorSetsPerName.Keys;
        }


        /// <summary>
        /// Set the default buffers from the renderer
        /// </summary>
        public void SetRendererBuffers()
        {
            foreach (string name in GetNames())
            {
                switch (name)
                {
                    case ShaderVariableNames.Scene:
                        Set(ShaderVariableNames.Scene, _renderer.SceneBuffer);
                        break;
                    case ShaderVariableNames.Objects:
                        Set(ShaderVariableNames.Objects, _renderer.ObjectsBuffer);
                        break;
                    case ShaderVariableNames.MeshletInstances:
                        Set(ShaderVariableNames.MeshletInstances, _renderer.MeshLetInstancesBuffer);
                        break;
                    case ShaderVariableNames.SamplerDiffuse:
                        //Bindless... nothing to do here
                        break;
                    case ShaderVariableNames.DrawCallsBuffers:
                        //This buffer will be bind later when drawcallsbuffers are created
                        break;
                    default:
                        Debug.Warning($"Descriptor name not supported in shader: {name}");
                        break;
                }
            }
        }

        /// <summary>
        /// Set a uniform buffer
        /// </summary>
        public PipelineDescriptorSet Set(string name, BufferWrapper uniformBuffer, uint arrayElementIndex = 0)
        {
            if (!_descriptorSetsPerName.TryGetValue(name, out var descriptorData))
                throw new InvalidOperationException($"Descriptor name not found '{name}'");

            if (descriptorData.DescriptorType != DescriptorType.UniformBuffer && descriptorData.DescriptorType != DescriptorType.StorageBuffer)
                throw new InvalidOperationException($"Wrong DescryptorName, expected '{DescriptorType.UniformBuffer}' or '{DescriptorType.StorageBuffer}', current type: {descriptorData.DescriptorType}");

            if (arrayElementIndex > 0 && !descriptorData.IsArray)
                throw new InvalidOperationException($"Cannot set an {nameof(arrayElementIndex)} != 0 for a non array descriptor.");

            var uniformBufferInfo = new DescriptorBufferInfo
            {
                Buffer = uniformBuffer.Buffer,
                Offset = 0,
                Range = uniformBuffer.Size
            };

            using (WriteDescriptorSet writeSet = new WriteDescriptorSet
            {
                DstSet = descriptorData.DescriptorSet,
                DescriptorType = descriptorData.DescriptorType,
                BufferInfo = new DescriptorBufferInfo[] { uniformBufferInfo },
                DstBinding = descriptorData.Binding,
                DstArrayElement = arrayElementIndex
            })
            {
                _device.UpdateDescriptorSets(writeSet);
            }

            return this;
        }

        /// <summary>
        /// Set a combined image and sampler
        /// </summary>
        public PipelineDescriptorSet Set(string name, ImageView imageView, Sampler sampler, uint arrayElementIndex = 0)
        {

            if (!_descriptorSetsPerName.TryGetValue(name, out var descriptorData))
                throw new InvalidOperationException("Descriptor name not found '{name}'");

            if (descriptorData.DescriptorType != DescriptorType.CombinedImageSampler)
                throw new InvalidOperationException($"Wrong DescryptorName, expected '{DescriptorType.CombinedImageSampler}', current type: {descriptorData.DescriptorType}");

            if (arrayElementIndex > 0 && !descriptorData.IsArray)
                throw new InvalidOperationException($"Cannot set an {nameof(arrayElementIndex)} != 0 for a non array descriptor.");

            var imageInfo = new DescriptorImageInfo
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                ImageView = imageView,
                Sampler = sampler,
            };

            using (WriteDescriptorSet writeSet = new WriteDescriptorSet
            {
                DstSet = descriptorData.DescriptorSet,
                DescriptorType = DescriptorType.CombinedImageSampler,
                ImageInfo = new DescriptorImageInfo[] { imageInfo },
                DstBinding = descriptorData.Binding,
                DstArrayElement = arrayElementIndex
            })
            {
                _device.UpdateDescriptorSets(writeSet);
            }

            return this;
        }

        /// <summary>
        /// Set a sampled image only
        /// </summary>
        public PipelineDescriptorSet Set(string name, ImageView imageView, uint arrayElementIndex = 0)
        {

            if (!_descriptorSetsPerName.TryGetValue(name, out var descriptorData))
                throw new InvalidOperationException("Descriptor name not found '{name}'");

            if(descriptorData.DescriptorType != DescriptorType.SampledImage)
                throw new InvalidOperationException($"Wrong DescryptorName, expected '{DescriptorType.SampledImage}', current type: {descriptorData.DescriptorType}");

            if (arrayElementIndex > 0 && !descriptorData.IsArray)
                throw new InvalidOperationException($"Cannot set an {nameof(arrayElementIndex)} != 0 for a non array descriptor.");

            var imageInfo = new DescriptorImageInfo
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                ImageView = imageView
            };

            using (WriteDescriptorSet writeSet = new WriteDescriptorSet
            {
                DstSet = descriptorData.DescriptorSet,
                DescriptorType = DescriptorType.SampledImage,
                ImageInfo = new DescriptorImageInfo[] { imageInfo },
                DstBinding = descriptorData.Binding,
                DstArrayElement = arrayElementIndex
            })
            {
                _device.UpdateDescriptorSets(writeSet);
            }

            return this;
        }

        /// <summary>
        /// Set a sampler only
        /// </summary>
        public PipelineDescriptorSet Set(string name, Sampler sampler, uint arrayElementIndex = 0)
        {

            if (!_descriptorSetsPerName.TryGetValue(name, out var descriptorData))
                throw new InvalidOperationException("Descriptor name not found '{name}'");

            if (descriptorData.DescriptorType != DescriptorType.Sampler)
                throw new InvalidOperationException($"Wrong DescryptorName, expected '{DescriptorType.Sampler}', current type: {descriptorData.DescriptorType}");

            if(arrayElementIndex > 0 && !descriptorData.IsArray)
                throw new InvalidOperationException($"Cannot set an {nameof(arrayElementIndex)} != 0 for a non array descriptor.");

            var imageInfo = new DescriptorImageInfo
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                Sampler = sampler,
            };

            using (WriteDescriptorSet writeSet = new WriteDescriptorSet
            {
                DstSet = descriptorData.DescriptorSet,
                DescriptorType = DescriptorType.Sampler,
                ImageInfo = new DescriptorImageInfo[] { imageInfo },
                DstBinding = descriptorData.Binding,
                DstArrayElement = arrayElementIndex
            })
            {
                _device.UpdateDescriptorSets(writeSet);
            }

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
            Dictionary<DescriptorType, uint> poolSizesDict = new Dictionary<DescriptorType, uint>();
            DescriptorPoolCreateFlags flags = 0;

            for (int iSet = 0; iSet < _bindingSets.Length; iSet++)
            {
                foreach (var binding in _bindingSets[iSet])
                {
                    poolSizesDict.TryGetValue(binding.DescriptorType, out uint nb);
                    poolSizesDict[binding.DescriptorType] = nb + binding.DescriptorCount;

                    if (binding.Bindless)
                        flags |= DescriptorPoolCreateFlags.UpdateAfterBind;
                }
            }

            List<DescriptorPoolSize> poolSizes = new List<DescriptorPoolSize>();
            foreach (var kv in poolSizesDict)
            {
                poolSizes.Add(new DescriptorPoolSize()
                {
                    Type = kv.Key,
                    DescriptorCount = kv.Value,
                });
            }
            using (var descriptorPoolCreateInfo = new DescriptorPoolCreateInfo
            {
                PoolSizes = poolSizes.ToArray(),
                MaxSets = 1,
                Flags = flags
            })
            {
                DescriptorPool = _device.CreateDescriptorPool(descriptorPoolCreateInfo);
            }
        }

        /// <summary>
        /// Create the descriptors...
        /// </summary>
        private void CreateDescriptorSets()
        {
            if (_bindingSets.Length > 0)
            {
                using (var descriptorSetAllocateInfo = new DescriptorSetAllocateInfo
                {
                    SetLayouts = _descriptorSetLayouts,
                    DescriptorPool = DescriptorPool
                })
                {
                    DescriptorSets = _device.AllocateDescriptorSets(descriptorSetAllocateInfo);
                }

                if (DescriptorSets.Length != _bindingSets.Length)
                    throw new InvalidOperationException("Number of DescriptorSets ({DescriptorSets.Length}) different then the shader ({_bindingSets.Count})");


                for (int iSet = 0; iSet < _bindingSets.Length; iSet++)
                {
                    foreach (var binding in _bindingSets[iSet])
                    {
                        _descriptorSetsPerName.Add(binding.Name, new DescriptorSetData()
                        {
                            DescriptorSet = DescriptorSets[iSet],
                            DescriptorType = binding.DescriptorType,
                            Binding = binding.Binding,
                            IsArray = binding.IsArray
                        });
                    }
                }
            }




        }


        /// <summary>
        /// Implicit conversion to a DescriptorSet[]
        /// </summary>
        public static implicit operator DescriptorSet[](PipelineDescriptorSet pipelineDescriptorSet) { return pipelineDescriptorSet.DescriptorSets; }


        private class DescriptorSetData
        {
            public DescriptorSet DescriptorSet;
            public DescriptorType DescriptorType;
            public bool IsArray;
            //public BufferWrapper UniformBuffer;
            //public ImageView ImageView;
            //public Sampler Sampler;
            public uint Binding;
        }

    }

}
