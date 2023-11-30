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

        public VkMeshletData[] MeshletDatas;

        private MeshletContainer[] _meshletContainers;

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
            List<VkMeshletData> oldmeshLets = null;
            
            if(MeshletDatas != null)
                oldmeshLets = new List<VkMeshletData>(MeshletDatas);

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
            if (MeshletDatas != null)
            {
                foreach (var submeshLet in MeshletDatas)
                {
                    DisposeMeshLet(submeshLet);
                }
            }

            _factory?.Remove(this);
        }



        /// <summary>
        /// Dispose mesh data
        /// </summary>
        private void DisposeMeshLet(VkMeshletData meshLet)
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
            List<VkMeshletData> newmeshLets = new List<VkMeshletData>();

            //We want to create an array of VkMeshletData per material index, so we can regroup meshlets ot optimize memory transferts
            uint maxMaterialIndex = subMeshes.Max(s => s.MaterialIndex);

            int[] nbMeshLetsPerMats = new int[(int)maxMaterialIndex + 1];
            for (int i = 0; i < subMeshes.Count; i++)
            {
                nbMeshLetsPerMats[subMeshes[i].MaterialIndex] += _meshletContainers[i].Meshlets.Length;
            }


            for (int matIndex = 0; matIndex <= maxMaterialIndex; matIndex++)
            {
                int nbMeshLets = nbMeshLetsPerMats[matIndex];

                if (nbMeshLets == 0)
                    continue;

                MeshletData[] meshletDatas = new MeshletData[nbMeshLets];
                uint offset = _renderer.MeshLetsBuffer.Reserve((uint)nbMeshLets, out uint firstElementIndex);

                for (int i = 0; i < subMeshes.Count; i++)
                {
                    if (subMeshes[i].MaterialIndex != matIndex)
                        continue;

                    var container = _meshletContainers[i];

                    _renderer.VerticesBuffer.Append(container.Vertices, out uint verticesIndex);
                    _renderer.IndicesBuffer.Append(container.Indices, out uint indicesIndex);

                    for (int im = 0; im < container.Meshlets.Length; im++)
                    {
                        meshletDatas[im].VerticesBufferIndex = verticesIndex + container.Meshlets[im].VertexOffset;
                        meshletDatas[im].IndicesBufferIndex = indicesIndex + container.Meshlets[im].IndicesOffset;
                        meshletDatas[im].NbIndices = container.Meshlets[im].IndicesCount;
                        meshletDatas[im].center = container.Meshlets[im].Bounds.center;
                        meshletDatas[im].radius = container.Meshlets[im].Bounds.radius;
                        meshletDatas[im].cone_axis_s8_x = container.Meshlets[im].Bounds.cone_axis_s8_x;
                        meshletDatas[im].cone_axis_s8_y = container.Meshlets[im].Bounds.cone_axis_s8_y;
                        meshletDatas[im].cone_axis_s8_z = container.Meshlets[im].Bounds.cone_axis_s8_z;
                        meshletDatas[im].cone_cutoff_s8 = container.Meshlets[im].Bounds.cone_cutoff_s8;
                    }

                }


                //And now we can update the meshlet buffer all at once...
                _renderer.MeshLetsBuffer.Update(meshletDatas, offset);


                VkMeshletData meshlet = new VkMeshletData();
                meshlet.MaterialIndex = (uint)matIndex;
                meshlet.FirstMeshLetIndex = firstElementIndex;
                meshlet.NbMeshLets = (uint)nbMeshLets;
                newmeshLets.Add(meshlet);

            }

            //All good, we can switch it now...
            MeshletDatas = newmeshLets.ToArray();

            //Create default materials slots
            Materials = meshDef.Materials.ToArray();

            //Everything is ready!
            IsLoaded = true;
        }


    }

    /// <summary>
    /// Information on meshlet
    /// </summary>
    public class VkMeshletData
    {
        public uint FirstMeshLetIndex;
        public uint NbMeshLets;
        public uint MaterialIndex;
    }
}
