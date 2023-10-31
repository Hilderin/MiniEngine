using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Create primitives GameObjects
    /// </summary>
    public static class PrimitiveObjects
    {
        /// <summary>
        /// Create a Triangle mesh actor
        /// </summary>
        public static MeshObject CreateTriangleMeshObject()
        {
            return new MeshObject()
            {
                Mesh = Primitives.CreateTriangleMesh()
            };
        }

        /// <summary>
        /// Create a Pyramid mesh actor
        /// </summary>
        public static MeshObject CreatePyramidMeshObject()
        {
            return new MeshObject()
            {
                Mesh = Primitives.CreatePyramidMesh()
            };
        }

        /// <summary>
        /// Create a Plane mesh actor
        /// </summary>
        public static MeshObject CreatePlaneMeshObject()
        {
            return new MeshObject()
            {
                Mesh = Primitives.CreatePlaneMesh()
            };
        }

        /// <summary>
        /// Create a cube mesh actor
        /// </summary>
        public static MeshObject CreateCubeMeshObject()
        {
            return new MeshObject()
            {
                Mesh = Primitives.CreateCubeMesh()
            };
        }
    }
}
