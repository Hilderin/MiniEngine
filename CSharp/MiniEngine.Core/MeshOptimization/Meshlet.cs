using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.MeshOptimization
{
    /// <summary>
    /// Contains the definition for the meshlets
    /// </summary>
    public class MeshletContainer
    {
        public Vertex[] Vertices;
        public ushort[] Indices;          //byte because we never have more then 255 vertices in a meshlet
        public Meshlet[] Meshlets;
    }

    /// <summary>
    /// Representation of a meshlet
    /// </summary>
    public class Meshlet
    {
        /// <summary>
        /// Offset in the MeshLetContainer.Vertices
        /// </summary>
        public uint VertexOffset;

        /// <summary>
        /// Offset in the MeshLetContainer.Indices
        /// </summary>
        public uint IndicesOffset;

        
        /// <summary>
        /// Number of vertices for this meshlet in MeshLetContainer.Vertices from the offset. Data is stored in consecutive range defined by offset and count
        /// </summary>
        public byte VertexCount;

        /// <summary>
        /// Number of indices for this meshlet in MeshLetContainer.Indices from the offset. Data is stored in consecutive range defined by offset and count
        /// </summary>
        public ushort IndicesCount;

        /// <summary>
        /// Bounds
        /// </summary>
        public MeshletBounds Bounds;

    }

    /// <summary>
    /// Bounds of a meshlet
    /// </summary>
    public struct MeshletBounds
    {
        /* bounding sphere, useful for frustum and occlusion culling */
        public Vector3 center;
        public float radius;

        /* normal cone, useful for backface culling */
        public Vector3 cone_apex;
        public Vector3 cone_axis;
        public float cone_cutoff; /* = cos(angle/2) */

        /* normal cone axis and cutoff, stored in 8-bit SNORM format; decode using x/127.0 */
        public byte cone_axis_s8_x;
        public byte cone_axis_s8_y;
        public byte cone_axis_s8_z;
        public byte cone_cutoff_s8;
    }

}
