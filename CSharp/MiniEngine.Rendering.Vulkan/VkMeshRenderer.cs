using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Renderer for the meshes
    /// </summary>
    public unsafe class VkMeshRenderer : IDisposable, IRenderHandle
    {
        private VkRenderer _renderer;
        private VkShader _shader;

        private BufferWrapper _indirectDrawBuffer;

        private PipelineWrapper _pipeline;
        private Dictionary<uint, IndirectCommand> _commandsPerMesh = new Dictionary<uint, IndirectCommand>();
        private PipelineDescriptorSet _objectDataDescriptorSet;
        private uint _drawCount = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkMeshRenderer(VkShader shader, VkRenderer renderer)
        {

            _renderer = renderer;
            _shader = shader;

            //TODO: To allocate base on some graphic memory
            _indirectDrawBuffer = _renderer.CreateBufferWrapper((uint)(sizeof(DrawIndexedIndirectCommand) * 100), BufferUsageFlags.IndirectBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);

            _pipeline = _renderer.GetPipeline(_shader);

            _objectDataDescriptorSet = _pipeline.GetBindlessDescriptorSet();
            _objectDataDescriptorSet.Set("object_data_array", _renderer.ObjectsBuffer);
        }

        /// <summary>
        /// Add a mesh to render
        /// </summary>
        public void AddMesh(uint objectIndex, VkMaterial mat, ref VulkanMeshData meshData)
        {
            //New command...
            var indirectCommand = new IndirectCommand();
            indirectCommand.Command.IndexCount = (uint)meshData.NbIndices;
            indirectCommand.Command.InstanceCount = 1;
            indirectCommand.Command.FirstIndex = (uint)meshData.IndexBufferIndex;
            indirectCommand.Command.VertexOffset = (int)meshData.VertexBufferIndex;
            indirectCommand.Command.FirstInstance = objectIndex;

            indirectCommand.InstanceData.ObjectIndex = objectIndex;
            indirectCommand.InstanceData.BindlessDiffuseTextureIndex = _pipeline.GetOrAddBindlessIndex(mat.VkDiffuseTexture.ImageWrapper.ImageView, _renderer.DefaultSampler);

            indirectCommand.BufferOffset = _indirectDrawBuffer.Append(ref indirectCommand.Command);

            _drawCount++;

            //uint key = BytesHelper.CombineHash(instanceIndex, instanceMeshDataIndex);
            //if (!_commandsPerMesh.TryGetValue(instanceIndex, out var indirectCommand))
            //{

            //}
            //else
            //{
            //    //Update the instance count...
            //    indirectCommand.Command.InstanceCount++;
            //    _indirectDrawBuffer.Update(ref indirectCommand.Command, indirectCommand.BufferOffset);
            //}


        }

        /// <summary>
        /// Render the mesh
        /// </summary>
        public void PopulateCommandBuffers(CommandBuffer commandBuffer)
        {
            if (_drawCount == 0)
                return;

            commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, _pipeline.Pipeline);

            //commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _pipeline.GetBindlessDescriptorSet().DescriptorSets, null);
            commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _objectDataDescriptorSet.DescriptorSets, null);


            //Matrix4 matrixVP = _renderer.MatrixViewProjection; // * _transform.GetMatrix();





            //Constants...
            for (int iConst = 0; iConst < _shader.ShaderData.Constants.Length; iConst++)
            {
                var pushContant = _shader.ShaderData.Constants[iConst];

                switch (pushContant.Name)
                {
                    case ShaderVariableNames.MatrixVP:
                        commandBuffer.CmdPushConstants(_pipeline.PipelineLayout, pushContant.StageFlags, pushContant.Offset, ref _renderer.MatrixViewProjection);
                        break;

                    //case ShaderVariableNames.VertexBufferIndex:
                    //    commandBuffer.CmdPushConstants(pipeline.PipelineLayout, pushContant.StageFlags, pushContant.Offset, ref renderData.BindlessVertexBufferIndex);
                    //    break;

                    //case ShaderVariableNames.MaterialDiffuseIndex:
                    //    commandBuffer.CmdPushConstants(_pipeline.PipelineLayout, pushContant.StageFlags, pushContant.Offset, ref renderData.BindlessDiffuseTextureIndex);
                    //    break;

                    default:
                        Debug.Warning($"Constant not found: {pushContant.Name}");
                        break;
                }
            }


            commandBuffer.CmdDrawIndexedIndirect(_indirectDrawBuffer.Buffer, 0, _drawCount, (uint)Marshal.SizeOf<DrawIndexedIndirectCommand>());
        }

        /// <summary>
        /// Disposing of the mesh
        /// </summary>
        public void Dispose()
        {
            //We will not dispose pipelines because they can be used by another meshrenderer...
            _pipeline?.Dispose();
            _pipeline = null;
        }

        private class IndirectCommand
        {
            public DrawIndexedIndirectCommand Command;
            public uint BufferOffset;
            public InstanceData InstanceData;
        }

        private struct InstanceData
        {
            public uint ObjectIndex;
            public uint BindlessDiffuseTextureIndex;
        }

    }



}
