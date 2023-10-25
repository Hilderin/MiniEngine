using System;

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

        private CommandBuffer[] commandBuffers;
        private DescriptorSet[] descriptorSets;
        private DescriptorSetLayout descriptorSetLayout;
        public PipelineLayout pipelineLayout;
        private Buffer uniformBuffer;
        public Pipeline pipeline;

        public PipelineWrapper(Device device, RenderPass renderPass, VkShader shader)
        {
            _device = device;
            _shader = shader;
            _renderPass = renderPass;

            Build();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
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

        }

        /// <summary>
        /// Build the pipeline...
        /// </summary>
        public void Build()
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
                    Name = "main"
                },
                new PipelineShaderStageCreateInfo {
                    Stage = ShaderStageFlags.Fragment,
                    Module = fragmentShaderModule,
                    Name = "main"
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
        /// Implicit conversion to a Pipeline
        /// </summary>
        public static implicit operator Pipeline(PipelineWrapper pipeline) { return pipeline.pipeline; }

    }
}
