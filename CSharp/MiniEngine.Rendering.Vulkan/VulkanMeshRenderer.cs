using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static MiniEngine.Mesh;
using Image = Silk.NET.Vulkan.Image;
using Buffer = Silk.NET.Vulkan.Buffer;
using Silk.NET.Vulkan;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Renderer for the meshes
    /// </summary>
    public unsafe class VulkanMeshRenderer: IDisposable
    {
        //private const int SHADER_POSITION_LOCATION = 0;
        //private const int SHADER_TEX_COORD_LOCATION = 1;
        //private const int SHADER_NORMAL_LOCATION = 2;
        //private static uint SHADER_COLOR_TEXTURE_UNIT = GL.GL_TEXTURE0;
        //private static int SHADER_COLOR_TEXTURE_UNIT_INDEX = 0;
        //private static uint SHADER_SPECULAR_EXPONENT_UNIT = GL.GL_TEXTURE6;
        //private static int SHADER_SPECULAR_EXPONENT_UNIT_INDEX = 6;

        //private const int INDEX_BUFFER = 0;
        //private const int POS_VB = 1;
        //private const int TEXCOORD_VB = 2;
        //private const int NORMAL_VB = 3;
        //private const int WVP_MAT_VB = 4;  // required only for instancing
        //private const int WORLD_MAT_VB = 5;  // required only for instancing
        //private const int NB_BUFFERS = 6;

        /// <summary>
        /// Mesh that uses this renderer
        /// </summary>
        private Mesh _mesh = null;

        private VulkanInstance _vi;

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
        public VulkanMeshRenderer(Mesh mesh, VulkanInstance vi)
        {
            _mesh = mesh;
            _vi = vi;

            _meshData = _mesh.GetMeshData();
            _subMeshes = _meshData.SubMeshes;

        }

        /// <summary>
        /// Init the mesh
        /// </summary>
        public void Init()
        {   

            List<Material> materials = new List<Material>(_meshData.Materials);

            _vulkanMeshDatas = new VulkanMeshData[_subMeshes.Count];

            for (int i = 0; i < _subMeshes.Count; i++)
            {
                CreateVertexBuffer(_subMeshes[i], ref _vulkanMeshDatas[i]);
                CreateIndexBuffer(_subMeshes[i], ref _vulkanMeshDatas[i]);
                //AddMeshData(_subMeshes[i].Positions, _subMeshes[i].TexCoords, _subMeshes[i].Normals, _subMeshes[i].Indices, _subMeshes[i].MaterialIndex, i);

                //Check to be sure we have a material...
                while (materials.Count <= _subMeshes[i].MaterialIndex)
                    materials.Add(Material.NotFound);

            }

            //Creation of the array of material... (faster to acces!)
            _materials = _meshData.Materials.ToArray();

        }


        /// <summary>
        /// Render the mesh
        /// </summary>
        public void Render()
        {
            
        }

        /// <summary>
        /// Disposing of the mesh
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _vulkanMeshDatas.Length; i++)
            {
                _vi.Api.DestroyBuffer(_vi.device, _vulkanMeshDatas[i].indexBuffer, null);
                _vi.Api.FreeMemory(_vi.device, _vulkanMeshDatas[i].indexBufferMemory, null);

                _vi.Api.DestroyBuffer(_vi.device, _vulkanMeshDatas[i].vertexBuffer, null);
                _vi.Api.FreeMemory(_vi.device, _vulkanMeshDatas[i].vertexBufferMemory, null);
            }
            

        }

        private void CreateVertexBuffer(SubMeshData subMeshData, ref VulkanMeshData vulkanMeshData)
        {
            Vertex[] vertices = new Vertex[subMeshData.Positions.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vertex()
                {
                    pos = new Vector3(subMeshData.Positions[i].X, subMeshData.Positions[i].Y, subMeshData.Positions[i].Z),
                    color = Vector3.One,
                    textCoord = subMeshData.TexCoords[i]
                };
            }

            ulong bufferSize = (ulong)(Unsafe.SizeOf<Vertex>() * vertices.Length);

            Buffer stagingBuffer = default;
            DeviceMemory stagingBufferMemory = default;
            _vi.CreateBuffer(bufferSize, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, ref stagingBuffer, ref stagingBufferMemory);

            void* data;
            _vi.Api.MapMemory(_vi.device, stagingBufferMemory, 0, bufferSize, 0, &data);
            vertices.AsSpan().CopyTo(new Span<Vertex>(data, vertices.Length));
            _vi.Api.UnmapMemory(_vi.device, stagingBufferMemory);

            _vi.CreateBuffer(bufferSize, BufferUsageFlags.TransferDstBit | BufferUsageFlags.VertexBufferBit, MemoryPropertyFlags.DeviceLocalBit, ref vulkanMeshData.vertexBuffer, ref vulkanMeshData.vertexBufferMemory);

            _vi.CopyBuffer(stagingBuffer, vulkanMeshData.vertexBuffer, bufferSize);

            _vi.Api.DestroyBuffer(_vi.device, stagingBuffer, null);
            _vi.Api.FreeMemory(_vi.device, stagingBufferMemory, null);
        }

        private void CreateIndexBuffer(SubMeshData subMeshData, ref VulkanMeshData vulkanMeshData)
        {
            vulkanMeshData.indexBufferLength = subMeshData.Indices.Length;
            ulong bufferSize = (ulong)(Unsafe.SizeOf<uint>() * subMeshData.Indices.Length);

            Buffer stagingBuffer = default;
            DeviceMemory stagingBufferMemory = default;
            _vi.CreateBuffer(bufferSize, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, ref stagingBuffer, ref stagingBufferMemory);

            void* data;
            _vi.Api.MapMemory(_vi.device, stagingBufferMemory, 0, bufferSize, 0, &data);
            subMeshData.Indices.AsSpan().CopyTo(new Span<int>(data, subMeshData.Indices.Length));
            _vi.Api.UnmapMemory(_vi.device, stagingBufferMemory);

            _vi.CreateBuffer(bufferSize, BufferUsageFlags.TransferDstBit | BufferUsageFlags.IndexBufferBit, MemoryPropertyFlags.DeviceLocalBit, ref vulkanMeshData.indexBuffer, ref vulkanMeshData.indexBufferMemory);

            _vi.CopyBuffer(stagingBuffer, vulkanMeshData.indexBuffer, bufferSize);

            _vi.Api.DestroyBuffer(_vi.device, stagingBuffer, null);
            _vi.Api.FreeMemory(_vi.device, stagingBufferMemory, null);
        }

        
    }

    public struct VulkanMeshData
    {
        public Buffer vertexBuffer;
        public DeviceMemory vertexBufferMemory;
        public Buffer indexBuffer;
        public DeviceMemory indexBufferMemory;
        public int indexBufferLength;
    }
}
