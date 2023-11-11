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
        private const int NB_MESHLET_MAX = 1024 * 1024;

        private VkRenderer _renderer;
        private VkShader _shader;

        private BufferWrapper _meshLetInstancesBuffer;
        private BufferWrapper _indirectDrawBuffer;

        private PipelineWrapper _pipeline;
        private PipelineDescriptorSet _descriptorSet;
        private uint _drawCount = 0;

        private Dictionary<int, uint> _imageSamplerBindlessIndex = new Dictionary<int, uint>();
        private List<MeshLetInstance> _meshLetInstances = new List<MeshLetInstance>();

        /// <summary>
        /// Constructor
        /// </summary>
        public VkMeshRenderer(VkShader shader, VkRenderer renderer)
        {

            _renderer = renderer;
            _shader = shader;

            //TODO: To allocate base on some graphic memory
            _indirectDrawBuffer = _renderer.CreateBufferWrapper<DrawIndexedIndirectCommand>(NB_MESHLET_MAX, BufferUsageFlags.IndirectBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);
            _meshLetInstancesBuffer = _renderer.CreateBufferWrapper<MeshletData>(NB_MESHLET_MAX, BufferUsageFlags.StorageBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);

            _pipeline = _renderer.GetPipeline(_shader);

            _descriptorSet = _pipeline.CreateDescriptorSet();
            _descriptorSet.Set("_objects", _renderer.ObjectsBuffer);
            //_descriptorSet.Set("_meshlets", _renderer.MeshLetsBuffer);
            _descriptorSet.Set("_meshlet_instances", _meshLetInstancesBuffer);
        }

        /// <summary>
        /// Add a meshlet to render
        /// </summary>
        public void AddMeshLetInstance(uint objectIndex, VkMaterial mat, ref Meshlet meshLet)
        {

            MeshLetInstance meshLetInstance = new MeshLetInstance();

            meshLetInstance.InstanceData.ObjectIndex = objectIndex;
            meshLetInstance.InstanceData.MeshLetIndex = meshLet.MeshLetIndex;
            meshLetInstance.InstanceData.TextureIndex = GetOrAddBindlessIndex(mat.VkDiffuseTexture.ImageWrapper.ImageView, _renderer.DefaultSampler);
            meshLetInstance.MeshLetIndex = _meshLetInstancesBuffer.Append(ref meshLetInstance.InstanceData) / _meshLetInstancesBuffer.SizeOf<MeshLetInstanceData>();

            _meshLetInstances.Add(meshLetInstance);


            //New command...
            var indirectCommand = new IndirectCommand();
            indirectCommand.Command.IndexCount = (uint)meshLet.NbIndices;
            indirectCommand.Command.InstanceCount = 1;
            indirectCommand.Command.FirstIndex = (uint)meshLet.IndexBufferIndex;
            indirectCommand.Command.VertexOffset = (int)meshLet.VertexBufferIndex;
            indirectCommand.Command.FirstInstance = meshLetInstance.MeshLetIndex;           //Important so the gl_InstanceIndex will correspond to index in _meshlet_instances buffer

            indirectCommand.MeshLetInstance = meshLetInstance;

            indirectCommand.BufferOffset = _indirectDrawBuffer.Append(ref indirectCommand.Command);

            _drawCount++;


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
            commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _descriptorSet.DescriptorSets, null);


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


            commandBuffer.CmdDrawIndexedIndirect(_indirectDrawBuffer.Buffer, 0, _drawCount, _indirectDrawBuffer.SizeOf<DrawIndexedIndirectCommand>());
        }



        /// <summary>
        /// Get or Add an bindless index for an image and a sampler
        /// </summary>
        private uint GetOrAddBindlessIndex(ImageView imageView, Sampler sampler)
        {
            int key = BytesHelper.CombineHash(imageView.GetHashCode(), sampler.GetHashCode());

            lock (_imageSamplerBindlessIndex)
            {
                if (!_imageSamplerBindlessIndex.TryGetValue(key, out uint index))
                {
                    index = (uint)_imageSamplerBindlessIndex.Count;
                    _imageSamplerBindlessIndex.Add(key, index);

                    _descriptorSet.Set(ShaderVariableNames.SamplerDiffuse, imageView, sampler, index);
                }
                return index;
            }
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
            public MeshLetInstance MeshLetInstance;
        }

        private class MeshLetInstance
        {
            public uint MeshLetIndex;
            public MeshLetInstanceData InstanceData;
        }

    }



}
