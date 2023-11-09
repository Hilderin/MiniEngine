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
        private VkRenderer _vk;
        private VkMesh _mesh;
        private List<Material> _materials;
        private WorldTransform _transform;

        private RenderData[] _renderDatas;
        private BufferWrapper _indirectDrawBuffer;

        private bool _initialized = false;


        /// <summary>
        /// Constructor
        /// </summary>
        public VkMeshRenderer(Mesh mesh, List<Material> materials, WorldTransform transform, VkRenderer vi)
        {
            _mesh = (VkMesh)mesh;
            _materials = materials;
            _transform = transform;


            _vk = vi;


        }


        /// <summary>
        /// Render the mesh
        /// </summary>
        public void PopulateCommandBuffers(CommandBuffer commandBuffer)
        {
            if (!_initialized)
            {
                if (!Init())
                    return;
            }

            Matrix4 mvp = _vk.ViewProjectionMVPMatrix * _transform.GetMatrix();
            //Matrix4 mvpTransposed = Matrix4.Transpose(ref mvp);

            //Debug.Print("--------------");
            //Debug.Print("mvp:" + mvp.ToString());
            //foreach (var subMesh in _subMeshes)
            //{
            //    foreach (var vector in subMesh.Positions)
            //    {
            //        Vector4 vector4 = new Vector4(vector, 1);
            //        Vector4 vector4Res = mvp * vector4;
            //        Debug.Print(vector.ToString() + " => " + vector4Res.ToString());
            //    }
            //}

            PipelineWrapper lastPipeline = null;


            for (int i = 0; i < _renderDatas.Length; i++)
            {
                //not loaded?
                if (_renderDatas[i].Pipeline == null)
                    continue;

                if (lastPipeline != _renderDatas[i].Pipeline)
                {
                    //We have changed the pipeline...
                    commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, _renderDatas[i].Pipeline.Pipeline);
                    lastPipeline = _renderDatas[i].Pipeline;

                    //If bindless, we need to bind once for the pipeline, everything is in the GPU memory
                    if (_renderDatas[i].Pipeline.Bindless)
                        commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, _renderDatas[i].Pipeline.PipelineLayout, 0, _renderDatas[i].Pipeline.GetBindlessDescriptorSet().DescriptorSets, null);
                }


                PopulateCommandBuffer(commandBuffer, ref mvp, ref _mesh.MeshDatas[i], ref _renderDatas[i]);

            }
        }

        /// <summary>
        /// Populate and command buffer for a mesh
        /// </summary>
        private void PopulateCommandBuffer(CommandBuffer commandBuffer, ref Matrix4 mvp, ref VulkanMeshData meshData, ref RenderData renderData)
        {
            VkShader shader = renderData.Shader;
            PipelineWrapper pipeline = renderData.Pipeline;

            //Constants...
            for (int iConst = 0; iConst < shader.ShaderData.Constants.Length; iConst++)
            {
                var pushContant = shader.ShaderData.Constants[iConst];

                switch (pushContant.Name)
                {
                    case ShaderVariableNames.MatrixMVP:
                        commandBuffer.CmdPushConstants(pipeline.PipelineLayout, pushContant.StageFlags, pushContant.Offset, ref mvp);
                        break;

                    //case ShaderVariableNames.VertexBufferIndex:
                    //    commandBuffer.CmdPushConstants(pipeline.PipelineLayout, pushContant.StageFlags, pushContant.Offset, ref renderData.BindlessVertexBufferIndex);
                    //    break;

                    case ShaderVariableNames.MaterialDiffuseIndex:
                        commandBuffer.CmdPushConstants(pipeline.PipelineLayout, pushContant.StageFlags, pushContant.Offset, ref renderData.BindlessDiffuseTextureIndex);
                        break;

                    default:
                        Debug.Warning($"Constant not found: {pushContant.Name}");
                        break;
                }
            }

            //DescriptorSets...
            if (renderData.DescriptorSet != null)
            {
                commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, pipeline.PipelineLayout, 0, renderData.DescriptorSet.DescriptorSets, null);
            }

            //commandBuffer.CmdBindIndexBuffer(meshData.IndexBuffer, 0, IndexType.Uint32);


            //if (_indirectDrawBuffer == null)
            //{
            //    commandBuffer.CmdBindIndexBuffer(meshData.IndexBuffer, 0, IndexType.Uint32);
            //    commandBuffer.CmdDrawIndexed((uint)meshData.NbIndices, 1, 0, (int)meshData.VertexBufferIndex, 0);
            //}
            //else
            commandBuffer.CmdDrawIndexedIndirect(_indirectDrawBuffer.Buffer, 0, 1, (uint)Marshal.SizeOf<DrawIndexedIndirectCommand>());


        }

        /// <summary>
        /// Disposing of the mesh
        /// </summary>
        public void Dispose()
        {
            //We will not dispose pipelines because they can be used by another meshrenderer...
            //_pipeline.Dispose();
            //_pipeline = null;

            if (_renderDatas != null)
            {
                for (int i = 0; i < _renderDatas.Length; i++)
                {
                    _renderDatas[i].DescriptorSet?.Dispose();

                }
                _renderDatas = null;
            }

        }


        /// <summary>
        /// Init the mesh
        /// </summary>
        private bool Init()
        {
            if (!_mesh.IsLoaded)
                return false;

            //Pipeline creation...
            if (_renderDatas == null)
                _renderDatas = new RenderData[_mesh.MeshDatas.Length];

            _initialized = true;

            for (int i = 0; i < _mesh.MeshDatas.Length; i++)
            {
                


                if (_mesh.MeshDatas[i].MaterialIndex >= 0)
                {
                    VkMaterial mat;

                    if (_materials.Count > _mesh.MeshDatas[i].MaterialIndex && _materials[_mesh.MeshDatas[i].MaterialIndex] != null)
                        mat = (VkMaterial)_materials[_mesh.MeshDatas[i].MaterialIndex];
                    //Default mat?
                    else if (_mesh.Materials != null && _mesh.Materials.Length > _mesh.MeshDatas[i].MaterialIndex && _mesh.Materials[_mesh.MeshDatas[i].MaterialIndex] != null)
                        mat = (VkMaterial)_mesh.Materials[_mesh.MeshDatas[i].MaterialIndex];
                    else
                        //Material not found...
                        mat = (VkMaterial)BaseMaterials.Magenta;

                    if (mat != null)
                    {
                        if (mat.VkDiffuseTexture.IsLoaded)
                        {
                            VkShader shader = mat.Shader;

                            var pipeline = _vk.GetPipeline(shader);

                            _renderDatas[i].Pipeline = pipeline;

                            _renderDatas[i].Shader = shader;


                            if (pipeline.Bindless)
                                _renderDatas[i].BindlessDiffuseTextureIndex = pipeline.GetOrAddBindlessIndex(mat.VkDiffuseTexture.ImageWrapper.ImageView, _vk.DefaultSampler);
                            else
                                _renderDatas[i].DescriptorSet = pipeline.CreateDescriptorSet().Set(ShaderVariableNames.SamplerDiffuse, mat.VkDiffuseTexture.ImageWrapper.ImageView, _vk.DefaultSampler);
                        }
                        else
                        {
                            //Not totally initialized...
                            _initialized = false;
                        }
                    }
                }

            }


            //When everything is initialized, then we can create the indirect command buffer...
            if (_initialized)
                CreateIndirectCommandsBuffer();

            return _initialized;

        }

        /// <summary>
        /// Create the indirect command buffers
        /// </summary>
        private void CreateIndirectCommandsBuffer()
        {

            DrawIndexedIndirectCommand[] drawIndexedIndirectCommands = new DrawIndexedIndirectCommand[_mesh.MeshDatas.Length];

            for (int i = 0; i < _mesh.MeshDatas.Length; i++)
            {
                var meshData = _mesh.MeshDatas[i];

                drawIndexedIndirectCommands[i] = new()
                {
                    IndexCount = (uint)meshData.NbIndices,
                    InstanceCount = 2,
                    FirstIndex = (uint)meshData.IndexBufferIndex,
                    VertexOffset = (int)meshData.VertexBufferIndex,
                    FirstInstance = 0
                };
            }

            _indirectDrawBuffer = _vk.CreateBufferWrapper(drawIndexedIndirectCommands, BufferUsageFlags.IndirectBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);

        }


        private struct RenderData
        {
            public bool IsVertexBufferInitialized;
            public PipelineWrapper Pipeline;
            public PipelineDescriptorSet DescriptorSet;
            public VkShader Shader;
            //public uint BindlessVertexBufferIndex;
            public uint BindlessDiffuseTextureIndex;
        }


    }



}
