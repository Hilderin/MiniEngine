using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MiniEngine.Core.MeshOptimization
{
    /// <summary>
    /// Generates meshlet from mesh
    /// Based on MeshOptimizer from https://github.com/zeux/meshoptimizer
    /// </summary>
    public class MeshletGenerator
    {
        // This must be <= 255 since index byte.MaxValue is used internally to indice a vertex that doesn't belong to a meshlet
        private const int kMeshletMaxVertices = 255;

        // A reasonable limit is around 2*max_vertices or less
        private const int kMeshletMaxTriangles = 512;

        private int max_vertices = kMeshletMaxVertices;
        private int max_triangles = kMeshletMaxTriangles;
        private float cone_weight = 1f;

        private List<Vertex> new_vertices;
        private uint[] meshlet_vertices;
        private uint[] meshlet_indices;

        public MeshDefinition Convert(MeshDefinition meshDef)
        {
            MeshDefinition newMeshDef = new MeshDefinition();
            newMeshDef.Materials.AddRange(meshDef.Materials);

            for (int i = 0; i < meshDef.SubMeshes.Count; i++)
            {

                List<Meshlet> meshlets = new List<Meshlet>();
                Generate(meshDef.SubMeshes[i], meshlets);

                SubMeshDefinition newSubMesh = new SubMeshDefinition();
                newSubMesh.MaterialIndex = meshDef.SubMeshes[i].MaterialIndex;
                newSubMesh.Vertices = new_vertices.ToArray();
                newSubMesh.Indices = meshlet_indices;
                newSubMesh.Meshlets = meshlets.ToArray();

                newMeshDef.SubMeshes.Add(newSubMesh);
            }

            return newMeshDef;
        }

        private void Generate(SubMeshDefinition subMeshDef, List<Meshlet> meshlets)
        {

            int index_count = subMeshDef.Indices.Length;
            int vertex_count = subMeshDef.Vertices.Length;
            int face_count = index_count / 3;
            meshlet_vertices = new uint[subMeshDef.Indices.Length];
            meshlet_indices = new uint[subMeshDef.Indices.Length];
            new_vertices = new List<Vertex>(subMeshDef.Indices.Length);

            Debug.Assert(index_count % 3 == 0);

            Debug.Assert(max_vertices >= 3 && max_vertices <= kMeshletMaxVertices);
            Debug.Assert(max_triangles >= 1 && max_triangles <= kMeshletMaxTriangles);
            Debug.Assert(max_triangles % 4 == 0); // ensures the caller will compute output space properly as index data is 4b aligned

            Debug.Assert(cone_weight >= 0 && cone_weight <= 1);


            TriangleAdjacency adjacency = BuildTriangleAdjacency(subMeshDef);

            uint[] live_vertices = new uint[vertex_count];
            Array.Copy(adjacency.vertices_usage_count, live_vertices, live_vertices.Length);


            byte[] emitted_flags = new byte[face_count];

            // for each triangle, precompute centroid & normal to use for scoring
            List<Cone> cones = ComputeTriangleCones(subMeshDef, out float mesh_area);


            // assuming each meshlet is a square patch, expected radius is sqrt(expected area)
            float triangle_area_avg = face_count == 0 ? 0f : mesh_area / face_count * 0.5f;
            float meshlet_expected_radius = Math.Sqrt(triangle_area_avg * max_triangles) * 0.5f;

            // build a kd-tree for nearest neighbor lookup
            uint[] kdindices = new uint[face_count];
            for (int i = 0; i < face_count; ++i)
                kdindices[i] = (uint)i;

            KDNode[] nodes = new KDNode[face_count * 2];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new KDNode();
            }
            KDTreeBuild(0, nodes, cones, kdindices, 0, face_count);

            //index of the vertex in the meshlet, byte.MaxValue if the vertex isn't used
            byte[] used = new byte[vertex_count];
            Array.Fill(used, byte.MaxValue);

            Meshlet meshlet = new Meshlet();
            Cone meshlet_cone_acc = new Cone();

            while (true)
            {
                Cone meshlet_cone = GetMeshletCone(meshlet_cone_acc, meshlet.indices_count);

                uint best_extra;
                uint best_triangle = GetNeighborTriangle(subMeshDef, meshlet, meshlet_cone, adjacency, cones, live_vertices, used, meshlet_expected_radius, out best_extra);

                // if the best triangle doesn't fit into current meshlet, the spatial scoring we've used is not very meaningful, so we re-select using topological scoring
                if (best_triangle != uint.MaxValue && (meshlet.vertex_count + best_extra > max_vertices || meshlet.indices_count >= max_triangles))
                {
                    best_triangle = GetNeighborTriangle(subMeshDef, meshlet, null, adjacency, cones, live_vertices, used, meshlet_expected_radius, out _);
                }

                // when we run out of neighboring triangles we need to switch to spatial search; we currently just pick the closest triangle irrespective of connectivity
                if (best_triangle == uint.MaxValue)
                {
                    Vector3 position = new(meshlet_cone.px, meshlet_cone.py, meshlet_cone.pz);
                    uint index = uint.MaxValue;
                    float limit = float.MaxValue;

                    KDTreeNearest(nodes, 0, cones, emitted_flags, ref position, ref index, ref limit);

                    best_triangle = index;
                }

                if (best_triangle == uint.MaxValue)
                    break;

                uint a = subMeshDef.Indices[best_triangle * 3 + 0], b = subMeshDef.Indices[best_triangle * 3 + 1], c = subMeshDef.Indices[best_triangle * 3 + 2];
                Debug.Assert(a < vertex_count && b < vertex_count && c < vertex_count);

                // add meshlet to the output; when the current meshlet is full we reset the accumulated bounds
                if (AppendMeshlet(ref meshlet, a, b, c, used, meshlets, subMeshDef))
                {
                    meshlet_cone_acc = new Cone();

                    
                }

                live_vertices[a]--;
                live_vertices[b]--;
                live_vertices[c]--;

                // remove emitted triangle from adjacency data
                // this makes sure that we spend less time traversing these lists on subsequent iterations
                for (int k = 0; k < 3; ++k)
                {
                    uint index = subMeshDef.Indices[best_triangle * 3 + k];

                    uint neighbors_index = adjacency.offsets[index];
                    uint neighbors_size = adjacency.vertices_usage_count[index];

                    for (int i = 0; i < neighbors_size; ++i)
                    {
                        uint tri = adjacency.triangle_indices[neighbors_index + i];

                        if (tri == best_triangle)
                        {
                            adjacency.triangle_indices[neighbors_index + i] = adjacency.triangle_indices[neighbors_index + neighbors_size - 1];
                            adjacency.vertices_usage_count[index]--;
                            break;
                        }
                    }
                }

                // update aggregated meshlet cone data for scoring subsequent triangles
                meshlet_cone_acc.px += cones[(int)best_triangle].px;
                meshlet_cone_acc.py += cones[(int)best_triangle].py;
                meshlet_cone_acc.pz += cones[(int)best_triangle].pz;
                meshlet_cone_acc.nx += cones[(int)best_triangle].nx;
                meshlet_cone_acc.ny += cones[(int)best_triangle].ny;
                meshlet_cone_acc.nz += cones[(int)best_triangle].nz;

                emitted_flags[best_triangle] = 1;
                
                
            }

            if (meshlet.indices_count > 0)
            {
                FinishMeshlet(meshlet, meshlet_indices);
                meshlets.Add(meshlet);
            }

            //Debug.Assert(meshlet_offset <= meshopt_buildMeshletsBound(index_count, max_vertices, max_triangles));
            //return meshlet_offset;
        }


        private void FinishMeshlet(Meshlet meshlet, uint[] meshlet_triangles)
        {
            int offset = (int)(meshlet.indices_offset + meshlet.indices_count);

            // fill 4b padding with 0
            while ((offset & 3) != 0)
                meshlet_triangles[offset++] = 0;
        }

        private bool AppendMeshlet(ref Meshlet meshlet, uint a, uint b, uint c, byte[] used, List<Meshlet> meshlets, SubMeshDefinition subMeshDef)
        {
            byte av = used[a];
            byte bv = used[b];
            byte cv = used[c];

            bool result = false;

            uint used_extra = 0;
            if (av == byte.MaxValue)
                used_extra++;
            if (bv == byte.MaxValue)
                used_extra++;
            if (cv == byte.MaxValue)
                used_extra++;


            if (meshlet.vertex_count + used_extra > max_vertices || meshlet.indices_count >= max_triangles)
            {
                meshlets.Add(meshlet);


                for (int j = 0; j < meshlet.vertex_count; ++j)
                    used[meshlet_vertices[meshlet.vertex_offset + j]] = byte.MaxValue;

                FinishMeshlet(meshlet, meshlet_indices);

                var newMeshlet = new Meshlet();
                newMeshlet.vertex_offset = newMeshlet.vertex_offset + meshlet.vertex_count;
                newMeshlet.indices_offset = newMeshlet.indices_offset + meshlet.indices_count;

                meshlet = newMeshlet;

                

                //meshlet.vertex_offset += meshlet.vertex_count;
                //meshlet.indices_offset += (uint)((meshlet.indices_count + 3) & ~3); // 4b padding
                //meshlet.vertex_count = 0;
                //meshlet.indices_count = 0;

                result = true;
            }

            if (av == byte.MaxValue)
            {
                av = (byte)meshlet.vertex_count;
                meshlet_vertices[meshlet.vertex_offset + meshlet.vertex_count++] = a;
                new_vertices.Add(subMeshDef.Vertices[a]);
                used[a] = av;
            }

            if (bv == byte.MaxValue)
            {
                bv = (byte)meshlet.vertex_count;
                meshlet_vertices[meshlet.vertex_offset + meshlet.vertex_count++] = b;
                new_vertices.Add(subMeshDef.Vertices[b]);
                used[b] = bv;
            }

            if (cv == byte.MaxValue)
            {
                cv = (byte)meshlet.vertex_count;
                meshlet_vertices[meshlet.vertex_offset + meshlet.vertex_count++] = c;
                new_vertices.Add(subMeshDef.Vertices[c]);
                used[c] = cv;
            }

            meshlet_indices[meshlet.indices_offset + meshlet.indices_count + 0] = av;
            meshlet_indices[meshlet.indices_offset + meshlet.indices_count + 1] = bv;
            meshlet_indices[meshlet.indices_offset + meshlet.indices_count + 2] = cv;
            meshlet.indices_count += 3;

            return result;
        }

        /// <summary>
        /// Build Trianbles Adjacency
        /// </summary>
        private TriangleAdjacency BuildTriangleAdjacency(SubMeshDefinition subMeshDef)
        {
            uint[] indices = subMeshDef.Indices;

            int index_count = indices.Length;
            int face_count = index_count / 3;
            int vertex_count = subMeshDef.Vertices.Length;


            TriangleAdjacency adjacency = new TriangleAdjacency();

            // allocate arrays
            adjacency.vertices_usage_count = new uint[vertex_count];
            adjacency.offsets = new uint[vertex_count];
            adjacency.triangle_indices = new uint[index_count];


            for (int i = 0; i < index_count; ++i)
            {
                Debug.Assert(indices[i] < vertex_count);

                adjacency.vertices_usage_count[indices[i]]++;
            }

            // fill offset table
            uint offset = 0;

            for (int i = 0; i < vertex_count; ++i)
            {
                adjacency.offsets[i] = offset;
                offset += adjacency.vertices_usage_count[i];
            }

            Debug.Assert(offset == index_count);

            // fill triangle data
            for (int i = 0; i < face_count; ++i)
            {
                uint a = indices[i * 3], b = indices[i * 3 + 1], c = indices[i * 3 + 2];

                //Set the triangle index for each vertex index. 
                //that wa, we known each vertex is used in which triangle
                adjacency.triangle_indices[adjacency.offsets[a]++] = (uint)i;
                adjacency.triangle_indices[adjacency.offsets[b]++] = (uint)i;
                adjacency.triangle_indices[adjacency.offsets[c]++] = (uint)i;
            }

            // fix offsets that have been disturbed by the previous pass
            for (int i = 0; i < vertex_count; ++i)
            {
                Debug.Assert(adjacency.offsets[i] >= adjacency.vertices_usage_count[i]);

                adjacency.offsets[i] -= adjacency.vertices_usage_count[i];
            }

            return adjacency;
        }

        /// <summary>
        /// Compute triangle cones and return mesh area
        /// </summary>
        private List<Cone> ComputeTriangleCones(SubMeshDefinition subMeshDef, out float mesh_area)
        {
            uint[] indices = subMeshDef.Indices;
            Vertex[] vertices = subMeshDef.Vertices;

            int index_count = indices.Length;
            int face_count = index_count / 3;
            int vertex_count = vertices.Length;

            List<Cone> cones = new List<Cone>();

            mesh_area = 0;

            for (int i = 0; i < face_count; ++i)
            {
                uint a = indices[i * 3 + 0], b = indices[i * 3 + 1], c = indices[i * 3 + 2];
                Debug.Assert(a < vertex_count && b < vertex_count && c < vertex_count);

                Vector3 p0 = vertices[a].Pos;
                Vector3 p1 = vertices[b].Pos;
                Vector3 p2 = vertices[c].Pos;

                Vector3 p10 = new Vector3(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
                Vector3 p20 = new Vector3(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);

                float normalx = p10.Y * p20.Z - p10.Z * p20.Y;
                float normaly = p10.Z * p20.X - p10.X * p20.Z;
                float normalz = p10.X * p20.Y - p10.Y * p20.X;

                float area = Math.Sqrt(normalx * normalx + normaly * normaly + normalz * normalz);
                float invarea = (area == 0f) ? 0f : 1f / area;

                cones.Add(new Cone()
                {
                    px = (p0.X + p1.X + p2.X) / 3f,
                    py = (p0.Y + p1.Y + p2.Y) / 3f,
                    pz = (p0.Z + p1.Z + p2.Z) / 3f,

                    nx = normalx * invarea,
                    ny = normaly * invarea,
                    nz = normalz * invarea,
                });

                mesh_area += area;
            }


            return cones;
        }



        private int KDTreeBuild(int offset, KDNode[] nodes, List<Cone> cones, uint[] kdindices, int start_index, int stop_index)
        {

            Debug.Assert(stop_index > 0);
            Debug.Assert(offset < nodes.Length);

            if (stop_index <= 1)
                return KDTreeBuildLeaf(offset, nodes, kdindices, start_index, stop_index);

            Vector3 mean = Vector3.Zero;
            Vector3 vars = Vector3.Zero;
            float runc = 1, runs = 1;

            // gather statistics on the points in the subtree using Welford's algorithm
            for (int i = start_index; i < stop_index; ++i, runc += 1f, runs = 1f / runc)
            {
                Cone pone = cones[(int)kdindices[i]];

                for (int k = 0; k < 3; ++k)
                {
                    float delta = pone[k] - mean[k];
                    mean[k] += delta * runs;
                    vars[k] += delta * (pone[k] - mean[k]);
                }
            }

            // split axis is one where the variance is largest
            int axis;
            if (vars.X >= vars.Y && vars.X >= vars.Z)
                //X axis...
                axis = 0;
            else if (vars.Y >= vars.Z)
                //Y axis...
                axis = 1;
            else
                //Z axis
                axis = 2;

            float split = mean[axis];
            int middle = KDTreePartition(cones, kdindices, start_index, stop_index, axis, split);

            // when the partition is degenerate simply consolidate the points into a single node
            if (middle <= 0 || middle >= stop_index)
                return KDTreeBuildLeaf(offset, nodes, kdindices, start_index, stop_index);

            KDNode result = nodes[offset];

            result.split = split;
            result.axis = axis;

            // left subtree is right after our node
            int next_offset = KDTreeBuild(offset + 1, nodes, cones, kdindices, start_index, middle);

            // distance to the right subtree is represented explicitly
            result.children = next_offset - offset - 1;

            return KDTreeBuild(next_offset, nodes, cones, kdindices, middle, stop_index - middle);
        }



        private int KDTreePartition(List<Cone> cones, uint[] kdindices, int start_index, int stop_index, int axis, float pivot)
        {

            int m = 0;

            // invariant: elements in range [0, m) are < pivot, elements in range [m, i) are >= pivot
            for (int i = start_index; i < stop_index; ++i)
            {
                float v = cones[(int)kdindices[i]][axis];


                if (m != i)
                {
                    // swap(m, i)
                    uint t = kdindices[m];
                    kdindices[m] = kdindices[i];
                    kdindices[i] = t;
                }

                // when v >= pivot, we swap i with m without advancing it, preserving invariants
                if (v < pivot)
                    m++;
            }

            return m;
        }


        private int KDTreeBuildLeaf(int offset, KDNode[] nodes, uint[] kdindices, int start_index, int stop_index)
        {
            Debug.Assert(offset + stop_index <= nodes.Length);

            KDNode result = nodes[offset];

            result.index = kdindices[start_index];
            result.axis = 3;
            result.children = stop_index - 1;

            // all remaining points are stored in nodes immediately following the leaf
            for (int i = start_index + 1; i < stop_index; ++i)
            {
                KDNode tail = nodes[offset + i];

                tail.index = kdindices[i];
                tail.axis = 3;
                tail.children = Int32.MaxValue;
            }

            return offset + stop_index;
        }


        private Cone GetMeshletCone(Cone acc, uint triangle_count)
        {

            Cone result = acc.Clone();

            if (triangle_count > 0)
            {
                float center_scale = 1f / triangle_count;
                result.px *= center_scale;
                result.py *= center_scale;
                result.pz *= center_scale;
            }

            float axis_length = result.nx * result.nx + result.ny * result.ny + result.nz * result.nz;
            if (axis_length != 0)
            {
                float axis_scale = 1f / Math.Sqrt(axis_length);

                result.nx *= axis_scale;
                result.ny *= axis_scale;
                result.nz *= axis_scale;
            }

            return result;
        }



        private uint GetNeighborTriangle(SubMeshDefinition subMeshDef, Meshlet meshlet, Cone meshlet_cone, TriangleAdjacency adjacency, List<Cone> cones, uint[] live_vertices, byte[] used, float meshlet_expected_radius, out uint out_extra)
        {

            uint best_triangle = uint.MaxValue;
            uint best_extra = 5;
            float best_score = float.MaxValue;

            for (int i = 0; i < meshlet.vertex_count; ++i)
            {

                uint index = meshlet_vertices[meshlet.vertex_offset + i];

                uint neighbors_index = adjacency.offsets[index];
                uint neighbors_size = adjacency.vertices_usage_count[index];

                for (int j = 0; j < neighbors_size; ++j)
                {
                    uint triangle = adjacency.triangle_indices[neighbors_index + j];
                    uint a = subMeshDef.Indices[triangle * 3 + 0], b = subMeshDef.Indices[triangle * 3 + 1], c = subMeshDef.Indices[triangle * 3 + 2];

                    uint extra = 0;
                    if (used[a] == byte.MaxValue)
                        extra++;
                    if (used[b] == byte.MaxValue)
                        extra++;
                    if (used[c] == byte.MaxValue)
                        extra++;

                    // triangles that don't add new vertices to meshlets are max. priority
                    if (extra != 0)
                    {
                        // artificially increase the priority of dangling triangles as they're expensive to add to new meshlets
                        if (live_vertices[a] == 1 || live_vertices[b] == 1 || live_vertices[c] == 1)
                            extra = 0;

                        extra++;
                    }

                    // since topology-based priority is always more important than the score, we can skip scoring in some cases
                    if (extra > best_extra)
                        continue;

                    float score;

                    // caller selects one of two scoring functions: geometrical (based on meshlet cone) or topological (based on remaining triangles)
                    if (meshlet_cone != null)
                    {
                        Cone tri_cone = cones[(int)triangle];

                        float distance2 =
                            (tri_cone.px - meshlet_cone.px) * (tri_cone.px - meshlet_cone.px) +
                            (tri_cone.py - meshlet_cone.py) * (tri_cone.py - meshlet_cone.py) +
                            (tri_cone.pz - meshlet_cone.pz) * (tri_cone.pz - meshlet_cone.pz);

                        float spread = tri_cone.nx * meshlet_cone.nx + tri_cone.ny * meshlet_cone.ny + tri_cone.nz * meshlet_cone.nz;

                        score = GetMeshletScore(distance2, spread, cone_weight, meshlet_expected_radius);
                    }
                    else
                    {
                        // each live_triangles entry is >= 1 since it includes the current triangle we're processing
                        score = live_vertices[a] + live_vertices[b] + live_vertices[c] - 3;
                    }

                    // note that topology-based priority is always more important than the score
                    // this helps maintain reasonable effectiveness of meshlet data and reduces scoring cost
                    if (extra < best_extra || score < best_score)
                    {
                        best_triangle = triangle;
                        best_extra = extra;
                        best_score = score;
                    }
                }
            }

            out_extra = best_extra;

            return best_triangle;
        }



        private void KDTreeNearest(KDNode[] nodes, uint root, List<Cone> cones, byte[] emitted_flags, ref Vector3 position, ref uint result, ref float limit)
        {
            KDNode node = nodes[root];

            if (node.axis == 3)
            {
                // leaf
                for (uint i = 0; i <= node.children; ++i)
                {
                    uint index = nodes[root + i].index;

                    if (emitted_flags[index] > 0)
                        continue;

                    Cone cone = cones[(int)index];

                    float distance2 =
                        (cone.px - position.X) * (cone.px - position.X) +
                        (cone.py - position.Y) * (cone.py - position.Y) +
                        (cone.pz - position.Z) * (cone.pz - position.Z);

                    float distance = Math.Sqrt(distance2);

                    if (distance < limit)
                    {
                        result = index;
                        limit = distance;
                    }
                }
            }
            else
            {
                // branch; we order recursion to process the node that search position is in first
                float delta = position[node.axis] - node.split;
                uint first = (delta <= 0) ? 0 : (uint)node.children;
                uint second = first ^ (uint)node.children;

                KDTreeNearest(nodes, root + 1 + first, cones, emitted_flags, ref position, ref result, ref limit);

                // only process the other node if it can have a match based on closest distance so far
                if (Math.Abs(delta) <= limit)
                    KDTreeNearest(nodes, root + 1 + second, cones, emitted_flags, ref position, ref result, ref limit);
            }
        }

        public float GetMeshletScore(float distance2, float spread, float cone_weight, float expected_radius)
        {
            float cone = 1f - spread * cone_weight;
            float cone_clamped = cone < 1e-3f ? 1e-3f : cone;

            return (1 + Math.Sqrt(distance2) / expected_radius * (1 - cone_weight)) * cone_clamped;
        }

        private class TriangleAdjacency
        {
            /// <summary>
            /// Number of times that a vertex is used
            /// </summary>
            public uint[] vertices_usage_count;
            public uint[] offsets;
            public uint[] triangle_indices;
        }

        private class Cone
        {
            public float px, py, pz;
            public float nx, ny, nz;

            public float this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return px;
                        case 1:
                            return py;
                        case 2:
                            return pz;
                        default:
                            return 0;
                    }
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            px = value;
                            break;
                        case 1:
                            py = value;
                            break;
                        case 2:
                            pz = value;
                            break;
                    }
                }
            }

            public Cone Clone()
            {
                return new Cone()
                {
                    px = this.px,
                    py = this.py,
                    pz = this.pz,
                    nx = this.nx,
                    ny = this.ny,
                    nz = this.nz
                };
            }
        }

        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
        private class KDNode
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public float split;

            [System.Runtime.InteropServices.FieldOffset(0)]
            public uint index;

            // leaves: axis = 3, children = number of extra points after this one (0 if 'index' is the only point)
            // branches: axis != 3, left subtree = skip 1, right subtree = skip 1+children
            [System.Runtime.InteropServices.FieldOffset(4)]
            public int axis = 3;

            [System.Runtime.InteropServices.FieldOffset(8)]
            public int children = 30;

        }

    }

}