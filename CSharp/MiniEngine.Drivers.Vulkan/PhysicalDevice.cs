using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Vulkan Physical device
    /// </summary>
    public partial class PhysicalDevice : IMarshalling, IDisposable
    {
        /// <summary>
        /// Device
        /// </summary>
        public List<Device> Devices = new List<Device>();


        internal PhysicalDevice() { }

        internal IntPtr m;

        IntPtr IMarshalling.Handle
        {
            get
            {
                return m;
            }
        }

        public PhysicalDeviceProperties GetProperties()
        {
            PhysicalDeviceProperties pProperties;
            unsafe
            {
                pProperties = new PhysicalDeviceProperties();
                Interop.NativeMethods.vkGetPhysicalDeviceProperties(this.m, pProperties != null ? pProperties.m : (Interop.PhysicalDeviceProperties*)default(IntPtr));

                return pProperties;
            }
        }

        public QueueFamilyProperties[] GetQueueFamilyProperties()
        {
            unsafe
            {
                UInt32 pQueueFamilyPropertyCount;
                Interop.NativeMethods.vkGetPhysicalDeviceQueueFamilyProperties(this.m, &pQueueFamilyPropertyCount, null);
                if (pQueueFamilyPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(QueueFamilyProperties));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpQueueFamilyProperties = new NativeReference((int)(size * pQueueFamilyPropertyCount));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var ptrpQueueFamilyProperties = refpQueueFamilyProperties.Handle;
                Interop.NativeMethods.vkGetPhysicalDeviceQueueFamilyProperties(this.m, &pQueueFamilyPropertyCount, (QueueFamilyProperties*)ptrpQueueFamilyProperties);

                if (pQueueFamilyPropertyCount <= 0)
                    return null;
                var arr = new QueueFamilyProperties[pQueueFamilyPropertyCount];
                for (int i = 0; i < pQueueFamilyPropertyCount; i++)
                {
                    arr[i] = (((QueueFamilyProperties*)ptrpQueueFamilyProperties)[i]);
                }

                return arr;

            }
        }

        public PhysicalDeviceMemoryProperties GetMemoryProperties()
        {
            PhysicalDeviceMemoryProperties pMemoryProperties;
            unsafe
            {
                pMemoryProperties = new PhysicalDeviceMemoryProperties();
                Interop.NativeMethods.vkGetPhysicalDeviceMemoryProperties(this.m, pMemoryProperties != null ? pMemoryProperties.m : (Interop.PhysicalDeviceMemoryProperties*)default(IntPtr));

                return pMemoryProperties;
            }
        }

        public PhysicalDeviceFeatures GetFeatures()
        {
            PhysicalDeviceFeatures pFeatures;
            unsafe
            {
                pFeatures = new PhysicalDeviceFeatures();
                Interop.NativeMethods.vkGetPhysicalDeviceFeatures(this.m, &pFeatures);

                return pFeatures;
            }
        }

        public FormatProperties GetFormatProperties(Format format)
        {
            FormatProperties pFormatProperties;
            unsafe
            {
                pFormatProperties = new FormatProperties();
                Interop.NativeMethods.vkGetPhysicalDeviceFormatProperties(this.m, format, &pFormatProperties);

                return pFormatProperties;
            }
        }

        public ImageFormatProperties GetImageFormatProperties(Format format, ImageType type, ImageTiling tiling, ImageUsageFlags usage, ImageCreateFlags flags = (ImageCreateFlags)0)
        {
            Result result;
            ImageFormatProperties pImageFormatProperties;
            unsafe
            {
                pImageFormatProperties = new ImageFormatProperties();
                result = Interop.NativeMethods.vkGetPhysicalDeviceImageFormatProperties(this.m, format, type, tiling, usage, flags, &pImageFormatProperties);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pImageFormatProperties;
            }
        }



        /// <summary>
        /// Create the device with it's queues
        /// </summary>
        public Device CreateDevice(SurfaceKhr surface)
        {
            var queueFamilyProperties = GetQueueFamilyProperties();

            uint queueFamilyUsedIndex;
            for (queueFamilyUsedIndex = 0; queueFamilyUsedIndex < queueFamilyProperties.Length; ++queueFamilyUsedIndex)
            {
                if (!GetSurfaceSupportKHR(queueFamilyUsedIndex, surface))
                    //This queue does not support SurfaceKHR...
                    continue;

                if (queueFamilyProperties[queueFamilyUsedIndex].QueueFlags.HasFlag(QueueFlags.Graphics))
                    //Found it! Should be good
                    break;
            }

            var queueInfo = new DeviceQueueCreateInfo
            {
                QueuePriorities = new float[] { 1.0f },
                QueueFamilyIndex = queueFamilyUsedIndex,
            };

            using (var deviceInfo = new DeviceCreateInfo
            {
                EnabledExtensionNames = new string[] { "VK_KHR_swapchain" },
                QueueCreateInfos = new DeviceQueueCreateInfo[] { queueInfo },
                EnabledFeatures = new()
                {
                    SamplerAnisotropy = true            //Enable Anisotrophy
                }
            })
            {
                return CreateDevice(deviceInfo, surface);
            }

        }

        public Device CreateDevice(DeviceCreateInfo pCreateInfo, SurfaceKhr surface, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Device pDevice;
            unsafe
            {
                pDevice = new Device();

                fixed (IntPtr* ptrpDevice = &pDevice.m)
                {
                    result = Interop.NativeMethods.vkCreateDevice(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.DeviceCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpDevice);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                pDevice.PhysicalDevice = this;
                pDevice.Surface = surface;

                pDevice.UpdateSurfaceCapabilities();

                pDevice.MemoryManager = new MemoryManager(pDevice);

                Devices.Add(pDevice);

                return pDevice;
            }
        }

        public LayerProperties[] EnumerateDeviceLayerProperties()
        {
            Result result;
            unsafe
            {
                UInt32 pPropertyCount;
                result = Interop.NativeMethods.vkEnumerateDeviceLayerProperties(this.m, &pPropertyCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.LayerProperties));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpProperties = new NativeReference((int)(size * pPropertyCount));
                var ptrpProperties = refpProperties.Handle;
                result = Interop.NativeMethods.vkEnumerateDeviceLayerProperties(this.m, &pPropertyCount, (Interop.LayerProperties*)ptrpProperties);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pPropertyCount <= 0)
                    return null;
                var arr = new LayerProperties[pPropertyCount];
                for (int i = 0; i < pPropertyCount; i++)
                {
                    arr[i] = new LayerProperties(new NativePointer(refpProperties, (IntPtr)(&((Interop.LayerProperties*)ptrpProperties)[i])));
                }

                return arr;

            }
        }

        public ExtensionProperties[] EnumerateDeviceExtensionProperties(string pLayerName = null)
        {
            Result result;
            unsafe
            {
                UInt32 pPropertyCount;
                result = Interop.NativeMethods.vkEnumerateDeviceExtensionProperties(this.m, pLayerName, &pPropertyCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.ExtensionProperties));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpProperties = new NativeReference((int)(size * pPropertyCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpProperties = refpProperties.Handle;
                result = Interop.NativeMethods.vkEnumerateDeviceExtensionProperties(this.m, pLayerName, &pPropertyCount, (Interop.ExtensionProperties*)ptrpProperties);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pPropertyCount <= 0)
                    return null;
                var arr = new ExtensionProperties[pPropertyCount];
                for (int i = 0; i < pPropertyCount; i++)
                {
                    arr[i] = new ExtensionProperties(new NativePointer(refpProperties, (IntPtr)(&((Interop.ExtensionProperties*)ptrpProperties)[i])));
                }

                return arr;

            }
        }

        public SparseImageFormatProperties[] GetSparseImageFormatProperties(Format format, ImageType type, SampleCountFlags samples, ImageUsageFlags usage, ImageTiling tiling)
        {
            unsafe
            {
                UInt32 pPropertyCount;
                Interop.NativeMethods.vkGetPhysicalDeviceSparseImageFormatProperties(this.m, format, type, samples, usage, tiling, &pPropertyCount, null);
                if (pPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(SparseImageFormatProperties));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpProperties = new NativeReference((int)(size * pPropertyCount));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var ptrpProperties = refpProperties.Handle;
                Interop.NativeMethods.vkGetPhysicalDeviceSparseImageFormatProperties(this.m, format, type, samples, usage, tiling, &pPropertyCount, (SparseImageFormatProperties*)ptrpProperties);

                if (pPropertyCount <= 0)
                    return null;
                var arr = new SparseImageFormatProperties[pPropertyCount];
                for (int i = 0; i < pPropertyCount; i++)
                {
                    arr[i] = (((SparseImageFormatProperties*)ptrpProperties)[i]);
                }

                return arr;

            }
        }

        public DisplayPropertiesKhr[] GetDisplayPropertiesKHR()
        {
            Result result;
            unsafe
            {
                UInt32 pPropertyCount;
                result = Interop.NativeMethods.vkGetPhysicalDeviceDisplayPropertiesKHR(this.m, &pPropertyCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.DisplayPropertiesKhr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpProperties = new NativeReference((int)(size * pPropertyCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpProperties = refpProperties.Handle;
                result = Interop.NativeMethods.vkGetPhysicalDeviceDisplayPropertiesKHR(this.m, &pPropertyCount, (Interop.DisplayPropertiesKhr*)ptrpProperties);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pPropertyCount <= 0)
                    return null;
                var arr = new DisplayPropertiesKhr[pPropertyCount];
                for (int i = 0; i < pPropertyCount; i++)
                {
                    arr[i] = new DisplayPropertiesKhr(new NativePointer(refpProperties, (IntPtr)(&((Interop.DisplayPropertiesKhr*)ptrpProperties)[i])));
                }

                return arr;

            }
        }

        public DisplayPlanePropertiesKhr[] GetDisplayPlanePropertiesKHR()
        {
            Result result;
            unsafe
            {
                UInt32 pPropertyCount;
                result = Interop.NativeMethods.vkGetPhysicalDeviceDisplayPlanePropertiesKHR(this.m, &pPropertyCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.DisplayPlanePropertiesKhr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpProperties = new NativeReference((int)(size * pPropertyCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpProperties = refpProperties.Handle;
                result = Interop.NativeMethods.vkGetPhysicalDeviceDisplayPlanePropertiesKHR(this.m, &pPropertyCount, (Interop.DisplayPlanePropertiesKhr*)ptrpProperties);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pPropertyCount <= 0)
                    return null;
                var arr = new DisplayPlanePropertiesKhr[pPropertyCount];
                for (int i = 0; i < pPropertyCount; i++)
                {
                    arr[i] = new DisplayPlanePropertiesKhr(new NativePointer(refpProperties, (IntPtr)(&((Interop.DisplayPlanePropertiesKhr*)ptrpProperties)[i])));
                }

                return arr;

            }
        }

        public DisplayKhr[] GetDisplayPlaneSupportedDisplaysKHR(UInt32 planeIndex)
        {
            Result result;
            unsafe
            {
                UInt32 pDisplayCount;
                result = Interop.NativeMethods.vkGetDisplayPlaneSupportedDisplaysKHR(this.m, planeIndex, &pDisplayCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pDisplayCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(UInt64));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpDisplays = new NativeReference((int)(size * pDisplayCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpDisplays = refpDisplays.Handle;
                result = Interop.NativeMethods.vkGetDisplayPlaneSupportedDisplaysKHR(this.m, planeIndex, &pDisplayCount, (UInt64*)ptrpDisplays);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pDisplayCount <= 0)
                    return null;
                var arr = new DisplayKhr[pDisplayCount];
                for (int i = 0; i < pDisplayCount; i++)
                {
                    arr[i] = new DisplayKhr();
                    arr[i].m = ((UInt64*)ptrpDisplays)[i];
                }

                return arr;

            }
        }

        public DisplayModePropertiesKhr[] GetDisplayModePropertiesKHR(DisplayKhr display)
        {
            Result result;
            unsafe
            {
                UInt32 pPropertyCount;
                result = Interop.NativeMethods.vkGetDisplayModePropertiesKHR(this.m, display != null ? display.m : default(UInt64), &pPropertyCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.DisplayModePropertiesKhr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpProperties = new NativeReference((int)(size * pPropertyCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpProperties = refpProperties.Handle;
                result = Interop.NativeMethods.vkGetDisplayModePropertiesKHR(this.m, display != null ? display.m : default(UInt64), &pPropertyCount, (Interop.DisplayModePropertiesKhr*)ptrpProperties);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pPropertyCount <= 0)
                    return null;
                var arr = new DisplayModePropertiesKhr[pPropertyCount];
                for (int i = 0; i < pPropertyCount; i++)
                {
                    arr[i] = new DisplayModePropertiesKhr(new NativePointer(refpProperties, (IntPtr)(&((Interop.DisplayModePropertiesKhr*)ptrpProperties)[i])));
                }

                return arr;

            }
        }

        public DisplayModeKhr CreateDisplayModeKHR(DisplayKhr display, DisplayModeCreateInfoKhr pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            DisplayModeKhr pMode;
            unsafe
            {
                pMode = new DisplayModeKhr();

                fixed (UInt64* ptrpMode = &pMode.m)
                {
                    result = Interop.NativeMethods.vkCreateDisplayModeKHR(this.m, display != null ? display.m : default(UInt64), pCreateInfo != null ? pCreateInfo.m : (Interop.DisplayModeCreateInfoKhr*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpMode);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pMode;
            }
        }

        public DisplayPlaneCapabilitiesKhr GetDisplayPlaneCapabilitiesKHR(DisplayModeKhr mode, UInt32 planeIndex)
        {
            Result result;
            DisplayPlaneCapabilitiesKhr pCapabilities;
            unsafe
            {
                pCapabilities = new DisplayPlaneCapabilitiesKhr();
                result = Interop.NativeMethods.vkGetDisplayPlaneCapabilitiesKHR(this.m, mode != null ? mode.m : default(UInt64), planeIndex, &pCapabilities);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pCapabilities;
            }
        }

        public Bool32 GetSurfaceSupportKHR(UInt32 queueFamilyIndex, SurfaceKhr surface)
        {
            Result result;
            Bool32 pSupported;
            unsafe
            {
                pSupported = new Bool32();
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfaceSupportKHR(this.m, queueFamilyIndex, surface != null ? surface.Handle : default(UInt64), &pSupported);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSupported;
            }
        }

        public SurfaceCapabilitiesKhr GetSurfaceCapabilitiesKHR(SurfaceKhr surface)
        {
            Result result;
            SurfaceCapabilitiesKhr pSurfaceCapabilities;
            unsafe
            {
                pSurfaceCapabilities = new SurfaceCapabilitiesKhr();
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(this.m, surface != null ? surface.Handle : default(UInt64), &pSurfaceCapabilities);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSurfaceCapabilities;
            }
        }

        public SurfaceFormatKhr[] GetSurfaceFormatsKHR(SurfaceKhr surface)
        {
            Result result;
            unsafe
            {
                UInt32 pSurfaceFormatCount;
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfaceFormatsKHR(this.m, surface != null ? surface.Handle : default(UInt64), &pSurfaceFormatCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pSurfaceFormatCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(SurfaceFormatKhr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpSurfaceFormats = new NativeReference((int)(size * pSurfaceFormatCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpSurfaceFormats = refpSurfaceFormats.Handle;
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfaceFormatsKHR(this.m, surface != null ? surface.Handle : default(UInt64), &pSurfaceFormatCount, (SurfaceFormatKhr*)ptrpSurfaceFormats);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pSurfaceFormatCount <= 0)
                    return null;
                var arr = new SurfaceFormatKhr[pSurfaceFormatCount];
                for (int i = 0; i < pSurfaceFormatCount; i++)
                {
                    arr[i] = (((SurfaceFormatKhr*)ptrpSurfaceFormats)[i]);
                }

                return arr;

            }
        }

        public PresentModeKhr[] GetSurfacePresentModesKHR(SurfaceKhr surface)
        {
            Result result;
            unsafe
            {
                UInt32 pPresentModeCount;
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfacePresentModesKHR(this.m, surface != null ? surface.Handle : default(UInt64), &pPresentModeCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pPresentModeCount <= 0)
                    return null;

                int size = 4;
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpPresentModes = new NativeReference((int)(size * pPresentModeCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpPresentModes = refpPresentModes.Handle;
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfacePresentModesKHR(this.m, surface != null ? surface.Handle : default(UInt64), &pPresentModeCount, (PresentModeKhr*)ptrpPresentModes);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pPresentModeCount <= 0)
                    return null;
                var arr = new PresentModeKhr[pPresentModeCount];
                for (int i = 0; i < pPresentModeCount; i++)
                {
                    arr[i] = new PresentModeKhr();
                    arr[i] = ((PresentModeKhr*)ptrpPresentModes)[i];
                }

                return arr;

            }
        }

        public ExternalImageFormatPropertiesNv GetExternalImageFormatPropertiesNV(Format format, ImageType type, ImageTiling tiling, ImageUsageFlags usage, ImageCreateFlags flags = (ImageCreateFlags)0, ExternalMemoryHandleTypeFlagsNv externalHandleType = (ExternalMemoryHandleTypeFlagsNv)0)
        {
            Result result;
            ExternalImageFormatPropertiesNv pExternalImageFormatProperties;
            unsafe
            {
                pExternalImageFormatProperties = new ExternalImageFormatPropertiesNv();
                result = Interop.NativeMethods.vkGetPhysicalDeviceExternalImageFormatPropertiesNV(this.m, format, type, tiling, usage, flags, externalHandleType, &pExternalImageFormatProperties);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pExternalImageFormatProperties;
            }
        }

        public void GetGeneratedCommandsPropertiesNVX(out DeviceGeneratedCommandsFeaturesNvx pFeatures, out DeviceGeneratedCommandsLimitsNvx pLimits)
        {
            unsafe
            {
                pFeatures = new DeviceGeneratedCommandsFeaturesNvx();
                pLimits = new DeviceGeneratedCommandsLimitsNvx();
                Interop.NativeMethods.vkGetPhysicalDeviceGeneratedCommandsPropertiesNVX(this.m, pFeatures != null ? pFeatures.m : (Interop.DeviceGeneratedCommandsFeaturesNvx*)default(IntPtr), pLimits != null ? pLimits.m : (Interop.DeviceGeneratedCommandsLimitsNvx*)default(IntPtr));
            }
        }

        public PhysicalDeviceFeatures2Khr GetFeatures2KHR()
        {
            PhysicalDeviceFeatures2Khr pFeatures;
            unsafe
            {
                pFeatures = new PhysicalDeviceFeatures2Khr();
                Interop.NativeMethods.vkGetPhysicalDeviceFeatures2KHR(this.m, pFeatures != null ? pFeatures.m : (Interop.PhysicalDeviceFeatures2Khr*)default(IntPtr));

                return pFeatures;
            }
        }

        public PhysicalDeviceProperties2Khr GetProperties2KHR()
        {
            PhysicalDeviceProperties2Khr pProperties;
            unsafe
            {
                pProperties = new PhysicalDeviceProperties2Khr();
                Interop.NativeMethods.vkGetPhysicalDeviceProperties2KHR(this.m, pProperties != null ? pProperties.m : (Interop.PhysicalDeviceProperties2Khr*)default(IntPtr));

                return pProperties;
            }
        }

        public FormatProperties2Khr GetFormatProperties2KHR(Format format)
        {
            FormatProperties2Khr pFormatProperties;
            unsafe
            {
                pFormatProperties = new FormatProperties2Khr();
                Interop.NativeMethods.vkGetPhysicalDeviceFormatProperties2KHR(this.m, format, pFormatProperties != null ? pFormatProperties.m : (Interop.FormatProperties2Khr*)default(IntPtr));

                return pFormatProperties;
            }
        }

        public ImageFormatProperties2Khr GetImageFormatProperties2KHR(PhysicalDeviceImageFormatInfo2Khr pImageFormatInfo)
        {
            Result result;
            ImageFormatProperties2Khr pImageFormatProperties;
            unsafe
            {
                pImageFormatProperties = new ImageFormatProperties2Khr();
                result = Interop.NativeMethods.vkGetPhysicalDeviceImageFormatProperties2KHR(this.m, pImageFormatInfo != null ? pImageFormatInfo.m : (Interop.PhysicalDeviceImageFormatInfo2Khr*)default(IntPtr), pImageFormatProperties != null ? pImageFormatProperties.m : (Interop.ImageFormatProperties2Khr*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);

                return pImageFormatProperties;
            }
        }

        public QueueFamilyProperties2Khr[] GetQueueFamilyProperties2KHR()
        {
            unsafe
            {
                UInt32 pQueueFamilyPropertyCount;
                Interop.NativeMethods.vkGetPhysicalDeviceQueueFamilyProperties2KHR(this.m, &pQueueFamilyPropertyCount, null);
                if (pQueueFamilyPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.QueueFamilyProperties2Khr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpQueueFamilyProperties = new NativeReference((int)(size * pQueueFamilyPropertyCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpQueueFamilyProperties = refpQueueFamilyProperties.Handle;
                Interop.NativeMethods.vkGetPhysicalDeviceQueueFamilyProperties2KHR(this.m, &pQueueFamilyPropertyCount, (Interop.QueueFamilyProperties2Khr*)ptrpQueueFamilyProperties);

                if (pQueueFamilyPropertyCount <= 0)
                    return null;
                var arr = new QueueFamilyProperties2Khr[pQueueFamilyPropertyCount];
                for (int i = 0; i < pQueueFamilyPropertyCount; i++)
                {
                    arr[i] = new QueueFamilyProperties2Khr(new NativePointer(refpQueueFamilyProperties, (IntPtr)(&((Interop.QueueFamilyProperties2Khr*)ptrpQueueFamilyProperties)[i])));
                }

                return arr;

            }
        }

        public PhysicalDeviceMemoryProperties2Khr GetMemoryProperties2KHR()
        {
            PhysicalDeviceMemoryProperties2Khr pMemoryProperties;
            unsafe
            {
                pMemoryProperties = new PhysicalDeviceMemoryProperties2Khr();
                Interop.NativeMethods.vkGetPhysicalDeviceMemoryProperties2KHR(this.m, pMemoryProperties != null ? pMemoryProperties.m : (Interop.PhysicalDeviceMemoryProperties2Khr*)default(IntPtr));

                return pMemoryProperties;
            }
        }

        public SparseImageFormatProperties2Khr[] GetSparseImageFormatProperties2KHR(PhysicalDeviceSparseImageFormatInfo2Khr pFormatInfo)
        {
            unsafe
            {
                UInt32 pPropertyCount;
                Interop.NativeMethods.vkGetPhysicalDeviceSparseImageFormatProperties2KHR(this.m, pFormatInfo != null ? pFormatInfo.m : (Interop.PhysicalDeviceSparseImageFormatInfo2Khr*)default(IntPtr), &pPropertyCount, null);
                if (pPropertyCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.SparseImageFormatProperties2Khr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpProperties = new NativeReference((int)(size * pPropertyCount));
#pragma warning disable CA2000 // Dispose objects before losing scope

                var ptrpProperties = refpProperties.Handle;
                Interop.NativeMethods.vkGetPhysicalDeviceSparseImageFormatProperties2KHR(this.m, pFormatInfo != null ? pFormatInfo.m : (Interop.PhysicalDeviceSparseImageFormatInfo2Khr*)default(IntPtr), &pPropertyCount, (Interop.SparseImageFormatProperties2Khr*)ptrpProperties);

                if (pPropertyCount <= 0)
                    return null;
                var arr = new SparseImageFormatProperties2Khr[pPropertyCount];
                for (int i = 0; i < pPropertyCount; i++)
                {
                    arr[i] = new SparseImageFormatProperties2Khr(new NativePointer(refpProperties, (IntPtr)(&((Interop.SparseImageFormatProperties2Khr*)ptrpProperties)[i])));
                }

                return arr;

            }
        }

        public ExternalBufferPropertiesKhr GetExternalBufferPropertiesKHR(PhysicalDeviceExternalBufferInfoKhr pExternalBufferInfo)
        {
            ExternalBufferPropertiesKhr pExternalBufferProperties;
            unsafe
            {
                pExternalBufferProperties = new ExternalBufferPropertiesKhr();
                Interop.NativeMethods.vkGetPhysicalDeviceExternalBufferPropertiesKHR(this.m, pExternalBufferInfo != null ? pExternalBufferInfo.m : (Interop.PhysicalDeviceExternalBufferInfoKhr*)default(IntPtr), pExternalBufferProperties != null ? pExternalBufferProperties.m : (Interop.ExternalBufferPropertiesKhr*)default(IntPtr));

                return pExternalBufferProperties;
            }
        }

        public ExternalSemaphorePropertiesKhr GetExternalSemaphorePropertiesKHR(PhysicalDeviceExternalSemaphoreInfoKhr pExternalSemaphoreInfo)
        {
            ExternalSemaphorePropertiesKhr pExternalSemaphoreProperties;
            unsafe
            {
                pExternalSemaphoreProperties = new ExternalSemaphorePropertiesKhr();
                Interop.NativeMethods.vkGetPhysicalDeviceExternalSemaphorePropertiesKHR(this.m, pExternalSemaphoreInfo != null ? pExternalSemaphoreInfo.m : (Interop.PhysicalDeviceExternalSemaphoreInfoKhr*)default(IntPtr), pExternalSemaphoreProperties != null ? pExternalSemaphoreProperties.m : (Interop.ExternalSemaphorePropertiesKhr*)default(IntPtr));

                return pExternalSemaphoreProperties;
            }
        }

        public ExternalFencePropertiesKhr GetExternalFencePropertiesKHR(PhysicalDeviceExternalFenceInfoKhr pExternalFenceInfo)
        {
            ExternalFencePropertiesKhr pExternalFenceProperties;
            unsafe
            {
                pExternalFenceProperties = new ExternalFencePropertiesKhr();
                Interop.NativeMethods.vkGetPhysicalDeviceExternalFencePropertiesKHR(this.m, pExternalFenceInfo != null ? pExternalFenceInfo.m : (Interop.PhysicalDeviceExternalFenceInfoKhr*)default(IntPtr), pExternalFenceProperties != null ? pExternalFenceProperties.m : (Interop.ExternalFencePropertiesKhr*)default(IntPtr));

                return pExternalFenceProperties;
            }
        }

        public void ReleaseDisplayEXT(DisplayKhr display)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkReleaseDisplayEXT(this.m, display != null ? display.m : default(UInt64));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public IntPtr AcquireXlibDisplayEXT(DisplayKhr display)
        {
            Result result;
            IntPtr dpy;
            unsafe
            {
                dpy = new IntPtr();
                result = Interop.NativeMethods.vkAcquireXlibDisplayEXT(this.m, &dpy, display != null ? display.m : default(UInt64));
                if (result != Result.Success)
                    throw new ResultException(result);

                return dpy;
            }
        }

        public void GetRandROutputDisplayEXT(out IntPtr dpy, UInt32 rrOutput, out DisplayKhr pDisplay)
        {
            Result result;
            unsafe
            {
                pDisplay = new DisplayKhr();

                fixed (IntPtr* ptrdpy = &dpy)
                {
                    fixed (UInt64* ptrpDisplay = &pDisplay.m)
                    {
                        result = Interop.NativeMethods.vkGetRandROutputDisplayEXT(this.m, ptrdpy, rrOutput, ptrpDisplay);
                    }
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public SurfaceCapabilities2Ext GetSurfaceCapabilities2EXT(SurfaceKhr surface)
        {
            Result result;
            SurfaceCapabilities2Ext pSurfaceCapabilities;
            unsafe
            {
                pSurfaceCapabilities = new SurfaceCapabilities2Ext();
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfaceCapabilities2EXT(this.m, surface != null ? surface.Handle : default(UInt64), pSurfaceCapabilities != null ? pSurfaceCapabilities.m : (Interop.SurfaceCapabilities2Ext*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSurfaceCapabilities;
            }
        }

        public Rect2D[] GetPresentRectanglesKHX(SurfaceKhr surface)
        {
            Result result;
            unsafe
            {
                UInt32 pRectCount;
                result = Interop.NativeMethods.vkGetPhysicalDevicePresentRectanglesKHX(this.m, surface != null ? surface.Handle : default(UInt64), &pRectCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pRectCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Rect2D));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpRects = new NativeReference((int)(size * pRectCount));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var ptrpRects = refpRects.Handle;
                    result = Interop.NativeMethods.vkGetPhysicalDevicePresentRectanglesKHX(this.m, surface != null ? surface.Handle : default(UInt64), &pRectCount, (Rect2D*)ptrpRects);
                    if (result != Result.Success)
                        throw new ResultException(result);

                    if (pRectCount <= 0)
                        return null;
                    var arr = new Rect2D[pRectCount];
                    for (int i = 0; i < pRectCount; i++)
                    {
                        arr[i] = (((Rect2D*)ptrpRects)[i]);
                    }

                    return arr;
                
            }
        }

        public MultisamplePropertiesExt GetMultisamplePropertiesEXT(SampleCountFlags samples)
        {
            MultisamplePropertiesExt pMultisampleProperties;
            unsafe
            {
                pMultisampleProperties = new MultisamplePropertiesExt();
                Interop.NativeMethods.vkGetPhysicalDeviceMultisamplePropertiesEXT(this.m, samples, pMultisampleProperties != null ? pMultisampleProperties.m : (Interop.MultisamplePropertiesExt*)default(IntPtr));

                return pMultisampleProperties;
            }
        }

        public SurfaceCapabilities2Khr GetSurfaceCapabilities2KHR(PhysicalDeviceSurfaceInfo2Khr pSurfaceInfo)
        {
            Result result;
            SurfaceCapabilities2Khr pSurfaceCapabilities;
            unsafe
            {
                pSurfaceCapabilities = new SurfaceCapabilities2Khr();
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfaceCapabilities2KHR(this.m, pSurfaceInfo != null ? pSurfaceInfo.m : (Interop.PhysicalDeviceSurfaceInfo2Khr*)default(IntPtr), pSurfaceCapabilities != null ? pSurfaceCapabilities.m : (Interop.SurfaceCapabilities2Khr*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSurfaceCapabilities;
            }
        }

        public SurfaceFormat2Khr[] GetSurfaceFormats2KHR(PhysicalDeviceSurfaceInfo2Khr pSurfaceInfo)
        {
            Result result;
            unsafe
            {
                UInt32 pSurfaceFormatCount;
                result = Interop.NativeMethods.vkGetPhysicalDeviceSurfaceFormats2KHR(this.m, pSurfaceInfo != null ? pSurfaceInfo.m : (Interop.PhysicalDeviceSurfaceInfo2Khr*)default(IntPtr), &pSurfaceFormatCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pSurfaceFormatCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.SurfaceFormat2Khr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpSurfaceFormats = new NativeReference((int)(size * pSurfaceFormatCount));
#pragma warning disable CA2000 // Dispose objects before losing scope
                
                    var ptrpSurfaceFormats = refpSurfaceFormats.Handle;
                    result = Interop.NativeMethods.vkGetPhysicalDeviceSurfaceFormats2KHR(this.m, pSurfaceInfo != null ? pSurfaceInfo.m : (Interop.PhysicalDeviceSurfaceInfo2Khr*)default(IntPtr), &pSurfaceFormatCount, (Interop.SurfaceFormat2Khr*)ptrpSurfaceFormats);
                    if (result != Result.Success)
                        throw new ResultException(result);

                    if (pSurfaceFormatCount <= 0)
                        return null;
                    var arr = new SurfaceFormat2Khr[pSurfaceFormatCount];
                    for (int i = 0; i < pSurfaceFormatCount; i++)
                    {
                        arr[i] = new SurfaceFormat2Khr(new NativePointer(refpSurfaceFormats, (IntPtr)(&((Interop.SurfaceFormat2Khr*)ptrpSurfaceFormats)[i])));
                    }

                    return arr;
                
            }
        }

        public void Dispose()
        {
            foreach (Device device in Devices)
                device.Dispose();

            Devices.Clear();
        }

        /// <summary>
        /// Return the format supported by the surface
        /// </summary>
        public SurfaceFormatKhr GetSurfaceFormat(SurfaceKhr surface, Format[] expectedFormats, ColorSpaceKhr[] expectedColorSpaces)
        {
            foreach (var f in GetSurfaceFormatsKHR(surface))
            {
                if (expectedFormats.Contains(f.Format) && expectedColorSpaces.Contains(f.ColorSpace))
                    return f;
            }

            throw new System.Exception("didn't find the expected formats and colorspaces.");
        }
    }
}
