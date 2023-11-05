//using MiniEngine.Drivers.Vulkan;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;

//namespace MiniEngine.Rendering.Vulkan
//{
//    /// <summary>
//    /// Contains a pipeline for bindless rendering
//    /// </summary>
//    public class PipelineBindlessWrapper : IDisposable
//    {
//        // Select a binding for each descriptor type
//        private int STORAGE_BINDING = 0;
//        private int SAMPLER_BINDING = 1;
//        private int IMAGE_BINDING = 2;
//        // Max count of each descriptor type
//        // You can query the max values for these with
//        //TODO: physicalDevice.getProperties().limits.maxDescriptrorSet*******
//        private int STORAGE_COUNT = 65536;
//        private int SAMPLER_COUNT = 65536;
//        private int IMAGE_COUNT = 65536;

//        #region Privates members

//        private Device _device;
//        private ShaderWrapper _shader;
//        private SwapchainWrapper _swapchain;

//        private DescriptorSetLayout[] descriptorSetLayouts;
//        private PipelineLayout _pipelineLayout;
//        private Pipeline _pipeline;
//        private PipelineShaderStageCreateInfo[] pipelineShaderStages;

//        private CullModeFlags _cullMode = CullModeFlags.None;
//        private DynamicState[] _dynamicStates = Array.Empty<DynamicState>();

//        private bool _depthTest;

//        #endregion


//        #region Public properties

//        public Pipeline Pipeline { get { return _pipeline; } }
//        public PipelineLayout PipelineLayout { get { return _pipelineLayout; } }
//        public ShaderWrapper Shader { get { return _shader; } }
//        public DescriptorSetLayout[] DescriptorSetLayouts { get { return descriptorSetLayouts; } }

//        #endregion

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        internal PipelineBindlessWrapper(Device device, SwapchainWrapper swapchain, ShaderWrapper shader)
//        {
//            _device = device;
//            _shader = shader;
//            _swapchain = swapchain;

//            //Create layaout only once, anyway, that will never change because we cannot change the shader
//            CreateLayouts();

//            //Same for the shaders
//            CreateShaders();
//        }

//        /// <summary>
//        /// Set the CullMode...
//        /// </summary>
//        public PipelineBindlessWrapper SetCullMode(CullModeFlags cullMode)
//        {
//            _cullMode = cullMode;
//            return this;
//        }

//        /// <summary>
//        /// Setup the depth test
//        /// </summary>
//        public PipelineBindlessWrapper SetDepthTest(bool depthTest)
//        {
//            _depthTest = depthTest;
//            return this;
//        }

//        /// <summary>
//        /// Add a dynamic state
//        /// </summary>
//        public PipelineBindlessWrapper AddDynamicState(DynamicState dynamicState)
//        {

//            if (!_dynamicStates.Contains(dynamicState))
//            {
//                List<DynamicState> newList = new List<DynamicState>(_dynamicStates)
//                {
//                    dynamicState
//                };
//                _dynamicStates = newList.ToArray();
//            }

//            return this;
//        }

//        /// <summary>
//        /// Remove a dynamic state
//        /// </summary>
//        public PipelineBindlessWrapper RemoveDynamicState(DynamicState dynamicState)
//        {

//            if (_dynamicStates.Contains(dynamicState))
//            {
//                List<DynamicState> newList = new List<DynamicState>(_dynamicStates);
//                newList.Remove(dynamicState);
//                _dynamicStates = newList.ToArray();
//            }

//            return this;
//        }

//        /// <summary>
//        /// Build the pipeline
//        /// </summary>
//        public PipelineBindlessWrapper Build()
//        {
//            if (_pipeline != null)
//                throw new InvalidOperationException("Pipeline already built. Use Rebuild method.");

//            CreatePipeline();

//            return this;
//        }

//        /// <summary>
//        /// Rebuild the pipeline
//        /// </summary>
//        public PipelineBindlessWrapper Rebuild()
//        {
//            if (_pipeline == null)
//                throw new InvalidOperationException("Pipeline not built. Use Nuild method.");

//            DestroyPipeline();

//            Build();

//            return this;
//        }

//        /// <summary>
//        /// Dispose
//        /// </summary>
//        public void Dispose()
//        {
//            DestroyPipeline();

//            if (_pipelineLayout != null)
//            {
//                _device.DestroyPipelineLayout(_pipelineLayout);
//                _pipelineLayout = null;
//            }

//            if (descriptorSetLayouts != null)
//            {
//                foreach (var descriptorSetLayout in descriptorSetLayouts)
//                    _device.DestroyDescriptorSetLayout(descriptorSetLayout);
//                descriptorSetLayouts = null;
//            }

//            _swapchain?.RemovePipelineWrapper(this);
//            _swapchain = null;

//        }

//        /// <summary>
//        /// Create a descriptor set
//        /// </summary>
//        public PipelineDescriptorSet CreateDescriptorSet()
//        {
//            return new PipelineDescriptorSet(_device, this);
//        }

//        /// <summary>
//        /// Create a descriptor set for one setIndex
//        /// </summary>
//        public PipelineDescriptorSet CreateDescriptorSet(int setIndex)
//        {
//            return new PipelineDescriptorSet(_device, this, setIndex);
//        }


//        /// <summary>
//        /// Create DescriptorSetLayout and PipelineLayout
//        /// </summary>
//        private void CreateLayouts()
//        {
//            //Descriptor set layout creation from shader...
//            descriptorSetLayouts = new DescriptorSetLayout[_shader.BindingSets.Length];
//            for (uint i = 0; i < descriptorSetLayouts.Length; i++)
//            {
//                descriptorSetLayouts[i] = _device.CreateDescriptorSetLayout(_shader.BindingSets[i]);
//            }

