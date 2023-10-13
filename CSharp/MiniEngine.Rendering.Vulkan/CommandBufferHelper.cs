using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Helper for using the command buffer
    /// </summary>
    public unsafe static class CommandBufferHelper
    {
        /// <summary>
        /// Create a single time command buffer
        /// </summary>
        public static CommandBuffer BeginSingleTimeCommands(VulkanInstance vi)
        {
            CommandBufferAllocateInfo allocateInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                Level = CommandBufferLevel.Primary,
                CommandPool = vi.commandPool,
                CommandBufferCount = 1,
            };

            vi.VkApi.AllocateCommandBuffers(vi.device, allocateInfo, out CommandBuffer commandBuffer);

            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
            };

            vi.VkApi.BeginCommandBuffer(commandBuffer, beginInfo);

            return commandBuffer;
        }

        /// <summary>
        /// Execute and wait for the result of the command buffer
        /// </summary>
        public static void EndSingleTimeCommands(VulkanInstance vi, CommandBuffer commandBuffer)
        {
            vi.VkApi.EndCommandBuffer(commandBuffer);

            
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &commandBuffer,
            };

            vi.VkApi.QueueSubmit(vi.graphicsQueue, 1, submitInfo, default);
            vi.VkApi.QueueWaitIdle(vi.graphicsQueue);

            vi.VkApi.FreeCommandBuffers(vi.device, vi.commandPool, 1, commandBuffer);
        }

    }
}
