using Assimp;
using System.Collections.Generic;
using System.IO;

namespace MiniEngine.AssertManager
{
    internal class MeshImporter
    {
        
        //private List<Vector3> _positions = new List<Vector3>();
        //private List<Vector3> _normals = new List<Vector3>();
        //private List<Vector3> _texCoords = new List<Vector3>();
        //private List<int> _indices = new List<int>();

        //private List<Material> materials = new List<Material>();

        private string _workingDirectory = null;
        private Mesh2 _mesh = null;

        /// <summary>
        /// Import a mesh from file
        /// </summary>
        public Mesh2 GetMeshFromFile(string path)
        {
            _workingDirectory = Path.GetDirectoryName(path);

            using (AssimpContext context = new AssimpContext())
            {
                Scene scene = context.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateSmoothNormals);

                _mesh = new Mesh2(scene.MeshCount, scene.MaterialCount);

                //Loading meshes.....
                for (int i = 0; i < scene.MeshCount; i++)
                {
                    LoadMesh(scene.Meshes[i], i);
                }

                //Loading textures.....
                for (int i = 0; i < scene.MaterialCount; i++)
                {
                    LoadMaterial(scene.Materials[i], i);
                }


            }

            //All we need now is to init the mesh!
            _mesh.Init();

            return _mesh;

        }

        /// <summary>
        /// Init a material
        /// </summary>
        private void LoadMaterial(Assimp.Material assmat, int matIndex)
        {
            Material material = new Material();

            //Diffuse texture...
            material.Diffuse = GetTexture(TextureType.Diffuse, assmat);

            //Specular texture...
            material.Specular = GetTexture(TextureType.Specular, assmat);

            _mesh.SetMaterial(material, matIndex);

        }

        /// <summary>
        /// Get a texture of a type
        /// </summary>
        private Texture2D GetTexture(TextureType type, Assimp.Material assmat)
        {
            //Diffuse texture...
            if (assmat.GetMaterialTextureCount(type) > 0)
            {
                if (assmat.GetMaterialTexture(type, 0, out TextureSlot assTexture))
                {
                    return new Texture2D(Path.Combine(_workingDirectory, assTexture.FilePath));
                }
            }
            return null;
        }


        /// <summary>
        /// Init a mesh
        /// </summary>
        private void LoadMesh(Assimp.Mesh mesh, int meshIndex)
        {
            Vector3[] positions = new Vector3[mesh.Vertices.Count];
            Vector3[] normals = new Vector3[mesh.Vertices.Count];
            Vector2[] texCoords = new Vector2[mesh.Vertices.Count];
            int[] indices = new int[mesh.FaceCount * 3];

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                positions[i] = new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z);

                if (mesh.HasNormals)
                    normals[i] = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                else
                    normals[i] = Vector3.Up;

                if (mesh.HasTextureCoords(0))
                    texCoords[i] = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
                else
                    texCoords[i] = Vector2.Zero;
            }

            int indexIndice = 0;
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                indices[indexIndice++] = mesh.Faces[i].Indices[0];
                indices[indexIndice++] = mesh.Faces[i].Indices[1];
                indices[indexIndice++] = mesh.Faces[i].Indices[2];
            }

            _mesh.SetMeshData(positions, texCoords, normals, indices, mesh.MaterialIndex, meshIndex);

        }


    }
}
