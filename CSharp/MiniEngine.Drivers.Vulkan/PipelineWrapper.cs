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
        #region Privates members

        private Device _device;
        private VkShader _shader;
        private RenderPass _renderPass;

        private DescriptorSetLayout[] descriptorSetLayouts;
        private PipelineLayout _pipelineLayout;
        private Pipeline _pipeline;
        private PipelineShaderStageCreateInfo[] pipelineShaderStages;

        private CullModeFlags _cullMode = CullModeFlags.None;
        private DynamicState[] _dynamicStates = new DynamicState[0];

        #endregion


        #region Public properties

        public Pipeline Pipeline { get { return _pipeline; } }
        public PipelineLayout PipelineLayout { get { return _pipelineLayout; } }
        public VkShader Shader { get { return _shader; } }
        public DescriptorSetLayout[] DescriptorSetLayouts { get { return descriptorSetLayouts; } }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public PipelineWrapper(Device device, RenderPass renderPass, VkShader shader)
        {
            _device = device;
            _shader = shader;
            _renderPass = renderPass;

            //Create layaout only once, anyway, that will never change because we cannot change the shader
            CreateLayouts();

            //Same for the shaders
            CreateShaders();
        }

        /// <summary>
        /// Set the CullMode...
        /// </summary>
        public PipelineWrapper SetCullMode(CullModeFlags cullMode)
        {
            _cullMode = cullMode;
            return this;
        }

        /// <summary>
        /// Add a dynamic state
        /// </summary>
        public PipelineWrapper AddDynamicState(DynamicState dynamicState)
        {

            if (!_dynamicStates.Contains(dynamicState))
            {
                List<DynamicState> newList = new List<DynamicState>(_dynamicStates);
                newList.Add(dynamicState);
                _dynamicStates = newList.ToArray();
            }

            return this;
        }

        /// <summary>
        /// Remove a dynamic state
        /// </summary>
        public PipelineWrapper RemoveDynamicState(DynamicState dynamicState)
        {

            if (_dynamicStates.Contains(dynamicState))
            {
                List<DynamicState> newList = new List<DynamicState>(_dynamicStates);
                newList.Remove(dynamicState);
                _dynamicStates = newList.ToArray();
            }

            return this;
        }

        /// <summary>
        /// Build the pipeline
        /// </summary>
        public PipelineWrapper Build()
        {
            if (_pipeline != null)
                DestroyPipeline();

            CreatePipeline();

            return this;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            DestroyPipeline();

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
        /// Create DescriptorSetLayout and PipelineLayout
        /// </summary>
        private void CreateLayouts()
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
        }

        /// <summary>
        /// Create the shaders
        /// </summary>
        private void CreateShaders()
        {
            var vertexShaderModule = _device.CreateShaderModule(_shader.VertexSpirv);
            var fragmentShaderModule = _device.CreateShaderModule(_shader.FragmentSpirv);

            pipelineShaderStages = new[] {
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
        }

        /// <summary>
        /// Create the pipeline
        /// </summary>
        private void CreatePipeline()
        {            
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
                CullMode = _cullMode,
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

            
            var dynamicStateCreateInfo = new PipelineDynamicStateCreateInfo
            {
                DynamicStates = _dynamicStates
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
                RenderPass = _renderPass,
                DynamicState = dynamicStateCreateInfo
            };

            //var pipelines = _device.CreateGraphicsPipelines(_device.CreatePipelineCache(new PipelineCacheCreateInfo()), new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });
            var pipelines = _device.CreateGraphicsPipelines(null, new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });

            _pipeline = pipelines[0];

        }


        /// <summary>
        /// Destroy the internal pipeline
        /// </summary>
        private void DestroyPipeline()
        {
            if (_pipeline != null)
            {
                _device.DestroyPipeline(_pipeline);
                _pipeline = null;
            }
        }


        ///// <summary>
        ///// Implicit conversion to a Pipeline
        ///// </summary>
        //public static implicit operator Pipeline(PipelineWrapper pipeline) { return pipeline._pipeline; }

    }

}
