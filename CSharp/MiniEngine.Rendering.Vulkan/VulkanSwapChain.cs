using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MiniEngine.Rendering.Vulkan
{
    public unsafe class VulkanSwapChain: IDisposable
    {


        #region Private members

        private VulkanInstance _vi;
        private Format swapChainImageFormat;
        private Extent2D swapChainExtent;
        public SwapchainKHR swapChain;
        private Image[] swapChainImages = null;
        private Vk VkApi;


        private ImageView[] swapChainImageViews = null;
        private Framebuffer[] swapChainFramebuffers = null;
        private ImageView depthImageView;


        private Image colorImage;
        private DeviceMemory colorImageMemory;
        private ImageView colorImageView;

        private Image depthImage;
        private DeviceMemory depthImageMemory;

        public CommandBuffer[] commandBuffers = null;
        

        private RenderPass renderPass;
        private PipelineLayout pipelineLayout;
        private Pipeline graphicsPipeline;

        private Buffer[] uniformBuffers = null;
        private DeviceMemory[] uniformBuffersMemory = null;

        private DescriptorPool descriptorPool;
        private DescriptorSet[] descriptorSets = null;

        #endregion

        /// <summary>
        /// Number of image in the swap chain
        /// </summary>
        public int NbSwapChainImages { get { return swapChainImages.Length; } }

        #region Constructor

                /// <summary>
                /// Constructor
                /// </summary>
        public VulkanSwapChain(VulkanInstance vi)
        {
            _vi = vi;
            VkApi = vi.VkApi;
        }

        #endregion

        #region Public methods

        public void Init()
        {
            CreateSwapChain();
            CreateImageViews();
            CreateRenderPass();
            CreateGraphicsPipeline();
            CreateColorResources();
            CreateDepthResources();
            CreateFramebuffers();
            CreateUniformBuffers();
            CreateDescriptorPool();
            CreateDescriptorSets();
            CreateCommandBuffers();
        }


        public void UpdateUniformBuffer(uint currentImage)
        {
            //Silk Window has timing information so we are skipping the time code.
            //var time = (float)sw.Elapsed.TotalSeconds;

            UniformBufferObject ubo = new()
            {
                model = Matrix4.Identity * Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), Math.DegToRad(90.0f)),
                view = Matrix4.CreateLookAt(new Vector3(2, 2, 2), new Vector3(0, 0, 0), new Vector3(0, 0, 1)),
                proj = Matrix4.CreatePerspectiveFieldOfView(Math.DegToRad(45.0f), (float)swapChainExtent.Width / swapChainExtent.Height, 0.1f, 10.0f),
            };
            ubo.proj.M22 *= -1;


            void* data;
            VkApi.MapMemory(_vi.device, uniformBuffersMemory[currentImage], 0, (ulong)Unsafe.SizeOf<UniformBufferObject>(), 0, &data);
            new Span<UniformBufferObject>(data, 1)[0] = ubo;
            VkApi.UnmapMemory(_vi.device, uniformBuffersMemory[currentImage]);

        }

        public void Dispose()
        {
            VkApi.DestroyImageView(_vi.device, depthImageView, null);
            VkApi.DestroyImage(_vi.device, depthImage, null);
            VkApi.FreeMemory(_vi.device, depthImageMemory, null);

            VkApi.DestroyImageView(_vi.device, colorImageView, null);
            VkApi.DestroyImage(_vi.device, colorImage, null);
            VkApi.FreeMemory(_vi.device, colorImageMemory, null);

            foreach (var framebuffer in swapChainFramebuffers)
            {
                VkApi.DestroyFramebuffer(_vi.device, framebuffer, null);
            }

            fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
            {
                VkApi.FreeCommandBuffers(_vi.device, _vi.commandPool, (uint)commandBuffers.Length, commandBuffersPtr);
            }

            VkApi.DestroyPipeline(_vi.device, graphicsPipeline, null);
            VkApi.DestroyPipelineLayout(_vi.device, pipelineLayout, null);
            VkApi.DestroyRenderPass(_vi.device, renderPass, null);

            foreach (var imageView in swapChainImageViews!)
            {
                VkApi.DestroyImageView(_vi.device, imageView, null);
            }

            _vi.khrSwapChain.DestroySwapchain(_vi.device, swapChain, null);

            for (int i = 0; i < swapChainImages.Length; i++)
            {
                VkApi.DestroyBuffer(_vi.device, uniformBuffers[i], null);
                VkApi.FreeMemory(_vi.device, uniformBuffersMemory[i], null);
            }

            VkApi.DestroyDescriptorPool(_vi.device, descriptorPool, null);
        }


        #endregion

        #region Private methods



        private void CreateSwapChain()
        {
            var swapChainSupport = _vi.QuerySwapChainSupport(_vi.physicalDevice);

            var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
            var extent = ChooseSwapExtent(swapChainSupport.Capabilities);

            var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.Capabilities.MaxImageCount;
            }

            SwapchainCreateInfoKHR creatInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _vi.surface,

                MinImageCount = imageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            };

            var indices = QueueFamiliesHelper.FindQueueFamilies(_vi, _vi.physicalDevice);
            var queueFamilyIndices = stackalloc[] { indices.GraphicsFamily.Value, indices.PresentFamily.Value };

            if (indices.GraphicsFamily != indices.PresentFamily)
            {
                creatInfo = creatInfo with
                {
                    ImageSharingMode = SharingMode.Concurrent,
                    QueueFamilyIndexCount = 2,
                    PQueueFamilyIndices = queueFamilyIndices,
                };
            }
            else
            {
                creatInfo.ImageSharingMode = SharingMode.Exclusive;
            }

            creatInfo = creatInfo with
            {
                PreTransform = swapChainSupport.Capabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = presentMode,
                Clipped = true,
            };

            //if (khrSwapChain is null)
            //{
            //    if (!VkApi.TryGetDeviceExtension(Instance, device, out khrSwapChain))
            //    {
            //        throw new NotSupportedException("VK_KHR_swapchain extension not found.");
            //    }
            //}

            if (_vi.khrSwapChain.CreateSwapchain(_vi.device, creatInfo, null, out swapChain) != Result.Success)
            {
                throw new Exception("failed to create swap chain!");
            }

            _vi.khrSwapChain.GetSwapchainImages(_vi.device, swapChain, ref imageCount, null);
            swapChainImages = new Image[imageCount];
            fixed (Image* swapChainImagesPtr = swapChainImages)
            {
                _vi.khrSwapChain.GetSwapchainImages(_vi.device, swapChain, ref imageCount, swapChainImagesPtr);
            }

            swapChainImageFormat = surfaceFormat.Format;
            swapChainExtent = extent;
        }


        private void CreateImageViews()
        {
            swapChainImageViews = new ImageView[swapChainImages.Length];

            for (int i = 0; i < swapChainImages.Length; i++)
            {

                swapChainImageViews[i] = VulkanImageHelper.CreateImageView(_vi, swapChainImages[i], swapChainImageFormat, ImageAspectFlags.ColorBit, 1);
            }
        }



        private void CreateRenderPass()
        {
            AttachmentDescription colorAttachment = new()
            {
                Format = swapChainImageFormat,
                Samples = _vi.msaaSamples,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.ColorAttachmentOptimal,
            };

            AttachmentDescription depthAttachment = new()
            {
                Format = FindDepthFormat(),
                Samples = _vi.msaaSamples,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
            };

            AttachmentDescription colorAttachmentResolve = new()
            {
                Format = swapChainImageFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.DontCare,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            };

            AttachmentReference colorAttachmentRef = new()
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            AttachmentReference depthAttachmentRef = new()
            {
                Attachment = 1,
                Layout = ImageLayout.DepthStencilAttachmentOptimal,
            };

            AttachmentReference colorAttachmentResolveRef = new()
            {
                Attachment = 2,
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            SubpassDescription subpass = new()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachmentRef,
                PDepthStencilAttachment = &depthAttachmentRef,
                PResolveAttachments = &colorAttachmentResolveRef,
            };

            SubpassDependency dependency = new()
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
                DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit
            };

            var attachments = new[] { colorAttachment, depthAttachment, colorAttachmentResolve };

            fixed (AttachmentDescription* attachmentsPtr = attachments)
            {
                RenderPassCreateInfo renderPassInfo = new()
                {
                    SType = StructureType.RenderPassCreateInfo,
                    AttachmentCount = (uint)attachments.Length,
                    PAttachments = attachmentsPtr,
                    SubpassCount = 1,
                    PSubpasses = &subpass,
                    DependencyCount = 1,
                    PDependencies = &dependency,
                };

                if (VkApi.CreateRenderPass(_vi.device, renderPassInfo, null, out renderPass) != Result.Success)
                {
                    throw new Exception("failed to create render pass!");
                }
            }
        }

        private void CreateGraphicsPipeline()
        {
            var vertShaderCode = System.IO.File.ReadAllBytes("shaders/vert.spv");
            var fragShaderCode = System.IO.File.ReadAllBytes("shaders/frag.spv");

            var vertShaderModule = VulkanShaderHelper.CreateShaderModule(_vi, vertShaderCode);
            var fragShaderModule = VulkanShaderHelper.CreateShaderModule(_vi, fragShaderCode);

            PipelineShaderStageCreateInfo vertShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = vertShaderModule,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };

            PipelineShaderStageCreateInfo fragShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = fragShaderModule,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };

            var shaderStages = stackalloc[]
            {
                vertShaderStageInfo,
                fragShaderStageInfo
            };

            var bindingDescription = Vertex.GetBindingDescription();
            var attributeDescriptions = Vertex.GetAttributeDescriptions();

            fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions)
            fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &_vi.descriptorSetLayout)
            {

                PipelineVertexInputStateCreateInfo vertexInputInfo = new()
                {
                    SType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexBindingDescriptionCount = 1,
                    VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length,
                    PVertexBindingDescriptions = &bindingDescription,
                    PVertexAttributeDescriptions = attributeDescriptionsPtr,
                };

                PipelineInputAssemblyStateCreateInfo inputAssembly = new()
                {
                    SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                    Topology = PrimitiveTopology.TriangleList,
                    PrimitiveRestartEnable = false,
                };

                Viewport viewport = new()
                {
                    X = 0,
                    Y = 0,
                    Width = swapChainExtent.Width,
                    Height = swapChainExtent.Height,
                    MinDepth = 0,
                    MaxDepth = 1,
                };

                Rect2D scissor = new()
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = swapChainExtent,
                };

                PipelineViewportStateCreateInfo viewportState = new()
                {
                    SType = StructureType.PipelineViewportStateCreateInfo,
                    ViewportCount = 1,
                    PViewports = &viewport,
                    ScissorCount = 1,
                    PScissors = &scissor,
                };

                PipelineRasterizationStateCreateInfo rasterizer = new()
                {
                    SType = StructureType.PipelineRasterizationStateCreateInfo,
                    DepthClampEnable = false,
                    RasterizerDiscardEnable = false,
                    PolygonMode = PolygonMode.Fill,
                    LineWidth = 1,
                    CullMode = CullModeFlags.BackBit,
                    FrontFace = FrontFace.CounterClockwise,
                    DepthBiasEnable = false,
                };

                PipelineMultisampleStateCreateInfo multisampling = new()
                {
                    SType = StructureType.PipelineMultisampleStateCreateInfo,
                    SampleShadingEnable = false,
                    RasterizationSamples = _vi.msaaSamples,
                };

                PipelineDepthStencilStateCreateInfo depthStencil = new()
                {
                    SType = StructureType.PipelineDepthStencilStateCreateInfo,
                    DepthTestEnable = true,
                    DepthWriteEnable = true,
                    DepthCompareOp = CompareOp.Less,
                    DepthBoundsTestEnable = false,
                    StencilTestEnable = false,
                };

                PipelineColorBlendAttachmentState colorBlendAttachment = new()
                {
                    ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
                    BlendEnable = false,
                };

                PipelineColorBlendStateCreateInfo colorBlending = new()
                {
                    SType = StructureType.PipelineColorBlendStateCreateInfo,
                    LogicOpEnable = false,
                    LogicOp = LogicOp.Copy,
                    AttachmentCount = 1,
                    PAttachments = &colorBlendAttachment,
                };

                colorBlending.BlendConstants[0] = 0;
                colorBlending.BlendConstants[1] = 0;
                colorBlending.BlendConstants[2] = 0;
                colorBlending.BlendConstants[3] = 0;

                PipelineLayoutCreateInfo pipelineLayoutInfo = new()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    PushConstantRangeCount = 0,
                    SetLayoutCount = 1,
                    PSetLayouts = descriptorSetLayoutPtr
                };

                if (VkApi.CreatePipelineLayout(_vi.device, pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
                {
                    throw new Exception("failed to create pipeline layout!");
                }

                GraphicsPipelineCreateInfo pipelineInfo = new()
                {
                    SType = StructureType.GraphicsPipelineCreateInfo,
                    StageCount = 2,
                    PStages = shaderStages,
                    PVertexInputState = &vertexInputInfo,
                    PInputAssemblyState = &inputAssembly,
                    PViewportState = &viewportState,
                    PRasterizationState = &rasterizer,
                    PMultisampleState = &multisampling,
                    PDepthStencilState = &depthStencil,
                    PColorBlendState = &colorBlending,
                    Layout = pipelineLayout,
                    RenderPass = renderPass,
                    Subpass = 0,
                    BasePipelineHandle = default
                };

                if (VkApi.CreateGraphicsPipelines(_vi.device, default, 1, pipelineInfo, null, out graphicsPipeline) != Result.Success)
                {
                    throw new Exception("failed to create graphics pipeline!");
                }
            }

            VkApi.DestroyShaderModule(_vi.device, fragShaderModule, null);
            VkApi.DestroyShaderModule(_vi.device, vertShaderModule, null);

            SilkMarshal.Free((nint)vertShaderStageInfo.PName);
            SilkMarshal.Free((nint)fragShaderStageInfo.PName);
        }


        /// <summary>
        /// Create color resources
        /// </summary>
        private void CreateColorResources()
        {
            Format colorFormat = swapChainImageFormat;

            VulkanImageHelper.CreateImage(_vi, swapChainExtent.Width, swapChainExtent.Height, 1, _vi.msaaSamples, colorFormat, ImageTiling.Optimal, ImageUsageFlags.TransientAttachmentBit | ImageUsageFlags.ColorAttachmentBit, MemoryPropertyFlags.DeviceLocalBit, ref colorImage, ref colorImageMemory);
            colorImageView = VulkanImageHelper.CreateImageView(_vi, colorImage, colorFormat, ImageAspectFlags.ColorBit, 1);

        }

        /// <summary>
        /// Create the depth desource
        /// </summary>
        private void CreateDepthResources()
        {
            Format depthFormat = FindDepthFormat();

            VulkanImageHelper.CreateImage(_vi, swapChainExtent.Width, swapChainExtent.Height, 1, _vi.msaaSamples, depthFormat, ImageTiling.Optimal, ImageUsageFlags.DepthStencilAttachmentBit, MemoryPropertyFlags.DeviceLocalBit, ref depthImage, ref depthImageMemory);
            depthImageView = VulkanImageHelper.CreateImageView(_vi, depthImage, depthFormat, ImageAspectFlags.DepthBit, 1);
        }

        /// <summary>
        /// Create the frame buffers
        /// </summary>
        private void CreateFramebuffers()
        {
            swapChainFramebuffers = new Framebuffer[swapChainImageViews.Length];

            for (int i = 0; i < swapChainImageViews.Length; i++)
            {
                var attachments = new[] { colorImageView, depthImageView, swapChainImageViews[i] };

                fixed (ImageView* attachmentsPtr = attachments)
                {
                    FramebufferCreateInfo framebufferInfo = new()
                    {
                        SType = StructureType.FramebufferCreateInfo,
                        RenderPass = renderPass,
                        AttachmentCount = (uint)attachments.Length,
                        PAttachments = attachmentsPtr,
                        Width = swapChainExtent.Width,
                        Height = swapChainExtent.Height,
                        Layers = 1,
                    };

                    if (VkApi.CreateFramebuffer(_vi.device, framebufferInfo, null, out swapChainFramebuffers[i]) != Result.Success)
                    {
                        throw new Exception("failed to create framebuffer!");
                    }
                }
            }
        }


        /// <summary>
        /// Create the uniform buffers
        /// </summary>
        private void CreateUniformBuffers()
        {
            ulong bufferSize = (ulong)Unsafe.SizeOf<UniformBufferObject>();

            uniformBuffers = new Buffer[swapChainImages.Length];
            uniformBuffersMemory = new DeviceMemory[swapChainImages.Length];

            for (int i = 0; i < swapChainImages.Length; i++)
            {
                VulkanMemoryHelper.CreateBuffer(_vi, bufferSize, BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, ref uniformBuffers[i], ref uniformBuffersMemory[i]);
            }

        }


        /// <summary>
        /// Create the descriptor pool
        /// </summary>
        private void CreateDescriptorPool()
        {
            var poolSizes = new DescriptorPoolSize[]
            {
            new DescriptorPoolSize()
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = (uint)swapChainImages.Length,
            },
            new DescriptorPoolSize()
            {
                Type = DescriptorType.CombinedImageSampler,
                DescriptorCount = (uint)swapChainImages.Length,
            }
            };

            fixed (DescriptorPoolSize* poolSizesPtr = poolSizes)
            fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
            {

                DescriptorPoolCreateInfo poolInfo = new()
                {
                    SType = StructureType.DescriptorPoolCreateInfo,
                    PoolSizeCount = (uint)poolSizes.Length,
                    PPoolSizes = poolSizesPtr,
                    MaxSets = (uint)swapChainImages.Length,
                };

                if (VkApi.CreateDescriptorPool(_vi.device, poolInfo, null, descriptorPoolPtr) != Result.Success)
                {
                    throw new Exception("failed to create descriptor pool!");
                }

            }
        }


        private void CreateDescriptorSets()
        {
            var layouts = new DescriptorSetLayout[swapChainImages.Length];
            Array.Fill(layouts, _vi.descriptorSetLayout);

            fixed (DescriptorSetLayout* layoutsPtr = layouts)
            {
                DescriptorSetAllocateInfo allocateInfo = new()
                {
                    SType = StructureType.DescriptorSetAllocateInfo,
                    DescriptorPool = descriptorPool,
                    DescriptorSetCount = (uint)swapChainImages.Length,
                    PSetLayouts = layoutsPtr,
                };

                descriptorSets = new DescriptorSet[swapChainImages.Length];
                fixed (DescriptorSet* descriptorSetsPtr = descriptorSets)
                {
                    if (VkApi.AllocateDescriptorSets(_vi.device, allocateInfo, descriptorSetsPtr) != Result.Success)
                    {
                        throw new Exception("failed to allocate descriptor sets!");
                    }
                }
            }


            for (int i = 0; i < swapChainImages.Length; i++)
            {
                DescriptorBufferInfo bufferInfo = new()
                {
                    Buffer = uniformBuffers[i],
                    Offset = 0,
                    Range = (ulong)Unsafe.SizeOf<UniformBufferObject>(),

                };

                DescriptorImageInfo imageInfo = new()
                {
                    ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                    ImageView = _vi.textureImageView,
                    Sampler = _vi.textureSampler,
                };

                var descriptorWrites = new WriteDescriptorSet[]
                {
                new()
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSets[i],
                    DstBinding = 0,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.UniformBuffer,
                    DescriptorCount = 1,
                    PBufferInfo = &bufferInfo,
                },
                new()
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSets[i],
                    DstBinding = 1,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 1,
                    PImageInfo = &imageInfo,
                }
                };

                fixed (WriteDescriptorSet* descriptorWritesPtr = descriptorWrites)
                {
                    VkApi.UpdateDescriptorSets(_vi.device, (uint)descriptorWrites.Length, descriptorWritesPtr, 0, null);
                }
            }

        }


        private void CreateCommandBuffers()
        {
            commandBuffers = new CommandBuffer[swapChainFramebuffers.Length];

            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = _vi.commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)commandBuffers.Length,
            };

            fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
            {
                if (VkApi.AllocateCommandBuffers(_vi.device, allocInfo, commandBuffersPtr) != Result.Success)
                {
                    throw new Exception("failed to allocate command buffers!");
                }
            }


            for (int i = 0; i < commandBuffers.Length; i++)
            {
                CommandBufferBeginInfo beginInfo = new()
                {
                    SType = StructureType.CommandBufferBeginInfo,
                };

                if (VkApi.BeginCommandBuffer(commandBuffers[i], beginInfo) != Result.Success)
                {
                    throw new Exception("failed to begin recording command buffer!");
                }

                RenderPassBeginInfo renderPassInfo = new()
                {
                    SType = StructureType.RenderPassBeginInfo,
                    RenderPass = renderPass,
                    Framebuffer = swapChainFramebuffers[i],
                    RenderArea =
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = swapChainExtent,
                }
                };

                var clearValues = new ClearValue[]
                {
                new()
                {
                    Color = new (){ Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1 },
                },
                new()
                {
                    DepthStencil = new () { Depth = 1, Stencil = 0 }
                }
                };


                fixed (ClearValue* clearValuesPtr = clearValues)
                {
                    renderPassInfo.ClearValueCount = (uint)clearValues.Length;
                    renderPassInfo.PClearValues = clearValuesPtr;

                    VkApi.CmdBeginRenderPass(commandBuffers[i], &renderPassInfo, SubpassContents.Inline);
                }

                VkApi.CmdBindPipeline(commandBuffers[i], PipelineBindPoint.Graphics, graphicsPipeline);

                var vertexBuffers = new Buffer[] { _vi.vertexBuffer };
                var offsets = new ulong[] { 0 };

                fixed (ulong* offsetsPtr = offsets)
                fixed (Buffer* vertexBuffersPtr = vertexBuffers)
                {
                    VkApi.CmdBindVertexBuffers(commandBuffers[i], 0, 1, vertexBuffersPtr, offsetsPtr);
                }

                VkApi.CmdBindIndexBuffer(commandBuffers[i], _vi.indexBuffer, 0, IndexType.Uint32);

                VkApi.CmdBindDescriptorSets(commandBuffers[i], PipelineBindPoint.Graphics, pipelineLayout, 0, 1, descriptorSets[i], 0, null);

                VkApi.CmdDrawIndexed(commandBuffers[i], (uint)_vi.indices.Length, 1, 0, 0, 0);

                VkApi.CmdEndRenderPass(commandBuffers[i]);

                if (VkApi.EndCommandBuffer(commandBuffers[i]) != Result.Success)
                {
                    throw new Exception("failed to record command buffer!");
                }

            }
        }


        private SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
        {
            foreach (var availableFormat in availableFormats)
            {
                if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                {
                    return availableFormat;
                }
            }

            return availableFormats[0];
        }

        private PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes)
        {
            foreach (var availablePresentMode in availablePresentModes)
            {
                if (availablePresentMode == PresentModeKHR.MailboxKhr)
                {
                    return availablePresentMode;
                }
            }

            return PresentModeKHR.FifoKhr;
        }

        private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                return capabilities.CurrentExtent;
            }
            else
            {
                var framebufferSize = _vi.Window.FramebufferSize;

                Extent2D actualExtent = new()
                {
                    Width = (uint)framebufferSize.X,
                    Height = (uint)framebufferSize.Y
                };

                actualExtent.Width = (uint)Math.Clamp(actualExtent.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
                actualExtent.Height = (uint)Math.Clamp(actualExtent.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);

                return actualExtent;
            }
        }



        private Format FindDepthFormat()
        {
            return FindSupportedFormat(new[] { Format.D32Sfloat, Format.D32SfloatS8Uint, Format.D24UnormS8Uint }, ImageTiling.Optimal, FormatFeatureFlags.DepthStencilAttachmentBit);
        }


        private Format FindSupportedFormat(IEnumerable<Format> candidates, ImageTiling tiling, FormatFeatureFlags features)
        {
            foreach (var format in candidates)
            {
                VkApi.GetPhysicalDeviceFormatProperties(_vi.physicalDevice, format, out var props);

                if (tiling == ImageTiling.Linear && (props.LinearTilingFeatures & features) == features)
                {
                    return format;
                }
                else if (tiling == ImageTiling.Optimal && (props.OptimalTilingFeatures & features) == features)
                {
                    return format;
                }
            }

            throw new Exception("failed to find supported format!");
        }



        #endregion
    }
}
