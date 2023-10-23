using MiniEngine.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Contains a pipeline
    /// </summary>
    public class VkPipeline : IDisposable
    {
        private VkRenderer _vi;
        private ShaderBinder _shaderBinder;

        private CommandBuffer[] commandBuffers;
        private DescriptorSet[] descriptorSets;
        private DescriptorSetLayout descriptorSetLayout;
        public PipelineLayout pipelineLayout;
        private Buffer uniformBuffer;
        public Pipeline pipeline;

        public VkPipeline(VkRenderer vi, ShaderBinder shaderBinder)
        {
            _vi = vi;
            _shaderBinder = shaderBinder;

            Build();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (descriptorSetLayout != null)
            {
                _vi.Device.DestroyDescriptorSetLayout(descriptorSetLayout);
                descriptorSetLayout = null;
            }

            if (pipeline != null)
            {
                _vi.Device.DestroyPipeline(pipeline);
                pipeline = null;
            }

            if (pipelineLayout != null)
            {
                _vi.Device.DestroyPipelineLayout(pipelineLayout);
                pipelineLayout = null;
            }

            if (descriptorSetLayout != null)
            {
                _vi.Device.DestroyDescriptorSetLayout(descriptorSetLayout);
                descriptorSetLayout = null;
            }

        }

        /// <summary>
        /// Build the pipeline...
        /// </summary>
        public void Build()
        {
            //Descriptor set layout creation from shader...
            descriptorSetLayout = VkShaderHelper.CreateDescriptorSetLayout(_vi.Device, _shaderBinder);

            PushConstantRange[] constantRanges = {
                new() {
                    Size = (uint)Marshal.SizeOf<Matrix4>(),
                    StageFlags = ShaderStageFlags.Vertex
                }
            };

            //Pipeline layout creation...
            pipelineLayout = _vi.Device.CreatePipelineLayout(descriptorSetLayout, constantRanges);


            var vertShaderCode = VkShaderHelper.Compile(_shaderBinder.Shader.VertexCode, ShaderStageFlags.Vertex);
            var fragShaderCode = VkShaderHelper.Compile(_shaderBinder.Shader.FragmentCode, ShaderStageFlags.Fragment);


            var vertexShaderModule = _vi.Device.CreateShaderModule(vertShaderCode);
            var fragmentShaderModule = _vi.Device.CreateShaderModule(fragShaderCode);

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
                Width = _vi.CurrentExtent.Width,
                Height = -_vi.CurrentExtent.Height,      //Inverting Y axis so the coord will be 
                Y = _vi.CurrentExtent.Height
            };
            var scissor = new Rect2D { Extent = _vi.CurrentExtent };
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
                VertexBindingDescriptions = new VertexInputBindingDescription[] { GetVertexBindingDescription() },
                VertexAttributeDescriptions = GetVertexAttributeDescriptions()
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
                RenderPass = _vi.RenderPass
            };

            //var pipelines = _vi.Device.CreateGraphicsPipelines(_vi.Device.CreatePipelineCache(new PipelineCacheCreateInfo()), new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });
            var pipelines = _vi.Device.CreateGraphicsPipelines(null, new GraphicsPipelineCreateInfo[] { pipelineCreateInfo });

            pipeline = pipelines[0];

            //We don't need it anymore...
            _vi.Device.DestroyShaderModule(vertexShaderModule);
            _vi.Device.DestroyShaderModule(fragmentShaderModule);
        }


        /// <summary>
        /// Get the Vertex struct Binding Description
        /// </summary>
        public VertexInputBindingDescription GetVertexBindingDescription()
        {
            VertexInputBindingDescription bindingDescription = new()
            {
                Binding = 0,
                Stride = (uint)Unsafe.SizeOf<Vertex>(),
                InputRate = VertexInputRate.Vertex,
            };

            return bindingDescription;
        }

        /// <summary>
        /// Get the Vertex struct Binding Attributes Description
        /// </summary>
        public static VertexInputAttributeDescription[] GetVertexAttributeDescriptions()
        {
            var attributeDescriptions = new[]
            {
                new VertexInputAttributeDescription()
                {
                    Binding = 0,
                    Location = 0,
                    Format = Format.R32G32B32Sfloat,
                    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.Pos)),
                },
                new VertexInputAttributeDescription()
                {
                    Binding = 0,
                    Location = 1,
                    Format = Format.R32G32B32Sfloat,
                    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.Color)),
                },
                new VertexInputAttributeDescription()
                {
                    Binding = 0,
                    Location = 2,
                    Format = Format.R32G32Sfloat,
                    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord)),
                }
            };

            return attributeDescriptions;
        }

        /// <summary>
        /// Implicit conversion to a Pipeline
        /// </summary>
        public static implicit operator Pipeline(VkPipeline pipeline) { return pipeline.pipeline; }

    }
}
