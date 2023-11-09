using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Instance of a mesh
    /// </summary>
    public class VkMeshInstance : IRenderHandle, IDisposable
    {
        private VkRenderer _renderer;
        private WorldTransform _transform;
        private VkMesh _mesh;
        private List<Material> _materials;

        private uint _offsetIndex;
        private uint _objectIndex;
        private VkInstanceStruct _objectData;
        private RenderData[] _renderDatas;

        private bool _updateTransformNextFrame = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public VkMeshInstance(Mesh mesh, List<Material> materials, WorldTransform transform, VkRenderer renderer)
        {
            _mesh = (VkMesh)mesh;
            _materials = materials;
            _transform = transform;


            _renderer = renderer;

            CopyObjectData();

            _offsetIndex = _renderer.ObjectsBuffer.Append(ref _objectData);
            _objectIndex = _offsetIndex / (uint)Marshal.SizeOf<VkInstanceStruct>();
            _renderer.AddActionsBeforeNextFrame(Init);

            _transform.OnLocationChanged += _transform_OnLocationChanged;

        }

        private void _transform_OnLocationChanged(Vector3 oldLocation, Vector3 newLocation)
        {
            if (!_updateTransformNextFrame)
            {
                _updateTransformNextFrame = true;
                _renderer.AddActionsBeforeNextFrame(UpdateTransform);
            }
        }

        /// <summary>
        /// Init
        /// </summary>
        public void Init()
        {
            bool allLoaded = true;

            if (_mesh.IsLoaded)
            {
                //Pipeline creation...
                if (_renderDatas == null)
                    _renderDatas = new RenderData[_mesh.MeshDatas.Length];

                for (int i = 0; i < _mesh.MeshDatas.Length; i++)
                {
                    if (_renderDatas[i].MeshRenderer == null)
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

                            if (mat.VkDiffuseTexture.IsLoaded)
                            {
                                _renderDatas[i].MeshRenderer = _renderer.GetMeshRenderer(mat.Shader);
                                _renderDatas[i].MeshRenderer.AddMesh(_objectIndex, mat, ref _mesh.MeshDatas[i]);
                            }
                            else
                            {
                                allLoaded = false;
                            }
                        }
                    }
                }
            }
            else
            {
                allLoaded = false;
            }

            if(!allLoaded)
                //Trying again next frame...
                _renderer.AddActionsBeforeNextFrame(Init);
        }

        /// <summary>
        /// Update Instance Data from transformation
        /// </summary>
        private void CopyObjectData()
        {

            _objectData.Location = _transform.Location;
            _objectData.Rotation = _transform.Rotation;
            _objectData.Scale = _transform.Scale;

            

        }

        /// <summary>
        /// Update Instance Data from transformation
        /// </summary>
        private void UpdateTransform()
        {

            CopyObjectData();

            _renderer.ObjectsBuffer.Update(ref _objectData, _offsetIndex);

            _updateTransformNextFrame = false;

        }


        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            //TODO: To something to remove from the scene
        }

        private struct RenderData
        {
            public bool IsVertexBufferInitialized;
            public PipelineWrapper Pipeline;
            public PipelineDescriptorSet DescriptorSet;
            public VkShader Shader;
            //public uint BindlessVertexBufferIndex;
            public uint BindlessDiffuseTextureIndex;
            public VkMeshRenderer MeshRenderer;
        }
    }
}
