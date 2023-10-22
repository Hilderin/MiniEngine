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

            AddMeshData(positions, texCoords, new Vector3[positions.Length], colors, indices, 0);

            SetMaterial(Material.Default, 0);

        }
    }
}
