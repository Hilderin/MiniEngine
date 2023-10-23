using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    public unsafe static class QueueFamiliesExtensions
    {
        /// <summary>
        /// Find queue families for a device
        /// </summary>
        public static QueueFamilyIndices FindQueueFamilies(this VulkanInstance vi)
        {
            return FindQueueFamilies(vi, vi.physicalDevice);

        }

        /// <summary>
        /// Find queue families for a device
        /// </summary>
        public static QueueFamilyIndices FindQueueFamilies(this VulkanInstance vi, PhysicalDevice device)
        {
            var indices = new QueueFamilyIndices();

            uint queueFamilityCount = 0;
            vi.Api.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

            var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
            fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
            {
                vi.Api.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
            }


            uint i = 0;
            foreach (var queueFamily in queueFamilies)
            {
                if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    indices.GraphicsFamily = i;
                }

                vi.khrSurface.GetPhysicalDeviceSurfaceSupport(device, i, vi.surface, out var presentSupport);

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
    }
}
