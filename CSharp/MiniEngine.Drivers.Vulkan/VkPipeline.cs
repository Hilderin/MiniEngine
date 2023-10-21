using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Contains a pipeline
    /// </summary>
    public class VkPipeline: IDisposable
    {
        private VkRenderer _vi;
        private Shader _shader;

        private CommandBuffer[] commandBuffers;
        private DescriptorSet[] descriptorSets;
        private DescriptorSetLayout descriptorSetLayout;
        private PipelineLayout pipelineLayout;
        private Buffer uniformBuffer;
        public Pipeline pipeline;

        public VkPipeline(VkRenderer vi, Shader shader)
        {
            _vi = vi;
            _shader = shader;

            Build();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
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
        }

        public void Build()
        {
            //var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
            //{
            //    SetLayouts = new DescriptorSetLayout[] { descriptorSetLayout }
            //};
            //pipelineLayout = device.CreatePipelineLayout(pipelineLayoutCreateInfo);
            var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
            {
                SetLayouts = new DescriptorSetLayout[0]
            };
            pipelineLayout = _vi.Device.CreatePipelineLayout(pipelineLayoutCreateInfo);


            var vertShaderCode = ShaderHelper.Compile(_shader.VertexCode, ShaderStageFlags.Vertex);
            var fragShaderCode = ShaderHelper.Compile(_shader.FragmentCode, ShaderStageFlags.Fragment);


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
            var viewport = new Viewport
            {
                MinDepth = 0,
                MaxDepth = 1.0f,
                Width = _vi.CurrentExtent.Width,
                Height = _vi.CurrentExtent.Height
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
                CullMode = (uint)CullModeFlags.None,
                FrontFace = FrontFace.Clockwise,
                LineWidth = 1.0f
            };
            var inputAssemblyStateCreateInfo = new PipelineInputAssemblyStateCreateInfo
            {
                Topology = PrimitiveTopology.TriangleList
            };
            var vertexInputBindingDescription = new VertexInputBindingDescription
            {
                Stride = 3 * sizeof(float),
                InputRate = VertexInputRate.Vertex
            };
            var vertexInputAttributeDescription = new VertexInputAttributeDescription
            {
                Format = Format.R32G32B32Sfloat
            };
            //var vertexInputStateCreateInfo = new PipelineVertexInputStateCreateInfo
            //{
            //    VertexBindingDescriptions = new VertexInputBindingDescription[] { vertexInputBindingDescription },
            //    VertexAttributeDescriptions = new VertexInputAttributeDescription[] { vertexInputAttributeDescription }
            //};
            var vertexInputStateCreateInfo = new PipelineVertexInputStateCreateInfo
            {
                VertexBindingDescriptions = new VertexInputBindingDescription[0],
                VertexAttributeDescriptions = new VertexInputAttributeDescription[0]
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
        /// Implicit conversion to a Pipeline
        /// </summary>
        public static implicit operator Pipeline(VkPipeline pipeline) { return pipeline.pipeline; }

    }
}
