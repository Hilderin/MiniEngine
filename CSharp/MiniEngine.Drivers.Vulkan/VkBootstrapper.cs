using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Drivers.Vulkan
{
    public static class VkBootstrapper
    {

        public static VkInstance CreateInstance(string applicationName, DebugReportCallback debugCallback = null)
        {
            VkInstance vk;

            var layerProperties = VkCommands.EnumerateInstanceLayerProperties();

            string[] layersToEnable = new string[0];
            if (debugCallback != null)
            {
                if (!layerProperties.Any(l => l.LayerName == "VK_LAYER_KHRONOS_validation"))
                    throw new NotSupportedException("Layer 'VK_LAYER_KHRONOS_validation' not supported, impossible to enable debug mode.");

                layersToEnable = new[] { "VK_LAYER_KHRONOS_validation" };
            };

            vk = new VkInstance(new InstanceCreateInfo
            {
                EnabledExtensionNames = new string[] { "VK_KHR_surface", "VK_KHR_win32_surface", "VK_EXT_debug_report" },
                EnabledLayerNames = layersToEnable,
                ApplicationInfo = new ApplicationInfo
                {
                    ApplicationName = applicationName,
                    EngineName = "MiniEngine",
                    EngineVersion = MiniEngine.Drivers.Vulkan.Version.Make(1, 0, 0),
                    ApiVersion = MiniEngine.Drivers.Vulkan.Version.Make(1, 2, 0)
                }
            });

            if (debugCallback != null)
                vk.EnableDebug(debugCallback);

            return vk;

        }

        /// <summary>
        /// Get the right Physicl device
        /// </summary>
        public static PhysicalDevice PickPhysicalDevice(VkInstance vk)
        {
            //TODO: Check the physical device suitable for our project
            return vk.EnumeratePhysicalDevices()[0];
        }

        /// <summary>
        /// Create the device with it's queues
        /// </summary>
        public static Device CreateDevice(PhysicalDevice physicalDevice, SurfaceKhr surface)
        {
            var queueFamilyProperties = physicalDevice.GetQueueFamilyProperties();

            uint queueFamilyUsedIndex;
            for (queueFamilyUsedIndex = 0; queueFamilyUsedIndex < queueFamilyProperties.Length; ++queueFamilyUsedIndex)
            {
                if (!physicalDevice.GetSurfaceSupportKHR(queueFamilyUsedIndex, surface))
                    //This queue does not support SurfaceKHR...
                    continue;

                if (queueFamilyProperties[queueFamilyUsedIndex].QueueFlags.HasFlag(QueueFlags.Graphics))
                    //Found it! Should be good
                    break;
            }

            var queueInfo = new DeviceQueueCreateInfo { QueuePriorities = new float[] { 1.0f }, QueueFamilyIndex = queueFamilyUsedIndex };

            var deviceInfo = new DeviceCreateInfo
            {
                EnabledExtensionNames = new string[] { "VK_KHR_swapchain" },
                QueueCreateInfos = new DeviceQueueCreateInfo[] { queueInfo }
            };

            return physicalDevice.CreateDevice(deviceInfo);
        }

        



    }
}
