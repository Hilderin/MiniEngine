using Evergine.Bindings.Vulkan;
using MiniEngine.GLFW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Wrapper for vulkan fonctions
    /// </summary>
    public unsafe class Vk: IDisposable
    {

        private string[] _validationLayers = new[] { "VK_LAYER_KHRONOS_validation" };

        private string[] _extensions = new[]
        {
            "VK_KHR_surface",
            "VK_KHR_win32_surface",
            "VK_KHR_get_physical_device_properties2",
            "VK_EXT_debug_utils",
        };

        private delegate VkBool32 DebugCallbackDelegate(VkDebugUtilsMessageSeverityFlagsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, VkDebugUtilsMessengerCallbackDataEXT pCallbackData, void* pUserData);
        private static DebugCallbackDelegate CallbackDelegate = new DebugCallbackDelegate(DebugCallback);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate VkResult vkCreateDebugUtilsMessengerEXTDelegate(VkInstance instance, VkDebugUtilsMessengerCreateInfoEXT* pCreateInfo, VkAllocationCallbacks* pAllocator, VkDebugUtilsMessengerEXT* pMessenger);
        private static vkCreateDebugUtilsMessengerEXTDelegate vkCreateDebugUtilsMessengerEXT_ptr;
        private static VkResult vkCreateDebugUtilsMessengerEXT(VkInstance instance, VkDebugUtilsMessengerCreateInfoEXT* pCreateInfo, VkAllocationCallbacks* pAllocator, VkDebugUtilsMessengerEXT* pMessenger)
            => vkCreateDebugUtilsMessengerEXT_ptr(instance, pCreateInfo, pAllocator, pMessenger);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void vkDestroyDebugUtilsMessengerEXTDelegate(VkInstance instance, VkDebugUtilsMessengerEXT messenger, VkAllocationCallbacks* pAllocator);
        private static vkDestroyDebugUtilsMessengerEXTDelegate vkDestroyDebugUtilsMessengerEXT_ptr;
        private static void vkDestroyDebugUtilsMessengerEXT(VkInstance instance, VkDebugUtilsMessengerEXT messenger, VkAllocationCallbacks* pAllocator)
            => vkDestroyDebugUtilsMessengerEXT_ptr(instance, messenger, pAllocator);

        private VkDebugUtilsMessengerEXT debugMessenger;

        /// <summary>
        /// Global instance to the vulkan engine
        /// </summary>
        private VkInstance instance;

        /// <summary>
        /// Private constructor
        /// </summary>
        public Vk()
        {
        }


        /// <summary>
        /// Create the vulkan instance
        /// </summary>
        public void CreateInstance(string applicationName, bool addValidationLayers)
        {

#if DEBUG
            if (!this.CheckValidationLayerSupport())
            {
                throw new Exception("Validation layers requested, but not available!");
            }
#endif
            VkApplicationInfo appInfo = new VkApplicationInfo()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_APPLICATION_INFO,
                pApplicationName = applicationName.ToPointer(),
                applicationVersion = Helpers.Version(1, 0, 0),
                pEngineName = "MiniEngine".ToPointer(),
                engineVersion = Helpers.Version(1, 0, 0),
                apiVersion = Helpers.Version(1, 2, 0),
            };

            VkInstanceCreateInfo createInfo = default;
            createInfo.sType = VkStructureType.VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
            createInfo.pApplicationInfo = &appInfo;

            // Extensions
            //this.GetAllInstanceExtensionsAvailables();

            IntPtr* extensionsToBytesArray = stackalloc IntPtr[_extensions.Length];
            for (int i = 0; i < _extensions.Length; i++)
            {
                extensionsToBytesArray[i] = Marshal.StringToHGlobalAnsi(_extensions[i]);
            }
            createInfo.enabledExtensionCount = (uint)_extensions.Length;
            createInfo.ppEnabledExtensionNames = (byte**)extensionsToBytesArray;

            // Validation layers
            if (addValidationLayers)
            {
                IntPtr* layersToBytesArray = stackalloc IntPtr[_validationLayers.Length];
                for (int i = 0; i < _validationLayers.Length; i++)
                {
                    layersToBytesArray[i] = Marshal.StringToHGlobalAnsi(_validationLayers[i]);
                }

                createInfo.enabledLayerCount = (uint)_validationLayers.Length;
                createInfo.ppEnabledLayerNames = (byte**)layersToBytesArray;
            }
            else
            {
                createInfo.enabledLayerCount = 0;
                createInfo.pNext = null;
            }

            fixed (VkInstance* instancePtr = &instance)
            {
                Helpers.CheckErrors(VulkanNative.vkCreateInstance(&createInfo, null, instancePtr));
                VulkanNative.LoadFuncionPointers(instance);
            }        
        
        }



        public void SetupDebugMessenger()
        {    
            fixed (VkDebugUtilsMessengerEXT* debugMessengerPtr = &debugMessenger)
            {
                var funcPtr = VulkanNative.vkGetInstanceProcAddr(instance, "vkCreateDebugUtilsMessengerEXT".ToPointer());
                if (funcPtr != IntPtr.Zero)
                {
                    vkCreateDebugUtilsMessengerEXT_ptr = Marshal.GetDelegateForFunctionPointer<vkCreateDebugUtilsMessengerEXTDelegate>(funcPtr);

                    VkDebugUtilsMessengerCreateInfoEXT createInfo;
                    this.PopulateDebugMessengerCreateInfo(out createInfo);
                    Helpers.CheckErrors(vkCreateDebugUtilsMessengerEXT(instance, &createInfo, null, debugMessengerPtr));
                }
            }
        }


        private void PopulateDebugMessengerCreateInfo(out VkDebugUtilsMessengerCreateInfoEXT createInfo)
        {
            createInfo = default;
            createInfo.sType = VkStructureType.VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
            createInfo.messageSeverity = VkDebugUtilsMessageSeverityFlagsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT | VkDebugUtilsMessageSeverityFlagsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VkDebugUtilsMessageSeverityFlagsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
            createInfo.messageType = VkDebugUtilsMessageTypeFlagsEXT.VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VkDebugUtilsMessageTypeFlagsEXT.VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT | VkDebugUtilsMessageTypeFlagsEXT.VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT;
            createInfo.pfnUserCallback = Marshal.GetFunctionPointerForDelegate(CallbackDelegate);
            createInfo.pUserData = null;
        }

        private void DestroyDebugMessenger()
        {
#if DEBUG
            var funcPtr = VulkanNative.vkGetInstanceProcAddr(instance, "vkDestroyDebugUtilsMessengerEXT".ToPointer());
            if (funcPtr != IntPtr.Zero)
            {
                vkDestroyDebugUtilsMessengerEXT_ptr = Marshal.GetDelegateForFunctionPointer<vkDestroyDebugUtilsMessengerEXTDelegate>(funcPtr);
                vkDestroyDebugUtilsMessengerEXT(instance, debugMessenger, null);
            }
#endif
        }

        public static VkBool32 DebugCallback(VkDebugUtilsMessageSeverityFlagsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, VkDebugUtilsMessengerCallbackDataEXT pCallbackData, void* pUserData)
        {
            Debug.WriteLine($"<<Vulkan Validation Layer>> {Helpers.GetString(pCallbackData.pMessage)}");
            return false;
        }

        /// <summary>
        /// Check if all the validation layers are supported
        /// </summary>
        /// <returns></returns>
        private bool CheckValidationLayerSupport()
        {
            uint layerCount;
            Helpers.CheckErrors(VulkanNative.vkEnumerateInstanceLayerProperties(&layerCount, null));
            VkLayerProperties* availableLayers = stackalloc VkLayerProperties[(int)layerCount];
            Helpers.CheckErrors(VulkanNative.vkEnumerateInstanceLayerProperties(&layerCount, availableLayers));

            for (int i = 0; i < layerCount; i++)
            {
                Debug.WriteLine($"ValidationLayer: {Helpers.GetString(availableLayers[i].layerName)} version: {availableLayers[i].specVersion} description: {Helpers.GetString(availableLayers[i].description)}");
            }

            //Return
            //ValidationLayer: VK_LAYER_NV_optimus version: 4202634 description: NVIDIA Optimus layer
            //ValidationLayer: VK_LAYER_RENDERDOC_Capture version: 4202627 description: Debugging capture layer for RenderDoc
            //ValidationLayer: VK_LAYER_VALVE_steam_overlay version: 4198473 description: Steam Overlay Layer
            //ValidationLayer: VK_LAYER_VALVE_steam_fossilize version: 4198473 description: Steam Pipeline Caching Layer
            //ValidationLayer: VK_LAYER_NV_nomad_release_public_2020_2_0 version: 4202627 description: NVIDIA Nsight Graphics interception layer
            //ValidationLayer: VK_LAYER_NV_GPU_Trace_release_public_2020_2_0 version: 4202627 description: NVIDIA Nsight Graphics GPU Trace interception layer
            //ValidationLayer: VK_LAYER_EOS_Overlay version: 4198473 description: Vulkan overlay layer for Epic Online Services
            //ValidationLayer: VK_LAYER_EOS_Overlay version: 4198473 description: Vulkan overlay layer for Epic Online Services
            //ValidationLayer: VK_LAYER_LUNARG_api_dump version: 4202631 description: LunarG API dump layer
            //ValidationLayer: VK_LAYER_LUNARG_device_simulation version: 4202631 description: LunarG device simulation layer
            //ValidationLayer: VK_LAYER_KHRONOS_validation version: 4202631 description: Khronos Validation Layer
            //ValidationLayer: VK_LAYER_LUNARG_monitor version: 4202631 description: Execution Monitoring Layer
            //ValidationLayer: VK_LAYER_LUNARG_screenshot version: 4202631 description: LunarG image capture layer
            //ValidationLayer: VK_LAYER_LUNARG_vktrace version: 4202631 description: Vktrace tracing library

            for (int i = 0; i < _validationLayers.Length; i++)
            {
                bool layerFound = false;
                string validationLayer = _validationLayers[i];
                for (int j = 0; j < layerCount; j++)
                {
                    if (validationLayer.Equals(Helpers.GetString(availableLayers[j].layerName)))
                    {
                        layerFound = true;
                        break;
                    }
                }

                if (!layerFound)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            if(instance.Handle != IntPtr.Zero)
                VulkanNative.vkDestroyInstance(instance, null);
        }

    }
}
