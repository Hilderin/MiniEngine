using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace MiniEngine.Rendering.Vulkan
{

    /// <summary>
    /// Renderer for the meshes
    /// </summary>
    public unsafe class VkMeshRenderer : IDisposable
    {
        private VkRenderer _renderer;
        private VkShader _shader;
        private uint _drawCallsBufferIndex;
        private uint _drawCallsCountsOffset;
        private uint _maxDrawCall;

        private DrawCallManagementType _drawCallsBufferManagementType = DrawCallManagementType.ComputeDrawCallBufferPerWorkgroup;

        private BufferWrapper<DrawIndexedIndirectCommand> _drawCallsBuffer;
        private Dictionary<string, BufferWrapper> _shaderVariables;

        private PipelineWrapper _pipeline;
        private PipelineDescriptorSet _descriptorSet;
        private uint _nb_meshlets = 0;

        private Dictionary<int, uint> _imageSamplerBindlessIndex = new Dictionary<int, uint>();
        private ConcurrentDictionary<uint, MeshLetInstance> _meshLetInstances = new ConcurrentDictionary<uint, MeshLetInstance>();
        private List<MeshLetInstance> _availableMeshLetInstances = new List<MeshLetInstance>();
        private ConcurrentDictionary<uint, ObjectIndexInfo> _objectIndexInfos = new ConcurrentDictionary<uint, ObjectIndexInfo>();
        private uint _lastObjectIndexInfosIndex = 0;

        private uint _nbWorkGroupMax;
        private uint _drawCallsBufferElementSize;
        private uint _drawCallsCountsElementSize;
        private CommandBuffer[] _secondaryCommandBuffers;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkMeshRenderer(VkShader shader, VkRenderer renderer)
        {

            _renderer = renderer;
            _shader = shader;


            _pipeline = _renderer.GetPipeline(_shader);

            _descriptorSet = _pipeline.CreateDescriptorSet();

            //Render buffers...
            _descriptorSet.SetRendererBuffers();

            //Custom name...
            foreach (string name in _descriptorSet.GetNames().Where(n => !n.StartsWith("_")))
            {
                if (_shaderVariables == null)
                    _shaderVariables = new Dictionary<string, BufferWrapper>();

                uint size = _descriptorSet.GetSize(name);
                //TODO: Dynamic buffer resize...
                var buffer = new BufferWrapper(_renderer, Math.RoundUp(size * 100, 16), BufferUsageFlags.UniformBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);

                _shaderVariables.Add(name, buffer);
                _descriptorSet.Set(name, buffer);
            }



            _nbWorkGroupMax = _renderer.MaxComputeWorkgroupSize[0];

            //TODO: To allocate base on some graphic memory            
            _drawCallsCountsElementSize = _renderer.DrawCallsCountsBuffer.ElementSize;

            //Each workgroup will have less draw calls available...
            _drawCallsBuffer = _renderer.CreateDrawCallsBuffer(_nbWorkGroupMax, out _drawCallsBufferIndex, out _drawCallsCountsOffset);
            _drawCallsBufferElementSize = _drawCallsBuffer.ElementSize;
            _maxDrawCall = _drawCallsBuffer.Size / _drawCallsBufferElementSize / _nbWorkGroupMax;



            _renderer.AddActionsBeforeNextFrame(InitSecondaryCommandBuffers);

        }



        /// <summary>
        /// Add a meshlet to render and return the MeshLetInstanceIndex
        /// </summary>
        public uint AddMeshLetInstance(uint objectIndex, VkMaterial mat, ref VkMeshletData meshLet)
        {
            MeshLetInstance meshLetInstance = null;

            //If we have an availale slot we will use it...
            if (_availableMeshLetInstances.Count > 0)
            {
                lock (_availableMeshLetInstances)
                {
                    for (int i = 0; i < _availableMeshLetInstances.Count; i++)
                    {
                        if (_availableMeshLetInstances[i].AvailableMeshLetCount >= meshLet.NbMeshLets)
                        {
                            meshLetInstance = _availableMeshLetInstances[i];
                            _availableMeshLetInstances.RemoveAt(i);
                            break;
                        }

                    }
                }
            }

            if (meshLetInstance == null)
                meshLetInstance = new MeshLetInstance();

            MeshLetInstanceData[] instanceDatas = new MeshLetInstanceData[meshLet.NbMeshLets];

            uint textureIndex = uint.MaxValue;
            if (mat.VkDiffuseTexture != null)
                textureIndex = GetOrAddBindlessIndex(mat.VkDiffuseTexture.ImageWrapper.ImageView, _renderer.DefaultSampler);

            for (int i = 0; i < meshLet.NbMeshLets; i++)
            {
                instanceDatas[i].ObjectIndex = objectIndex;
                instanceDatas[i].MeshLetIndex = meshLet.FirstMeshLetIndex + (uint)i;
                instanceDatas[i].DrawCallsBufferIndex = _drawCallsBufferIndex;
                instanceDatas[i].TextureIndex = textureIndex;
            }


            //Reserve the space for the MeshLetInstanceData...
            if (meshLetInstance.BufferOffset == uint.MaxValue)
            {
                //New instance...
                meshLetInstance.BufferOffset = _renderer.MeshLetInstancesBuffer.Reserve(meshLet.NbMeshLets, out meshLetInstance.MeshLetFirstInstanceIndex);
                meshLetInstance.AvailableMeshLetCount = meshLet.NbMeshLets;     //Number in the buffer available
            }


            //Keep some informations on the object...
            meshLetInstance.ObjectIndex = objectIndex;
            meshLetInstance.NbMeshLets = meshLet.NbMeshLets;

            //And now that all the informations on MeshLetInstance are calculated, we can upload to the GPU...
            _renderer.MeshLetInstancesBuffer.Update(instanceDatas, meshLetInstance.BufferOffset);



            _meshLetInstances.TryAdd(meshLetInstance.MeshLetFirstInstanceIndex, meshLetInstance);
            lock (_meshLetInstances)
                _nb_meshlets = (uint)_meshLetInstances.Count;

            //Adding the object...
            if (!_objectIndexInfos.TryGetValue(objectIndex, out var objectIndexInfo))
            {
                objectIndexInfo = new ObjectIndexInfo();
                lock (_objectIndexInfos)
                {
                    objectIndexInfo.ShaderVariableBufferIndex = _lastObjectIndexInfosIndex++;
                }
                objectIndexInfo.MeshletInstanceCount = 1;
                _objectIndexInfos.TryAdd(objectIndex, objectIndexInfo);
            }
            else
            {
                objectIndexInfo.MeshletInstanceCount++;
            }

            return meshLetInstance.MeshLetFirstInstanceIndex;

        }


        /// <summary>
        /// Remove a mesh instance from the scene
        /// </summary>
        public void RemoveMeshInstance(uint meshLetInstanceIndex)
        {
            MeshLetInstance meshLetInstance = _meshLetInstances[meshLetInstanceIndex];


            //Removing the object...
            if (_objectIndexInfos.TryGetValue(meshLetInstance.ObjectIndex, out var objectIndexInfo))
            {
                objectIndexInfo.MeshletInstanceCount--;
                if (objectIndexInfo.MeshletInstanceCount <= 0)
                    _objectIndexInfos.TryRemove(meshLetInstance.ObjectIndex, out _);
            }

            _meshLetInstances.TryRemove(meshLetInstanceIndex, out _);
            lock (_meshLetInstances)
                _nb_meshlets = (uint)_meshLetInstances.Count;

            //Reset everything...
            MeshLetInstanceData[] instanceDatas = new MeshLetInstanceData[meshLetInstance.NbMeshLets];
            for (int i = 0; i < meshLetInstance.NbMeshLets; i++)
            {
                instanceDatas[i].ObjectIndex = uint.MaxValue;
                instanceDatas[i].MeshLetIndex = uint.MaxValue;
                instanceDatas[i].DrawCallsBufferIndex = uint.MaxValue;
                instanceDatas[i].TextureIndex = uint.MaxValue;
            }
            
            _renderer.MeshLetInstancesBuffer.Update(instanceDatas, meshLetInstance.BufferOffset);

            meshLetInstance.NbMeshLets = 0;
            meshLetInstance.ObjectIndex = uint.MaxValue;
            lock(_availableMeshLetInstances)
                _availableMeshLetInstances.Add(meshLetInstance);
        }

        /// <summary>
        /// Set a variable for an object
        /// </summary>
        public unsafe void SetShaderVariable<T>(uint objectIndex, string name, T value)
        {
            if (!_shaderVariables.TryGetValue(name, out var buffer))
                throw new InvalidOperationException($"Shader variable does not exists: {name}");

            if (_objectIndexInfos.TryGetValue(objectIndex, out var objectIndexInfo))
            {
                buffer.Update(&value, objectIndexInfo.ShaderVariableBufferIndex, VkSizeOfHelper.SizeOf<T>());
            }
            else
                throw new InvalidOperationException($"ObjectIndex does not exists: {objectIndex}");
        }

        /// <summary>
        /// Render the mesh
        /// </summary>
        public void PopulateCommandBuffers(CommandBuffer commandBuffer)
        {
            if (_nb_meshlets == 0)
                return;


            //We create a draw indexed indirect per workgroup....
            commandBuffer.CmdExecuteCommand(_secondaryCommandBuffers[((RenderCommandBuffer)commandBuffer).ImageIndex]);

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


        /// <summary>
        /// Init the secondary command buffers
        /// </summary>
        private void InitSecondaryCommandBuffers()
        {

            _secondaryCommandBuffers = _renderer.Swapchain.CreateSecondaryCommandBuffers();

            for (int ib = 0; ib < _secondaryCommandBuffers.Length; ib++)
            {
                _secondaryCommandBuffers[ib].Begin();

                _secondaryCommandBuffers[ib].CmdBindVertexBuffer(0, _renderer.VerticesBuffer, 0);
                _secondaryCommandBuffers[ib].CmdBindIndexBuffer(_renderer.IndicesBuffer, 0, IndexType.Uint16);

                _secondaryCommandBuffers[ib].CmdBindPipeline(PipelineBindPoint.Graphics, _pipeline.Pipeline);

                //commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _pipeline.GetBindlessDescriptorSet().DescriptorSets, null);
                _secondaryCommandBuffers[ib].CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _descriptorSet.DescriptorSets, null);


                uint max = _nbWorkGroupMax;
                for (uint i = 0; i < max; i++)
                {
                    _secondaryCommandBuffers[ib].CmdDrawIndexedIndirectCount(_drawCallsBuffer.Buffer, _drawCallsBufferElementSize * _maxDrawCall * i, _renderer.DrawCallsCountsBuffer, _drawCallsCountsOffset + (_drawCallsCountsElementSize * i), _maxDrawCall, _drawCallsBufferElementSize);
                }
                _secondaryCommandBuffers[ib].End();
            }
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





        private class IndirectCommand
        {
            public DrawIndexedIndirectCommand Command;
            public uint DrawCallIndex = uint.MaxValue;
            public uint BufferOffset = uint.MaxValue;
            public MeshLetInstance MeshLetInstance;
        }

        private class MeshLetInstance
        {
            public uint MeshLetFirstInstanceIndex = uint.MaxValue;
            public uint BufferOffset = uint.MaxValue;
            public uint AvailableMeshLetCount;
            public uint NbMeshLets;
            public uint ObjectIndex;
            //public MeshLetInstanceData InstanceData;
        }

        private class ObjectIndexInfo
        {
            public uint MeshletInstanceCount;
            public uint ShaderVariableBufferIndex;

        }

        private enum DrawCallManagementType
        {
            ComputeAdjustDrawCountOnly,
            ComputeCreatePackedDrawCallsBuffer,
            ComputeDrawCallBufferPerWorkgroup
        }

    }



}
