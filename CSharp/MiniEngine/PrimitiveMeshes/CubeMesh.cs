using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.PrimitiveMeshes
{
    /// <summary>
    /// Cube primitive
    /// </summary>
    public class CubeMesh : Mesh
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CubeMesh() : base()
        {
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

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = new Vector3(Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f));
            }

            SetVertices(vertices);

            SetIndices(new int[] {
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
            });
        }
    }
}
