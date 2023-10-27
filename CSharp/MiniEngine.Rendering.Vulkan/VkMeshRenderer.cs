using MiniEngine.Drivers.Vulkan;
using System.Net.Http.Headers;
using static MiniEngine.MeshActor;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Renderer for the meshes
    /// </summary>
    public unsafe class VkMeshRenderer : IDisposable
    {
        /// <summary>
        /// Mesh that uses this renderer
        /// </summary>
        private MeshActor _meshActor = null;

        private VkRenderer _vk;

        private VkMesh _mesh;

        private RenderData[] _renderDatas;


        /// <summary>
        /// Constructor
        /// </summary>
        public VkMeshRenderer(MeshActor meshActor, VkRenderer vi)
        {
            _meshActor = meshActor;
            _vk = vi;

            Init();
        }


        /// <summary>
        /// Render the mesh
        /// </summary>
        public void PopulateCommandBuffers(CommandBuffer commandBuffer)
        {

            Matrix4 mvp = _vk.MVPMatrix * _meshActor.GetMatrix();
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



            for (int i = 0; i < _renderDatas.Length; i++)
            {
                //Push constant...
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
            for (int iConst = 0; iConst < shader.Constants.Count; iConst++)
            {
                commandBuffer.CmdPushConstants(renderData.Pipeline.PipelineLayout, shader.Constants[iConst].StageFlags, 0, ref mvp);
            }

            //DescriptorSets...
            if (pipeline.DescriptorSets != null)
            {
                commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, pipeline.PipelineLayout, 0, pipeline.DescriptorSets, null);
            }

            commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, pipeline.Pipeline);
            commandBuffer.CmdBindVertexBuffer(0, meshData.vertexBuffer, 0);
            commandBuffer.CmdBindIndexBuffer(meshData.indexBuffer, 0, IndexType.Uint32);
            commandBuffer.CmdDrawIndexed((uint)meshData.nbIndices, 1, 0, 0, 0);

        }

        /// <summary>
        /// Disposing of the mesh
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _renderDatas.Length; i++)
            {
                if (_renderDatas[i].Pipeline != null)
                {
                    _renderDatas[i].Pipeline.Dispose();
                    _renderDatas[i].Pipeline = null;
                }

            }

            _renderDatas = null;
            _meshActor.RendererStateObj = null;
            _meshActor = null;
        }


        /// <summary>
        /// Init the mesh
        /// </summary>
        private void Init()
        {
            _mesh = (VkMesh)_meshActor.Mesh;


            //Pipeline creation...
            _renderDatas = new RenderData[_mesh.MeshDatas.Length];

            for (int i = 0; i < _mesh.MeshDatas.Length; i++)
            {
                VkMaterial mat = (VkMaterial)_meshActor.Materials[_mesh.MeshDatas[i].MaterialIndex];
                VkShader shader = mat.Shader;

                _renderDatas[i].Shader = shader;
                _renderDatas[i].Pipeline = new PipelineWrapper(_vk.Device, _vk.Swapchain.RenderPass, shader, mat.VkDiffuseTexture?.ImageWrapper?.ImageView, _vk.Sampler);
            }

        }



    }

    public struct RenderData
    {
        public PipelineWrapper Pipeline;
        public VkShader Shader;
    }

}
