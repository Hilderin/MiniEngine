using MiniEngine.Drivers.Vulkan;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// A Vuklan Mesh
    /// </summary>
    public class VkMesh: Mesh
    {
        private VkResourceFactory _factory;
        private VkRenderer _vk;

        public VulkanMeshData[] MeshDatas;


        /// <summary>
        /// Constructor
        /// </summary>
        public VkMesh(MeshDefinition meshDef, VkRenderer vk, VkResourceFactory factory)
        {
            _vk = vk;

            Init(meshDef);

            _factory = factory;
            factory?.Add(this);
        }


        /// <summary>
        /// Reload the asset
        /// </summary>
        public override void Reload(MeshDefinition meshDef)
        {
            var oldMeshDatas = new List<VulkanMeshData>(MeshDatas);


            Init(meshDef);

            foreach(var oldMeshData in oldMeshDatas)
            {
                oldMeshData.vertexBuffer?.Dispose();
                oldMeshData.indexBuffer?.Dispose();
            }
        }


        /// <summary>
        /// Destruction
        /// </summary>
        protected override void Destroy()
        {
            foreach (var subMeshData in MeshDatas)
            {
                subMeshData.vertexBuffer?.Dispose();
                subMeshData.indexBuffer?.Dispose();
            }

            _factory?.Remove(this);
        }



        /// <summary>
        /// Init the mesh
        /// </summary>
        private void Init(MeshDefinition meshDef)
        {
            List<SubMeshDefinition> subMeshes = meshDef.SubMeshes;

            VulkanMeshData[] newMeshDatas = new VulkanMeshData[subMeshes.Count];

            for (int i = 0; i < subMeshes.Count; i++)
            {
                CreateVertexBuffer(subMeshes[i], ref newMeshDatas[i]);
                CreateIndexBuffer(subMeshes[i], ref newMeshDatas[i]);

                newMeshDatas[i].MaterialIndex = subMeshes[i].MaterialIndex;
            }

            //All good, we can switch it now...
            MeshDatas = newMeshDatas;

            //Create default materials slots
            Materials = meshDef.Materials.ToArray();
        }


        /// <summary>
        /// Create the vertex buffer
        /// </summary>
        private void CreateVertexBuffer(SubMeshDefinition subMeshData, ref VulkanMeshData vulkanMeshData)
        {
            Vertex[] vertices = new Vertex[subMeshData.Positions.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vertex()
                {
                    Pos = new Vector3(subMeshData.Positions[i].X, subMeshData.Positions[i].Y, subMeshData.Positions[i].Z),
                    //Color = subMeshData.Colors[i],
                    TexCoord = subMeshData.TexCoords[i]
                };
            }

            //vulkanMeshData.vertexBuffer = _vi.Device.CreateBuffer(vertices, BufferUsageFlags.VertexBuffer);
            vulkanMeshData.vertexBuffer = _vk.Device.MemoryManager.CreateBufferOnGPU(vertices, BufferUsageFlags.VertexBuffer);

        }

        /// <summary>
        /// Create the index buffer
        /// </summary>
        private void CreateIndexBuffer(SubMeshDefinition subMeshData, ref VulkanMeshData vulkanMeshData)
        {
            vulkanMeshData.nbIndices = subMeshData.Indices.Length;
            vulkanMeshData.indexBuffer = _vk.Device.MemoryManager.CreateBufferOnGPU(subMeshData.Indices, BufferUsageFlags.IndexBuffer);

        }

    }



    public struct VulkanMeshData
    {
        public BufferWrapper vertexBuffer;
        public BufferWrapper indexBuffer;
        public int nbIndices;
        public int MaterialIndex;
    }
}
