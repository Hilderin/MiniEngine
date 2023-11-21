using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Information on the scene
    /// </summary>
    public struct SceneData
    {
        public Matrix4 ViewProjectionMatrix;
        public Vector3 CameraLocation;
        public uint NbMeshletInstances;
        public float NearZ;
        public float FarZ;
        public float FrustumLeft;
        public float FrustumRight;
        public float FrustumTop;
        public float FrustumBottom;

    }

    /// <summary>
    /// Information on an instance on the GPU buffer
    /// </summary>
    //The explicit is very important because of alignment in struct in vulkan (ref: https://registry.khronos.org/vulkan/specs/1.0-wsi_extensions/html/vkspec.html#interfaces-resources-layout)
    [StructLayout(LayoutKind.Explicit, Size = 112)]
    public struct ObjectInstanceData
    {
        [FieldOffset(0)]
        public Vector3 Location;
        [FieldOffset(16)]
        public Vector3 Rotation;
        [FieldOffset(32)]
        public Vector3 Scale;
        [FieldOffset(48)]
        public Matrix4 TransformMatrix;
    }




    /// <summary>
    /// Information on MeshLet in the GPU buffer
    /// </summary>
    public struct MeshletData
    {
        public uint VerticesBufferIndex;
        public uint IndicesBufferIndex;
        public ushort NbIndices;
    }


    /// <summary>
    /// Information on a meshlet on the scene in the GPU buffer
    /// </summary>
    public struct MeshLetInstanceData
    {
        public uint ObjectIndex;
        public uint MeshLetIndex;
        public uint TextureIndex;
        public uint DrawCallsBufferIndex;
        public uint DrawCallIndex;
        public uint Visible;
    }

}
