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
    public unsafe static class CommandBufferExtensions
    {
        /// <summary>
        /// Create a single time command buffer
        /// </summary>
        public static CommandBuffer BeginSingleTimeCommands(this VulkanInstance vi)
        {
            CommandBufferAllocateInfo allocateInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                Level = CommandBufferLevel.Primary,
                CommandPool = vi.commandPool,
                CommandBufferCount = 1,
            };

            vi.Api.AllocateCommandBuffers(vi.device, allocateInfo, out CommandBuffer commandBuffer);

            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
            };

            vi.Api.BeginCommandBuffer(commandBuffer, beginInfo);

            return commandBuffer;
        }

        /// <summary>
        /// Execute and wait for the result of the command buffer
        /// </summary>
        public static void EndSingleTimeCommands(this VulkanInstance vi, CommandBuffer commandBuffer)
        {
            vi.Api.EndCommandBuffer(commandBuffer);

            
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &commandBuffer,
            };

            vi.Api.QueueSubmit(vi.graphicsQueue, 1, submitInfo, default);
            vi.Api.QueueWaitIdle(vi.graphicsQueue);

            vi.Api.FreeCommandBuffers(vi.device, vi.commandPool, 1, commandBuffer);
        }

    }
}
