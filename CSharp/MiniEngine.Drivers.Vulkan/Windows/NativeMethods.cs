using System;
using System.Runtime.InteropServices;


namespace MiniEngine.Drivers.Vulkan.Interop
{
	internal static partial class NativeMethods
	{
		[DllImport(VulkanLibrary, CallingConvention = CallingConvention.Winapi)]
		internal static unsafe extern Result vkCreateWin32SurfaceKHR(IntPtr instance, MiniEngine.Drivers.Vulkan.Interop.Windows.Win32SurfaceCreateInfoKhr* pCreateInfo, Vulkan.Interop.AllocationCallbacks* pAllocator, UInt64* pSurface);

		[DllImport(VulkanLibrary, CallingConvention = CallingConvention.Winapi)]
		internal static unsafe extern Bool32 vkGetPhysicalDeviceWin32PresentationSupportKHR(IntPtr physicalDevice, UInt32 queueFamilyIndex);

		[DllImport(VulkanLibrary, CallingConvention = CallingConvention.Winapi)]
		internal static unsafe extern Result vkGetMemoryWin32HandleNV(IntPtr device, UInt64 memory, ExternalMemoryHandleTypeFlagsNv handleType, IntPtr* pHandle);

	}
}
