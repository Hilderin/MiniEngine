using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.PrimitiveMeshes
{
    /// <summary>
    /// Triangle primitive
    /// </summary>
    public class TriangleMesh : Mesh
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TriangleMesh() : base()
        {
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

            AddMeshData(positions, texCoords, new Vector3[positions.Length], colors, indices, 0);

            SetMaterial(Material.Default, 0);

        }
    }
}
