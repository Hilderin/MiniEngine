using System;
using System.Linq;
using System.Runtime.InteropServices;
using MiniEngine.Drivers.Vulkan.Interop;
using MiniEngine.Drivers.Vulkan.Windows;

namespace MiniEngine.Drivers.Vulkan
{
    public partial class VkInstance : IMarshalling, IDisposable
    {
        /// <summary>
        /// Physical device
        /// </summary>
        public PhysicalDevice PhysicalDevice;

        /// <summary>
        /// Surface where to render
        /// </summary>
        public SurfaceKhr Surface;

        /// <summary>
        /// Device used
        /// </summary>
        public Device Device;


        public IntPtr Handle;

        private NativeMethods.vkCreateDebugReportCallbackEXT vkCreateDebugReportCallbackEXT;
        private NativeMethods.vkDestroyDebugReportCallbackEXT vkDestroyDebugReportCallbackEXT;
        private NativeMethods.vkDebugReportMessageEXT vkDebugReportMessageEXT;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkInstance(string applicationName, VkVersion applicationVersion, Func<VkInstance, SurfaceKhr> surfaceCreationCallback, DebugReportCallback debugCallback = null)
        {

            var layerProperties = VkCommands.EnumerateInstanceLayerProperties();

            string[] layersToEnable = new string[0];
            if (debugCallback != null)
            {
                if (!layerProperties.Any(l => l.LayerName == "VK_LAYER_KHRONOS_validation"))
                    throw new NotSupportedException("Layer 'VK_LAYER_KHRONOS_validation' not supported, impossible to enable debug mode.");

                layersToEnable = new[] { "VK_LAYER_KHRONOS_validation" };
            };

            using (var createInfo = new InstanceCreateInfo
            {
                EnabledExtensionNames = new string[] { "VK_KHR_surface", "VK_KHR_win32_surface", "VK_EXT_debug_report" },
                EnabledLayerNames = layersToEnable,
                ApplicationInfo = new ApplicationInfo
                {
                    ApplicationName = applicationName,
                    ApplicationVersion = applicationVersion.ToUInt(),
                    EngineName = "MiniEngine",
                    EngineVersion = MiniEngine.Drivers.Vulkan.VkVersion.ToUInt(1, 0, 0),
                    ApiVersion = MiniEngine.Drivers.Vulkan.VkVersion.ToUInt(1, 2, 0)
                }
            })
            {
                CreateInstance(createInfo);
            }

            if (debugCallback != null)
                EnableDebug(debugCallback);

            //Surface creation...
            Surface = surfaceCreationCallback(this);

            //Physical device...
            PhysicalDevice = PickPhysicalDevice();

            //And we can create a device...
            Device = PhysicalDevice.CreateDevice(Surface);
        }

        /// <summary>
        /// Get the right Physical device
        /// </summary>
        public PhysicalDevice PickPhysicalDevice()
        {
            //TODO: Check the physical device suitable for our project
            PhysicalDevice = EnumeratePhysicalDevices()[0];

            return PhysicalDevice;
        }

        /// <summary>
        /// Dispose high level objects
        /// </summary>
        private void DisposeHighLevelObjects()
        {
            if (PhysicalDevice != null)
            {
                PhysicalDevice.Dispose();
                PhysicalDevice = null;
            }


            if (Surface != null)
            {
                DestroySurfaceKHR(Surface);
                Surface = null;
            }
        }

        private Delegate GetMethod(string name, Type type)
        {
            var funcPtr = GetProcAddr(name);

            if (funcPtr == IntPtr.Zero)
                return null;

            return Marshal.GetDelegateForFunctionPointer(funcPtr, type);
        }

        private void InitializeFunctions()
        {

            vkCreateDebugReportCallbackEXT = (NativeMethods.vkCreateDebugReportCallbackEXT)GetMethod("vkCreateDebugReportCallbackEXT", typeof(NativeMethods.vkCreateDebugReportCallbackEXT));
            vkDestroyDebugReportCallbackEXT = (NativeMethods.vkDestroyDebugReportCallbackEXT)GetMethod("vkDestroyDebugReportCallbackEXT", typeof(NativeMethods.vkDestroyDebugReportCallbackEXT));
            vkDebugReportMessageEXT = (NativeMethods.vkDebugReportMessageEXT)GetMethod("vkDebugReportMessageEXT", typeof(NativeMethods.vkDebugReportMessageEXT));
        }

        private void CreateInstance(InstanceCreateInfo CreateInfo, AllocationCallbacks Allocator = null)
        {
            Result result;

            unsafe
            {
                fixed (IntPtr* ptrInstance = &Handle)
                {
                    result = Interop.NativeMethods.vkCreateInstance(CreateInfo.m, Allocator != null ? Allocator.m : null, ptrInstance);
                }
            }

            if (result != Result.Success)
                throw new ResultException(result);

            InitializeFunctions();
        }


        public void Dispose()
        {
            DisposeHighLevelObjects();

            if (debugCallback != null && vkDestroyDebugReportCallbackEXT != null)
            {
                DestroyDebugReportCallbackEXT(debugCallback);
                debugCallback = null;
                debugCallbackFuncInternal = null;
                debugCallbackDelegate = null;
            }
            if (Handle != IntPtr.Zero)
            {
                Destroy();
                Handle = IntPtr.Zero;
            }
        }


        DebugReportCallbackExt debugCallback;
        /// <summary>
        /// Important the create a variable so the garbage collector will not destroy it
        /// </summary>
        DebugReportCallbackInternal debugCallbackFuncInternal;
        DebugReportCallback debugCallbackDelegate;

        private delegate Bool32 DebugReportCallbackInternal(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData);

