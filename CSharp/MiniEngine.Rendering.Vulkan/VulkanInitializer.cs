using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    internal unsafe class VulkanInitializer: IDisposable
    {

        #region Private members

        private VulkanInstance _vi;
        private VulkanDebugMessenger _debugMessenger = null;


        #endregion


        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public VulkanInitializer(VulkanInstance vi)
        {
            _vi = vi;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Execute the initialization
        /// </summary>
        public void Init()
        {
            CreateInstance();
            SetupDebugMessenger();
            CreateSurface();
            PickPhysicalDevice();
            CreateLogicalDevice();
            PrepareKhrSwapChain();
            CreateCommandPool();
        }

        /// <summary>
        /// Create the instance
        /// </summary>
        private void CreateInstance()
        {
            _vi.VkApi = Vk.GetApi();

            if (_vi.EnableValidationLayers && !CheckValidationLayerSupport())
            {
                throw new Exception("validation layers requested, but not available!");
            }

            var version = typeof(Context).Assembly.GetName().Version;

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi(_vi.ApplicationName),
                ApplicationVersion = new Version32(1, 0, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("MiniEngine"),
                EngineVersion = new Version32((uint)version.Major, (uint)version.Minor, (uint)version.Build),
                ApiVersion = Vk.Version12
            };

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo
            };

            var extensions = GetRequiredExtensions();
            createInfo.EnabledExtensionCount = (uint)extensions.Length;
            createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions); ;

            if (_vi.EnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)_vi.validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(_vi.validationLayers);

                DebugUtilsMessengerCreateInfoEXT debugCreateInfo = VulkanDebugMessenger.CreateDebugMessengerCreateInfo();
                createInfo.PNext = &debugCreateInfo;
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
                createInfo.PNext = null;
            }

            if (_vi.VkApi.CreateInstance(createInfo, null, out _vi.Instance) != Result.Success)
            {
                throw new Exception("failed to create instance!");
            }

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
            SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

            if (_vi.EnableValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }
        }

        /// <summary>
        /// Create surface
        /// </summary>
        private void CreateSurface()
        {
            if (!_vi.VkApi.TryGetInstanceExtension<KhrSurface>(_vi.Instance, out _vi.khrSurface))
            {
                throw new NotSupportedException("KHR_surface extension not found.");
            }

            SurfaceKHR surface;
            _vi.Window.CreateSurface(_vi.Instance.Handle, (IntPtr)(&surface));
            _vi.surface = surface;
        }

        /// <summary>
        /// Choose a physical device
        /// </summary>
        private void PickPhysicalDevice()
        {
            uint devicedCount = 0;
            _vi.VkApi.EnumeratePhysicalDevices(_vi.Instance, ref devicedCount, null);

            if (devicedCount == 0)
            {
                throw new Exception("failed to find GPUs with Vulkan support!");
            }

            var devices = new PhysicalDevice[devicedCount];
            fixed (PhysicalDevice* devicesPtr = devices)
            {
                _vi.VkApi.EnumeratePhysicalDevices(_vi.Instance, ref devicedCount, devicesPtr);
            }

            foreach (var device in devices)
            {
                if (IsDeviceSuitable(device))
                {
                    _vi.physicalDevice = device;
                    _vi.msaaSamples = GetMaxUsableSampleCount();
                    break;
                }
            }

            if (_vi.physicalDevice.Handle == 0)
            {
                throw new Exception("failed to find a suitable GPU!");
            }
        }

        /// <summary>
        /// Create a logical device
        /// </summary>
        private void CreateLogicalDevice()
        {
            var indices = QueueFamiliesHelper.FindQueueFamilies(_vi, _vi.physicalDevice);

            var uniqueQueueFamilies = new[] { indices.GraphicsFamily.Value, indices.PresentFamily.Value };
            uniqueQueueFamilies = uniqueQueueFamilies.Distinct().ToArray();

            using var mem = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
            var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            float queuePriority = 1.0f;
            for (int i = 0; i < uniqueQueueFamilies.Length; i++)
            {
                queueCreateInfos[i] = new()
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = uniqueQueueFamilies[i],
                    QueueCount = 1,
                    PQueuePriorities = &queuePriority
                };
            }

            PhysicalDeviceFeatures deviceFeatures = new()
            {
                SamplerAnisotropy = true,
            };


            DeviceCreateInfo createInfo = new()
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = (uint)uniqueQueueFamilies.Length,
                PQueueCreateInfos = queueCreateInfos,

                PEnabledFeatures = &deviceFeatures,

                EnabledExtensionCount = (uint)_vi.deviceExtensions.Length,
                PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(_vi.deviceExtensions)
            };

            if (_vi.EnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)_vi.validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(_vi.validationLayers);
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
            }

            if (_vi.VkApi.CreateDevice(_vi.physicalDevice, in createInfo, null, out _vi.device) != Result.Success)
            {
                throw new Exception("failed to create logical device!");
            }


            _vi.VkApi.GetDeviceQueue(_vi.device, indices.GraphicsFamily.Value, 0, out _vi.graphicsQueue);
            _vi.VkApi.GetDeviceQueue(_vi.device, indices.PresentFamily.Value, 0, out _vi.presentQueue);


            if (_vi.EnableValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }

            SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

        }

        /// <summary>
        /// Create the KhrSwapChain
        /// </summary>
        private void PrepareKhrSwapChain()
        {
            if (_vi.khrSwapChain is null)
            {
                if (!_vi.VkApi.TryGetDeviceExtension(_vi.Instance, _vi.device, out _vi.khrSwapChain))
                {
                    throw new NotSupportedException("VK_KHR_swapchain extension not found.");
                }
            }
        }


        /// <summary>
        /// Setup de debug messenger
        /// </summary>
        public void SetupDebugMessenger()
        {
            if (!_vi.EnableValidationLayers)
                return;

            _debugMessenger = new VulkanDebugMessenger(_vi);
            _debugMessenger.Init();


        }


        /// <summary>
        /// Create the command pool
        /// </summary>
        private void CreateCommandPool()
        {
            var queueFamiliyIndicies = QueueFamiliesHelper.FindQueueFamilies(_vi, _vi.physicalDevice);

            CommandPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = queueFamiliyIndicies.GraphicsFamily.Value,
            };

            if (_vi.VkApi.CreateCommandPool(_vi.device, poolInfo, null, out _vi.commandPool) != Result.Success)
            {
                throw new Exception("failed to create command pool!");
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _vi.VkApi.DestroyCommandPool(_vi.device, _vi.commandPool, null);

            _vi.VkApi.DestroyDevice(_vi.device, null);

            _vi.khrSurface.DestroySurface(_vi.Instance, _vi.surface, null);

            _debugMessenger?.Dispose();

            _vi.VkApi.DestroyInstance(_vi.Instance, null);
            _vi.VkApi.Dispose();
        }

        #endregion

        #region Private methods



        private bool IsDeviceSuitable(PhysicalDevice device)
        {
            var indices = QueueFamiliesHelper.FindQueueFamilies(_vi, device);

            bool extensionsSupported = CheckDeviceExtensionsSupport(device);

            bool swapChainAdequate = false;
            if (extensionsSupported)
            {
                var swapChainSupport = _vi.QuerySwapChainSupport(device);
                swapChainAdequate = swapChainSupport.Formats.Any() && swapChainSupport.PresentModes.Any();
            }

            _vi.VkApi.GetPhysicalDeviceFeatures(device, out PhysicalDeviceFeatures supportedFeatures);

            return indices.IsComplete() && extensionsSupported && swapChainAdequate && supportedFeatures.SamplerAnisotropy;
        }


        private bool CheckDeviceExtensionsSupport(PhysicalDevice device)
        {
            uint extentionsCount = 0;
            _vi.VkApi.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extentionsCount, null);

            var availableExtensions = new ExtensionProperties[extentionsCount];
            fixed (ExtensionProperties* availableExtensionsPtr = availableExtensions)
            {
                _vi.VkApi.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extentionsCount, availableExtensionsPtr);
            }

            var availableExtensionNames = availableExtensions.Select(extension => Marshal.PtrToStringAnsi((IntPtr)extension.ExtensionName)).ToHashSet();

            return _vi.deviceExtensions.All(availableExtensionNames.Contains);

        }

        

        private bool CheckValidationLayerSupport()
        {
            uint layerCount = 0;
            _vi.VkApi.EnumerateInstanceLayerProperties(ref layerCount, null);
            var availableLayers = new LayerProperties[layerCount];
            fixed (LayerProperties* availableLayersPtr = availableLayers)
            {
                _vi.VkApi.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
            }

            var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();

            return _vi.validationLayers.All(availableLayerNames.Contains);
        }

        private string[] GetRequiredExtensions()
        {
            string[] glfwExtensions = MiniEngine.GLFW.Glfw.GetRequiredInstanceExtensions();

            if (_vi.EnableValidationLayers)
                return glfwExtensions.Append(ExtDebugUtils.ExtensionName).ToArray();
            return glfwExtensions;
        }

        private SampleCountFlags GetMaxUsableSampleCount()
        {
            _vi.VkApi.GetPhysicalDeviceProperties(_vi.physicalDevice, out var physicalDeviceProperties);

            var counts = physicalDeviceProperties.Limits.FramebufferColorSampleCounts & physicalDeviceProperties.Limits.FramebufferDepthSampleCounts;

            return counts switch
            {
                var c when (c & SampleCountFlags.Count64Bit) != 0 => SampleCountFlags.Count64Bit,
                var c when (c & SampleCountFlags.Count32Bit) != 0 => SampleCountFlags.Count32Bit,
                var c when (c & SampleCountFlags.Count16Bit) != 0 => SampleCountFlags.Count16Bit,
                var c when (c & SampleCountFlags.Count8Bit) != 0 => SampleCountFlags.Count8Bit,
                var c when (c & SampleCountFlags.Count4Bit) != 0 => SampleCountFlags.Count4Bit,
                var c when (c & SampleCountFlags.Count2Bit) != 0 => SampleCountFlags.Count2Bit,
                _ => SampleCountFlags.Count1Bit
            };
        }

        #endregion


    }
}
