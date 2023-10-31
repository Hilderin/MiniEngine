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
        /// Create a triangle mesh
        /// </summary>
        public static MeshDefinition CreateTriangleMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();

            Vector2 t11 = new Vector2(0.5f, 1.0f);  // TopCenter
            Vector2 t10 = new Vector2(1.0f, 0.0f);  // Bottom right
            Vector2 t00 = new Vector2(0.0f, 0.0f);  // Bottom left



            Vector3[] positions = new Vector3[3];
            positions[0] = new Vector3(0.0f, 0.5f, 0f);     // TopCenter
            positions[1] = new Vector3(0.5f, -0.5f, 0f);    // Bottom right
            positions[2] = new Vector3(-0.5f, -0.5f, 0f);   // Bottom left

            Vector3[] colors = new Vector3[3];
            colors[0] = new Vector3(1f, 0f, 0f);            // TopCenter
            colors[1] = new Vector3(0f, 1f, 0f);            // Bottom right
            colors[2] = new Vector3(0f, 0f, 1f);            // Bottom left

            Vector2[] texCoords = new Vector2[3];
            texCoords[0] = t11;                             // TopCenter
            texCoords[1] = t10;                             // Bottom right
            texCoords[2] = t00;                             // Bottom left

            int[] indices = new int[] {
                              0, 1, 2,                      // TopCenter, Bottom right, Bottom left
            };

            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Positions = positions,
                Colors = colors,
                Indices = indices,
                TexCoords = texCoords
            });

            return meshDef;
        }

        /// <summary>
        /// Create a Triangle mesh actor
        /// </summary>
        public static Mesh CreateTriangleMesh()
        {
            return Renderer.Current.CreateMesh(CreateTriangleMeshDefinition());
        }

        /// <summary>
        /// Create a pyramid mesh
        /// </summary>
        public static MeshDefinition CreatePyramidMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();


            Vector3[] positions = new Vector3[4];
            positions[0] = new Vector3(-1.0f, -1.0f, 0.5773f);
            positions[1] = new Vector3(0.0f, -1.0f, -1.15475f);
            positions[2] = new Vector3(1.0f, -1.0f, 0.5773f);
            positions[3] = new Vector3(0.0f, 1.0f, 0.0f);

            Vector3[] colors = new Vector3[4];
            colors[0] = new Vector3(1f, 0f, 0f);
            colors[1] = new Vector3(0f, 1f, 0f);
            colors[2] = new Vector3(0f, 0f, 1f);
            colors[3] = new Vector3(1f, 1f, 0f);

            Vector2[] texCoords = new Vector2[4];
            texCoords[0] = new Vector2(0.0f, 0.0f);
            texCoords[1] = new Vector2(0.5f, 0.0f);
            texCoords[2] = new Vector2(1.0f, 0.0f);
            texCoords[3] = new Vector2(0.5f, 1.0f);

            //Indices...
            int[] indices = new int[] {
                              0, 3, 1,
                              1, 3, 2,
                              2, 3, 0,
                              0, 1, 2
            };


            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Positions = positions,
                Colors = colors,
                Indices = indices,
                TexCoords = texCoords
            });

            return meshDef;
        }

        /// <summary>
        /// Create a pyramid mesh
        /// </summary>
        public static Mesh CreatePyramidMesh()
        {
            return Renderer.Current.CreateMesh(CreatePyramidMeshDefinition());
        }


        /// <summary>
        /// Create a plane mesh
        /// </summary>
        public static MeshDefinition CreatePlaneMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();


            Vector2 t11 = new Vector2(1.0f, 1.0f);  // Top right
            Vector2 t01 = new Vector2(0.0f, 1.0f);  // Top left
            Vector2 t10 = new Vector2(1.0f, 0.0f);  // Bottom right
            Vector2 t00 = new Vector2(0.0f, 0.0f);  // Bottom left           



            Vector3[] positions = new Vector3[4];
            positions[0] = new Vector3(0.5f, 0.5f, 0f);     // Top right
            positions[1] = new Vector3(-0.5f, 0.5f, 0f);    // Top left
            positions[2] = new Vector3(0.5f, -0.5f, 0f);    // Bottom right
            positions[3] = new Vector3(-0.5f, -0.5f, 0f);   // Bottom left

            Vector3[] colors = new Vector3[4];
            colors[0] = new Vector3(1f, 0f, 0f);            // Top right
            colors[1] = new Vector3(0f, 1f, 0f);            // Top left
            colors[2] = new Vector3(0f, 0f, 1f);            // Bottom right
            colors[3] = new Vector3(1f, 1f, 1f);            // Bottom left

            Vector2[] texCoords = new Vector2[4];
            texCoords[0] = t11;                             // Top right
            texCoords[1] = t01;                             // Top left
            texCoords[2] = t10;                             // Bottom right
            texCoords[3] = t00;                             // Bottom left

            int[] indices = new int[] {
                              3, 1, 0,                      // Bottom left, Top left, Top right
                              3, 0, 2                       // Bottom left, Top right, Bottom right
            };



            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Positions = positions,
                Colors = colors,
                Indices = indices,
                TexCoords = texCoords
            });

            return meshDef;

        }

        /// <summary>
        /// Create a plane mesh
        /// </summary>
        public static Mesh CreatePlaneMesh()
        {
            return Renderer.Current.CreateMesh(CreatePlaneMeshDefinition());
        }

        /// <summary>
        /// Create a cube mesh
        /// </summary>
        public static MeshDefinition CreateCubeMeshDefinition()
        {
            MeshDefinition meshDef = new MeshDefinition();


            Vector2 t00 = new Vector2(0.0f, 0.0f);  // Bottom left
            Vector2 t01 = new Vector2(0.0f, 1.0f);  // Top left
            Vector2 t10 = new Vector2(1.0f, 0.0f);  // Bottom right
            Vector2 t11 = new Vector2(1.0f, 1.0f);  // Top right

            Vector3[] positions = new Vector3[8];
            positions[0] = new Vector3(0.5f, 0.5f, 0.5f);
            positions[1] = new Vector3(-0.5f, 0.5f, -0.5f);
            positions[2] = new Vector3(-0.5f, 0.5f, 0.5f);
            positions[3] = new Vector3(0.5f, -0.5f, -0.5f);
            positions[4] = new Vector3(-0.5f, -0.5f, -0.5f);
            positions[5] = new Vector3(0.5f, 0.5f, -0.5f);
            positions[6] = new Vector3(0.5f, -0.5f, 0.5f);
            positions[7] = new Vector3(-0.5f, -0.5f, 0.5f);

            Vector3[] colors = new Vector3[8];
            colors[0] = new Vector3(1f, 0f, 0f);
            colors[1] = new Vector3(0f, 1f, 0f);
            colors[2] = new Vector3(0f, 0f, 1f);
            colors[3] = new Vector3(1f, 1f, 0f);
            colors[4] = new Vector3(0f, 1f, 1f);
            colors[5] = new Vector3(1f, 0f, 1f);
            colors[6] = new Vector3(0f, 0f, 0f);
            colors[7] = new Vector3(1f, 1f, 1f);

            Vector2[] texCoords = new Vector2[8];
            texCoords[0] = t00;
            texCoords[1] = t01;
            texCoords[2] = t10;
            texCoords[3] = t11;
            texCoords[4] = t00;
            texCoords[5] = t10;
            texCoords[6] = t01;
            texCoords[7] = t11;

            int[] indices = new int[] {
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
                Positions = positions,
                Colors = colors,
                Indices = indices,
                TexCoords = texCoords
            });

            return meshDef;
        }

        /// <summary>
        /// Create a cube mesh
        /// </summary>
        public static Mesh CreateCubeMesh()
        {
            return Renderer.Current.CreateMesh(CreateCubeMeshDefinition());
        }
    }
}