//            PushConstantRange[] constantRanges = _shader.Constants;

//            //Pipeline layout creation...
//            _pipelineLayout = _device.CreatePipelineLayout(descriptorSetLayouts, constantRanges);
//        }

//        /// <summary>
//        /// Create the shaders
//        /// </summary>
//        private void CreateShaders()
//        {
//            pipelineShaderStages = new[] {
//                new PipelineShaderStageCreateInfo {
//                    Stage = ShaderStageFlags.Vertex,
//                    Module = _shader.VertexShaderModule,
//                    Name = _shader.VertexEntryPoint
//                },
//                new PipelineShaderStageCreateInfo {
//                    Stage = ShaderStageFlags.Fragment,
//                    Module = _shader.FragmentShaderModule,
//                    Name = _shader.FragmentEntryPoint
//                }
//            };
//        }

//        /// <summary>
//        /// Create the pipeline
//        /// </summary>
//        private void CreatePipeline()
//        {            
//            var viewport = new Viewport
//            {
//                MinDepth = 0,
//                MaxDepth = 1.0f,
//                Width = _swapchain.CurrentExtent.Width,
//                Height = -_swapchain.CurrentExtent.Height,      //Inverting Y axis so the coord will be 
//                Y = _swapchain.CurrentExtent.Height
//            };
//            var scissor = new Rect2D { Extent = _swapchain.CurrentExtent };
//            var viewportCreateInfo = new PipelineViewportStateCreateInfo
//            {
//                Viewports = new Viewport[] { viewport },
//                Scissors = new Rect2D[] { scissor }
//            };

//            var multisampleCreateInfo = new PipelineMultisampleStateCreateInfo
//            {
//                RasterizationSamples = SampleCountFlags.Count1
//            };
//            var colorBlendAttachmentState = new PipelineColorBlendAttachmentState
//            {
//                ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G | ColorComponentFlags.B | ColorComponentFlags.A,
//                //TODO: Parameter that... see: C:\Projects\veldrid\src\Veldrid\BlendAttachmentDescription.cs
//                BlendEnable = true,
//                SrcColorBlendFactor = BlendFactor.SrcAlpha,
//                DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
//                ColorBlendOp = BlendOp.Add,
//                SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
//                DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
//                AlphaBlendOp = BlendOp.Add
//            };
//            var colorBlendStateCreatInfo = new PipelineColorBlendStateCreateInfo
//            {
//                LogicOp = LogicOp.Copy,
//                Attachments = new PipelineColorBlendAttachmentState[] { colorBlendAttachmentState }
//            };
//            var rasterizationStateCreateInfo = new PipelineRasterizationStateCreateInfo
//            {
//                PolygonMode = PolygonMode.Fill,
//                //CullMode = CullModeFlags.Front,
//                CullMode = _cullMode,
//                //CullMode = CullModeFlags.None,
//                FrontFace = FrontFace.Clockwise,
//                LineWidth = 1.0f
//            };
//            var inputAssemblyStateCreateInfo = new PipelineInputAssemblyStateCreateInfo
//            {
//                Topology = PrimitiveTopology.TriangleList
//            };

//            var vertexInputStateCreateInfo = new PipelineVertexInputStateCreateInfo
//            {
//                VertexBindingDescriptions = _shader.VertexBindings.ToArray(),
//                VertexAttributeDescriptions = _shader.VertexInputAttributes.ToArray()
//            };

            
//            var dynamicStateCreateInfo = new PipelineDynamicStateCreateInfo
//            {
//                DynamicStates = _dynamicStates
//            };

//            PipelineDepthStencilStateCreateInfo pipelineDepthStencil = new PipelineDepthStencilStateCreateInfo()
//            {
//                DepthTestEnable = _depthTest,
//                DepthWriteEnable = _depthTest,
//                DepthCompareOp = CompareOp.Less,
//                DepthBoundsTestEnable = false,
//                MinDepthBounds = 0f,
//                MaxDepthBounds = 1f,
//                StencilTestEnable = false
//            };

//            var pipelineCreateInfo = new GraphicsPipelineCreateInfo
//            {
//                Layout = _pipelineLayout,
//                ViewportState = viewportCreateInfo,
//                Stages = pipelineShaderStages,
//                MultisampleState = multisampleCreateInfo,
//                ColorBlendState = colorBlendStateCreatInfo,
//                RasterizationState = rasterizationStateCreateInfo,
//                InputAssemblyState = inputAssemblyStateCreateInfo,
//                VertexInputState = vertexInputStateCreateInfo,
//                RenderPass = _swapchain.RenderPass,
//                DynamicState = dynamicStateCreateInfo,
//                DepthStencilState = pipelineDepthStencil
//            };

//            //var pipelines = _device.CreateGraphicsPipelines(_device.CreatePipelineCache(new PipelineCacheCreateInfo()), new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });
//            var pipelines = _device.CreateGraphicsPipelines(null, new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });

//            _pipeline = pipelines[0];

//        }


//        /// <summary>
//        /// Destroy the internal pipeline
//        /// </summary>
//        private void DestroyPipeline()
//        {
//            if (_pipeline != null)
//            {
//                _device.DestroyPipeline(_pipeline);
//                _pipeline = null;
//            }
//        }


//        ///// <summary>
//        ///// Implicit conversion to a Pipeline
//        ///// </summary>
//        //public static implicit operator Pipeline(PipelineBindlessWrapper pipeline) { return pipeline._pipeline; }

//    }

//}
