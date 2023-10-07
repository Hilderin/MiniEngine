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
        private Mesh _mesh = null;

        /// <summary>
        /// Import a mesh from file
        /// </summary>
        public Mesh GetMeshFromFile(string path, bool inverseFaces)
        {
            _workingDirectory = Path.GetDirectoryName(path);

            using (AssimpContext context = new AssimpContext())
            {
                PostProcessSteps postProcessSteps = PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.MakeLeftHanded;
                if (!inverseFaces)
                    postProcessSteps = postProcessSteps | PostProcessSteps.FlipWindingOrder;

                Scene scene = context.ImportFile(path, postProcessSteps);

                _mesh = new Mesh(scene.MeshCount, scene.MaterialCount);

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

            //Ambient color...
            if(assmat.HasColorAmbient)
                material.AmbientColor = new Color3(assmat.ColorAmbient.R, assmat.ColorAmbient.G, assmat.ColorAmbient.B);

            //Diffuse color...
            if (assmat.HasColorDiffuse)
                material.DiffuseColor = new Color3(assmat.ColorDiffuse.R, assmat.ColorDiffuse.G, assmat.ColorDiffuse.B);

            //Specular color...
            if (assmat.HasColorSpecular)
                material.SpecularColor = new Color3(assmat.ColorSpecular.R, assmat.ColorSpecular.G, assmat.ColorSpecular.B);

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
                    return new AssetManager().GetTexture2DFromFile(Path.Combine(_workingDirectory, assTexture.FilePath));
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

            _mesh.AddMeshData(positions, texCoords, normals, indices, mesh.MaterialIndex);

        }


    }
}
