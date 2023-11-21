using MiniEngine.Drivers.Vulkan;
using MiniEngine.MeshOptimization;
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

        public event Action OnReload;

        public bool IsLoaded { get; private set; }

        public VkMeshlet[] MeshLets;

        private MeshLetContainer[] _meshletContainers;

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
            List<VkMeshlet> oldmeshLets = null;
            
            if(MeshLets != null)
                oldmeshLets = new List<VkMeshlet>(MeshLets);

            Init(meshDef);

            if (oldmeshLets != null)
            {
                foreach (var oldmeshLet in oldmeshLets)
                {
                    _renderer.AddActionsBeforeNextFrameAsync(() => DisposeMeshLet(oldmeshLet));
                }
            }

            OnReload?.Invoke();

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
        private void DisposeMeshLet(VkMeshlet meshLet)
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

            MeshletGenerator meshletGenerator = new MeshletGenerator();

            _meshletContainers = meshletGenerator.Generate(meshDef);

            List<SubMeshDefinition> subMeshes = meshDef.SubMeshes;

            List<VkMeshlet> newmeshLets = new List<VkMeshlet>();

            for (int i = 0; i < subMeshes.Count; i++)
            {

                var container = _meshletContainers[i];

                _renderer.VerticesBuffer.Append(container.Vertices, out uint verticesIndex);
                _renderer.IndicesBuffer.Append(container.Indices, out uint indicesIndex);

                for (int im = 0; im < container.Meshlets.Length; im++)
                {
                    VkMeshlet meshlet = new VkMeshlet();
                    meshlet.MaterialIndex = subMeshes[i].MaterialIndex;
                    meshlet.MeshLetData.VerticesBufferIndex = verticesIndex + container.Meshlets[im].VertexOffset;
                    meshlet.MeshLetData.IndicesBufferIndex = indicesIndex + container.Meshlets[im].IndicesOffset;
                    meshlet.MeshLetData.NbIndices = container.Meshlets[im].IndicesCount;

                    newmeshLets.Add(meshlet);

                }
            }

            //All good, we can switch it now...
            MeshLets = newmeshLets.ToArray();

            //Create default materials slots
            Materials = meshDef.Materials.ToArray();

            //Everything is ready!
            IsLoaded = true;
        }


    }

    /// <summary>
    /// Information on MeshLet
    /// </summary>
    public class VkMeshlet
    {
        public uint MeshLetIndex;
        public MeshletData MeshLetData;
        public uint MaterialIndex;

        public uint VertexBufferIndex
        {
            get { return this.MeshLetData.VerticesBufferIndex; }
        }

        public uint IndexBufferIndex
        {
            get { return this.MeshLetData.IndicesBufferIndex; }
        }

        public uint NbIndices
        {
            get { return this.MeshLetData.NbIndices; }
        }
    }
}
