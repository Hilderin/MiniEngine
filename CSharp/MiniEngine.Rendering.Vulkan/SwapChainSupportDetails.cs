using Silk.NET.Vulkan;

namespace MiniEngine.Rendering.Vulkan
{
    struct SwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR Capabilities;
        public SurfaceFormatKHR[] Formats;
        public PresentModeKHR[] PresentModes;
    }
}
