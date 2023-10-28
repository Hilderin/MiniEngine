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

        public BufferWrapper UniformBuffer;

        private DescriptorSetLayout[] descriptorSetLayouts;
        private PipelineLayout _pipelineLayout;
        private Pipeline _pipeline;


        public Pipeline Pipeline { get { return _pipeline; } }
        public PipelineLayout PipelineLayout { get { return _pipelineLayout; } }
        public VkShader Shader { get { return _shader; } }
        public DescriptorSetLayout[] DescriptorSetLayouts { get { return descriptorSetLayouts; } }


        /// <summary>
        /// Constructor
        /// </summary>
        public PipelineWrapper(Device device, RenderPass renderPass, VkShader shader, CullModeFlags cullMode = CullModeFlags.Back)
        {
            _device = device;
            _shader = shader;
            _renderPass = renderPass;
           

            CreatePipeline(cullMode);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {

            if (_pipeline != null)
            {
                _device.DestroyPipeline(_pipeline);
                _pipeline = null;
            }

            if (_pipelineLayout != null)
            {
                _device.DestroyPipelineLayout(_pipelineLayout);
                _pipelineLayout = null;
            }

            if (descriptorSetLayouts != null)
            {
                foreach (var descriptorSetLayout in descriptorSetLayouts)
                    _device.DestroyDescriptorSetLayout(descriptorSetLayout);
                descriptorSetLayouts = null;
            }

            if (UniformBuffer != null)
            {
                UniformBuffer.Dispose();
                UniformBuffer = null;
            }

        }

        /// <summary>
        /// Create a descriptor set
        /// </summary>
        public PipelineDescriptorSet CreateDescriptorSet()
        {
            return new PipelineDescriptorSet(_device, this);
        }

        /// <summary>
        /// Create a descriptor set for one setIndex
        /// </summary>
        public PipelineDescriptorSet CreateDescriptorSet(int setIndex)
        {
            return new PipelineDescriptorSet(_device, this, setIndex);
        }

        /// <summary>
        /// Create the pipeline
        /// </summary>
        private void CreatePipeline(CullModeFlags cullMode)
        {
            //Descriptor set layout creation from shader...
            descriptorSetLayouts = new DescriptorSetLayout[_shader.BindingSets.Length];
            for (uint i = 0; i < descriptorSetLayouts.Length; i++)
            {
                descriptorSetLayouts[i] = _device.CreateDescriptorSetLayout(_shader.BindingSets[i]);
            }

            PushConstantRange[] constantRanges = _shader.Constants;

            //Pipeline layout creation...
            _pipelineLayout = _device.CreatePipelineLayout(descriptorSetLayouts, constantRanges);


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
                ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G | ColorComponentFlags.B | ColorComponentFlags.A,
                //TODO: Parameter that... see: C:\Projects\veldrid\src\Veldrid\BlendAttachmentDescription.cs
                BlendEnable = true,
                SrcColorBlendFactor = BlendFactor.SrcAlpha,
                DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
                DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
                AlphaBlendOp = BlendOp.Add
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
                CullMode = cullMode,
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
                Layout = _pipelineLayout,
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

            _pipeline = pipelines[0];

            //We don't need it anymore...
            _device.DestroyShaderModule(vertexShaderModule);
            _device.DestroyShaderModule(fragmentShaderModule);
        }


        ///// <summary>
        ///// Implicit conversion to a Pipeline
        ///// </summary>
        //public static implicit operator Pipeline(PipelineWrapper pipeline) { return pipeline._pipeline; }

    }

}
