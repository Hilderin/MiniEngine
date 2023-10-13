using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using System.Runtime.InteropServices;

namespace MiniEngine.Rendering.Vulkan
{
    internal unsafe class VulkanDebugMessenger : IDisposable
    {
        private VulkanInstance _vi;

        private ExtDebugUtils _debugUtils = null;
        private DebugUtilsMessengerEXT _debugMessenger;

        public VulkanDebugMessenger(VulkanInstance vi)
        {
            _vi = vi;
        }

        public void Init()
        {
            //TryGetInstanceExtension equivilant to method CreateDebugUtilsMessengerEXT from original tutorial.
            if (!_vi.Api.TryGetInstanceExtension(_vi.Instance, out _debugUtils))
                return;

            DebugUtilsMessengerCreateInfoEXT createInfo = CreateDebugMessengerCreateInfo();

            if (_debugUtils.CreateDebugUtilsMessenger(_vi.Instance, in createInfo, null, out _debugMessenger) != Result.Success)
            {
                throw new Exception("failed to set up debug messenger!");
            }

        }

        public static DebugUtilsMessengerCreateInfoEXT CreateDebugMessengerCreateInfo()
        {
            DebugUtilsMessengerCreateInfoEXT createInfo = new DebugUtilsMessengerCreateInfoEXT();

            createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
            createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
            createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;

            return createInfo;
        }

        private static uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
        {
            if (messageSeverity == DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt)
                //Validation error...
                throw new Exception("Vulkan validation error: " + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));

            System.Diagnostics.Debug.WriteLine($"[{messageSeverity}] validation layer:" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));


            return Vk.False;
        }

        public void Dispose()
        {
            _debugUtils?.DestroyDebugUtilsMessenger(_vi.Instance, _debugMessenger, null);
        }
    }
}
