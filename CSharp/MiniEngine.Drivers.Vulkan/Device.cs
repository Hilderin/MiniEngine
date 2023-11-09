using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MiniEngine.Drivers.Vulkan
{
    /// <summary>
    /// Vulkan Device
    /// </summary>
    public partial class Device : IDisposable, IMarshalling
    {
        private bool _isDisposed;
        internal IntPtr m;

        internal Device() { }

        public PhysicalDevice PhysicalDevice;
        public SurfaceKhr Surface;


        /// <summary>
        /// Create ImageViews from Images
        /// </summary>
        public ImageView[] CreateImageViews(Image[] images, SurfaceFormatKhr surfaceFormat)
        {
            var displayViews = new ImageView[images.Length];

            for (int i = 0; i < images.Length; i++)
            {
                using (var viewCreateInfo = new ImageViewCreateInfo
                {
                    Image = images[i],
                    ViewType = ImageViewType.View2D,
                    Format = surfaceFormat.Format,
                    //TODO: à mettre en commentaire???
                    Components = new ComponentMapping
                    {
                        R = ComponentSwizzle.R,
                        G = ComponentSwizzle.G,
                        B = ComponentSwizzle.B,
                        A = ComponentSwizzle.A
                    },
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.Color,
                        LevelCount = 1,
                        LayerCount = 1
                    }
                })
                {
                    displayViews[i] = this.CreateImageView(viewCreateInfo);
                }
            }

            return displayViews;
        }

        /// <summary>
        /// Create framebuffers from imageviews
        /// </summary>
        public Framebuffer[] CreateFramebuffers(RenderPass renderPass, ImageView[] displayViews, Extent2D extent, ImageView depthImageView)
        {
            var framebuffers = new Framebuffer[displayViews.Length];

            for (int i = 0; i < displayViews.Length; i++)
            {
                ImageView[] attachments;
                if (depthImageView != null)
                {
                    attachments = new ImageView[]
                    {
                        displayViews[i], depthImageView
                    };
                }
                else
                {
                    attachments = new ImageView[]
                    {
                        displayViews[i]
                    };
                }

                using (var frameBufferCreateInfo = new FramebufferCreateInfo
                {
                    Layers = 1,
                    RenderPass = renderPass,
                    Attachments = attachments,
                    Width = extent.Width,
                    Height = extent.Height
                })
                {
                    framebuffers[i] = this.CreateFramebuffer(frameBufferCreateInfo);
                }
            }

            return framebuffers;
        }

        /// <summary>
        /// Allocate a DeviceMemory for a buffer
        /// </summary>
        public DeviceMemory CreateDeviceMemory(Buffer buffer, MemoryPropertyFlags memoryPropertyFlags)
        {
            var memoryReq = this.GetBufferMemoryRequirements(buffer);

            using (var allocInfo = new MemoryAllocateInfo
            { 
                AllocationSize = memoryReq.Size,
                MemoryTypeIndex = GetMemoryTypeIndex(memoryReq.MemoryTypeBits, memoryPropertyFlags)
            })
            {
                return this.AllocateMemory(allocInfo);
            }
        }

        /// <summary>
        /// Allocate a DeviceMemory for an image
        /// </summary>
        public DeviceMemory CreateDeviceMemory(Image image, MemoryPropertyFlags memoryPropertyFlags)
        {
            var memoryReq = this.GetImageMemoryRequirements(image);

            using (var allocInfo = new MemoryAllocateInfo
            { 
                AllocationSize = memoryReq.Size,
                MemoryTypeIndex = GetMemoryTypeIndex(memoryReq.MemoryTypeBits, memoryPropertyFlags)
            })
            {
                return this.AllocateMemory(allocInfo);
            }
        }

        /// <summary>
        /// Find MemoryTypeIndex on a device for memorytype and memoryproperties
        /// </summary>
        public uint GetMemoryTypeIndex(UInt32 memoryTypeBits, MemoryPropertyFlags memoryPropertyFlags)
        {

            using (var memoryProperties = this.PhysicalDevice.GetMemoryProperties())
            {
                var memoryTypes = memoryProperties.MemoryTypes;

                for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
                {
                    if ((memoryTypeBits & 1) == 1 &&
                        (memoryTypes[i].PropertyFlags & memoryPropertyFlags) == memoryPropertyFlags)
                    {
                        return i;
                    }
                    memoryTypeBits >>= 1;
                }

                //Not found, we take the first one...
                return 0;
            }
        }



        IntPtr IMarshalling.Handle
        {
            get
            {
                return m;
            }
        }

        public IntPtr GetProcAddr(string pName)
        {
            unsafe
            {
                return Interop.NativeMethods.vkGetDeviceProcAddr(this.m, pName);
            }
        }

        public void Destroy(AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyDevice(this.m, pAllocator != null ? pAllocator.m : null);
            }
        }

        public Queue GetQueue(UInt32 queueFamilyIndex, UInt32 queueIndex)
        {
            Queue pQueue;
            unsafe
            {
                pQueue = new Queue();

                fixed (IntPtr* ptrpQueue = &pQueue.m)
                {
                    Interop.NativeMethods.vkGetDeviceQueue(this.m, queueFamilyIndex, queueIndex, ptrpQueue);
                }

                return pQueue;
            }
        }

        public void WaitIdle()
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkDeviceWaitIdle(this.m);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public DeviceMemory AllocateMemory(MemoryAllocateInfo pAllocateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            DeviceMemory pMemory;
            unsafe
            {
                pMemory = new DeviceMemory();

                fixed (UInt64* ptrpMemory = &pMemory.m)
                {
                    result = Interop.NativeMethods.vkAllocateMemory(this.m, pAllocateInfo != null ? pAllocateInfo.m : (Interop.MemoryAllocateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpMemory);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pMemory;
            }
        }

        public void FreeMemory(DeviceMemory memory = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkFreeMemory(this.m, memory != null ? memory.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public IntPtr MapMemory(DeviceMemory memory, DeviceSize offset, DeviceSize size, UInt32 flags = 0)
        {
            Result result;
            IntPtr ppData;
            unsafe
            {
                ppData = new IntPtr();
                result = Interop.NativeMethods.vkMapMemory(this.m, memory != null ? memory.m : default(UInt64), offset, size, flags, &ppData);
                if (result != Result.Success)
                    throw new ResultException(result);

                return ppData;
            }
        }

        public void UnmapMemory(DeviceMemory memory)
        {
            unsafe
            {
                Interop.NativeMethods.vkUnmapMemory(this.m, memory != null ? memory.m : default(UInt64));
            }
        }

        public void FlushMappedMemoryRanges(MappedMemoryRange[] pMemoryRanges)
        {
            Result result;
            unsafe
            {
                var arraypMemoryRanges = pMemoryRanges == null ? IntPtr.Zero : Marshal.AllocHGlobal(pMemoryRanges.Length * sizeof(Interop.MappedMemoryRange));
                var lenpMemoryRanges = pMemoryRanges == null ? 0 : pMemoryRanges.Length;
                if (pMemoryRanges != null)
                    for (int i = 0; i < pMemoryRanges.Length; i++)
                        ((Interop.MappedMemoryRange*)arraypMemoryRanges)[i] = *(pMemoryRanges[i].m);
                result = Interop.NativeMethods.vkFlushMappedMemoryRanges(this.m, (uint)lenpMemoryRanges, (Interop.MappedMemoryRange*)arraypMemoryRanges);
                Marshal.FreeHGlobal(arraypMemoryRanges);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void FlushMappedMemoryRange(MappedMemoryRange pMemoryRange)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkFlushMappedMemoryRanges(this.m, (UInt32)(pMemoryRange != null ? 1 : 0), pMemoryRange != null ? pMemoryRange.m : (Interop.MappedMemoryRange*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void InvalidateMappedMemoryRanges(MappedMemoryRange[] pMemoryRanges)
        {
            Result result;
            unsafe
            {
                var arraypMemoryRanges = pMemoryRanges == null ? IntPtr.Zero : Marshal.AllocHGlobal(pMemoryRanges.Length * sizeof(Interop.MappedMemoryRange));
                var lenpMemoryRanges = pMemoryRanges == null ? 0 : pMemoryRanges.Length;
                if (pMemoryRanges != null)
                    for (int i = 0; i < pMemoryRanges.Length; i++)
                        ((Interop.MappedMemoryRange*)arraypMemoryRanges)[i] = *(pMemoryRanges[i].m);
                result = Interop.NativeMethods.vkInvalidateMappedMemoryRanges(this.m, (uint)lenpMemoryRanges, (Interop.MappedMemoryRange*)arraypMemoryRanges);
                Marshal.FreeHGlobal(arraypMemoryRanges);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void InvalidateMappedMemoryRange(MappedMemoryRange pMemoryRange)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkInvalidateMappedMemoryRanges(this.m, (UInt32)(pMemoryRange != null ? 1 : 0), pMemoryRange != null ? pMemoryRange.m : (Interop.MappedMemoryRange*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public DeviceSize GetMemoryCommitment(DeviceMemory memory)
        {
            DeviceSize pCommittedMemoryInBytes;
            unsafe
            {
                pCommittedMemoryInBytes = new DeviceSize();
                Interop.NativeMethods.vkGetDeviceMemoryCommitment(this.m, memory != null ? memory.m : default(UInt64), &pCommittedMemoryInBytes);

                return pCommittedMemoryInBytes;
            }
        }

        public MemoryRequirements GetBufferMemoryRequirements(Buffer buffer)
        {
            MemoryRequirements pMemoryRequirements;
            unsafe
            {
                pMemoryRequirements = new MemoryRequirements();
                Interop.NativeMethods.vkGetBufferMemoryRequirements(this.m, buffer != null ? buffer.m : default(UInt64), &pMemoryRequirements);

                return pMemoryRequirements;
            }
        }

        public void BindBufferMemory(Buffer buffer, DeviceMemory memory, DeviceSize memoryOffset)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkBindBufferMemory(this.m, buffer != null ? buffer.m : default(UInt64), memory != null ? memory.m : default(UInt64), memoryOffset);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public MemoryRequirements GetImageMemoryRequirements(Image image)
        {
            MemoryRequirements pMemoryRequirements;
            unsafe
            {
                pMemoryRequirements = new MemoryRequirements();
                Interop.NativeMethods.vkGetImageMemoryRequirements(this.m, image != null ? image.m : default(UInt64), &pMemoryRequirements);

                return pMemoryRequirements;
            }
        }

        public void BindImageMemory(Image image, DeviceMemory memory, DeviceSize memoryOffset)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkBindImageMemory(this.m, image != null ? image.m : default(UInt64), memory != null ? memory.m : default(UInt64), memoryOffset);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public SparseImageMemoryRequirements[] GetImageSparseMemoryRequirements(Image image)
        {
            unsafe
            {
                UInt32 pSparseMemoryRequirementCount;
                Interop.NativeMethods.vkGetImageSparseMemoryRequirements(this.m, image != null ? image.m : default(UInt64), &pSparseMemoryRequirementCount, null);
                if (pSparseMemoryRequirementCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(SparseImageMemoryRequirements));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpSparseMemoryRequirements = new NativeReference((int)(size * pSparseMemoryRequirementCount));
                var ptrpSparseMemoryRequirements = refpSparseMemoryRequirements.Handle;
                Interop.NativeMethods.vkGetImageSparseMemoryRequirements(this.m, image != null ? image.m : default(UInt64), &pSparseMemoryRequirementCount, (SparseImageMemoryRequirements*)ptrpSparseMemoryRequirements);

                if (pSparseMemoryRequirementCount <= 0)
                    return null;
                var arr = new SparseImageMemoryRequirements[pSparseMemoryRequirementCount];
                for (int i = 0; i < pSparseMemoryRequirementCount; i++)
                {
                    arr[i] = (((SparseImageMemoryRequirements*)ptrpSparseMemoryRequirements)[i]);
                }

                return arr;

            }
        }

        public Fence CreateFence()
        {
            using (var fenceInfo = new FenceCreateInfo())
            {
                return CreateFence(fenceInfo);
            }
        }

        public Fence CreateFence(FenceCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Fence pFence;
            unsafe
            {
                pFence = new Fence(this);

                fixed (UInt64* ptrpFence = &pFence.m)
                {
                    result = Interop.NativeMethods.vkCreateFence(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.FenceCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpFence);
                }
                if (result != Result.Success)
                    throw new ResultException(result);


                return pFence;
            }
        }

        public Fence[] CreateFences(int count)
        {
            Fence[] fences = new Fence[count];
            for (int i = 0; i < count; i++)
            {
                fences[i] = CreateFence();
            }
            return fences;
        }

        public void DestroyFence(Fence fence = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyFence(this.m, fence != null ? fence.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }

        }

        public void ResetFences(Fence[] pFences)
        {
            Result result;
            unsafe
            {
                var arraypFences = pFences == null ? IntPtr.Zero : Marshal.AllocHGlobal(pFences.Length * sizeof(UInt64));
                var lenpFences = pFences == null ? 0 : pFences.Length;
                if (pFences != null)
                    for (int i = 0; i < pFences.Length; i++)
                        ((UInt64*)arraypFences)[i] = (pFences[i].m);
                result = Interop.NativeMethods.vkResetFences(this.m, (uint)lenpFences, (UInt64*)arraypFences);
                Marshal.FreeHGlobal(arraypFences);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void ResetFence(Fence pFence)
        {
            Result result;
            unsafe
            {
                fixed (UInt64* ptrpFence = &pFence.m)
                {
                    result = Interop.NativeMethods.vkResetFences(this.m, (UInt32)(pFence != null ? 1 : 0), ptrpFence);
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void GetFenceStatus(Fence fence)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkGetFenceStatus(this.m, fence != null ? fence.m : default(UInt64));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void WaitForFences(Fence[] pFences, Bool32 waitAll, UInt64 timeout)
        {
            Result result;
            unsafe
            {
                var arraypFences = pFences == null ? IntPtr.Zero : Marshal.AllocHGlobal(pFences.Length * sizeof(UInt64));
                var lenpFences = pFences == null ? 0 : pFences.Length;
                if (pFences != null)
                    for (int i = 0; i < pFences.Length; i++)
                        ((UInt64*)arraypFences)[i] = (pFences[i].m);
                result = Interop.NativeMethods.vkWaitForFences(this.m, (uint)lenpFences, (UInt64*)arraypFences, waitAll, timeout);
                Marshal.FreeHGlobal(arraypFences);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void WaitForFence(Fence pFence, Bool32 waitAll, UInt64 timeout)
        {
            Result result;
            unsafe
            {
                fixed (UInt64* ptrpFence = &pFence.m)
                {
                    result = Interop.NativeMethods.vkWaitForFences(this.m, (UInt32)(pFence != null ? 1 : 0), ptrpFence, waitAll, timeout);
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public Semaphore CreateSemaphore()
        {
            using (var semaphoreInfo = new SemaphoreCreateInfo())
            {
                return CreateSemaphore(semaphoreInfo);
            }
        }

        public Semaphore CreateSemaphore(SemaphoreCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Semaphore pSemaphore;
            unsafe
            {
                pSemaphore = new Semaphore(this);

                fixed (UInt64* ptrpSemaphore = &pSemaphore.m)
                {
                    result = Interop.NativeMethods.vkCreateSemaphore(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.SemaphoreCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpSemaphore);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSemaphore;
            }
        }

        public Semaphore[] CreateSemaphores(int count)
        {
            Semaphore[] semaphores = new Semaphore[count];
            for (int i = 0; i < count; i++)
            {
                semaphores[i] = CreateSemaphore();
            }
            return semaphores;
        }

        public void DestroySemaphore(Semaphore semaphore = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroySemaphore(this.m, semaphore != null ? semaphore.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public Event CreateEvent(EventCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Event pEvent;
            unsafe
            {
                pEvent = new Event();

                fixed (UInt64* ptrpEvent = &pEvent.m)
                {
                    result = Interop.NativeMethods.vkCreateEvent(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.EventCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpEvent);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pEvent;
            }
        }

        public void DestroyEvent(Event @event = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyEvent(this.m, @event != null ? @event.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public void GetEventStatus(Event @event)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkGetEventStatus(this.m, @event != null ? @event.m : default(UInt64));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void SetEvent(Event @event)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkSetEvent(this.m, @event != null ? @event.m : default(UInt64));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void ResetEvent(Event @event)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkResetEvent(this.m, @event != null ? @event.m : default(UInt64));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public QueryPool CreateQueryPool(QueryPoolCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            QueryPool pQueryPool;
            unsafe
            {
                pQueryPool = new QueryPool();

                fixed (UInt64* ptrpQueryPool = &pQueryPool.m)
                {
                    result = Interop.NativeMethods.vkCreateQueryPool(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.QueryPoolCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpQueryPool);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pQueryPool;
            }
        }

        public void DestroyQueryPool(QueryPool queryPool = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyQueryPool(this.m, queryPool != null ? queryPool.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public IntPtr GetQueryPoolResults(QueryPool queryPool, UInt32 firstQuery, UInt32 queryCount, UIntPtr dataSize, DeviceSize stride, QueryResultFlags flags = (QueryResultFlags)0)
        {
            Result result;
            IntPtr pData;
            unsafe
            {
                pData = new IntPtr();
                result = Interop.NativeMethods.vkGetQueryPoolResults(this.m, queryPool != null ? queryPool.m : default(UInt64), firstQuery, queryCount, dataSize, pData, stride, flags);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pData;
            }
        }

        public Buffer CreateBuffer(BufferCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Buffer pBuffer;
            unsafe
            {
                pBuffer = new Buffer();

                fixed (UInt64* ptrpBuffer = &pBuffer.m)
                {
                    result = Interop.NativeMethods.vkCreateBuffer(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.BufferCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpBuffer);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pBuffer;
            }
        }

        public void DestroyBuffer(Buffer buffer = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyBuffer(this.m, buffer != null ? buffer.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public BufferView CreateBufferView(BufferViewCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            BufferView pView;
            unsafe
            {
                pView = new BufferView();

                fixed (UInt64* ptrpView = &pView.m)
                {
                    result = Interop.NativeMethods.vkCreateBufferView(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.BufferViewCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpView);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pView;
            }
        }

        public void DestroyBufferView(BufferView bufferView = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyBufferView(this.m, bufferView != null ? bufferView.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public Image CreateImage(ImageCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Image pImage;
            unsafe
            {
                pImage = new Image();

                fixed (UInt64* ptrpImage = &pImage.m)
                {
                    result = Interop.NativeMethods.vkCreateImage(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.ImageCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpImage);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pImage;
            }
        }

        public void DestroyImage(Image image = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyImage(this.m, image != null ? image.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public SubresourceLayout GetImageSubresourceLayout(Image image, ImageSubresource pSubresource)
        {
            SubresourceLayout pLayout;
            unsafe
            {
                pLayout = new SubresourceLayout();
                Interop.NativeMethods.vkGetImageSubresourceLayout(this.m, image != null ? image.m : default(UInt64), &pSubresource, &pLayout);

                return pLayout;
            }
        }

        public ImageView CreateImageView(ImageViewCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            ImageView pView;
            unsafe
            {
                pView = new ImageView();

                fixed (UInt64* ptrpView = &pView.m)
                {
                    result = Interop.NativeMethods.vkCreateImageView(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.ImageViewCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpView);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pView;
            }
        }

        public void DestroyImageView(ImageView imageView = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyImageView(this.m, imageView != null ? imageView.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public ShaderModule CreateShaderModule(ShaderModuleCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            ShaderModule pShaderModule;
            unsafe
            {
                pShaderModule = new ShaderModule();

                fixed (UInt64* ptrpShaderModule = &pShaderModule.m)
                {
                    result = Interop.NativeMethods.vkCreateShaderModule(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.ShaderModuleCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpShaderModule);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pShaderModule;
            }
        }

        public void DestroyShaderModule(ShaderModule shaderModule = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyShaderModule(this.m, shaderModule != null ? shaderModule.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public PipelineCache CreatePipelineCache(PipelineCacheCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            PipelineCache pPipelineCache;
            unsafe
            {
                pPipelineCache = new PipelineCache();

                fixed (UInt64* ptrpPipelineCache = &pPipelineCache.m)
                {
                    result = Interop.NativeMethods.vkCreatePipelineCache(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.PipelineCacheCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpPipelineCache);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pPipelineCache;
            }
        }

        public void DestroyPipelineCache(PipelineCache pipelineCache = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyPipelineCache(this.m, pipelineCache != null ? pipelineCache.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public void GetPipelineCacheData(PipelineCache pipelineCache, out UIntPtr pDataSize, IntPtr pData = default(IntPtr))
        {
            Result result;
            unsafe
            {
                fixed (UIntPtr* ptrpDataSize = &pDataSize)
                {
                    result = Interop.NativeMethods.vkGetPipelineCacheData(this.m, pipelineCache != null ? pipelineCache.m : default(UInt64), ptrpDataSize, pData);
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void MergePipelineCaches(PipelineCache dstCache, PipelineCache[] pSrcCaches)
        {
            Result result;
            unsafe
            {
                var arraypSrcCaches = pSrcCaches == null ? IntPtr.Zero : Marshal.AllocHGlobal(pSrcCaches.Length * sizeof(UInt64));
                var lenpSrcCaches = pSrcCaches == null ? 0 : pSrcCaches.Length;
                if (pSrcCaches != null)
                    for (int i = 0; i < pSrcCaches.Length; i++)
                        ((UInt64*)arraypSrcCaches)[i] = (pSrcCaches[i].m);
                result = Interop.NativeMethods.vkMergePipelineCaches(this.m, dstCache != null ? dstCache.m : default(UInt64), (uint)lenpSrcCaches, (UInt64*)arraypSrcCaches);
                Marshal.FreeHGlobal(arraypSrcCaches);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void MergePipelineCache(PipelineCache dstCache, PipelineCache pSrcCache)
        {
            Result result;
            unsafe
            {
                fixed (UInt64* ptrpSrcCache = &pSrcCache.m)
                {
                    result = Interop.NativeMethods.vkMergePipelineCaches(this.m, dstCache != null ? dstCache.m : default(UInt64), (UInt32)(pSrcCache != null ? 1 : 0), ptrpSrcCache);
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public Pipeline[] CreateGraphicsPipelines(PipelineCache pipelineCache, GraphicsPipelineCreateInfo[] pCreateInfos, AllocationCallbacks pAllocator = null)
        {
            Result result;
            unsafe
            {
                if (pCreateInfos.Length <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(UInt64));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpPipelines = new NativeReference((int)(size * pCreateInfos.Length));
                var ptrpPipelines = refpPipelines.Handle;
                var arraypCreateInfos = pCreateInfos == null ? IntPtr.Zero : Marshal.AllocHGlobal(pCreateInfos.Length * sizeof(Interop.GraphicsPipelineCreateInfo));
                var lenpCreateInfos = pCreateInfos == null ? 0 : pCreateInfos.Length;
                if (pCreateInfos != null)
                    for (int i = 0; i < pCreateInfos.Length; i++)
                        ((Interop.GraphicsPipelineCreateInfo*)arraypCreateInfos)[i] = *(pCreateInfos[i].m);
                result = Interop.NativeMethods.vkCreateGraphicsPipelines(this.m, pipelineCache != null ? pipelineCache.m : default(UInt64), (uint)lenpCreateInfos, (Interop.GraphicsPipelineCreateInfo*)arraypCreateInfos, pAllocator != null ? pAllocator.m : null, (UInt64*)ptrpPipelines);
                Marshal.FreeHGlobal(arraypCreateInfos);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pCreateInfos.Length <= 0)
                    return null;
                var arr = new Pipeline[pCreateInfos.Length];
                for (int i = 0; i < pCreateInfos.Length; i++)
                {
                    arr[i] = new Pipeline();
                    arr[i].m = ((UInt64*)ptrpPipelines)[i];
                }

                return arr;

            }
        }

        public Pipeline[] CreateComputePipelines(PipelineCache pipelineCache, ComputePipelineCreateInfo[] pCreateInfos, AllocationCallbacks pAllocator = null)
        {
            Result result;
            unsafe
            {
                if (pCreateInfos.Length <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(UInt64));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpPipelines = new NativeReference((int)(size * pCreateInfos.Length));
                var ptrpPipelines = refpPipelines.Handle;
                var arraypCreateInfos = pCreateInfos == null ? IntPtr.Zero : Marshal.AllocHGlobal(pCreateInfos.Length * sizeof(Interop.ComputePipelineCreateInfo));
                var lenpCreateInfos = pCreateInfos == null ? 0 : pCreateInfos.Length;
                if (pCreateInfos != null)
                    for (int i = 0; i < pCreateInfos.Length; i++)
                        ((Interop.ComputePipelineCreateInfo*)arraypCreateInfos)[i] = *(pCreateInfos[i].m);
                result = Interop.NativeMethods.vkCreateComputePipelines(this.m, pipelineCache != null ? pipelineCache.m : default(UInt64), (uint)lenpCreateInfos, (Interop.ComputePipelineCreateInfo*)arraypCreateInfos, pAllocator != null ? pAllocator.m : null, (UInt64*)ptrpPipelines);
                Marshal.FreeHGlobal(arraypCreateInfos);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pCreateInfos.Length <= 0)
                    return null;
                var arr = new Pipeline[pCreateInfos.Length];
                for (int i = 0; i < pCreateInfos.Length; i++)
                {
                    arr[i] = new Pipeline();
                    arr[i].m = ((UInt64*)ptrpPipelines)[i];
                }

                return arr;

            }
        }

        public void DestroyPipeline(Pipeline pipeline = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyPipeline(this.m, pipeline != null ? pipeline.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public PipelineLayout CreatePipelineLayout(DescriptorSetLayout descriptorSetLayout)
        {
            using (var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
            {
                SetLayouts = new DescriptorSetLayout[] { descriptorSetLayout }
            })
            {
                return CreatePipelineLayout(pipelineLayoutCreateInfo);
            }
        }

        public PipelineLayout CreatePipelineLayout(DescriptorSetLayout[] descriptorSetLayouts, PushConstantRange[] constantRanges)
        {
            using (var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
            {
                SetLayouts = descriptorSetLayouts,
                PushConstantRanges = constantRanges
            })
            {
                return CreatePipelineLayout(pipelineLayoutCreateInfo);
            }
        }


        public PipelineLayout CreatePipelineLayout(PipelineLayoutCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            PipelineLayout pPipelineLayout;
            unsafe
            {
                pPipelineLayout = new PipelineLayout();

                fixed (UInt64* ptrpPipelineLayout = &pPipelineLayout.m)
                {
                    result = Interop.NativeMethods.vkCreatePipelineLayout(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.PipelineLayoutCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpPipelineLayout);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pPipelineLayout;
            }
        }

        public void DestroyPipelineLayout(PipelineLayout pipelineLayout = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyPipelineLayout(this.m, pipelineLayout != null ? pipelineLayout.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public Sampler CreateSampler(SamplerCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Sampler pSampler;
            unsafe
            {
                pSampler = new Sampler();

                fixed (UInt64* ptrpSampler = &pSampler.m)
                {
                    result = Interop.NativeMethods.vkCreateSampler(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.SamplerCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpSampler);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSampler;
            }
        }

        public void DestroySampler(Sampler sampler = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroySampler(this.m, sampler != null ? sampler.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }


        //public DescriptorSetLayout CreateDescriptorSetLayout(DescriptorSetLayoutBinding[] bindings, AllocationCallbacks pAllocator = null, MarshalledObject next = null)
        //{
        //    using (var descriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo
        //    {
        //        Bindings = bindings,
        //        Next = next != null ? next.Handle : IntPtr.Zero
        //    })
        //    {
        //        return CreateDescriptorSetLayout(descriptorSetLayoutCreateInfo, pAllocator);
        //    }
        //}


        public DescriptorSetLayout CreateDescriptorSetLayout(DescriptorSetLayoutCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            DescriptorSetLayout pSetLayout;
            unsafe
            {
                pSetLayout = new DescriptorSetLayout();

                fixed (UInt64* ptrpSetLayout = &pSetLayout.m)
                {
                    result = Interop.NativeMethods.vkCreateDescriptorSetLayout(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.DescriptorSetLayoutCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpSetLayout);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSetLayout;
            }
        }

        public void DestroyDescriptorSetLayout(DescriptorSetLayout descriptorSetLayout = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyDescriptorSetLayout(this.m, descriptorSetLayout != null ? descriptorSetLayout.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public DescriptorPool CreateDescriptorPool(DescriptorPoolCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            DescriptorPool pDescriptorPool;
            unsafe
            {
                pDescriptorPool = new DescriptorPool();

                fixed (UInt64* ptrpDescriptorPool = &pDescriptorPool.m)
                {
                    result = Interop.NativeMethods.vkCreateDescriptorPool(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.DescriptorPoolCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpDescriptorPool);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pDescriptorPool;
            }
        }

        public void DestroyDescriptorPool(DescriptorPool descriptorPool = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyDescriptorPool(this.m, descriptorPool != null ? descriptorPool.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public void ResetDescriptorPool(DescriptorPool descriptorPool, UInt32 flags = 0)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkResetDescriptorPool(this.m, descriptorPool != null ? descriptorPool.m : default(UInt64), flags);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public DescriptorSet[] AllocateDescriptorSets(DescriptorSetAllocateInfo pAllocateInfo)
        {
            Result result;
            unsafe
            {
                if (pAllocateInfo.DescriptorSetCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(UInt64));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpDescriptorSets = new NativeReference((int)(size * pAllocateInfo.DescriptorSetCount));
                var ptrpDescriptorSets = refpDescriptorSets.Handle;
                result = Interop.NativeMethods.vkAllocateDescriptorSets(this.m, pAllocateInfo != null ? pAllocateInfo.m : (Interop.DescriptorSetAllocateInfo*)default(IntPtr), (UInt64*)ptrpDescriptorSets);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pAllocateInfo.DescriptorSetCount <= 0)
                    return null;
                var arr = new DescriptorSet[pAllocateInfo.DescriptorSetCount];
                for (int i = 0; i < pAllocateInfo.DescriptorSetCount; i++)
                {
                    arr[i] = new DescriptorSet();
                    arr[i].m = ((UInt64*)ptrpDescriptorSets)[i];
                }

                return arr;

            }
        }

        public void FreeDescriptorSets(DescriptorPool descriptorPool, DescriptorSet[] pDescriptorSets)
        {
            Result result;
            unsafe
            {
                var arraypDescriptorSets = pDescriptorSets == null ? IntPtr.Zero : Marshal.AllocHGlobal(pDescriptorSets.Length * sizeof(UInt64));
                var lenpDescriptorSets = pDescriptorSets == null ? 0 : pDescriptorSets.Length;
                if (pDescriptorSets != null)
                    for (int i = 0; i < pDescriptorSets.Length; i++)
                        ((UInt64*)arraypDescriptorSets)[i] = (pDescriptorSets[i].m);
                result = Interop.NativeMethods.vkFreeDescriptorSets(this.m, descriptorPool != null ? descriptorPool.m : default(UInt64), (uint)lenpDescriptorSets, (UInt64*)arraypDescriptorSets);
                Marshal.FreeHGlobal(arraypDescriptorSets);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void FreeDescriptorSet(DescriptorPool descriptorPool, DescriptorSet pDescriptorSet)
        {
            Result result;
            unsafe
            {
                fixed (UInt64* ptrpDescriptorSet = &pDescriptorSet.m)
                {
                    result = Interop.NativeMethods.vkFreeDescriptorSets(this.m, descriptorPool != null ? descriptorPool.m : default(UInt64), (UInt32)(pDescriptorSet != null ? 1 : 0), ptrpDescriptorSet);
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void UpdateDescriptorSets(WriteDescriptorSet[] pDescriptorWrites, CopyDescriptorSet[] pDescriptorCopies)
        {
            unsafe
            {
                var arraypDescriptorWrites = pDescriptorWrites == null ? IntPtr.Zero : Marshal.AllocHGlobal(pDescriptorWrites.Length * sizeof(Interop.WriteDescriptorSet));
                var lenpDescriptorWrites = pDescriptorWrites == null ? 0 : pDescriptorWrites.Length;
                if (pDescriptorWrites != null)
                    for (int i = 0; i < pDescriptorWrites.Length; i++)
                        ((Interop.WriteDescriptorSet*)arraypDescriptorWrites)[i] = *(pDescriptorWrites[i].m);
                var arraypDescriptorCopies = pDescriptorCopies == null ? IntPtr.Zero : Marshal.AllocHGlobal(pDescriptorCopies.Length * sizeof(Interop.CopyDescriptorSet));
                var lenpDescriptorCopies = pDescriptorCopies == null ? 0 : pDescriptorCopies.Length;
                if (pDescriptorCopies != null)
                    for (int i = 0; i < pDescriptorCopies.Length; i++)
                        ((Interop.CopyDescriptorSet*)arraypDescriptorCopies)[i] = *(pDescriptorCopies[i].m);
                Interop.NativeMethods.vkUpdateDescriptorSets(this.m, (uint)lenpDescriptorWrites, (Interop.WriteDescriptorSet*)arraypDescriptorWrites, (uint)lenpDescriptorCopies, (Interop.CopyDescriptorSet*)arraypDescriptorCopies);
                Marshal.FreeHGlobal(arraypDescriptorWrites);
                Marshal.FreeHGlobal(arraypDescriptorCopies);
            }
        }

        public void UpdateDescriptorSets(WriteDescriptorSet pDescriptorWrites)
        {
            unsafe
            {
                Interop.NativeMethods.vkUpdateDescriptorSets(this.m, 1, pDescriptorWrites.m, 0, (Interop.CopyDescriptorSet*)(IntPtr.Zero));
            }
        }

        public void UpdateDescriptorSet(WriteDescriptorSet pDescriptorWrite, CopyDescriptorSet pDescriptorCopie)
        {
            unsafe
            {
                Interop.NativeMethods.vkUpdateDescriptorSets(this.m, (UInt32)(pDescriptorWrite != null ? 1 : 0), pDescriptorWrite != null ? pDescriptorWrite.m : (Interop.WriteDescriptorSet*)default(IntPtr), (UInt32)(pDescriptorCopie != null ? 1 : 0), pDescriptorCopie != null ? pDescriptorCopie.m : (Interop.CopyDescriptorSet*)default(IntPtr));
            }
        }

        public Framebuffer CreateFramebuffer(FramebufferCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Framebuffer pFramebuffer;
            unsafe
            {
                pFramebuffer = new Framebuffer();

                fixed (UInt64* ptrpFramebuffer = &pFramebuffer.m)
                {
                    result = Interop.NativeMethods.vkCreateFramebuffer(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.FramebufferCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpFramebuffer);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pFramebuffer;
            }
        }

        public void DestroyFramebuffer(Framebuffer framebuffer = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyFramebuffer(this.m, framebuffer != null ? framebuffer.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public RenderPass CreateRenderPass(RenderPassCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            RenderPass pRenderPass;
            unsafe
            {
                pRenderPass = new RenderPass(this);

                fixed (UInt64* ptrpRenderPass = &pRenderPass.m)
                {
                    result = Interop.NativeMethods.vkCreateRenderPass(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.RenderPassCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpRenderPass);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pRenderPass;
            }
        }

        public void DestroyRenderPass(RenderPass renderPass = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyRenderPass(this.m, renderPass != null ? renderPass.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public Extent2D GetRenderAreaGranularity(RenderPass renderPass)
        {
            Extent2D pGranularity;
            unsafe
            {
                pGranularity = new Extent2D();
                Interop.NativeMethods.vkGetRenderAreaGranularity(this.m, renderPass != null ? renderPass.m : default(UInt64), &pGranularity);

                return pGranularity;
            }
        }


        public CommandPool CreateCommandPool(uint queueFamilyIndex, CommandPoolCreateFlags flags)
        {
            using (var createPoolInfo = new CommandPoolCreateInfo
            { 
                Flags = flags,
                QueueFamilyIndex = queueFamilyIndex
            })
            {
                return CreateCommandPool(createPoolInfo);
            }
        }

        public CommandPool CreateCommandPool(CommandPoolCreateInfo pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            CommandPool pCommandPool;
            unsafe
            {
                pCommandPool = new CommandPool(this);

                fixed (UInt64* ptrpCommandPool = &pCommandPool.m)
                {
                    result = Interop.NativeMethods.vkCreateCommandPool(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.CommandPoolCreateInfo*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpCommandPool);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pCommandPool;
            }
        }

        public void DestroyCommandPool(CommandPool commandPool = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyCommandPool(this.m, commandPool != null ? commandPool.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }


        public void ResetCommandPool(CommandPool commandPool, CommandPoolResetFlags flags = (CommandPoolResetFlags)0)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkResetCommandPool(this.m, commandPool != null ? commandPool.m : default(UInt64), flags);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        /// <summary>
        /// Create one CommandBuffer
        /// </summary>
        /// <param name="commandPool"></param>
        /// <returns></returns>
        public CommandBuffer AllocateCommandBuffer(CommandPool commandPool)
        {
            using (var commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                Level = CommandBufferLevel.Primary,
                CommandPool = commandPool,
                CommandBufferCount = 1
            })
            {
                var commandBuffers = this.AllocateCommandBuffers(commandBufferAllocateInfo);
                return commandBuffers[0];
            }
        }


        public CommandBuffer[] AllocateCommandBuffers(CommandBufferAllocateInfo pAllocateInfo)
        {
            return AllocateCommandBuffers<CommandBuffer>(pAllocateInfo, () => new CommandBuffer(pAllocateInfo.CommandPool));
        }

        public T[] AllocateCommandBuffers<T>(CommandBufferAllocateInfo pAllocateInfo, Func<T> createNewBufferFunc) where T : CommandBuffer
        {
            Result result;
            unsafe
            {
                if (pAllocateInfo.CommandBufferCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(IntPtr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpCommandBuffers = new NativeReference((int)(size * pAllocateInfo.CommandBufferCount));
                var ptrpCommandBuffers = refpCommandBuffers.Handle;
                result = Interop.NativeMethods.vkAllocateCommandBuffers(this.m, pAllocateInfo != null ? pAllocateInfo.m : (Interop.CommandBufferAllocateInfo*)default(IntPtr), (IntPtr*)ptrpCommandBuffers);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pAllocateInfo.CommandBufferCount <= 0)
                    return null;
                var arr = new T[pAllocateInfo.CommandBufferCount];
                for (int i = 0; i < pAllocateInfo.CommandBufferCount; i++)
                {
                    arr[i] = createNewBufferFunc();
                    arr[i].m = ((IntPtr*)ptrpCommandBuffers)[i];
                }

                return arr;

            }
        }

        public void FreeCommandBuffers(CommandPool commandPool, CommandBuffer[] pCommandBuffers)
        {
            unsafe
            {
                var arraypCommandBuffers = pCommandBuffers == null ? IntPtr.Zero : Marshal.AllocHGlobal(pCommandBuffers.Length * sizeof(IntPtr));
                var lenpCommandBuffers = pCommandBuffers == null ? 0 : pCommandBuffers.Length;
                if (pCommandBuffers != null)
                    for (int i = 0; i < pCommandBuffers.Length; i++)
                        ((IntPtr*)arraypCommandBuffers)[i] = (pCommandBuffers[i].m);
                Interop.NativeMethods.vkFreeCommandBuffers(this.m, commandPool != null ? commandPool.m : default(UInt64), (uint)lenpCommandBuffers, (IntPtr*)arraypCommandBuffers);
                Marshal.FreeHGlobal(arraypCommandBuffers);
            }
        }

        public void FreeCommandBuffer(CommandPool commandPool, CommandBuffer pCommandBuffer)
        {
            unsafe
            {
                fixed (IntPtr* ptrpCommandBuffer = &pCommandBuffer.m)
                {
                    Interop.NativeMethods.vkFreeCommandBuffers(this.m, commandPool != null ? commandPool.m : default(UInt64), (UInt32)(pCommandBuffer != null ? 1 : 0), ptrpCommandBuffer);
                }
            }
        }

        public SwapchainKhr[] CreateSharedSwapchainsKHR(SwapchainCreateInfoKhr[] pCreateInfos, AllocationCallbacks pAllocator = null)
        {
            Result result;
            unsafe
            {
                if (pCreateInfos.Length <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(UInt64));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpSwapchains = new NativeReference((int)(size * pCreateInfos.Length));
                var ptrpSwapchains = refpSwapchains.Handle;
                var arraypCreateInfos = pCreateInfos == null ? IntPtr.Zero : Marshal.AllocHGlobal(pCreateInfos.Length * sizeof(Interop.SwapchainCreateInfoKhr));
                var lenpCreateInfos = pCreateInfos == null ? 0 : pCreateInfos.Length;
                if (pCreateInfos != null)
                    for (int i = 0; i < pCreateInfos.Length; i++)
                        ((Interop.SwapchainCreateInfoKhr*)arraypCreateInfos)[i] = *(pCreateInfos[i].m);
                result = Interop.NativeMethods.vkCreateSharedSwapchainsKHR(this.m, (uint)lenpCreateInfos, (Interop.SwapchainCreateInfoKhr*)arraypCreateInfos, pAllocator != null ? pAllocator.m : null, (UInt64*)ptrpSwapchains);
                Marshal.FreeHGlobal(arraypCreateInfos);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pCreateInfos.Length <= 0)
                    return null;
                var arr = new SwapchainKhr[pCreateInfos.Length];
                for (int i = 0; i < pCreateInfos.Length; i++)
                {
                    arr[i] = new SwapchainKhr();
                    arr[i].m = ((UInt64*)ptrpSwapchains)[i];
                }

                return arr;

            }
        }

        public SwapchainKhr CreateSwapchainKHR(SwapchainCreateInfoKhr pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            SwapchainKhr pSwapchain;
            unsafe
            {
                pSwapchain = new SwapchainKhr();

                fixed (UInt64* ptrpSwapchain = &pSwapchain.m)
                {
                    result = Interop.NativeMethods.vkCreateSwapchainKHR(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.SwapchainCreateInfoKhr*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpSwapchain);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pSwapchain;
            }
        }

        public void DestroySwapchainKHR(SwapchainKhr swapchain = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroySwapchainKHR(this.m, swapchain != null ? swapchain.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public Image[] GetSwapchainImagesKHR(SwapchainKhr swapchain)
        {
            Result result;
            unsafe
            {
                UInt32 pSwapchainImageCount;
                result = Interop.NativeMethods.vkGetSwapchainImagesKHR(this.m, swapchain != null ? swapchain.m : default(UInt64), &pSwapchainImageCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pSwapchainImageCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(UInt64));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpSwapchainImages = new NativeReference((int)(size * pSwapchainImageCount));
                var ptrpSwapchainImages = refpSwapchainImages.Handle;
                result = Interop.NativeMethods.vkGetSwapchainImagesKHR(this.m, swapchain != null ? swapchain.m : default(UInt64), &pSwapchainImageCount, (UInt64*)ptrpSwapchainImages);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pSwapchainImageCount <= 0)
                    return null;
                var arr = new Image[pSwapchainImageCount];
                for (int i = 0; i < pSwapchainImageCount; i++)
                {
                    arr[i] = new Image();
                    arr[i].m = ((UInt64*)ptrpSwapchainImages)[i];
                }

                return arr;

            }
        }

        public UInt32 AcquireNextImageKHR(SwapchainKhr swapchain, UInt64 timeout, Semaphore semaphore = null, Fence fence = null)
        {
            Result result;
            UInt32 pImageIndex;
            unsafe
            {
                pImageIndex = new UInt32();
                result = Interop.NativeMethods.vkAcquireNextImageKHR(this.m, swapchain != null ? swapchain.m : default(UInt64), timeout, semaphore != null ? semaphore.m : default(UInt64), fence != null ? fence.m : default(UInt64), &pImageIndex);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pImageIndex;
            }
        }

        public void DebugMarkerSetObjectNameEXT(DebugMarkerObjectNameInfoExt pNameInfo)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkDebugMarkerSetObjectNameEXT(this.m, pNameInfo != null ? pNameInfo.m : (Interop.DebugMarkerObjectNameInfoExt*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void DebugMarkerSetObjectTagEXT(DebugMarkerObjectTagInfoExt pTagInfo)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkDebugMarkerSetObjectTagEXT(this.m, pTagInfo != null ? pTagInfo.m : (Interop.DebugMarkerObjectTagInfoExt*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public IndirectCommandsLayoutNvx CreateIndirectCommandsLayoutNVX(IndirectCommandsLayoutCreateInfoNvx pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            IndirectCommandsLayoutNvx pIndirectCommandsLayout;
            unsafe
            {
                pIndirectCommandsLayout = new IndirectCommandsLayoutNvx();

                fixed (UInt64* ptrpIndirectCommandsLayout = &pIndirectCommandsLayout.m)
                {
                    result = Interop.NativeMethods.vkCreateIndirectCommandsLayoutNVX(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.IndirectCommandsLayoutCreateInfoNvx*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpIndirectCommandsLayout);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pIndirectCommandsLayout;
            }
        }

        public void DestroyIndirectCommandsLayoutNVX(IndirectCommandsLayoutNvx indirectCommandsLayout, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyIndirectCommandsLayoutNVX(this.m, indirectCommandsLayout != null ? indirectCommandsLayout.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public ObjectTableNvx CreateObjectTableNVX(ObjectTableCreateInfoNvx pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            ObjectTableNvx pObjectTable;
            unsafe
            {
                pObjectTable = new ObjectTableNvx();

                fixed (UInt64* ptrpObjectTable = &pObjectTable.m)
                {
                    result = Interop.NativeMethods.vkCreateObjectTableNVX(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.ObjectTableCreateInfoNvx*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpObjectTable);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pObjectTable;
            }
        }

        public void DestroyObjectTableNVX(ObjectTableNvx objectTable, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyObjectTableNVX(this.m, objectTable != null ? objectTable.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public void RegisterObjectsNVX(ObjectTableNvx objectTable, ObjectTableEntryNvx[] ppObjectTableEntries, UInt32[] pObjectIndices)
        {
            Result result;
            unsafe
            {
                var arrayppObjectTableEntries = ppObjectTableEntries == null ? IntPtr.Zero : Marshal.AllocHGlobal(ppObjectTableEntries.Length * sizeof(ObjectTableEntryNvx));
                var lenppObjectTableEntries = ppObjectTableEntries == null ? 0 : ppObjectTableEntries.Length;
                if (ppObjectTableEntries != null)
                    for (int i = 0; i < ppObjectTableEntries.Length; i++)
                        ((ObjectTableEntryNvx*)arrayppObjectTableEntries)[i] = (ppObjectTableEntries[i]);
                var arraypObjectIndices = pObjectIndices == null ? IntPtr.Zero : Marshal.AllocHGlobal(pObjectIndices.Length * sizeof(UInt32));
                var lenpObjectIndices = pObjectIndices == null ? 0 : pObjectIndices.Length;
                if (pObjectIndices != null)
                    for (int i = 0; i < pObjectIndices.Length; i++)
                        ((UInt32*)arraypObjectIndices)[i] = (pObjectIndices[i]);
                result = Interop.NativeMethods.vkRegisterObjectsNVX(this.m, objectTable != null ? objectTable.m : default(UInt64), (uint)lenpObjectIndices, (ObjectTableEntryNvx*)arrayppObjectTableEntries, (UInt32*)arraypObjectIndices);
                Marshal.FreeHGlobal(arrayppObjectTableEntries);
                Marshal.FreeHGlobal(arraypObjectIndices);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void RegisterObjectsNVX(ObjectTableNvx objectTable, ObjectTableEntryNvx? ppObjectTableEntrie, UInt32? pObjectIndice)
        {
            Result result;
            unsafe
            {
                ObjectTableEntryNvx valppObjectTableEntrie = ppObjectTableEntrie ?? default(ObjectTableEntryNvx);
                ObjectTableEntryNvx* ptrppObjectTableEntrie = ppObjectTableEntrie != null ? &valppObjectTableEntrie : (ObjectTableEntryNvx*)IntPtr.Zero;
                UInt32 valpObjectIndice = pObjectIndice ?? default(UInt32);
                UInt32* ptrpObjectIndice = pObjectIndice != null ? &valpObjectIndice : (UInt32*)IntPtr.Zero;
                result = Interop.NativeMethods.vkRegisterObjectsNVX(this.m, objectTable != null ? objectTable.m : default(UInt64), (UInt32)(pObjectIndice != null ? 1 : 0), ptrppObjectTableEntrie, ptrpObjectIndice);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void UnregisterObjectsNVX(ObjectTableNvx objectTable, ObjectEntryTypeNvx[] pObjectEntryTypes, UInt32[] pObjectIndices)
        {
            Result result;
            unsafe
            {
                var arraypObjectEntryTypes = pObjectEntryTypes == null ? IntPtr.Zero : Marshal.AllocHGlobal(pObjectEntryTypes.Length * sizeof(ObjectEntryTypeNvx));
                var lenpObjectEntryTypes = pObjectEntryTypes == null ? 0 : pObjectEntryTypes.Length;
                if (pObjectEntryTypes != null)
                    for (int i = 0; i < pObjectEntryTypes.Length; i++)
                        ((ObjectEntryTypeNvx*)arraypObjectEntryTypes)[i] = (pObjectEntryTypes[i]);
                var arraypObjectIndices = pObjectIndices == null ? IntPtr.Zero : Marshal.AllocHGlobal(pObjectIndices.Length * sizeof(UInt32));
                var lenpObjectIndices = pObjectIndices == null ? 0 : pObjectIndices.Length;
                if (pObjectIndices != null)
                    for (int i = 0; i < pObjectIndices.Length; i++)
                        ((UInt32*)arraypObjectIndices)[i] = (pObjectIndices[i]);
                result = Interop.NativeMethods.vkUnregisterObjectsNVX(this.m, objectTable != null ? objectTable.m : default(UInt64), (uint)lenpObjectIndices, (ObjectEntryTypeNvx*)arraypObjectEntryTypes, (UInt32*)arraypObjectIndices);
                Marshal.FreeHGlobal(arraypObjectEntryTypes);
                Marshal.FreeHGlobal(arraypObjectIndices);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void UnregisterObjectsNVX(ObjectTableNvx objectTable, ObjectEntryTypeNvx pObjectEntryType, UInt32? pObjectIndice)
        {
            Result result;
            unsafe
            {
                UInt32 valpObjectIndice = pObjectIndice ?? default(UInt32);
                UInt32* ptrpObjectIndice = pObjectIndice != null ? &valpObjectIndice : (UInt32*)IntPtr.Zero;
                result = Interop.NativeMethods.vkUnregisterObjectsNVX(this.m, objectTable != null ? objectTable.m : default(UInt64), (UInt32)(pObjectIndice != null ? 1 : 0), &pObjectEntryType, ptrpObjectIndice);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void TrimCommandPoolKHR(CommandPool commandPool, UInt32 flags = 0)
        {
            unsafe
            {
                Interop.NativeMethods.vkTrimCommandPoolKHR(this.m, commandPool != null ? commandPool.m : default(UInt64), flags);
            }
        }

        public IntPtr GetMemoryWin32HandleKHR(MemoryGetWin32HandleInfoKhr pGetWin32HandleInfo)
        {
            Result result;
            IntPtr pHandle;
            unsafe
            {
                pHandle = new IntPtr();
                result = Interop.NativeMethods.vkGetMemoryWin32HandleKHR(this.m, pGetWin32HandleInfo != null ? pGetWin32HandleInfo.m : (Interop.MemoryGetWin32HandleInfoKhr*)default(IntPtr), &pHandle);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pHandle;
            }
        }

        public MemoryWin32HandlePropertiesKhr GetMemoryWin32HandlePropertiesKHR(ExternalMemoryHandleTypeFlagsKhr handleType, IntPtr handle)
        {
            Result result;
            MemoryWin32HandlePropertiesKhr pMemoryWin32HandleProperties;
            unsafe
            {
                pMemoryWin32HandleProperties = new MemoryWin32HandlePropertiesKhr();
                result = Interop.NativeMethods.vkGetMemoryWin32HandlePropertiesKHR(this.m, handleType, handle, pMemoryWin32HandleProperties != null ? pMemoryWin32HandleProperties.m : (Interop.MemoryWin32HandlePropertiesKhr*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);

                return pMemoryWin32HandleProperties;
            }
        }

        public int GetMemoryFdKHR(MemoryGetFdInfoKhr pGetFdInfo)
        {
            Result result;
            int pFd;
            unsafe
            {
                pFd = new int();
                result = Interop.NativeMethods.vkGetMemoryFdKHR(this.m, pGetFdInfo != null ? pGetFdInfo.m : (Interop.MemoryGetFdInfoKhr*)default(IntPtr), &pFd);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pFd;
            }
        }

        public MemoryFdPropertiesKhr GetMemoryFdPropertiesKHR(ExternalMemoryHandleTypeFlagsKhr handleType, int fd)
        {
            Result result;
            MemoryFdPropertiesKhr pMemoryFdProperties;
            unsafe
            {
                pMemoryFdProperties = new MemoryFdPropertiesKhr();
                result = Interop.NativeMethods.vkGetMemoryFdPropertiesKHR(this.m, handleType, fd, pMemoryFdProperties != null ? pMemoryFdProperties.m : (Interop.MemoryFdPropertiesKhr*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);

                return pMemoryFdProperties;
            }
        }

        public IntPtr GetSemaphoreWin32HandleKHR(SemaphoreGetWin32HandleInfoKhr pGetWin32HandleInfo)
        {
            Result result;
            IntPtr pHandle;
            unsafe
            {
                pHandle = new IntPtr();
                result = Interop.NativeMethods.vkGetSemaphoreWin32HandleKHR(this.m, pGetWin32HandleInfo != null ? pGetWin32HandleInfo.m : (Interop.SemaphoreGetWin32HandleInfoKhr*)default(IntPtr), &pHandle);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pHandle;
            }
        }

        public int GetSemaphoreFdKHR(SemaphoreGetFdInfoKhr pGetFdInfo)
        {
            Result result;
            int pFd;
            unsafe
            {
                pFd = new int();
                result = Interop.NativeMethods.vkGetSemaphoreFdKHR(this.m, pGetFdInfo != null ? pGetFdInfo.m : (Interop.SemaphoreGetFdInfoKhr*)default(IntPtr), &pFd);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pFd;
            }
        }

        public void ImportSemaphoreFdKHR(ImportSemaphoreFdInfoKhr pImportSemaphoreFdInfo)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkImportSemaphoreFdKHR(this.m, pImportSemaphoreFdInfo != null ? pImportSemaphoreFdInfo.m : (Interop.ImportSemaphoreFdInfoKhr*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public IntPtr GetFenceWin32HandleKHR(FenceGetWin32HandleInfoKhr pGetWin32HandleInfo)
        {
            Result result;
            IntPtr pHandle;
            unsafe
            {
                pHandle = new IntPtr();
                result = Interop.NativeMethods.vkGetFenceWin32HandleKHR(this.m, pGetWin32HandleInfo != null ? pGetWin32HandleInfo.m : (Interop.FenceGetWin32HandleInfoKhr*)default(IntPtr), &pHandle);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pHandle;
            }
        }

        public int GetFenceFdKHR(FenceGetFdInfoKhr pGetFdInfo)
        {
            Result result;
            int pFd;
            unsafe
            {
                pFd = new int();
                result = Interop.NativeMethods.vkGetFenceFdKHR(this.m, pGetFdInfo != null ? pGetFdInfo.m : (Interop.FenceGetFdInfoKhr*)default(IntPtr), &pFd);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pFd;
            }
        }

        public void ImportFenceFdKHR(ImportFenceFdInfoKhr pImportFenceFdInfo)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkImportFenceFdKHR(this.m, pImportFenceFdInfo != null ? pImportFenceFdInfo.m : (Interop.ImportFenceFdInfoKhr*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void DisplayPowerControlEXT(DisplayKhr display, DisplayPowerInfoExt pDisplayPowerInfo)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkDisplayPowerControlEXT(this.m, display != null ? display.m : default(UInt64), pDisplayPowerInfo != null ? pDisplayPowerInfo.m : (Interop.DisplayPowerInfoExt*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public Fence RegisterDeviceEventEXT(DeviceEventInfoExt pDeviceEventInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Fence pFence;
            unsafe
            {
                pFence = new Fence(this);

                fixed (UInt64* ptrpFence = &pFence.m)
                {
                    result = Interop.NativeMethods.vkRegisterDeviceEventEXT(this.m, pDeviceEventInfo != null ? pDeviceEventInfo.m : (Interop.DeviceEventInfoExt*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpFence);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pFence;
            }
        }

        public Fence RegisterDisplayEventEXT(DisplayKhr display, DisplayEventInfoExt pDisplayEventInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            Fence pFence;
            unsafe
            {
                pFence = new Fence(this);

                fixed (UInt64* ptrpFence = &pFence.m)
                {
                    result = Interop.NativeMethods.vkRegisterDisplayEventEXT(this.m, display != null ? display.m : default(UInt64), pDisplayEventInfo != null ? pDisplayEventInfo.m : (Interop.DisplayEventInfoExt*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpFence);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pFence;
            }
        }

        public UInt64 GetSwapchainCounterEXT(SwapchainKhr swapchain, SurfaceCounterFlagsExt counter)
        {
            Result result;
            UInt64 pCounterValue;
            unsafe
            {
                pCounterValue = new UInt64();
                result = Interop.NativeMethods.vkGetSwapchainCounterEXT(this.m, swapchain != null ? swapchain.m : default(UInt64), counter, &pCounterValue);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pCounterValue;
            }
        }

        public PeerMemoryFeatureFlagsKhx GetGroupPeerMemoryFeaturesKHX(UInt32 heapIndex, UInt32 localDeviceIndex, UInt32 remoteDeviceIndex)
        {
            PeerMemoryFeatureFlagsKhx pPeerMemoryFeatures;
            unsafe
            {
                pPeerMemoryFeatures = new PeerMemoryFeatureFlagsKhx();
                Interop.NativeMethods.vkGetDeviceGroupPeerMemoryFeaturesKHX(this.m, heapIndex, localDeviceIndex, remoteDeviceIndex, &pPeerMemoryFeatures);

                return pPeerMemoryFeatures;
            }
        }

        public void BindBufferMemory2KHR(BindBufferMemoryInfoKhr[] pBindInfos)
        {
            Result result;
            unsafe
            {
                var arraypBindInfos = pBindInfos == null ? IntPtr.Zero : Marshal.AllocHGlobal(pBindInfos.Length * sizeof(Interop.BindBufferMemoryInfoKhr));
                var lenpBindInfos = pBindInfos == null ? 0 : pBindInfos.Length;
                if (pBindInfos != null)
                    for (int i = 0; i < pBindInfos.Length; i++)
                        ((Interop.BindBufferMemoryInfoKhr*)arraypBindInfos)[i] = *(pBindInfos[i].m);
                result = Interop.NativeMethods.vkBindBufferMemory2KHR(this.m, (uint)lenpBindInfos, (Interop.BindBufferMemoryInfoKhr*)arraypBindInfos);
                Marshal.FreeHGlobal(arraypBindInfos);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void BindBufferMemory2KHR(BindBufferMemoryInfoKhr pBindInfo)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkBindBufferMemory2KHR(this.m, (UInt32)(pBindInfo != null ? 1 : 0), pBindInfo != null ? pBindInfo.m : (Interop.BindBufferMemoryInfoKhr*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void BindImageMemory2KHR(BindImageMemoryInfoKhr[] pBindInfos)
        {
            Result result;
            unsafe
            {
                var arraypBindInfos = pBindInfos == null ? IntPtr.Zero : Marshal.AllocHGlobal(pBindInfos.Length * sizeof(Interop.BindImageMemoryInfoKhr));
                var lenpBindInfos = pBindInfos == null ? 0 : pBindInfos.Length;
                if (pBindInfos != null)
                    for (int i = 0; i < pBindInfos.Length; i++)
                        ((Interop.BindImageMemoryInfoKhr*)arraypBindInfos)[i] = *(pBindInfos[i].m);
                result = Interop.NativeMethods.vkBindImageMemory2KHR(this.m, (uint)lenpBindInfos, (Interop.BindImageMemoryInfoKhr*)arraypBindInfos);
                Marshal.FreeHGlobal(arraypBindInfos);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void BindImageMemory2KHR(BindImageMemoryInfoKhr pBindInfo)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkBindImageMemory2KHR(this.m, (UInt32)(pBindInfo != null ? 1 : 0), pBindInfo != null ? pBindInfo.m : (Interop.BindImageMemoryInfoKhr*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public DeviceGroupPresentCapabilitiesKhx GetGroupPresentCapabilitiesKHX()
        {
            Result result;
            DeviceGroupPresentCapabilitiesKhx pDeviceGroupPresentCapabilities;
            unsafe
            {
                pDeviceGroupPresentCapabilities = new DeviceGroupPresentCapabilitiesKhx();
                result = Interop.NativeMethods.vkGetDeviceGroupPresentCapabilitiesKHX(this.m, pDeviceGroupPresentCapabilities != null ? pDeviceGroupPresentCapabilities.m : (Interop.DeviceGroupPresentCapabilitiesKhx*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);

                return pDeviceGroupPresentCapabilities;
            }
        }

        public DeviceGroupPresentModeFlagsKhx GetGroupSurfacePresentModesKHX(SurfaceKhr surface)
        {
            Result result;
            DeviceGroupPresentModeFlagsKhx pModes;
            unsafe
            {
                pModes = new DeviceGroupPresentModeFlagsKhx();
                result = Interop.NativeMethods.vkGetDeviceGroupSurfacePresentModesKHX(this.m, surface != null ? surface.Handle : default(UInt64), &pModes);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pModes;
            }
        }

        public UInt32 AcquireNextImage2KHX(AcquireNextImageInfoKhx pAcquireInfo)
        {
            Result result;
            UInt32 pImageIndex;
            unsafe
            {
                pImageIndex = new UInt32();
                result = Interop.NativeMethods.vkAcquireNextImage2KHX(this.m, pAcquireInfo != null ? pAcquireInfo.m : (Interop.AcquireNextImageInfoKhx*)default(IntPtr), &pImageIndex);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pImageIndex;
            }
        }

        public DescriptorUpdateTemplateKhr CreateDescriptorUpdateTemplateKHR(DescriptorUpdateTemplateCreateInfoKhr pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            DescriptorUpdateTemplateKhr pDescriptorUpdateTemplate;
            unsafe
            {
                pDescriptorUpdateTemplate = new DescriptorUpdateTemplateKhr();

                fixed (UInt64* ptrpDescriptorUpdateTemplate = &pDescriptorUpdateTemplate.m)
                {
                    result = Interop.NativeMethods.vkCreateDescriptorUpdateTemplateKHR(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.DescriptorUpdateTemplateCreateInfoKhr*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpDescriptorUpdateTemplate);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pDescriptorUpdateTemplate;
            }
        }

        public void DestroyDescriptorUpdateTemplateKHR(DescriptorUpdateTemplateKhr descriptorUpdateTemplate = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyDescriptorUpdateTemplateKHR(this.m, descriptorUpdateTemplate != null ? descriptorUpdateTemplate.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public void UpdateDescriptorSetWithTemplateKHR(DescriptorSet descriptorSet, DescriptorUpdateTemplateKhr descriptorUpdateTemplate, IntPtr pData)
        {
            unsafe
            {
                Interop.NativeMethods.vkUpdateDescriptorSetWithTemplateKHR(this.m, descriptorSet != null ? descriptorSet.m : default(UInt64), descriptorUpdateTemplate != null ? descriptorUpdateTemplate.m : default(UInt64), pData);
            }
        }

        public void SetHdrMetadataEXT(SwapchainKhr[] pSwapchains, HdrMetadataExt[] pMetadata)
        {
            unsafe
            {
                var arraypSwapchains = pSwapchains == null ? IntPtr.Zero : Marshal.AllocHGlobal(pSwapchains.Length * sizeof(UInt64));
                var lenpSwapchains = pSwapchains == null ? 0 : pSwapchains.Length;
                if (pSwapchains != null)
                    for (int i = 0; i < pSwapchains.Length; i++)
                        ((UInt64*)arraypSwapchains)[i] = (pSwapchains[i].m);
                var arraypMetadata = pMetadata == null ? IntPtr.Zero : Marshal.AllocHGlobal(pMetadata.Length * sizeof(Interop.HdrMetadataExt));
                var lenpMetadata = pMetadata == null ? 0 : pMetadata.Length;
                if (pMetadata != null)
                    for (int i = 0; i < pMetadata.Length; i++)
                        ((Interop.HdrMetadataExt*)arraypMetadata)[i] = *(pMetadata[i].m);
                Interop.NativeMethods.vkSetHdrMetadataEXT(this.m, (uint)lenpMetadata, (UInt64*)arraypSwapchains, (Interop.HdrMetadataExt*)arraypMetadata);
                Marshal.FreeHGlobal(arraypSwapchains);
                Marshal.FreeHGlobal(arraypMetadata);
            }
        }

        public void SetHdrMetadataEXT(SwapchainKhr pSwapchain, HdrMetadataExt pMetadata)
        {
            unsafe
            {
                fixed (UInt64* ptrpSwapchain = &pSwapchain.m)
                {
                    Interop.NativeMethods.vkSetHdrMetadataEXT(this.m, (UInt32)(pMetadata != null ? 1 : 0), ptrpSwapchain, pMetadata != null ? pMetadata.m : (Interop.HdrMetadataExt*)default(IntPtr));
                }
            }
        }

        public void GetSwapchainStatusKHR(SwapchainKhr swapchain)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkGetSwapchainStatusKHR(this.m, swapchain != null ? swapchain.m : default(UInt64));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public RefreshCycleDurationGoogle GetRefreshCycleDurationGOOGLE(SwapchainKhr swapchain)
        {
            Result result;
            RefreshCycleDurationGoogle pDisplayTimingProperties;
            unsafe
            {
                pDisplayTimingProperties = new RefreshCycleDurationGoogle();
                result = Interop.NativeMethods.vkGetRefreshCycleDurationGOOGLE(this.m, swapchain != null ? swapchain.m : default(UInt64), &pDisplayTimingProperties);
                if (result != Result.Success)
                    throw new ResultException(result);

                return pDisplayTimingProperties;
            }
        }

        public PastPresentationTimingGoogle[] GetPastPresentationTimingGOOGLE(SwapchainKhr swapchain)
        {
            Result result;
            unsafe
            {
                UInt32 pPresentationTimingCount;
                result = Interop.NativeMethods.vkGetPastPresentationTimingGOOGLE(this.m, swapchain != null ? swapchain.m : default(UInt64), &pPresentationTimingCount, null);
                if (result != Result.Success)
                    throw new ResultException(result);
                if (pPresentationTimingCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(PastPresentationTimingGoogle));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpPresentationTimings = new NativeReference((int)(size * pPresentationTimingCount));
                var ptrpPresentationTimings = refpPresentationTimings.Handle;
                result = Interop.NativeMethods.vkGetPastPresentationTimingGOOGLE(this.m, swapchain != null ? swapchain.m : default(UInt64), &pPresentationTimingCount, (PastPresentationTimingGoogle*)ptrpPresentationTimings);
                if (result != Result.Success)
                    throw new ResultException(result);

                if (pPresentationTimingCount <= 0)
                    return null;
                var arr = new PastPresentationTimingGoogle[pPresentationTimingCount];
                for (int i = 0; i < pPresentationTimingCount; i++)
                {
                    arr[i] = (((PastPresentationTimingGoogle*)ptrpPresentationTimings)[i]);
                }

                return arr;

            }
        }

        public MemoryRequirements2Khr GetBufferMemoryRequirements2KHR(BufferMemoryRequirementsInfo2Khr pInfo)
        {
            MemoryRequirements2Khr pMemoryRequirements;
            unsafe
            {
                pMemoryRequirements = new MemoryRequirements2Khr();
                Interop.NativeMethods.vkGetBufferMemoryRequirements2KHR(this.m, pInfo != null ? pInfo.m : (Interop.BufferMemoryRequirementsInfo2Khr*)default(IntPtr), pMemoryRequirements != null ? pMemoryRequirements.m : (Interop.MemoryRequirements2Khr*)default(IntPtr));

                return pMemoryRequirements;
            }
        }

        public MemoryRequirements2Khr GetImageMemoryRequirements2KHR(ImageMemoryRequirementsInfo2Khr pInfo)
        {
            MemoryRequirements2Khr pMemoryRequirements;
            unsafe
            {
                pMemoryRequirements = new MemoryRequirements2Khr();
                Interop.NativeMethods.vkGetImageMemoryRequirements2KHR(this.m, pInfo != null ? pInfo.m : (Interop.ImageMemoryRequirementsInfo2Khr*)default(IntPtr), pMemoryRequirements != null ? pMemoryRequirements.m : (Interop.MemoryRequirements2Khr*)default(IntPtr));

                return pMemoryRequirements;
            }
        }

        public SparseImageMemoryRequirements2Khr[] GetImageSparseMemoryRequirements2KHR(ImageSparseMemoryRequirementsInfo2Khr pInfo)
        {
            unsafe
            {
                UInt32 pSparseMemoryRequirementCount;
                Interop.NativeMethods.vkGetImageSparseMemoryRequirements2KHR(this.m, pInfo != null ? pInfo.m : (Interop.ImageSparseMemoryRequirementsInfo2Khr*)default(IntPtr), &pSparseMemoryRequirementCount, null);
                if (pSparseMemoryRequirementCount <= 0)
                    return null;

                int size = Marshal.SizeOf(typeof(Interop.SparseImageMemoryRequirements2Khr));
#pragma warning disable CA2000 // Dispose objects before losing scope
                var refpSparseMemoryRequirements = new NativeReference((int)(size * pSparseMemoryRequirementCount));
#pragma warning restore CA2000 // Dispose objects before losing scope
                var ptrpSparseMemoryRequirements = refpSparseMemoryRequirements.Handle;
                Interop.NativeMethods.vkGetImageSparseMemoryRequirements2KHR(this.m, pInfo != null ? pInfo.m : (Interop.ImageSparseMemoryRequirementsInfo2Khr*)default(IntPtr), &pSparseMemoryRequirementCount, (Interop.SparseImageMemoryRequirements2Khr*)ptrpSparseMemoryRequirements);

                if (pSparseMemoryRequirementCount <= 0)
                    return null;
                var arr = new SparseImageMemoryRequirements2Khr[pSparseMemoryRequirementCount];
                for (int i = 0; i < pSparseMemoryRequirementCount; i++)
                {
                    arr[i] = new SparseImageMemoryRequirements2Khr(new NativePointer(refpSparseMemoryRequirements, (IntPtr)(&((Interop.SparseImageMemoryRequirements2Khr*)ptrpSparseMemoryRequirements)[i])));
                }

                return arr;
            }
        }

        public SamplerYcbcrConversionKhr CreateSamplerYcbcrConversionKHR(SamplerYcbcrConversionCreateInfoKhr pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            SamplerYcbcrConversionKhr pYcbcrConversion;
            unsafe
            {
                pYcbcrConversion = new SamplerYcbcrConversionKhr();

                fixed (UInt64* ptrpYcbcrConversion = &pYcbcrConversion.m)
                {
                    result = Interop.NativeMethods.vkCreateSamplerYcbcrConversionKHR(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.SamplerYcbcrConversionCreateInfoKhr*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpYcbcrConversion);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pYcbcrConversion;
            }
        }

        public void DestroySamplerYcbcrConversionKHR(SamplerYcbcrConversionKhr ycbcrConversion = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroySamplerYcbcrConversionKHR(this.m, ycbcrConversion != null ? ycbcrConversion.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public ValidationCacheExt CreateValidationCacheEXT(ValidationCacheCreateInfoExt pCreateInfo, AllocationCallbacks pAllocator = null)
        {
            Result result;
            ValidationCacheExt pValidationCache;
            unsafe
            {
                pValidationCache = new ValidationCacheExt();

                fixed (UInt64* ptrpValidationCache = &pValidationCache.m)
                {
                    result = Interop.NativeMethods.vkCreateValidationCacheEXT(this.m, pCreateInfo != null ? pCreateInfo.m : (Interop.ValidationCacheCreateInfoExt*)default(IntPtr), pAllocator != null ? pAllocator.m : null, ptrpValidationCache);
                }
                if (result != Result.Success)
                    throw new ResultException(result);

                return pValidationCache;
            }
        }

        public void DestroyValidationCacheEXT(ValidationCacheExt validationCache = null, AllocationCallbacks pAllocator = null)
        {
            unsafe
            {
                Interop.NativeMethods.vkDestroyValidationCacheEXT(this.m, validationCache != null ? validationCache.m : default(UInt64), pAllocator != null ? pAllocator.m : null);
            }
        }

        public void GetValidationCacheDataEXT(ValidationCacheExt validationCache, out UIntPtr pDataSize, IntPtr pData = default(IntPtr))
        {
            Result result;
            unsafe
            {
                fixed (UIntPtr* ptrpDataSize = &pDataSize)
                {
                    result = Interop.NativeMethods.vkGetValidationCacheDataEXT(this.m, validationCache != null ? validationCache.m : default(UInt64), ptrpDataSize, pData);
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void MergeValidationCachesEXT(ValidationCacheExt dstCache, ValidationCacheExt[] pSrcCaches)
        {
            Result result;
            unsafe
            {
                var arraypSrcCaches = pSrcCaches == null ? IntPtr.Zero : Marshal.AllocHGlobal(pSrcCaches.Length * sizeof(UInt64));
                var lenpSrcCaches = pSrcCaches == null ? 0 : pSrcCaches.Length;
                if (pSrcCaches != null)
                    for (int i = 0; i < pSrcCaches.Length; i++)
                        ((UInt64*)arraypSrcCaches)[i] = (pSrcCaches[i].m);
                result = Interop.NativeMethods.vkMergeValidationCachesEXT(this.m, dstCache != null ? dstCache.m : default(UInt64), (uint)lenpSrcCaches, (UInt64*)arraypSrcCaches);
                Marshal.FreeHGlobal(arraypSrcCaches);
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void MergeValidationCachesEXT(ValidationCacheExt dstCache, ValidationCacheExt pSrcCache)
        {
            Result result;
            unsafe
            {
                fixed (UInt64* ptrpSrcCache = &pSrcCache.m)
                {
                    result = Interop.NativeMethods.vkMergeValidationCachesEXT(this.m, dstCache != null ? dstCache.m : default(UInt64), (UInt32)(pSrcCache != null ? 1 : 0), ptrpSrcCache);
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public int GetSwapchainGrallocUsageANDROID(Format format, ImageUsageFlags imageUsage)
        {
            Result result;
            int grallocUsage;
            unsafe
            {
                grallocUsage = new int();
                result = Interop.NativeMethods.vkGetSwapchainGrallocUsageANDROID(this.m, format, imageUsage, &grallocUsage);
                if (result != Result.Success)
                    throw new ResultException(result);

                return grallocUsage;
            }
        }

        public void AcquireImageANDROID(Image image, int nativeFenceFd, Semaphore semaphore, Fence fence)
        {
            Result result;
            unsafe
            {
                result = Interop.NativeMethods.vkAcquireImageANDROID(this.m, image != null ? image.m : default(UInt64), nativeFenceFd, semaphore != null ? semaphore.m : default(UInt64), fence != null ? fence.m : default(UInt64));
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public void GetShaderInfoAMD(Pipeline pipeline, ShaderStageFlags shaderStage, ShaderInfoTypeAmd infoType, out UIntPtr pInfoSize, IntPtr pInfo = default(IntPtr))
        {
            Result result;
            unsafe
            {
                fixed (UIntPtr* ptrpInfoSize = &pInfoSize)
                {
                    result = Interop.NativeMethods.vkGetShaderInfoAMD(this.m, pipeline != null ? pipeline.m : default(UInt64), shaderStage, infoType, ptrpInfoSize, pInfo);
                }
                if (result != Result.Success)
                    throw new ResultException(result);
            }
        }

        public MemoryHostPointerPropertiesExt GetMemoryHostPointerPropertiesEXT(ExternalMemoryHandleTypeFlagsKhr handleType, IntPtr pHostPointer)
        {
            Result result;
            MemoryHostPointerPropertiesExt pMemoryHostPointerProperties;
            unsafe
            {
                pMemoryHostPointerProperties = new MemoryHostPointerPropertiesExt();
                result = Interop.NativeMethods.vkGetMemoryHostPointerPropertiesEXT(this.m, handleType, pHostPointer, pMemoryHostPointerProperties != null ? pMemoryHostPointerProperties.m : (Interop.MemoryHostPointerPropertiesExt*)default(IntPtr));
                if (result != Result.Success)
                    throw new ResultException(result);

                return pMemoryHostPointerProperties;
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Destroy();
                _isDisposed = true;
            }
        }
    }
}
