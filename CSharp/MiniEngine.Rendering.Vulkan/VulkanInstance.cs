using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Assimp;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Instance for Vulkan engine
    /// </summary>
    public unsafe class VulkanInstance : IDisposable
    {
        #region Constants

        const string MODEL_PATH = @"Assets\viking_room.obj";
        const string TEXTURE_PATH = @"Assets\viking_room.png";

        const int MAX_FRAMES_IN_FLIGHT = 2;

       

        

        #endregion

        #region Public members

        public readonly string[] validationLayers = new[]
        {
            "VK_LAYER_KHRONOS_validation"
        };

        public readonly string[] deviceExtensions = new[]
        {
            KhrSwapchain.ExtensionName
        };

        public Vk VkApi;

        public Instance Instance;

        public bool EnableValidationLayers = true;

        public string ApplicationName;

        public KhrSurface khrSurface = null;
        public SurfaceKHR surface;

        public Window Window;

        public PhysicalDevice physicalDevice;

        public SampleCountFlags msaaSamples = SampleCountFlags.Count1Bit;

        public Device device;

        public Queue graphicsQueue;
        public Queue presentQueue;

        public KhrSwapchain khrSwapChain = null;

        public CommandPool commandPool;


        public ImageView textureImageView;
        public Sampler textureSampler;

        public Buffer vertexBuffer;
        public DeviceMemory vertexBufferMemory;
        public Buffer indexBuffer;
        public DeviceMemory indexBufferMemory;

        public Vertex[] vertices = null;

        public uint[] indices = null;


        public DescriptorSetLayout descriptorSetLayout;

        #endregion

        #region Private members










        private uint mipLevels;
        private Image textureImage;
        private DeviceMemory textureImageMemory;


        

        

        private Semaphore[] imageAvailableSemaphores = null;
        private Semaphore[] renderFinishedSemaphores = null;
        private Fence[] inFlightFences = null;
        private Fence[] imagesInFlight = null;
        private int currentFrame = 0;

        private bool frameBufferResized = false;

        

        private VulkanInitializer _initializer = null;

        private VulkanSwapChain _swapChain = null;


        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="applicationName"></param>
        public VulkanInstance(string applicationName, bool enableValidationLayers)
        {
            ApplicationName = applicationName;
            EnableValidationLayers = enableValidationLayers;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Set the current window
        /// </summary>
        /// <param name="window"></param>
        public void SetWindow(Window window)
        {
            this.Window = window;
            window.OnWindowResized += FramebufferResizeCallback;
        }

        #endregion

        /// <summary>
        /// Init the vulkan engine
        /// </summary>
        public void Init()
        {
            _initializer = new VulkanInitializer(this);

            _initializer.Init();

            CreateTextureImage();
            CreateTextureImageView();
            CreateTextureSampler();
            LoadModel();
            CreateVertexBuffer();
            CreateIndexBuffer();
            CreateDescriptorSetLayout();

            _swapChain = new VulkanSwapChain(this);

            _swapChain.Init();

            //CreateImageViews();
            //CreateRenderPass();
            //CreateGraphicsPipeline();
            //CreateColorResources();
            //CreateDepthResources();
            //CreateFramebuffers();
            //CreateUniformBuffers();
            //CreateDescriptorPool();
            //CreateDescriptorSets();
            //CreateCommandBuffers();

            
            
            
            CreateSyncObjects();
        }



        private void FramebufferResizeCallback(Vector2 newSize)
        {
            frameBufferResized = true;
        }



        public void Dispose()
        {
            //Wait everything that is in progress to be done...
            VkApi.DeviceWaitIdle(device);

            _swapChain?.Dispose();

            VkApi.DestroySampler(device, textureSampler, null);
            VkApi.DestroyImageView(device, textureImageView, null);

            VkApi.DestroyImage(device, textureImage, null);
            VkApi.FreeMemory(device, textureImageMemory, null);

            VkApi.DestroyDescriptorSetLayout(device, descriptorSetLayout, null);

            VkApi.DestroyBuffer(device, indexBuffer, null);
            VkApi.FreeMemory(device, indexBufferMemory, null);

            VkApi.DestroyBuffer(device, vertexBuffer, null);
            VkApi.FreeMemory(device, vertexBufferMemory, null);

            for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                VkApi.DestroySemaphore(device, renderFinishedSemaphores[i], null);
                VkApi.DestroySemaphore(device, imageAvailableSemaphores[i], null);
                VkApi.DestroyFence(device, inFlightFences[i], null);
            }

            


            

            _initializer.Dispose();

        }

        private void RecreateSwapChain()
        {
            Vector2 framebufferSize = Window.FramebufferSize;
            //Vector2D<int> framebufferSize = window.FramebufferSize;

            if (framebufferSize.X == 0 || framebufferSize.Y == 0)
                return;
            //while (framebufferSize.X == 0 || framebufferSize.Y == 0)
            //{
            //    framebufferSize = window.FramebufferSize;
            //    window.DoEvents();
            //}

            VkApi.DeviceWaitIdle(device);

            _swapChain.Dispose();


            _swapChain = new VulkanSwapChain(this);
            _swapChain.Init();

            //CreateImageViews();
            //CreateRenderPass();
            //CreateGraphicsPipeline();
            //CreateColorResources();
            //CreateDepthResources();
            //CreateFramebuffers();
            //CreateUniformBuffers();
            //CreateDescriptorPool();
            //CreateDescriptorSets();
            //CreateCommandBuffers();

            imagesInFlight = new Fence[_swapChain.NbSwapChainImages];
        }




        private void CreateSyncObjects()
        {
            imageAvailableSemaphores = new Semaphore[MAX_FRAMES_IN_FLIGHT];
            renderFinishedSemaphores = new Semaphore[MAX_FRAMES_IN_FLIGHT];
            inFlightFences = new Fence[MAX_FRAMES_IN_FLIGHT];
            imagesInFlight = new Fence[_swapChain.NbSwapChainImages];

            SemaphoreCreateInfo semaphoreInfo = new()
            {
                SType = StructureType.SemaphoreCreateInfo,
            };

            FenceCreateInfo fenceInfo = new()
            {
                SType = StructureType.FenceCreateInfo,
                Flags = FenceCreateFlags.SignaledBit,
            };

            for (var i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                if (VkApi.CreateSemaphore(device, semaphoreInfo, null, out imageAvailableSemaphores[i]) != Result.Success ||
                    VkApi.CreateSemaphore(device, semaphoreInfo, null, out renderFinishedSemaphores[i]) != Result.Success ||
                    VkApi.CreateFence(device, fenceInfo, null, out inFlightFences[i]) != Result.Success)
                {
                    throw new Exception("failed to create synchronization objects for a frame!");
                }
            }
        }



        private void CreateDescriptorSetLayout()
        {
            DescriptorSetLayoutBinding uboLayoutBinding = new()
            {
                Binding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.VertexBit,
            };

            DescriptorSetLayoutBinding samplerLayoutBinding = new()
            {
                Binding = 1,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.FragmentBit,
            };

            var bindings = new DescriptorSetLayoutBinding[] { uboLayoutBinding, samplerLayoutBinding };

            fixed (DescriptorSetLayoutBinding* bindingsPtr = bindings)
            fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &descriptorSetLayout)
            {
                DescriptorSetLayoutCreateInfo layoutInfo = new()
                {
                    SType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = (uint)bindings.Length,
                    PBindings = bindingsPtr,
                };

                if (VkApi.CreateDescriptorSetLayout(device, layoutInfo, null, descriptorSetLayoutPtr) != Result.Success)
                {
                    throw new Exception("failed to create descriptor set layout!");
                }
            }
        }




        private void CreateTextureImage()
        {
            using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(TEXTURE_PATH);

            ulong imageSize = (ulong)(img.Width * img.Height * img.PixelType.BitsPerPixel / 8);
            mipLevels = (uint)(Math.Floor(Math.Log2(Math.Max(img.Width, img.Height))) + 1);

            Buffer stagingBuffer = default;
            DeviceMemory stagingBufferMemory = default;
            VulkanMemoryHelper.CreateBuffer(this, imageSize, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, ref stagingBuffer, ref stagingBufferMemory);

            void* data;
            VkApi.MapMemory(device, stagingBufferMemory, 0, imageSize, 0, &data);
            img.CopyPixelDataTo(new Span<byte>(data, (int)imageSize));
            VkApi.UnmapMemory(device, stagingBufferMemory);

            VulkanImageHelper.CreateImage(this, (uint)img.Width, (uint)img.Height, mipLevels, SampleCountFlags.Count1Bit, Format.R8G8B8A8Srgb, ImageTiling.Optimal, ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit, MemoryPropertyFlags.DeviceLocalBit, ref textureImage, ref textureImageMemory);

            TransitionImageLayout(textureImage, Format.R8G8B8A8Srgb, ImageLayout.Undefined, ImageLayout.TransferDstOptimal, mipLevels);
            CopyBufferToImage(stagingBuffer, textureImage, (uint)img.Width, (uint)img.Height);
            //Transitioned to VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL while generating mipmaps

            VkApi.DestroyBuffer(device, stagingBuffer, null);
            VkApi.FreeMemory(device, stagingBufferMemory, null);

            GenerateMipMaps(textureImage, Format.R8G8B8A8Srgb, (uint)img.Width, (uint)img.Height, mipLevels);
        }

        private void GenerateMipMaps(Image image, Format imageFormat, uint width, uint height, uint mipLevels)
        {
            VkApi.GetPhysicalDeviceFormatProperties(physicalDevice, imageFormat, out var formatProperties);

            if ((formatProperties.OptimalTilingFeatures & FormatFeatureFlags.SampledImageFilterLinearBit) == 0)
            {
                throw new Exception("texture image format does not support linear blitting!");
            }

            var commandBuffer = BeginSingleTimeCommands();

            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                Image = image,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                SubresourceRange =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseArrayLayer = 0,
                LayerCount = 1,
                LevelCount = 1,
            }
            };

            var mipWidth = width;
            var mipHeight = height;

            for (uint i = 1; i < mipLevels; i++)
            {
                barrier.SubresourceRange.BaseMipLevel = i - 1;
                barrier.OldLayout = ImageLayout.TransferDstOptimal;
                barrier.NewLayout = ImageLayout.TransferSrcOptimal;
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;

                VkApi.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.TransferBit, 0,
                    0, null,
                    0, null,
                    1, barrier);

                ImageBlit blit = new()
                {
                    SrcOffsets =
                {
                    Element0 = new Offset3D(0,0,0),
                    Element1 = new Offset3D((int)mipWidth, (int)mipHeight, 1),
                },
                    SrcSubresource =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    MipLevel = i - 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
                    DstOffsets =
                {
                    Element0 = new Offset3D(0,0,0),
                    Element1 = new Offset3D((int)(mipWidth > 1 ? mipWidth / 2 : 1), (int)(mipHeight > 1 ? mipHeight / 2 : 1),1),
                },
                    DstSubresource =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    MipLevel = i,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },

                };

                VkApi.CmdBlitImage(commandBuffer,
                    image, ImageLayout.TransferSrcOptimal,
                    image, ImageLayout.TransferDstOptimal,
                    1, blit,
                    Filter.Linear);

                barrier.OldLayout = ImageLayout.TransferSrcOptimal;
                barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;

                VkApi.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit, 0,
                    0, null,
                    0, null,
                    1, barrier);

                if (mipWidth > 1) mipWidth /= 2;
                if (mipHeight > 1) mipHeight /= 2;
            }

            barrier.SubresourceRange.BaseMipLevel = mipLevels - 1;
            barrier.OldLayout = ImageLayout.TransferDstOptimal;
            barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;

            VkApi.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit, 0,
                0, null,
                0, null,
                1, barrier);

            EndSingleTimeCommands(commandBuffer);
        }

        

        private void CreateTextureImageView()
        {
            textureImageView = CreateImageView(textureImage, Format.R8G8B8A8Srgb, ImageAspectFlags.ColorBit, mipLevels);
        }

        private void CreateTextureSampler()
        {
            VkApi.GetPhysicalDeviceProperties(physicalDevice, out PhysicalDeviceProperties properties);

            SamplerCreateInfo samplerInfo = new()
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Linear,
                MinFilter = Filter.Linear,
                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                AnisotropyEnable = true,
                MaxAnisotropy = properties.Limits.MaxSamplerAnisotropy,
                BorderColor = BorderColor.IntOpaqueBlack,
                UnnormalizedCoordinates = false,
                CompareEnable = false,
                CompareOp = CompareOp.Always,
                MipmapMode = SamplerMipmapMode.Linear,
                MinLod = 0,
                MaxLod = mipLevels,
                MipLodBias = 0,
            };

            fixed (Sampler* textureSamplerPtr = &textureSampler)
            {
                if (VkApi.CreateSampler(device, samplerInfo, null, textureSamplerPtr) != Result.Success)
                {
                    throw new Exception("failed to create texture sampler!");
                }
            }
        }

        private ImageView CreateImageView(Image image, Format format, ImageAspectFlags aspectFlags, uint mipLevels)
        {
            ImageViewCreateInfo createInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.Type2D,
                Format = format,
                //Components =
                //    {
                //        R = ComponentSwizzle.Identity,
                //        G = ComponentSwizzle.Identity,
                //        B = ComponentSwizzle.Identity,
                //        A = ComponentSwizzle.Identity,
                //    },
                SubresourceRange =
                {
                    AspectMask = aspectFlags,
                    BaseMipLevel = 0,
                    LevelCount = mipLevels,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

            };


            if (VkApi.CreateImageView(device, createInfo, null, out ImageView imageView) != Result.Success)
            {
                throw new Exception("failed to create image views!");
            }

            return imageView;
        }

        private void TransitionImageLayout(Image image, Format format, ImageLayout oldLayout, ImageLayout newLayout, uint mipLevels)
        {
            CommandBuffer commandBuffer = BeginSingleTimeCommands();

            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = image,
                SubresourceRange =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = mipLevels,
                BaseArrayLayer = 0,
                LayerCount = 1,
            }
            };

            PipelineStageFlags sourceStage;
            PipelineStageFlags destinationStage;

            if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;

                sourceStage = PipelineStageFlags.TopOfPipeBit;
                destinationStage = PipelineStageFlags.TransferBit;
            }
            else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;

                sourceStage = PipelineStageFlags.TransferBit;
                destinationStage = PipelineStageFlags.FragmentShaderBit;
            }
            else
            {
                throw new Exception("unsupported layout transition!");
            }

            VkApi.CmdPipelineBarrier(commandBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, barrier);

            EndSingleTimeCommands(commandBuffer);

        }

        private void CopyBufferToImage(Buffer buffer, Image image, uint width, uint height)
        {
            CommandBuffer commandBuffer = BeginSingleTimeCommands();

            BufferImageCopy region = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1,
            },
                ImageOffset = new Offset3D(0, 0, 0),
                ImageExtent = new Extent3D(width, height, 1),

            };

            VkApi.CmdCopyBufferToImage(commandBuffer, buffer, image, ImageLayout.TransferDstOptimal, 1, region);

            EndSingleTimeCommands(commandBuffer);
        }

        private void LoadModel()
        {
            using var assimp = Assimp.GetApi();
            var scene = assimp.ImportFile(MODEL_PATH, (uint)PostProcessPreset.TargetRealTimeMaximumQuality);

            var vertexMap = new Dictionary<Vertex, uint>();
            var vertices = new List<Vertex>();
            var indices = new List<uint>();

            VisitSceneNode(scene->MRootNode);

            assimp.ReleaseImport(scene);

            this.vertices = vertices.ToArray();
            this.indices = indices.ToArray();

            void VisitSceneNode(Node* node)
            {
                for (int m = 0; m < node->MNumMeshes; m++)
                {
                    var mesh = scene->MMeshes[node->MMeshes[m]];

                    for (int f = 0; f < mesh->MNumFaces; f++)
                    {
                        var face = mesh->MFaces[f];

                        for (int i = 0; i < face.MNumIndices; i++)
                        {
                            uint index = face.MIndices[i];

                            var position = mesh->MVertices[index];
                            var texture = mesh->MTextureCoords[0][(int)index];

                            Vertex vertex = new()
                            {
                                pos = new Vector3(position.X, position.Y, position.Z),
                                color = new Vector3(1, 1, 1),
                                //Flip Y for OBJ in Vulkan
                                textCoord = new Vector2(texture.X, 1.0f - texture.Y)
                            };

                            if (vertexMap.TryGetValue(vertex, out var meshIndex))
                            {
                                indices.Add(meshIndex);
                            }
                            else
                            {
                                indices.Add((uint)vertices.Count);
                                vertexMap[vertex] = (uint)vertices.Count;
                                vertices.Add(vertex);
                            }
                        }
                    }
                }

                for (int c = 0; c < node->MNumChildren; c++)
                {
                    VisitSceneNode(node->MChildren[c]);
                }
            }
        }


        private void CreateVertexBuffer()
        {
            ulong bufferSize = (ulong)(Unsafe.SizeOf<Vertex>() * vertices.Length);

            Buffer stagingBuffer = default;
            DeviceMemory stagingBufferMemory = default;
            VulkanMemoryHelper.CreateBuffer(this, bufferSize, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, ref stagingBuffer, ref stagingBufferMemory);

            void* data;
            VkApi.MapMemory(device, stagingBufferMemory, 0, bufferSize, 0, &data);
            vertices.AsSpan().CopyTo(new Span<Vertex>(data, vertices.Length));
            VkApi.UnmapMemory(device, stagingBufferMemory);

            VulkanMemoryHelper.CreateBuffer(this, bufferSize, BufferUsageFlags.TransferDstBit | BufferUsageFlags.VertexBufferBit, MemoryPropertyFlags.DeviceLocalBit, ref vertexBuffer, ref vertexBufferMemory);

            CopyBuffer(stagingBuffer, vertexBuffer, bufferSize);

            VkApi.DestroyBuffer(device, stagingBuffer, null);
            VkApi.FreeMemory(device, stagingBufferMemory, null);
        }

        private void CreateIndexBuffer()
        {
            ulong bufferSize = (ulong)(Unsafe.SizeOf<uint>() * indices.Length);

            Buffer stagingBuffer = default;
            DeviceMemory stagingBufferMemory = default;
            VulkanMemoryHelper.CreateBuffer(this, bufferSize, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, ref stagingBuffer, ref stagingBufferMemory);

            void* data;
            VkApi.MapMemory(device, stagingBufferMemory, 0, bufferSize, 0, &data);
            indices.AsSpan().CopyTo(new Span<uint>(data, indices.Length));
            VkApi.UnmapMemory(device, stagingBufferMemory);

            VulkanMemoryHelper.CreateBuffer(this, bufferSize, BufferUsageFlags.TransferDstBit | BufferUsageFlags.IndexBufferBit, MemoryPropertyFlags.DeviceLocalBit, ref indexBuffer, ref indexBufferMemory);

            CopyBuffer(stagingBuffer, indexBuffer, bufferSize);

            VkApi.DestroyBuffer(device, stagingBuffer, null);
            VkApi.FreeMemory(device, stagingBufferMemory, null);
        }



        private CommandBuffer BeginSingleTimeCommands()
        {
            CommandBufferAllocateInfo allocateInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                Level = CommandBufferLevel.Primary,
                CommandPool = commandPool,
                CommandBufferCount = 1,
            };

            VkApi.AllocateCommandBuffers(device, allocateInfo, out CommandBuffer commandBuffer);

            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
            };

            VkApi.BeginCommandBuffer(commandBuffer, beginInfo);

            return commandBuffer;
        }

        private void EndSingleTimeCommands(CommandBuffer commandBuffer)
        {
            VkApi.EndCommandBuffer(commandBuffer);

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &commandBuffer,
            };

            VkApi.QueueSubmit(graphicsQueue, 1, submitInfo, default);
            VkApi.QueueWaitIdle(graphicsQueue);

            VkApi.FreeCommandBuffers(device, commandPool, 1, commandBuffer);
        }

        private void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, ulong size)
        {
            CommandBuffer commandBuffer = BeginSingleTimeCommands();

            BufferCopy copyRegion = new()
            {
                Size = size,
            };

            VkApi.CmdCopyBuffer(commandBuffer, srcBuffer, dstBuffer, 1, copyRegion);

            EndSingleTimeCommands(commandBuffer);
        }



        public void DrawFrame(double time)
        {
            VkApi.WaitForFences(device, 1, inFlightFences[currentFrame], true, ulong.MaxValue);

            uint imageIndex = 0;
            var result = khrSwapChain.AcquireNextImage(device, _swapChain.swapChain, ulong.MaxValue, imageAvailableSemaphores[currentFrame], default, ref imageIndex);

            if (result == Result.ErrorOutOfDateKhr)
            {
                RecreateSwapChain();
                return;
            }
            else if (result != Result.Success && result != Result.SuboptimalKhr)
            {
                throw new Exception("failed to acquire swap chain image!");
            }

            _swapChain.UpdateUniformBuffer(imageIndex);

            if (imagesInFlight[imageIndex].Handle != default)
            {
                VkApi.WaitForFences(device, 1, imagesInFlight[imageIndex], true, ulong.MaxValue);
            }
            imagesInFlight[imageIndex] = inFlightFences[currentFrame];

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
            };

            var waitSemaphores = stackalloc[] { imageAvailableSemaphores[currentFrame] };
            var waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };

            var buffer = _swapChain.commandBuffers[imageIndex];

            submitInfo = submitInfo with
            {
                WaitSemaphoreCount = 1,
                PWaitSemaphores = waitSemaphores,
                PWaitDstStageMask = waitStages,

                CommandBufferCount = 1,
                PCommandBuffers = &buffer
            };

            var signalSemaphores = stackalloc[] { renderFinishedSemaphores[currentFrame] };
            submitInfo = submitInfo with
            {
                SignalSemaphoreCount = 1,
                PSignalSemaphores = signalSemaphores,
            };

            VkApi.ResetFences(device, 1, inFlightFences[currentFrame]);

            if (VkApi.QueueSubmit(graphicsQueue, 1, submitInfo, inFlightFences[currentFrame]) != Result.Success)
            {
                throw new Exception("failed to submit draw command buffer!");
            }

            var swapChains = stackalloc[] { _swapChain.swapChain };
            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = signalSemaphores,

                SwapchainCount = 1,
                PSwapchains = swapChains,

                PImageIndices = &imageIndex
            };

            result = khrSwapChain.QueuePresent(presentQueue, presentInfo);

            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || frameBufferResized)
            {
                frameBufferResized = false;
                RecreateSwapChain();
            }
            else if (result != Result.Success)
            {
                throw new Exception("failed to present swap chain image!");
            }

            currentFrame = (currentFrame + 1) % MAX_FRAMES_IN_FLIGHT;

        }



        public SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice)
        {
            var details = new SwapChainSupportDetails();

            khrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out details.Capabilities);

            uint formatCount = 0;
            khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, null);

            if (formatCount != 0)
            {
                details.Formats = new SurfaceFormatKHR[formatCount];
                fixed (SurfaceFormatKHR* formatsPtr = details.Formats)
                {
                    khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, formatsPtr);
                }
            }
            else
            {
                details.Formats = Array.Empty<SurfaceFormatKHR>();
            }

            uint presentModeCount = 0;
            khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null);

            if (presentModeCount != 0)
            {
                details.PresentModes = new PresentModeKHR[presentModeCount];
                fixed (PresentModeKHR* formatsPtr = details.PresentModes)
                {
                    khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, formatsPtr);
                }

            }
            else
            {
                details.PresentModes = Array.Empty<PresentModeKHR>();
            }

            return details;
        }


        ///// <summary>
        ///// Implicit conversion from VulkanInstance to Instance
        ///// </summary>
        //public static implicit operator Instance(VulkanInstance vi)
        //{
        //    return vi.Instance;
        //}

    }
}