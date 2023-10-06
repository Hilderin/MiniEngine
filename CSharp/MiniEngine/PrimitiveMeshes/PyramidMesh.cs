using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.PrimitiveMeshes
{
    /// <summary>
    /// Pyramid primitive
    /// </summary>
    public class PyramidMesh : Mesh
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PyramidMesh() : base()
        {
            Vector2 t00 =  new Vector2(0.0f, 0.0f);
            Vector2 t050 = new Vector2(0.5f, 0.0f);
            Vector2 t10 =  new Vector2(1.0f, 0.0f);
            Vector2 t051 = new Vector2(0.5f, 1.0f);

            Vertex[] vertices = new Vertex[4];
            vertices[0] = new Vertex(-1.0f, -1.0f, 0.5773f, t00);
            vertices[1] = new Vertex(0.0f, -1.0f, -1.15475f, t050);
            vertices[2] = new Vertex(1.0f, -1.0f, 0.5773f, t10);
            vertices[3] = new Vertex(0.0f, 1.0f, 0.0f, t051);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = new Vector3(Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f));
            }

            SetVertices(vertices);

            SetIndices(new int[] {
                              0, 3, 1,
                              1, 3, 2,
                              2, 3, 0,
                              0, 1, 2
            });
        }
    }
}
