using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.PrimitiveMeshes
{
    /// <summary>
    /// Plane primitive
    /// </summary>
    public class PlaneMesh : Mesh
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PlaneMesh() : base()
        {
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

            AddMeshData(positions, texCoords, new Vector3[positions.Length], colors, indices, 0);

            SetMaterial(Material.Default, 0);

        }
    }
}