        public void EnableDebug(DebugReportCallback callback, DebugReportFlagsExt flags = DebugReportFlagsExt.Debug | DebugReportFlagsExt.Error | DebugReportFlagsExt.Information | DebugReportFlagsExt.PerformanceWarning | DebugReportFlagsExt.Warning)
        {
            if (vkCreateDebugReportCallbackEXT == null)
                throw new InvalidOperationException("vkCreateDebugReportCallbackEXT is not available, possibly you might be missing VK_EXT_debug_report extension. Try to enable it when creating the Instance.");

            debugCallbackFuncInternal = DebugCallbackInternal;
            debugCallbackDelegate = callback;

            using (var debugCreateInfo = new DebugReportCallbackCreateInfoExt()
            {
                Flags = flags,
                PfnCallback = Marshal.GetFunctionPointerForDelegate(debugCallbackFuncInternal)
            })
            {
                if (debugCallback != null)
                    DestroyDebugReportCallbackEXT(debugCallback);
                debugCallback = CreateDebugReportCallbackEXT(debugCreateInfo);
            }
        }


        private Bool32 DebugCallbackInternal(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
        {
            string messageStr = Marshal.PtrToStringAnsi(message);
            return debugCallbackDelegate(flags, objectType, messageCode, messageStr);
        }
   
        IntPtr IMarshalling.Handle
        {
            get
            {
                return Handle;
            }
        }

        public void Destroy(AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyInstance(this.Handle, pAllocator != null ? pAllocator.m : null);
            }
        }

        public PhysicalDevice[] EnumeratePhysicalDevices()
        {
            Result result;
            unsafe
            {
                UInt32 pPhysicalDeviceCount;
                result = Interop.NativeMethods.vkEnumeratePhysicalDevices(this.Handle, &pPhysicalDeviceCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pPhysicalDeviceCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(IntPtr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpPhysicalDevices = new NativeReference((int)(size * pPhysicalDeviceCount));
#pragma warning restore CA2000 // Dispose objects before losing scope
                var ptrpPhysicalDevices = refpPhysicalDevices.Handle;
                result = Interop.NativeMethods.vkEnumeratePhysicalDevices(this.Handle, &pPhysicalDeviceCount, (IntPtr*)ptrpPhysicalDevices);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pPhysicalDeviceCount <= 0)
                    return null;
                var arr = new PhysicalDevice[pPhysicalDeviceCount];
                for (int i = 0; i < pPhysicalDeviceCount; i++)
                {
                    arr[i] = new PhysicalDevice();
                    arr[i].m = ((IntPtr*)ptrpPhysicalDevices)[i];
                }

                return arr;
            }
        }

        public IntPtr GetProcAddr(string pName)
        {
            unsafe
            {
                return Interop.NativeMethods.vkGetInstanceProcAddr(this.Handle, pName);
            }
        }

        public SurfaceKhr CreateDisplayPlaneSurfaceKHR(DisplaySurfaceCreateInfoKhr pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            SurfaceKhr pSurface;
            unsafe
            {
                pSurface = new SurfaceKhr();

                fixed (UInt64* ptrpSurface = &pSurface.Handle)
                {
                    result = Interop.NativeMethods.vkCreateDisplayPlaneSurfaceKHR(this.Handle, pCreateInfo != null ? pCreateInfo.m : (Interop.DisplaySurfaceCreateInfoKhr*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpSurface);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSurface;
            }
        }

        public void DestroySurfaceKHR(SurfaceKhr surface = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroySurfaceKHR(this.Handle, surface != null ? surface.Handle : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public SurfaceKhr CreateViSurfaceNN(ViSurfaceCreateInfoNn pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            SurfaceKhr pSurface;
            unsafe
            {
                pSurface = new SurfaceKhr();

                fixed (UInt64* ptrpSurface = &pSurface.Handle)
                {
                    result = Interop.NativeMethods.vkCreateViSurfaceNN(this.Handle, pCreateInfo != null ? pCreateInfo.m : (Interop.ViSurfaceCreateInfoNn*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpSurface);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSurface;
            }
        }

        public DebugReportCallbackExt CreateDebugReportCallbackEXT(DebugReportCallbackCreateInfoExt pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            DebugReportCallbackExt pCallback;
            unsafe
            {
                pCallback = new DebugReportCallbackExt();

                fixed (UInt64* ptrpCallback = &pCallback.m)
                {
                    result = vkCreateDebugReportCallbackEXT(this.Handle, pCreateInfo != null ? pCreateInfo.m : (Interop.DebugReportCallbackCreateInfoExt*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpCallback);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pCallback;
            }
        }

        public void DestroyDebugReportCallbackEXT(DebugReportCallbackExt callback, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                vkDestroyDebugReportCallbackEXT(this.Handle, callback != null ? callback.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public void DebugReportMessageEXT(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, UInt64 @object, UIntPtr location, Int32 messageCode, string pLayerPrefix, string pMessage)
        {
            unsafe
            {
                vkDebugReportMessageEXT(this.Handle, flags, objectType, @object, location, messageCode, pLayerPrefix, pMessage);
            }
        }

        public SurfaceKhr CreateMacOSSurfaceMVK(MacOSSurfaceCreateInfoMvk pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            SurfaceKhr pSurface;
            unsafe
            {
                pSurface = new SurfaceKhr();

                fixed (UInt64* ptrpSurface = &pSurface.Handle)
                {
                    result = Interop.NativeMethods.vkCreateMacOSSurfaceMVK(this.Handle, pCreateInfo != null ? pCreateInfo.m : (Interop.MacOSSurfaceCreateInfoMvk*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpSurface);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSurface;
            }
        }
    }

}
