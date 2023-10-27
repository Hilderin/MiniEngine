using MiniEngine.Drivers.Vulkan;
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
                PopulateCommandBuffer(commandBuffer, ref mvp, ref _vulkanMeshDatas[i]);

            }
        }

        /// <summary>
        /// Populate and command buffer for a mesh
        /// </summary>
        private void PopulateCommandBuffer(CommandBuffer commandBuffer, ref Matrix4 mvp, ref VulkanMeshData meshData)
        {
            VkShader shader = meshData.Shader;
            PipelineWrapper pipeline = meshData.Pipeline;

            //Constants...
            for (int iConst = 0; iConst < shader.Constants.Count; iConst++)
            {
                commandBuffer.CmdPushConstants(meshData.Pipeline.pipelineLayout, shader.Constants[iConst].StageFlags, 0, ref mvp);
            }

            //DescriptorSets...
            if (pipeline.DescriptorSets != null)
            {
                commandBuffer.CmdBindDescriptorSets(PipelineBindPoint.Graphics, pipeline.pipelineLayout, 0, pipeline.DescriptorSets, null);
            }

            commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, pipeline);
            commandBuffer.CmdBindVertexBuffer(0, meshData.vertexBuffer, 0);
            commandBuffer.CmdBindIndexBuffer(meshData.indexBuffer, 0, IndexType.Uint32);
            commandBuffer.CmdDrawIndexed((uint)meshData.indexBufferLength, 1, 0, 0, 0);

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
                Material mat = _subMeshes[i].MaterialIndex >= 0 ? _materials[_subMeshes[i].MaterialIndex] : null;
                VkMaterial vkMaterial = null;
                if (mat != null)
                {
                    vkMaterial = new VkMaterial();

                    byte[] data = new byte[] { 255, 0, 0, 255 };
                    vkMaterial.Diffuse = new ImageWrapper(_vi.Device, data, mat.Diffuse.Width, mat.Diffuse.Height, Format.R8G8B8A8Srgb);
                    //vkMaterial.Diffuse = new ImageWrapper(_vi.Device, mat.Diffuse.Data, mat.Diffuse.Width, mat.Diffuse.Height, Format.R8G8B8A8Srgb);

                }

                


                _vulkanMeshDatas[i].Shader = ShaderConverter.ConvertToVulkanShader(_materials[_subMeshes[i].MaterialIndex].Shader);
                _vulkanMeshDatas[i].Pipeline = new PipelineWrapper(_vi.Device, _vi.Swapchain.RenderPass, _vulkanMeshDatas[i].Shader, vkMaterial, _vi.Sampler);
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
            vulkanMeshData.vertexBuffer = _vi.Device.MemoryManager.CreateBufferOnGPU(vertices, BufferUsageFlags.VertexBuffer);

        }

        private void CreateIndexBuffer(SubMeshData subMeshData, ref VulkanMeshData vulkanMeshData)
        {
            vulkanMeshData.indexBufferLength = subMeshData.Indices.Length;
            vulkanMeshData.indexBuffer = _vi.Device.MemoryManager.CreateBufferOnGPU(subMeshData.Indices, BufferUsageFlags.IndexBuffer);

        }


    }

    public struct VulkanMeshData
    {
        public BufferWrapper vertexBuffer;
        public BufferWrapper indexBuffer;
        public int indexBufferLength;
        public PipelineWrapper Pipeline;
        public VkShader Shader;
        //public CommandBuffer[] CommandBuffers;
    }
}
