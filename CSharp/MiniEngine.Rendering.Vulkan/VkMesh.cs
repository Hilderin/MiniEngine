using MiniEngine.Drivers.Vulkan;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private VkRenderer _renderer;

        public bool IsLoaded { get; private set; }

        public Meshlet[] MeshLets;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkMesh(VkRenderer renderer, VkResourceFactory factory)
        {
            _renderer = renderer;

            //Init(meshDef);

            _factory = factory;
            factory?.Add(this);
        }


        /// <summary>
        /// Reload the asset
        /// </summary>
        public override Mesh Load(MeshDefinition meshDef)
        {
            List<Meshlet> oldmeshLets = null;
            
            if(MeshLets != null)
                oldmeshLets = new List<Meshlet>(MeshLets);

            Init(meshDef);

            if (oldmeshLets != null)
            {
                foreach (var oldmeshLet in oldmeshLets)
                {
                    _renderer.AddActionsBeforeNextFrameAsync(() => DisposeMeshLet(oldmeshLet));
                }
            }

            return this;
        }

        /// <summary>
        /// Destruction
        /// </summary>
        protected override void Destroy()
        {
            if (MeshLets != null)
            {
                foreach (var submeshLet in MeshLets)
                {
                    DisposeMeshLet(submeshLet);
                }
            }

            _factory?.Remove(this);
        }



        /// <summary>
        /// Dispose mesh data
        /// </summary>
        private void DisposeMeshLet(Meshlet meshLet)
        {
            //TODO: release memory from GPU buffer
            //meshLet.VertexBuffer?.Dispose();
            //meshLet.IndexBuffer?.Dispose();
        }



        /// <summary>
        /// Init the mesh
        /// </summary>
        private void Init(MeshDefinition meshDef)
        {
            List<SubMeshDefinition> subMeshes = meshDef.SubMeshes;

            Meshlet[] newmeshLets = new Meshlet[subMeshes.Count];

            for (int i = 0; i < subMeshes.Count; i++)
            {
                Meshlet meshlet = new Meshlet();

                CreateVertexBuffer(subMeshes[i], ref meshlet.MeshLetData);
                CreateIndexBuffer(subMeshes[i], ref meshlet.MeshLetData);

                meshlet.MaterialIndex = subMeshes[i].MaterialIndex;

                _renderer.MeshLetsBuffer.Append(ref meshlet.MeshLetData, out meshlet.MeshLetIndex);

                newmeshLets[i] = meshlet;
            }

            //All good, we can switch it now...
            MeshLets = newmeshLets;

            //Create default materials slots
            Materials = meshDef.Materials.ToArray();

            //Everything is ready!
            IsLoaded = true;
        }


        /// <summary>
        /// Create the vertex buffer
        /// </summary>
        private void CreateVertexBuffer(SubMeshDefinition submeshLet, ref MeshletData meshLet)
        {
            Vertex[] vertices = new Vertex[submeshLet.Positions.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vertex()
                {
                    Pos = new Vector3(submeshLet.Positions[i].X, submeshLet.Positions[i].Y, submeshLet.Positions[i].Z),
                    //Color = submeshLet.Colors[i],
                    TexCoord = submeshLet.TexCoords[i]
                };
            }

            //meshLet.vertexBuffer = _vi.Device.CreateBuffer(vertices, BufferUsageFlags.VertexBuffer);
            _renderer.VerticesBuffer.Append(vertices, out meshLet.VertexBufferIndex);
            //meshLet.VertexBuffer = _renderer.CreateBufferWrapper(vertices, BufferUsageFlags.VertexBuffer | BufferUsageFlags.TransferDst, MemoryPropertyFlags.DeviceLocal);

        }

        /// <summary>
        /// Create the index buffer
        /// </summary>
        private void CreateIndexBuffer(SubMeshDefinition submeshLet, ref MeshletData meshLet)
        {
            meshLet.NbIndices = (uint)submeshLet.Indices.Length;
            meshLet.IndexBufferIndex = _renderer.IndicesBuffer.Append(submeshLet.Indices) / sizeof(uint);

        }

    }

    /// <summary>
    /// Information on MeshLet
    /// </summary>
    public class Meshlet
    {
        public uint MeshLetIndex;
        public MeshletData MeshLetData;
        public uint MaterialIndex;

        public uint VertexBufferIndex
        {
            get { return this.MeshLetData.VertexBufferIndex; }
        }

        public uint IndexBufferIndex
        {
            get { return this.MeshLetData.IndexBufferIndex; }
        }

        public uint NbIndices
        {
            get { return this.MeshLetData.NbIndices; }
        }
    }
}
