using MiniEngine.GLFW;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Wrapper for vulkan instance and functions
    /// </summary>
    public unsafe class VkInstance: IDisposable
    {
        #region Private members

        private Vk vk = null;
        private Instance instance;
        private ExtDebugUtils debugUtils = null;
        private DebugUtilsMessengerEXT debugMessenger;
        private PhysicalDevice physicalDevice;
        private Device device;
        private Queue graphicsQueue;

        private KhrSurface khrSurface = null;
        private SurfaceKHR surface;
        private Queue presentQueue;

        
        private readonly string[] validationLayers = new[]
        {
            "VK_LAYER_KHRONOS_validation"
        };

        #endregion

        #region Public properties

        /// <summary>
        /// Indicate if we want to enable the validation layers
        /// </summary>
        public bool EnableValidationLayers = true;

        /// <summary>
        /// Application name passed to Vulkan instance
        /// </summary>
        public string ApplicationName = "MiniEngine";

        #endregion

        #region Public methods


        /// <summary>
        /// Create the vulkan instance
        /// </summary>
        public void CreateInstance()
        {
            vk = Vk.GetApi();

            if (EnableValidationLayers && !CheckValidationLayerSupport())
            {
                throw new Exception("validation layers requested, but not available!");
            }

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi(ApplicationName),
                ApplicationVersion = new Version32(1, 0, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("MiniEngine"),
                EngineVersion = new Version32(1, 0, 0),
                ApiVersion = Vk.Version11
            };

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo
            };

            var extensions = GetRequiredExtensions();
            createInfo.EnabledExtensionCount = (uint)extensions.Length;
            createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions); ;

            if (EnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);

                //Separate debug function calls juste for CreateInstance....
                DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
                PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
                createInfo.PNext = &debugCreateInfo;
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
                createInfo.PNext = null;
            }

            if (vk.CreateInstance(createInfo, null, out instance) != Result.Success)
            {
                throw new Exception("failed to create instance!");
            }

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
            SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

            if (EnableValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }
        }

        /// <summary>
        /// Activate the debug messages
        /// </summary>
        public void SetupDebugMessenger()
        {
            if (!EnableValidationLayers) return;

            //TryGetInstanceExtension equivilant to method CreateDebugUtilsMessengerEXT from original tutorial.
            if (!vk!.TryGetInstanceExtension(instance, out debugUtils)) return;

            DebugUtilsMessengerCreateInfoEXT createInfo = new();
            PopulateDebugMessengerCreateInfo(ref createInfo);

            if (debugUtils!.CreateDebugUtilsMessenger(instance, in createInfo, null, out debugMessenger) != Result.Success)
            {
                throw new Exception("failed to set up debug messenger!");
            }
        }

        /// <summary>
        /// Create the surface
        /// </summary>
        public void CreateSurface(Window window)
        {
            if (!vk.TryGetInstanceExtension<KhrSurface>(instance, out khrSurface))
            {
                throw new NotSupportedException("KHR_surface extension not found.");
            }
            
            fixed (SurfaceKHR* ptrsurface = &surface)
            {
                window.CreateSurface(instance.Handle, (IntPtr)ptrsurface);
            }


        }


        /// <summary>
        /// Search the physical device to use
        /// </summary>
        public void PickPhysicalDevice()
        {
            uint devicedCount = 0;
            vk!.EnumeratePhysicalDevices(instance, ref devicedCount, null);

            if (devicedCount == 0)
            {
                throw new Exception("failed to find GPUs with Vulkan support!");
            }

            var devices = new PhysicalDevice[devicedCount];
            fixed (PhysicalDevice* devicesPtr = devices)
            {
                vk!.EnumeratePhysicalDevices(instance, ref devicedCount, devicesPtr);
            }

            foreach (var device in devices)
            {
                if (IsDeviceSuitable(device))
                {
                    physicalDevice = device;
                    break;
                }
            }

            if (physicalDevice.Handle == 0)
            {
                throw new Exception("failed to find a suitable GPU!");
            }
        }

        /// <summary>
        /// Create Logical Device
        /// </summary>
        public void CreateLogicalDevice()
        {
            var indices = FindQueueFamilies(physicalDevice);

            DeviceQueueCreateInfo queueCreateInfo = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = indices.GraphicsFamily!.Value,
                QueueCount = 1
            };

            float queuePriority = 1.0f;
            queueCreateInfo.PQueuePriorities = &queuePriority;

            PhysicalDeviceFeatures deviceFeatures = new();

            DeviceCreateInfo createInfo = new()
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueCreateInfo,

                PEnabledFeatures = &deviceFeatures,

                EnabledExtensionCount = 0
            };

            if (EnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
            }

            if (vk!.CreateDevice(physicalDevice, in createInfo, null, out device) != Result.Success)
            {
                throw new Exception("failed to create logical device!");
            }

            vk!.GetDeviceQueue(device, indices.GraphicsFamily!.Value, 0, out graphicsQueue);

            if (EnableValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            if (vk != null)
            {
                vk.DestroyDevice(device, null);

                

                if (EnableValidationLayers)
                {
                    //DestroyDebugUtilsMessenger equivilant to method DestroyDebugUtilsMessengerEXT from original tutorial.
                    debugUtils?.DestroyDebugUtilsMessenger(instance, debugMessenger, null);
                }

                if(khrSurface != null)
                    khrSurface.DestroySurface(instance, surface, null);

                vk.DestroyInstance(instance, null);
                vk.Dispose();

                vk = null;
            }

        }





        #endregion

        #region Private methods

        /// <summary>
        /// Returns the required extensions for glfw and validation layers
        /// </summary>
        private string[] GetRequiredExtensions()
        {
            string[] glfwExtensions = Glfw.GetRequiredInstanceExtensions();

            if (EnableValidationLayers)
            {
                return glfwExtensions.Append(ExtDebugUtils.ExtensionName).ToArray();
            }

            return glfwExtensions;
        }

        /// <summary>
        /// Validate if the validation layers are supported (Vulkan SDK installed)
        /// </summary>
        private bool CheckValidationLayerSupport()
        {
            uint layerCount = 0;
            vk!.EnumerateInstanceLayerProperties(ref layerCount, null);
            var availableLayers = new LayerProperties[layerCount];
            fixed (LayerProperties* availableLayersPtr = availableLayers)
            {
                vk!.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
            }

            var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();

            return validationLayers.All(availableLayerNames.Contains);
        }

        /// <summary>
        /// Setup the struct DebugUtilsMessengerCreateInfoEXT
        /// </summary>
        private void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo)
        {
            createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
            createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
            createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;
        }

        /// <summary>
        /// Callback when we receive vulkan callback
        /// </summary>
        private uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
        {
            string data = Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage);

            //Console.WriteLine($"validation layer:" + data);
            Debug.WriteLine($"validation layer: {data}");

            return Vk.False;
        }

        /// <summary>
        /// Check if a device is suitable
        /// </summary>
        private bool IsDeviceSuitable(PhysicalDevice device)
        {
            var indices = FindQueueFamilies(device);

            return indices.IsComplete();
        }

        /// <summary>
        /// Search a family queue
        /// </summary>
        private QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
        {
            var indices = new QueueFamilyIndices();

            uint queueFamilityCount = 0;
            vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

            var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
            fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
            {
                vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
            }


            uint i = 0;
            foreach (var queueFamily in queueFamilies)
            {
                if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    indices.GraphicsFamily = i;
                }

                khrSurface!.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupport);

                if (presentSupport)
                {
                    indices.PresentFamily = i;
                }

                if (indices.IsComplete())
                {
                    break;
                }

                i++;
            }

            return indices;
        }

        #endregion

    }
}
