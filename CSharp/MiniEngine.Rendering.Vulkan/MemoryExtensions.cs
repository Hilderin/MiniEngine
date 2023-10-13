using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MiniEngine.Rendering.Vulkan
{
    public unsafe static class MemoryExtensions
    {
        /// <summary>
        /// Find a memory type
        /// </summary>
        public static uint FindMemoryType(this VulkanInstance vi, uint typeFilter, MemoryPropertyFlags properties)
        {
            vi.Api.GetPhysicalDeviceMemoryProperties(vi.physicalDevice, out PhysicalDeviceMemoryProperties memProperties);

            for (int i = 0; i < memProperties.MemoryTypeCount; i++)
            {
                if ((typeFilter & (1 << i)) != 0 && (memProperties.MemoryTypes[i].PropertyFlags & properties) == properties)
                {
                    return (uint)i;
                }
            }

            throw new Exception("failed to find suitable memory type!");
        }

        

        /// <summary>
        /// Create a buffer
        /// </summary>
        public static void CreateBuffer(this VulkanInstance vi, ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties, ref Buffer buffer, ref DeviceMemory bufferMemory)
        {
            BufferCreateInfo bufferInfo = new()
            {
                SType = StructureType.BufferCreateInfo,
                Size = size,
                Usage = usage,
                SharingMode = SharingMode.Exclusive,
            };

            fixed (Buffer* bufferPtr = &buffer)
            {
                if (vi.Api.CreateBuffer(vi.device, bufferInfo, null, bufferPtr) != Result.Success)
                {
                    throw new Exception("failed to create vertex buffer!");
                }
            }

            MemoryRequirements memRequirements = new();
            vi.Api.GetBufferMemoryRequirements(vi.device, buffer, out memRequirements);

            MemoryAllocateInfo allocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = MemoryExtensions.FindMemoryType(vi, memRequirements.MemoryTypeBits, properties),
            };

            fixed (DeviceMemory* bufferMemoryPtr = &bufferMemory)
            {
                if (vi.Api.AllocateMemory(vi.device, allocateInfo, null, bufferMemoryPtr) != Result.Success)
                {
                    throw new Exception("failed to allocate vertex buffer memory!");
                }
            }

            vi.Api.BindBufferMemory(vi.device, buffer, bufferMemory, 0);
        }

        /// <summary>
        /// Copy a buffer
        /// </summary>
        public static void CopyBuffer(this VulkanInstance vi, Buffer srcBuffer, Buffer dstBuffer, ulong size)
        {
            CommandBuffer commandBuffer = vi.BeginSingleTimeCommands();

            BufferCopy copyRegion = new()
            {
                Size = size,
            };

            vi.Api.CmdCopyBuffer(commandBuffer, srcBuffer, dstBuffer, 1, copyRegion);

            vi.EndSingleTimeCommands(commandBuffer);
        }

    }
}
