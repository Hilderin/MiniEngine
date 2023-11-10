using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Contains a pipeline
    /// </summary>
    public class PipelineWrapper : IDisposable
    {
        #region Privates members

        private Device _device;
        private ShaderWrapper _shader;
        private SwapchainWrapper _swapchain;

        private bool _bindless;
        private bool _depthTest;

        private PipelineDescriptorSet _bindlessDescriptorSet;
        private DescriptorSetLayout[] descriptorSetLayouts;
        private PipelineLayout _pipelineLayout;
        private Pipeline _pipeline;
        private PipelineShaderStageCreateInfo[] _pipelineShaderStages;

        private CullModeFlags _cullMode = CullModeFlags.None;
        private DynamicState[] _dynamicStates = Array.Empty<DynamicState>();
        private Dictionary<int, uint> _imageSamplerBindlessIndex = new Dictionary<int, uint>();
        private Dictionary<string, object> _specializationValues = new Dictionary<string, object>();


        #endregion


        #region Public properties

        public Pipeline Pipeline { get { return _pipeline; } }
        public PipelineLayout PipelineLayout { get { return _pipelineLayout; } }
        public ShaderWrapper Shader { get { return _shader; } }
        public DescriptorSetLayout[] DescriptorSetLayouts { get { return descriptorSetLayouts; } }
        public bool Bindless { get { return _bindless; } }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        internal PipelineWrapper(Device device, SwapchainWrapper swapchain, ShaderWrapper shader)
        {
            _device = device;
            _shader = shader;
            _swapchain = swapchain;


            //Create layaout only once, anyway, that will never change because we cannot change the shader
            CreateLayouts();

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
        /// Setup the depth test
        /// </summary>
        public PipelineWrapper SetDepthTest(bool depthTest)
        {
            _depthTest = depthTest;
            return this;
        }

        /// <summary>
        /// Add a dynamic state
        /// </summary>
        public PipelineWrapper AddDynamicState(DynamicState dynamicState)
        {

            if (!_dynamicStates.Contains(dynamicState))
            {
                List<DynamicState> newList = new List<DynamicState>(_dynamicStates)
                {
                    dynamicState
                };
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
        /// Set a value for a specialization
        /// </summary>
        public PipelineWrapper SetSpecializationValue(string specConstantName, object value)
        {
            if (!_shader.SpecializationConstants.Any(c => c.Name == specConstantName))
                throw new InvalidOperationException($"Specialization constant not found '{specConstantName}'");

            if (value == null)
                _specializationValues.Remove(specConstantName);
            else
                _specializationValues[specConstantName] = value;

            return this;
        }

        /// <summary>
        /// Build the pipeline
        /// </summary>
        public PipelineWrapper Build()
        {
            if (_pipeline != null)
                throw new InvalidOperationException("Pipeline already built. Use Rebuild method.");


            //Same for the shaders
            CreateShaderStages();

            CreatePipeline();

            return this;
        }

        /// <summary>
        /// Rebuild the pipeline
        /// </summary>
        public PipelineWrapper Rebuild()
        {
            if (_pipeline == null)
                throw new InvalidOperationException("Pipeline not built. Use Nuild method.");

            DestroyPipeline();

            Build();

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

            _swapchain?.RemovePipelineWrapper(this);
            _swapchain = null;

        }

        /// <summary>
        /// Get or Add an bindless index for an image and a sampler
        /// </summary>
        public uint GetOrAddBindlessIndex(ImageView imageView, Sampler sampler)
        {
            int key = BytesHelper.CombineHash(imageView.GetHashCode(), sampler.GetHashCode());
            if (!_imageSamplerBindlessIndex.TryGetValue(key, out uint index))
            {
                index = (uint)_imageSamplerBindlessIndex.Count;
                _imageSamplerBindlessIndex.Add(key, index);

                GetBindlessDescriptorSet().Set(ShaderVariableNames.SamplerDiffuse, imageView, sampler, index);
            }
            return index;
        }


        /// <summary>
        /// Create a descriptor set
        /// </summary>
        public PipelineDescriptorSet GetBindlessDescriptorSet()
        {
            if (!_bindless)
                throw new InvalidOperationException("Cannot use GetBindlessDescriptorSet for a bindless pipeline. Must use CreateDescriptorSet.");

            _bindlessDescriptorSet ??= new PipelineDescriptorSet(_device, this);

            return _bindlessDescriptorSet;
        }

        /// <summary>
        /// Create a descriptor set
        /// </summary>
        public PipelineDescriptorSet CreateDescriptorSet()
        {
            if (_bindless)
                throw new InvalidOperationException("Cannot create new DescriptorSet for bindless pipeline. You must use GetBindlessDescriptorSet.");

            return new PipelineDescriptorSet(_device, this);
        }

        /// <summary>
        /// Create a descriptor set for one setIndex
        /// </summary>
        public PipelineDescriptorSet CreateDescriptorSet(int setIndex)
        {
            if (_bindless)
                throw new InvalidOperationException("Cannot create new DescriptorSet for bindless pipeline. You must use GetBindlessDescriptorSet.");

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
                //Flag each set's bindings as partiallyBound and updateAfterBind features
                bool atLeastOneBindless = false;

                DescriptorSetLayoutCreateFlags layoutFlags = 0;
                DescriptorBindingFlags[] bindingFlags = new DescriptorBindingFlags[_shader.BindingSets[i].Length];

                for (int iDesc = 0; iDesc < bindingFlags.Length; iDesc++)
                {
                    if (_shader.BindingSets[i][iDesc].Bindless)
                    {
                        bindingFlags[iDesc] = DescriptorBindingFlags.PartiallyBound | DescriptorBindingFlags.UpdateAfterBind;
                        atLeastOneBindless = true;
                    }
                }

                DescriptorSetLayoutBindingFlagsCreateInfo extended_info = null;
                if (atLeastOneBindless)
                {
                    layoutFlags |= DescriptorSetLayoutCreateFlags.UpdaterAfterBindPool;

                    extended_info = new DescriptorSetLayoutBindingFlagsCreateInfo()
                    {
                        BindingFlags = bindingFlags
                    };

                    _bindless = true;
                }
                using (var descriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo
                {
                    Bindings = _shader.BindingSets[i],
                    Next = extended_info != null ? extended_info.Handle : IntPtr.Zero,
                    Flags = layoutFlags
                })
                {
                    descriptorSetLayouts[i] = _device.CreateDescriptorSetLayout(descriptorSetLayoutCreateInfo);
                }
            }

            PushConstantRange[] constantRanges = _shader.Constants;

            //Pipeline layout creation...
            _pipelineLayout = _device.CreatePipelineLayout(descriptorSetLayouts, constantRanges);
        }

        /// <summary>
        /// Create the shader stages
        /// </summary>
        private void CreateShaderStages()
        {
            List<PipelineShaderStageCreateInfo> stages = new List<PipelineShaderStageCreateInfo>();

            foreach (var shaderStageModule in _shader.StageModules)
            {
                var stageCreateInfo = new PipelineShaderStageCreateInfo
                {
                    Stage = shaderStageModule.Stage,
                    Module = shaderStageModule.Module,
                    Name = shaderStageModule.Entrypoint
                };

                //Check for specialization constants...
                if (_specializationValues.Count > 0 && _shader.SpecializationConstants.Length > 0)
                {
                    stageCreateInfo.SpecializationInfo = CreateSpecializationInfo(shaderStageModule);
                }

                stages.Add(stageCreateInfo);
            }

            _pipelineShaderStages = stages.ToArray();
        }

        /// <summary>
        /// Create the specialization data
        /// </summary>
        private SpecializationInfo CreateSpecializationInfo(ShaderStageModule shaderStageModule)
        {
            SpecializationInfo specializationInfo = null;

            uint dataLength = 0;
            bool redefinedFound = false;
            int nbEntry = 0;
            for (int i = 0; i < _shader.SpecializationConstants.Length; i++)
            {
                if (_shader.SpecializationConstants[i].Stage == shaderStageModule.Stage)
                {
                    dataLength += _shader.SpecializationConstants[i].Size;
                    nbEntry++;

                    if (_specializationValues.ContainsKey(_shader.SpecializationConstants[i].Name))
                        redefinedFound = true;
                }
            }

            if (redefinedFound)
            {
                //We must copy the data into memory...
                //byte[] data = new byte[dataLength];
                NativeReference data = new NativeReference((int)dataLength);

                SpecializationMapEntry[] entries = new SpecializationMapEntry[nbEntry];

                int entryIndex = 0;
                int offset = 0;
                for (int i = 0; i < _shader.SpecializationConstants.Length; i++)
                {
                    if (_shader.SpecializationConstants[i].Stage == shaderStageModule.Stage)
                    {
                        object value;

                        if (!_specializationValues.TryGetValue(_shader.SpecializationConstants[i].Name, out value))
                            value = _shader.SpecializationConstants[i].DefaultValue;

                        uint size = (uint)Marshal.SizeOf(value);
                        SpecializationMapEntry mapEntry = new SpecializationMapEntry()
                        {
                            ConstantId = _shader.SpecializationConstants[i].ConstantId,
                            Offset = (uint)offset,
                            Size = size
                        };
                        offset += (int)size;
                        entries[entryIndex] = mapEntry;
                        entryIndex++;

                        Type valueType = value.GetType();
                        if (valueType == typeof(int))
                        {
                            IntPtrHelper.Write((int)value, data.Handle, offset);
                        }
                        else if (valueType == typeof(uint))
                        {
                            IntPtrHelper.Write((uint)value, data.Handle, offset);
                        }
                        else if (valueType == typeof(float))
                        {
                            IntPtrHelper.Write((float)value, data.Handle, offset);
                        }
                        else
                            throw new NotSupportedException($"CreateSpecializationInfo unsupported datatype: {valueType.Name}");

                    }

                }

                specializationInfo = new SpecializationInfo()
                {
                    MapEntries = entries,
                    Data = data.Handle,
                    DataSize = dataLength
                };
            }

            return specializationInfo;
        }

        /// <summary>
        /// Create the pipeline
        /// </summary>
        private void CreatePipeline()
        {

            if (_shader.IsComputeOnly)
            {
                //Compute only...
                var pipelineCreateInfo = new ComputePipelineCreateInfo
                {
                    Layout = _pipelineLayout,
                    Stage = _pipelineShaderStages[0]
                };

                //var pipelines = _device.CreateGraphicsPipelines(_device.CreatePipelineCache(new PipelineCacheCreateInfo()), new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });
                var pipelines = _device.CreateComputePipelines(null, new ComputePipelineCreateInfo[] { pipelineCreateInfo });
                _pipeline = pipelines[0];
            }
            else
            {
                //Graphics....

                var viewport = new Viewport
                {
                    MinDepth = 0,
                    MaxDepth = 1.0f,
                    Width = _swapchain.CurrentExtent.Width,
                    Height = -_swapchain.CurrentExtent.Height,      //Inverting Y axis so the coord will be 
                    Y = _swapchain.CurrentExtent.Height
                };
                var scissor = new Rect2D { Extent = _swapchain.CurrentExtent };
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

                PipelineDepthStencilStateCreateInfo pipelineDepthStencil = new PipelineDepthStencilStateCreateInfo()
                {
                    DepthTestEnable = _depthTest,
                    DepthWriteEnable = _depthTest,
                    DepthCompareOp = CompareOp.Less,
                    DepthBoundsTestEnable = false,
                    MinDepthBounds = 0f,
                    MaxDepthBounds = 1f,
                    StencilTestEnable = false
                };

                var pipelineCreateInfo = new GraphicsPipelineCreateInfo
                {
                    Layout = _pipelineLayout,
                    ViewportState = viewportCreateInfo,
                    Stages = _pipelineShaderStages,
                    MultisampleState = multisampleCreateInfo,
                    ColorBlendState = colorBlendStateCreatInfo,
                    RasterizationState = rasterizationStateCreateInfo,
                    InputAssemblyState = inputAssemblyStateCreateInfo,
                    VertexInputState = vertexInputStateCreateInfo,
                    RenderPass = _swapchain.RenderPass,
                    DynamicState = dynamicStateCreateInfo,
                    DepthStencilState = pipelineDepthStencil
                };

                //var pipelines = _device.CreateGraphicsPipelines(_device.CreatePipelineCache(new PipelineCacheCreateInfo()), new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });
                var pipelines = _device.CreateGraphicsPipelines(null, new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });

                _pipeline = pipelines[0];

            }

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


        /// <summary>
        /// Implicit conversion to a Pipeline
        /// </summary>
        public static implicit operator Pipeline(PipelineWrapper pipeline) { return pipeline._pipeline; }

        /// <summary>
        /// Implicit conversion to a Pipeline layout
        /// </summary>
        public static implicit operator PipelineLayout(PipelineWrapper pipeline) { return pipeline._pipelineLayout; }

    }

}
