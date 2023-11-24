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
        private uint _drawCount = 0;
        private uint _nb_meshlets = 0;

        private Dictionary<int, uint> _imageSamplerBindlessIndex = new Dictionary<int, uint>();
        private ConcurrentDictionary<uint, MeshLetInstance> _meshLetInstances = new ConcurrentDictionary<uint, MeshLetInstance>();
        private ConcurrentDictionary<uint, IndirectCommand> _drawCallsPerMeshLetInstance = new ConcurrentDictionary<uint, IndirectCommand>();
        private ConcurrentQueue<MeshLetInstance> _availableMeshLetInstances = new ConcurrentQueue<MeshLetInstance>();
        private ConcurrentQueue<IndirectCommand> _availableDrawCalls = new ConcurrentQueue<IndirectCommand>();
        private ConcurrentDictionary<uint, ObjectIndexInfo> _objectIndexInfos = new ConcurrentDictionary<uint, ObjectIndexInfo>();
        private uint _lastObjectIndexInfosIndex = 0;

        private uint _nbWorkGroup;
        private uint _drawCallsBufferElementSize;
        private uint _drawCallsCountsElementSize;


        /// <summary>
        /// Constructor
        /// </summary>
        public VkMeshRenderer(VkShader shader, VkRenderer renderer)
        {

            _renderer = renderer;
            _shader = shader;
            _nbWorkGroup = _renderer.MaxComputeWorkgroupSize[0];

            //TODO: To allocate base on some graphic memory            
            _drawCallsCountsElementSize = _renderer.DrawCallsCountsBuffer.ElementSize;

            if (_drawCallsBufferManagementType == DrawCallManagementType.ComputeDrawCallBufferPerWorkgroup)
            {
                //Each workgroup will have less draw calls available...
                _drawCallsBuffer = _renderer.CreateDrawCallsBuffer(_nbWorkGroup, out _drawCallsBufferIndex, out _drawCallsCountsOffset);
                _drawCallsBufferElementSize = _drawCallsBuffer.ElementSize;
                _maxDrawCall = _drawCallsBuffer.Size / _drawCallsBufferElementSize / _nbWorkGroup;

                //We will need a count per workgroup
                
            }
            else
            {
                _drawCallsBuffer = _renderer.CreateDrawCallsBuffer(1, out _drawCallsBufferIndex, out _drawCallsCountsOffset);
                _drawCallsBufferElementSize = _drawCallsBuffer.ElementSize;
                _maxDrawCall = _drawCallsBuffer.Size / _drawCallsBufferElementSize;
            }


            


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



        }


        /// <summary>
        /// Add a meshlet to render and return the MeshLetInstanceIndex
        /// </summary>
        public uint AddMeshLetInstance(uint objectIndex, VkMaterial mat, ref VkMeshlet meshLet)
        {

            MeshLetInstance meshLetInstance;

            //If we have an availale slot we will use it...
            if (!_availableMeshLetInstances.TryDequeue(out meshLetInstance))
                meshLetInstance = new MeshLetInstance();

            meshLetInstance.InstanceData.ObjectIndex = objectIndex;
            meshLetInstance.InstanceData.MeshLetIndex = meshLet.MeshLetIndex;
            meshLetInstance.InstanceData.DrawCallsBufferIndex = _drawCallsBufferIndex;
            if (mat.VkDiffuseTexture != null)
                meshLetInstance.InstanceData.TextureIndex = GetOrAddBindlessIndex(mat.VkDiffuseTexture.ImageWrapper.ImageView, _renderer.DefaultSampler);


            //Reserve the space for the MeshLetInstanceData...
            if (meshLetInstance.BufferOffset == uint.MaxValue)
                //New instance...
                meshLetInstance.BufferOffset = _renderer.MeshLetInstancesBuffer.Reserve(out meshLetInstance.MeshLetInstanceIndex);


            _meshLetInstances.TryAdd(meshLetInstance.MeshLetInstanceIndex, meshLetInstance);
            lock(_meshLetInstances)
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



            //Draw call....
            IndirectCommand indirectCommand;
            if (!_availableDrawCalls.TryDequeue(out indirectCommand))
                indirectCommand = new IndirectCommand();

            indirectCommand.Command.IndexCount = (uint)meshLet.NbIndices;
            indirectCommand.Command.InstanceCount = 1;
            indirectCommand.Command.FirstIndex = (uint)meshLet.IndexBufferIndex;
            indirectCommand.Command.VertexOffset = (int)meshLet.VertexBufferIndex;
            indirectCommand.Command.FirstInstance = meshLetInstance.MeshLetInstanceIndex;           //Important so the gl_InstanceIndex will correspond to index in _meshlet_instances buffer

            indirectCommand.MeshLetInstance = meshLetInstance;

            //Updating the meshlet instance data with the draw call index at the same time...
            if (_drawCallsBufferManagementType != DrawCallManagementType.ComputeDrawCallBufferPerWorkgroup)
            {
                if (indirectCommand.BufferOffset == uint.MaxValue)
                {
                    //new draw call...
                    if (_drawCallsBufferManagementType == DrawCallManagementType.ComputeCreatePackedDrawCallsBuffer)
                        //We need to reserve the space for the compute to add the draw call command
                        indirectCommand.BufferOffset = _drawCallsBuffer.Reserve(out indirectCommand.DrawCallIndex);
                    else
                        //We need to add the draw call command
                        indirectCommand.BufferOffset = _drawCallsBuffer.Append(ref indirectCommand.Command, out indirectCommand.DrawCallIndex);
                }
                else if (_drawCallsBufferManagementType == DrawCallManagementType.ComputeAdjustDrawCountOnly)
                    //Update the draw call buffer...
                    _drawCallsBuffer.Update(ref indirectCommand.Command, indirectCommand.BufferOffset);
            }

            //And now that all the informations on MeshLetInstance are calculated, we can upload to the GPU...
            meshLetInstance.InstanceData.DrawCallIndex = indirectCommand.DrawCallIndex;
            _renderer.MeshLetInstancesBuffer.Update(ref meshLetInstance.InstanceData, meshLetInstance.BufferOffset);

            _drawCallsPerMeshLetInstance.TryAdd(meshLetInstance.MeshLetInstanceIndex, indirectCommand);

            if (_drawCallsBufferManagementType != DrawCallManagementType.ComputeDrawCallBufferPerWorkgroup)
            {
                _drawCount++;
                _renderer.DrawCallsCountsBuffer.Update(ref _drawCount, _drawCallsCountsOffset);
            }

            return meshLetInstance.MeshLetInstanceIndex;

        }

        /// <summary>
        /// Remove a mesh instance from the scene
        /// </summary>
        public void UpdateMeshInstance(uint meshLetInstanceIndex, VkMaterial mat, ref VkMeshlet meshLet)
        {
            MeshLetInstance meshLetInstance = _meshLetInstances[meshLetInstanceIndex];
            IndirectCommand indirectCommand = _drawCallsPerMeshLetInstance[meshLetInstanceIndex];

            //Update the mesh instance...
            meshLetInstance.InstanceData.MeshLetIndex = meshLet.MeshLetIndex;
            if (mat.VkDiffuseTexture != null)
                meshLetInstance.InstanceData.TextureIndex = GetOrAddBindlessIndex(mat.VkDiffuseTexture.ImageWrapper.ImageView, _renderer.DefaultSampler);
            else
                meshLetInstance.InstanceData.TextureIndex = 0;
            _renderer.MeshLetInstancesBuffer.Update(ref meshLetInstance.InstanceData, meshLetInstance.BufferOffset);

            if (_drawCallsBufferManagementType == DrawCallManagementType.ComputeAdjustDrawCountOnly)
            {
                //Update the draw call...
                indirectCommand.Command.IndexCount = (uint)meshLet.NbIndices;
                indirectCommand.Command.InstanceCount = 1;
                indirectCommand.Command.FirstIndex = (uint)meshLet.IndexBufferIndex;
                indirectCommand.Command.VertexOffset = (int)meshLet.VertexBufferIndex;
                _drawCallsBuffer.Update(ref indirectCommand.Command, indirectCommand.BufferOffset);
            }

        }

        /// <summary>
        /// Remove a mesh instance from the scene
        /// </summary>
        public void RemoveMeshInstance(uint meshLetInstanceIndex)
        {
            MeshLetInstance meshLetInstance = _meshLetInstances[meshLetInstanceIndex];
            IndirectCommand indirectCommand = _drawCallsPerMeshLetInstance[meshLetInstanceIndex];



            //Removing the object...
            if (_objectIndexInfos.TryGetValue(meshLetInstance.InstanceData.ObjectIndex, out var objectIndexInfo))
            {
                objectIndexInfo.MeshletInstanceCount--;
                if (objectIndexInfo.MeshletInstanceCount <= 0)
                    _objectIndexInfos.TryRemove(meshLetInstance.InstanceData.ObjectIndex, out _);
            }

            //Reset everything...
            meshLetInstance.InstanceData.ObjectIndex = uint.MaxValue;
            meshLetInstance.InstanceData.MeshLetIndex = uint.MaxValue;
            meshLetInstance.InstanceData.DrawCallsBufferIndex = uint.MaxValue;
            meshLetInstance.InstanceData.TextureIndex = uint.MaxValue;
            _renderer.MeshLetInstancesBuffer.Update(ref meshLetInstance.InstanceData, meshLetInstance.BufferOffset);

            if (_drawCallsBufferManagementType == DrawCallManagementType.ComputeAdjustDrawCountOnly)
            {
                //Update drawcall...
                indirectCommand.Command.InstanceCount = 0;
                indirectCommand.Command.InstanceCount = 0;
                indirectCommand.Command.FirstIndex = 0;
                indirectCommand.Command.VertexOffset = 0;
                _drawCallsBuffer.Update(ref indirectCommand.Command, indirectCommand.BufferOffset);
            }


            _meshLetInstances.TryRemove(meshLetInstanceIndex, out _);
            _drawCallsPerMeshLetInstance.TryRemove(meshLetInstanceIndex, out _);
            lock (_meshLetInstances)
                _nb_meshlets = (uint)_meshLetInstances.Count;

            _availableMeshLetInstances.Enqueue(meshLetInstance);
            _availableDrawCalls.Enqueue(indirectCommand);
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

            commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, _pipeline.Pipeline);

            //commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _pipeline.GetBindlessDescriptorSet().DescriptorSets, null);
            commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, _descriptorSet.DescriptorSets, null);


            //Matrix4 matrixVP = _renderer.MatrixViewProjection; // * _transform.GetMatrix();

            //Constants...
            _pipeline.UpdatePushConstants(commandBuffer);


            if (_drawCallsBufferManagementType == DrawCallManagementType.ComputeDrawCallBufferPerWorkgroup)
            {
                //We create a draw indexed indirect per workgroup....
                uint max = (_nb_meshlets > _nbWorkGroup ? _nbWorkGroup : _nb_meshlets);
                for (uint i = 0; i < max; i++)
                {
                    commandBuffer.CmdDrawIndexedIndirectCount(_drawCallsBuffer.Buffer, _drawCallsBufferElementSize * _maxDrawCall * i, _renderer.DrawCallsCountsBuffer, _drawCallsCountsOffset + (_drawCallsCountsElementSize * i), _maxDrawCall, _drawCallsBufferElementSize);
                }

            }
            else
            {
                commandBuffer.CmdDrawIndexedIndirectCount(_drawCallsBuffer.Buffer, 0, _renderer.DrawCallsCountsBuffer, _drawCallsCountsOffset, _maxDrawCall, _drawCallsBufferElementSize);
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
            public uint MeshLetInstanceIndex = uint.MaxValue;
            public uint BufferOffset = uint.MaxValue;
            public MeshLetInstanceData InstanceData;
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
