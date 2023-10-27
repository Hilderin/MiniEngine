using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Contains a pipeline
    /// </summary>
    public class PipelineWrapper : IDisposable
    {
        private Device _device;
        private VkShader _shader;
        private RenderPass _renderPass;
        private ImageView _texture;
        private Sampler _sampler;



        public DescriptorSet[] DescriptorSets;
        public DescriptorPool DescriptorPool;
        public BufferWrapper UniformBuffer;

        private CommandBuffer[] commandBuffers;
        //public PipelineWrapperDescriptorSet[] descriptorSets;
        private DescriptorSetLayout descriptorSetLayout;
        public PipelineLayout pipelineLayout;
        //private Buffer uniformBuffer;
        public Pipeline pipeline;

        

        
        /// <summary>
        /// Constructor
        /// </summary>
        public PipelineWrapper(Device device, RenderPass renderPass, VkShader shader, ImageView texture, Sampler sampler)
        {
            _device = device;
            _shader = shader;
            _renderPass = renderPass;
            _texture = texture;
            _sampler = sampler;

            Build();
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

            if (descriptorSetLayout != null)
            {
                _device.DestroyDescriptorSetLayout(descriptorSetLayout);
                descriptorSetLayout = null;
            }

            if (pipeline != null)
            {
                _device.DestroyPipeline(pipeline);
                pipeline = null;
            }

            if (pipelineLayout != null)
            {
                _device.DestroyPipelineLayout(pipelineLayout);
                pipelineLayout = null;
            }

            if (descriptorSetLayout != null)
            {
                _device.DestroyDescriptorSetLayout(descriptorSetLayout);
                descriptorSetLayout = null;
            }

            if (UniformBuffer != null)
            {
                UniformBuffer.Dispose();
                UniformBuffer = null;
            }

        }

        /// <summary>
        /// Build the pipeline...
        /// </summary>
        public void Build()
        {
            CreatePipeline();

            CreateDescriptorSets();

            UpdateDescriptorSets();
        }

        /// <summary>
        /// Create the pipeline
        /// </summary>
        private void CreatePipeline()
        {
            //Descriptor set layout creation from shader...
            descriptorSetLayout = _device.CreateDescriptorSetLayout(_shader.Bindings.ToArray());

            PushConstantRange[] constantRanges = _shader.Constants.ToArray();

            //Pipeline layout creation...
            pipelineLayout = _device.CreatePipelineLayout(descriptorSetLayout, constantRanges);


            var vertexShaderModule = _device.CreateShaderModule(_shader.VertexSpirv);
            var fragmentShaderModule = _device.CreateShaderModule(_shader.FragmentSpirv);

            PipelineShaderStageCreateInfo[] pipelineShaderStages = {
                new PipelineShaderStageCreateInfo {
                    Stage = ShaderStageFlags.Vertex,
                    Module = vertexShaderModule,
                    Name = _shader.VertexEntryPoint
                },
                new PipelineShaderStageCreateInfo {
                    Stage = ShaderStageFlags.Fragment,
                    Module = fragmentShaderModule,
                    Name = _shader.FragmentEntryPoint
                }
            };
            //var viewport = new Viewport
            //{
            //    MinDepth = 0,
            //    MaxDepth = 1.0f,
            //    Width = _vi.CurrentExtent.Width,
            //    Height = _vi.CurrentExtent.Height
            //};
            var viewport = new Viewport
            {
                MinDepth = 0,
                MaxDepth = 1.0f,
                Width = _device.CurrentExtent.Width,
                Height = -_device.CurrentExtent.Height,      //Inverting Y axis so the coord will be 
                Y = _device.CurrentExtent.Height
            };
            var scissor = new Rect2D { Extent = _device.CurrentExtent };
            var viewportCreateInfo = new PipelineViewportStateCreateInfo
            {
                Viewports = new Viewport[] { viewport },
                Scissors = new Rect2D[] { scissor }
            };

            var multisampleCreateInfo = new PipelineMultisampleStateCreateInfo
            {
                RasterizationSamples = SampleCountFlags.Count1
            };
            var colorBlendAttachmentState = new PipelineColorBlendAttachmentState
            {
                ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G | ColorComponentFlags.B | ColorComponentFlags.A
            };
            var colorBlendStateCreatInfo = new PipelineColorBlendStateCreateInfo
            {
                LogicOp = LogicOp.Copy,
                Attachments = new PipelineColorBlendAttachmentState[] { colorBlendAttachmentState }
            };
            var rasterizationStateCreateInfo = new PipelineRasterizationStateCreateInfo
            {
                PolygonMode = PolygonMode.Fill,
                //CullMode = CullModeFlags.Front,
                CullMode = CullModeFlags.Back,
                //CullMode = CullModeFlags.None,
                FrontFace = FrontFace.Clockwise,
                LineWidth = 1.0f
            };
            var inputAssemblyStateCreateInfo = new PipelineInputAssemblyStateCreateInfo
            {
                Topology = PrimitiveTopology.TriangleList
            };

            var vertexInputStateCreateInfo = new PipelineVertexInputStateCreateInfo
            {
                VertexBindingDescriptions = _shader.VertexBindings.ToArray(),
                VertexAttributeDescriptions = _shader.VertexInputAttributes.ToArray()
            };

            var pipelineCreateInfo = new GraphicsPipelineCreateInfo
            {
                Layout = pipelineLayout,
                ViewportState = viewportCreateInfo,
                Stages = pipelineShaderStages,
                MultisampleState = multisampleCreateInfo,
                ColorBlendState = colorBlendStateCreatInfo,
                RasterizationState = rasterizationStateCreateInfo,
                InputAssemblyState = inputAssemblyStateCreateInfo,
                VertexInputState = vertexInputStateCreateInfo,
                RenderPass = _renderPass
            };

            //var pipelines = _device.CreateGraphicsPipelines(_device.CreatePipelineCache(new PipelineCacheCreateInfo()), new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });
            var pipelines = _device.CreateGraphicsPipelines(null, new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });

            pipeline = pipelines[0];

            //We don't need it anymore...
            _device.DestroyShaderModule(vertexShaderModule);
            _device.DestroyShaderModule(fragmentShaderModule);
        }


        /// <summary>
        /// Create the descriptors...
        /// </summary>
        private void CreateDescriptorSets()
        {
            DescriptorPoolSize[] poolSizes = _shader.Bindings.GroupBy(b => b.DescriptorType)
                                                    .Select(g => new DescriptorPoolSize()
                                                    {
                                                        Type = g.Key,
                                                        DescriptorCount = (uint)g.Count()
                                                    }).ToArray();


            if (poolSizes.Length > 0)
            {

                var descriptorPoolCreateInfo = new DescriptorPoolCreateInfo
                {
                    PoolSizes = poolSizes,
                    MaxSets = 1
                };

                DescriptorPool = _device.CreateDescriptorPool(descriptorPoolCreateInfo);

                var descriptorSetAllocateInfo = new DescriptorSetAllocateInfo
                {
                    SetLayouts = new DescriptorSetLayout[] { descriptorSetLayout },
                    DescriptorPool = DescriptorPool
                };

                DescriptorSets = _device.AllocateDescriptorSets(descriptorSetAllocateInfo);
            }


        }

        /// <summary>
        /// Create uniform buffers....
        /// </summary>
        private void UpdateDescriptorSets()
        {
            

            foreach (DescriptorSet descriptorSet in DescriptorSets)
            {
                List<WriteDescriptorSet> writeSets = new List<WriteDescriptorSet>();

                foreach (var binding in _shader.Bindings)
                {
                    if (binding.DescriptorType == DescriptorType.UniformBuffer)
                    {
                        //UniformBuffer...
                        UniformBuffer = _device.CreateBufferWrapper(binding.Size, BufferUsageFlags.UniformBuffer);

                        var uniformBufferInfo = new DescriptorBufferInfo
                        {
                            Buffer = UniformBuffer,
                            Offset = 0,
                            Range = binding.Size
                        };

                        writeSets.Add(new WriteDescriptorSet
                        {
                            DstSet = descriptorSet,
                            DescriptorType = DescriptorType.UniformBuffer,
                            BufferInfo = new DescriptorBufferInfo[] { uniformBufferInfo },
                            DstBinding = binding.Binding
                        });
                    }
                    else if (binding.DescriptorType == DescriptorType.CombinedImageSampler)
                    {
                        //Texture...
                        var imageInfo = new DescriptorImageInfo
                        {
                            ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                            ImageView = _texture,
                            Sampler = _sampler,
                        };

                        writeSets.Add(new WriteDescriptorSet
                        {
                            DstSet = descriptorSet,
                            DescriptorType = DescriptorType.CombinedImageSampler,
                            ImageInfo = new DescriptorImageInfo[] { imageInfo },
                            DstBinding = binding.Binding
                        });
                    }
                    else
                        throw new NotSupportedException($"Descriptor type not supported: {binding.DescriptorType}");

                }


                _device.UpdateDescriptorSets(writeSets.ToArray(), null);

            }
        }


        /// <summary>
        /// Implicit conversion to a Pipeline
        /// </summary>
        public static implicit operator Pipeline(PipelineWrapper pipeline) { return pipeline.pipeline; }

    }

}
