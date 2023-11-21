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
    public class MeshLetContainer
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

    }

}
