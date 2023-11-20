using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Create primitives meshs
    /// </summary>
    public static class Primitives
    {
        /// <summary>
        /// Create a triangle mesh definition
        /// </summary>
        public static MeshDefinition CreateTriangleMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();

            Vector2 t11 = new Vector2(0.5f, 1.0f);  // TopCenter
            Vector2 t10 = new Vector2(1.0f, 0.0f);  // Bottom right
            Vector2 t00 = new Vector2(0.0f, 0.0f);  // Bottom left



            Vertex[] vertices = new Vertex[3];
            vertices[0] = new Vertex(0.0f, 0.5f, 0f, t11);     // TopCenter
            vertices[1] = new Vertex(0.5f, -0.5f, 0f, t10);    // Bottom right
            vertices[2] = new Vertex(-0.5f, -0.5f, 0f, t00);   // Bottom left


            uint[] indices = new uint[] {
                              0, 1, 2,                      // TopCenter, Bottom right, Bottom left
            };

            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Vertices = vertices,
                Indices = indices
            });

            if (Renderer.Current != null)
                meshDef.Materials.Add(BaseMaterials.Default);

            return meshDef;
        }

        /// <summary>
        /// Create a Triangle mesh
        /// </summary>
        public static Mesh CreateTriangleMesh()
        {
            return Renderer.Current.CreateMesh()
                                   .Load(CreateTriangleMeshDefinition());
        }

        /// <summary>
        /// Create a pyramid mesh definition
        /// </summary>
        public static MeshDefinition CreatePyramidMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();


            Vertex[] vertices = new Vertex[4];
            vertices[0] = new Vertex(-1.0f, -1.0f, 0.5773f, new Vector2(0.0f, 0.0f));
            vertices[1] = new Vertex(0.0f, -1.0f, -1.15475f, new Vector2(0.5f, 0.0f));
            vertices[2] = new Vertex(1.0f, -1.0f, 0.5773f, new Vector2(1.0f, 0.0f));
            vertices[3] = new Vertex(0.0f, 1.0f, 0.0f, new Vector2(0.5f, 1.0f));

            //Indices...
            uint[] indices = new uint[] {
                              0, 3, 1,
                              1, 3, 2,
                              2, 3, 0,
                              0, 1, 2
            };


            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Vertices = vertices,
                Indices = indices
            });

            if (Renderer.Current != null)
                meshDef.Materials.Add(BaseMaterials.Default);

            return meshDef;
        }

        /// <summary>
        /// Create a pyramid mesh
        /// </summary>
        public static Mesh CreatePyramidMesh()
        {
            return Renderer.Current.CreateMesh()
                                   .Load(CreatePyramidMeshDefinition());
        }


        /// <summary>
        /// Create a plane mesh definition
        /// </summary>
        public static MeshDefinition CreatePlaneMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();


            Vector2 t11 = new Vector2(1.0f, 1.0f);  // Top right
            Vector2 t01 = new Vector2(0.0f, 1.0f);  // Top left
            Vector2 t10 = new Vector2(1.0f, 0.0f);  // Bottom right
            Vector2 t00 = new Vector2(0.0f, 0.0f);  // Bottom left           


            Vertex[] vertices = new Vertex[4];
            vertices[0] = new Vertex(0.5f, 0.5f, 0f, t11);     // Top right
            vertices[1] = new Vertex(-0.5f, 0.5f, 0f, t01);    // Top left
            vertices[2] = new Vertex(0.5f, -0.5f, 0f, t10);    // Bottom right
            vertices[3] = new Vertex(-0.5f, -0.5f, 0f, t00);   // Bottom left

            Vector2[] texCoords = new Vector2[4];
            texCoords[0] = t11;                             // Top right
            texCoords[1] = t01;                             // Top left
            texCoords[2] = t10;                             // Bottom right
            texCoords[3] = t00;                             // Bottom left

            uint[] indices = new uint[] {
                              3, 1, 0,                      // Bottom left, Top left, Top right
                              3, 0, 2                       // Bottom left, Top right, Bottom right
            };



            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Vertices = vertices,
                Indices = indices
            });

            if (Renderer.Current != null)
                meshDef.Materials.Add(BaseMaterials.Default);

            return meshDef;

        }

        /// <summary>
        /// Create a plane mesh
        /// </summary>
        public static Mesh CreatePlaneMesh()
        {
            return Renderer.Current.CreateMesh()
                                   .Load(CreatePlaneMeshDefinition());
        }

        /// <summary>
        /// Create a cube mesh definition
        /// </summary>
        public static MeshDefinition CreateCubeMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();


            Vector2 t00 = new Vector2(0.0f, 0.0f);  // Bottom left
            Vector2 t01 = new Vector2(0.0f, 1.0f);  // Top left
            Vector2 t10 = new Vector2(1.0f, 0.0f);  // Bottom right
            Vector2 t11 = new Vector2(1.0f, 1.0f);  // Top right

            Vertex[] vertices = new Vertex[8];
            vertices[0] = new Vertex(0.5f, 0.5f, 0.5f, t00);
            vertices[1] = new Vertex(-0.5f, 0.5f, -0.5f, t01);
            vertices[2] = new Vertex(-0.5f, 0.5f, 0.5f, t10);
            vertices[3] = new Vertex(0.5f, -0.5f, -0.5f, t11);
            vertices[4] = new Vertex(-0.5f, -0.5f, -0.5f, t00);
            vertices[5] = new Vertex(0.5f, 0.5f, -0.5f, t10);
            vertices[6] = new Vertex(0.5f, -0.5f, 0.5f, t01);
            vertices[7] = new Vertex(-0.5f, -0.5f, 0.5f, t11);

            uint[] indices = new uint[] {
                              0, 1, 2,
                              1, 3, 4,
                              5, 6, 3,
                              7, 3, 6,
                              2, 4, 7,
                              0, 7, 6,
                              0, 5, 1,
                              1, 5, 3,
                              5, 0, 6,
                              7, 4, 3,
                              2, 1, 4,
                              0, 2, 7
            };

            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Vertices = vertices,
                Indices = indices
            });

            if(Renderer.Current != null)
                meshDef.Materials.Add(BaseMaterials.Default);

            return meshDef;
        }

        /// <summary>
        /// Create a cube mesh
        /// </summary>
        public static Mesh CreateCubeMesh()
        {
            return Renderer.Current.CreateMesh()
                                   .Load(CreateCubeMeshDefinition());
        }


        /// <summary>
        /// Create a empty mesh definition
        /// </summary>
        public static MeshDefinition CreateEmptyMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();


            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Vertices = new Vertex[0],
                Indices = new uint[0]
            });

            return meshDef;
        }

        /// <summary>
        /// Create a empty mesh
        /// </summary>
        public static Mesh CreateEmptyMesh()
        {
            return Renderer.Current.CreateMesh()
                                   .Load(CreateEmptyMeshDefinition());
        }
    }
}
