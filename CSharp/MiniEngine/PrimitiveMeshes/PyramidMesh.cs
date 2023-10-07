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

            Vector3[] positions = new Vector3[4];
            positions[0] = new Vector3(-1.0f, -1.0f, 0.5773f);
            positions[1] = new Vector3(0.0f, -1.0f, -1.15475f);
            positions[2] = new Vector3(1.0f, -1.0f, 0.5773f);
            positions[3] = new Vector3(0.0f, 1.0f, 0.0f);

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

            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    vertices[i].Color = new Vector3(Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f));
            //}

            AddMeshData(positions, texCoords, new Vector3[positions.Length], indices, 0);

        }
    }
}
