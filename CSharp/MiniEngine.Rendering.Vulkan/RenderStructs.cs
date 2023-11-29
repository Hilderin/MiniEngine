using MiniEngine.MeshOptimization;
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
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct MeshletData
    {
        [FieldOffset(0)]
        public uint VerticesBufferIndex;
        [FieldOffset(4)]
        public uint IndicesBufferIndex;
        [FieldOffset(8)]
        public uint NbIndices;


        /* bounding sphere, useful for frustum and occlusion culling */
        [FieldOffset(16)]
        public Vector3 center;
        [FieldOffset(28)]
        public float radius;

        /* normal cone axis and cutoff, stored in 8-bit SNORM format; decode using x/127.0 */
        [FieldOffset(32)]
        public byte cone_axis_s8_x;
        [FieldOffset(33)]
        public byte cone_axis_s8_y;
        [FieldOffset(34)]
        public byte cone_axis_s8_z;
        [FieldOffset(35)]
        public byte cone_cutoff_s8;
    }


    /// <summary>
    /// Information on a meshlet on the scene in the GPU buffer
    /// </summary>
    public struct MeshLetInstanceData
    {
        public uint ObjectIndex;
        public uint MeshLetIndex;
        public uint TextureIndex;
        /// <summary>
        /// Index of the drawcallbuffer. Each shader has a difference drawcallbuffer. So it's basiclly the shader index.
        /// </summary>
        public uint DrawCallsBufferIndex;

        public uint Visible;
    }

}
