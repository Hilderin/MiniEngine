using MiniEngine.Drivers.Vulkan;
using MiniEngine.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static MiniEngine.Mesh;

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
        private Mesh _mesh = null;

        private VkRenderer _vi;

        /// <summary>
        /// Materials
        /// </summary>
        private Material[] _materials;

        private MeshData _meshData;

        /// <summary>
        /// Datas for the sub meshes
        /// </summary>
        private List<SubMeshData> _subMeshes = new List<SubMeshData>();

        public VulkanMeshData[] _vulkanMeshDatas;

        /// <summary>
        /// List of materials
        /// </summary>
        public Material[] Materials { get { return _materials; } }




        /// <summary>
        /// Constructor
        /// </summary>
        public VkMeshRenderer(Mesh mesh, VkRenderer vi)
        {
            _mesh = mesh;
            _vi = vi;

            _meshData = _mesh.GetMeshData();
            _subMeshes = _meshData.SubMeshes;

            Init();
        }


        /// <summary>
        /// Render the mesh
        /// </summary>
        public void PopulateCommandBuffers(CommandBuffer commandBuffer)
        {

            Matrix4 mvp = _vi.MVPMatrix * _mesh.GetMatrix();
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



            for (int i = 0; i < _vulkanMeshDatas.Length; i++)
            {
                //Push constant...
                commandBuffer.CmdPushConstants(_vulkanMeshDatas[i].Pipeline.pipelineLayout, ShaderStageFlags.Vertex, 0, ref mvp);


                //buffers[i].CmdBindDescriptorSets(PipelineBindPoint.Graphics, pipelineLayout, 0, descriptorSets, null);
                commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, _vulkanMeshDatas[i].Pipeline);
                commandBuffer.CmdBindVertexBuffer(0, _vulkanMeshDatas[i].vertexBuffer, 0);
                commandBuffer.CmdBindIndexBuffer(_vulkanMeshDatas[i].indexBuffer, 0, IndexType.Uint32);
                commandBuffer.CmdDrawIndexed((uint)_vulkanMeshDatas[i].indexBufferLength, 1, 0, 0, 0);
                //commandBuffer.CmdDraw(3, 1, 0, 0);


            }
        }

        /// <summary>
        /// Disposing of the mesh
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _vulkanMeshDatas.Length; i++)
            {
                if (_vulkanMeshDatas[i].Pipeline != null)
                {
                    _vulkanMeshDatas[i].Pipeline.Dispose();
                    _vulkanMeshDatas[i].Pipeline = null;
                }

                _vulkanMeshDatas[i].vertexBuffer.Dispose();
                _vulkanMeshDatas[i].indexBuffer.Dispose();
            }

            _vulkanMeshDatas = null;
            _mesh.RendererStateObj = null;
            _mesh = null;
            _materials = null;
        }


        /// <summary>
        /// Init the mesh
        /// </summary>
        private void Init()
        {

            List<Material> materials = new List<Material>(_meshData.Materials);

            _vulkanMeshDatas = new VulkanMeshData[_subMeshes.Count];

            for (int i = 0; i < _subMeshes.Count; i++)
            {
                CreateVertexBuffer(_subMeshes[i], ref _vulkanMeshDatas[i]);
                CreateIndexBuffer(_subMeshes[i], ref _vulkanMeshDatas[i]);

                //Check to be sure we have a material...
                while (materials.Count <= _subMeshes[i].MaterialIndex)
                    materials.Add(Material.NotFound);
            }

            //Creation of the array of material... (faster to acces!)
            _materials = _meshData.Materials.ToArray();


            //Pipeline creation...
            for (int i = 0; i < _subMeshes.Count; i++)
            {
                _vulkanMeshDatas[i].ShaderBinder = ShaderCompiler.BuildBinder(_materials[_subMeshes[i].MaterialIndex].Shader);
                _vulkanMeshDatas[i].Pipeline = new PipelineWrapper(_vi.Device, _vi.Swapchain.RenderPass, _vulkanMeshDatas[i].ShaderBinder);
            }

        }


        /// <summary>
        /// Create the vertex buffer
        /// </summary>
        private void CreateVertexBuffer(SubMeshData subMeshData, ref VulkanMeshData vulkanMeshData)
        {
            Vertex[] vertices = new Vertex[subMeshData.Positions.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vertex()
                {
                    Pos = new Vector3(subMeshData.Positions[i].X, subMeshData.Positions[i].Y, subMeshData.Positions[i].Z),
                    Color = subMeshData.Colors[i],
                    TexCoord = subMeshData.TexCoords[i]
                };
            }

            //vulkanMeshData.vertexBuffer = _vi.Device.CreateBuffer(vertices, BufferUsageFlags.VertexBuffer);
            vulkanMeshData.vertexBuffer = _vi.MemoryManager.CreateBufferOnGPU(vertices, BufferUsageFlags.VertexBuffer);

        }

        private void CreateIndexBuffer(SubMeshData subMeshData, ref VulkanMeshData vulkanMeshData)
        {
            vulkanMeshData.indexBufferLength = subMeshData.Indices.Length;
            vulkanMeshData.indexBuffer = _vi.MemoryManager.CreateBufferOnGPU(subMeshData.Indices, BufferUsageFlags.IndexBuffer);

        }


    }

    public struct VulkanMeshData
    {
        public BufferWrapper vertexBuffer;
        public BufferWrapper indexBuffer;
        public int indexBufferLength;
        public PipelineWrapper Pipeline;
        public ShaderBinder ShaderBinder;
        //public CommandBuffer[] CommandBuffers;
    }
}
