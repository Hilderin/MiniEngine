using Assimp;
using MiniEngine.AssetDefinitions;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.AssetImporters
{
    /// <summary>
    /// Mesh importer
    /// </summary>
    public class MeshImporter : IAssetImporter
    {
        /// <summary>
        /// Supported extensions
        /// </summary>
        private static readonly string[] SUPPORTED_EXTENSIONS = new string[] { ".fbx", ".dae", ".gltf", ".glb", ".blend", ".3ds", ".ase", ".obj", ".ifc", ".xgl", ".zgl", ".ply", ".dxf", ".lwo", ".lws", ".lxo", ".stl", ".x", ".ms3d", ".cob", ".scn" };

        private static readonly MeshAssetDefinition _defaultMeshAssetDefinition = new MeshAssetDefinition();

        private AssetManager _assetManager;

        /// <summary>
        /// Cache meshs
        /// </summary>
        private Dictionary<string, Mesh> _cache = new Dictionary<string, Mesh>();

        /// <summary>
        /// Mesh importer
        /// </summary>
        public MeshImporter(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }


        /// <summary>
        /// Import a mesh...
        /// </summary>
        public object Import(string name)
        {

            if (!_cache.TryGetValue(name, out Mesh mesh))
            {
                try
                {
                    string meshPath = String.Empty;
                    MeshAssetDefinition assetInfo;


                    string assetPath = _assetManager.GetAssetPath(name, ".amesh");


                    if (!String.IsNullOrEmpty(assetPath))
                    {
                        //We ave a definition file...
                        assetInfo = _assetManager.DeserializeFile<MeshAssetDefinition>(assetPath);

                        if (String.IsNullOrEmpty(assetInfo.MeshPath))
                            throw new FormatException($"MeshPath not set in '{assetPath}'");

                        if (!File.Exists(assetInfo.MeshPath))
                            throw new FileNotFoundException($"Mesh file not found: {assetInfo.MeshPath}");

                        meshPath = assetInfo.MeshPath;


                    }
                    else
                    {
                        //Search directly with the extensions...
                        meshPath = _assetManager.GetAssetPath(name, SUPPORTED_EXTENSIONS);

                        if (String.IsNullOrEmpty(meshPath))
                            throw new FileNotFoundException($"Mesh not found: {name} for supported extensions: {String.Join(", ", SUPPORTED_EXTENSIONS)}");

                        assetInfo = _defaultMeshAssetDefinition;

                    }

                    mesh = CreateMesh(meshPath, assetInfo);

                }
                catch (Exception ex)
                {
                    //TODO: Ajouter un warning dans l'engine
                    Debug.Print("Erreur: " + ex.ToString());
                }
                finally
                {
                    //Mesh not found...
                    mesh ??= Primitives.CreateEmptyMesh();
                }

                _cache.Add(name, mesh);
            }

            return mesh;

        }

        /// <summary>
        /// Reset the cache
        /// </summary>
        public void ResetCache()
        {
            _cache.Clear();
        }


        /// <summary>
        /// Import a mesh from file
        /// </summary>
        public Mesh CreateMesh(string path, MeshAssetDefinition meshAssetDef)
        {
            string workingDirectory = Path.GetDirectoryName(path);
            //Matrix3 transformMatrix = Matrix3.FromEulerAnglesXYZ(Math.DegToRad(90), 0f, 0f);
            Matrix3 transformMatrix = Matrix3.Identity;
            MeshDefinition meshDef = new MeshDefinition();

            using (AssimpContext context = new AssimpContext())
            {


                    //------------------
                    //WORKING GOOD:
                    PostProcessSteps postProcessSteps = PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.PreTransformVertices | PostProcessSteps.MakeLeftHanded;

                    //The base unit of fbx is centimeters...
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase))
                        context.Scale = 0.01f;

                    //------------------



                    //PostProcessSteps postProcessSteps = PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices;  // | PostProcessSteps.MakeLeftHanded;
                    //PostProcessSteps postProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.SplitLargeMeshes | PostProcessSteps.LimitBoneWeights | PostProcessSteps.RemoveRedundantMaterials | PostProcessSteps.SortByPrimitiveType | PostProcessSteps.FindInvalidData | PostProcessSteps.GenerateUVCoords | PostProcessSteps.FindInstances | PostProcessSteps.ValidateDataStructure | PostProcessSteps.OptimizeMeshes;


                    //PAS SUR: PostProcessSteps postProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.SplitLargeMeshes | PostProcessSteps.LimitBoneWeights | PostProcessSteps.RemoveRedundantMaterials | PostProcessSteps.SortByPrimitiveType | PostProcessSteps.FindDegenerates | PostProcessSteps.FindInvalidData | PostProcessSteps.GenerateUVCoords | PostProcessSteps.FindInstances | PostProcessSteps.ValidateDataStructure | PostProcessSteps.OptimizeMeshes;
                    //PAS SUR: PostProcessSteps postProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.LimitBoneWeights | PostProcessSteps.RemoveRedundantMaterials | PostProcessSteps.FindDegenerates | PostProcessSteps.FindInvalidData | PostProcessSteps.GenerateUVCoords | PostProcessSteps.FindInstances | PostProcessSteps.ValidateDataStructure;


                    //if (!meshAssetDef.InverseFaces)
                    //    postProcessSteps |= PostProcessSteps.FlipWindingOrder;


                    //if (meshAssetDef.Scale != 1f)
                    //    transformMatrix *= Matrix3.FromScaling(new Vector3(meshAssetDef.Scale));
                    //if (meshAssetDef.FlipY)
                    //    transformMatrix *= Matrix3.FromFlipY();

                    //if (meshAssetDef.SmoothNormals)
                    //    postProcessSteps |= PostProcessSteps.GenerateSmoothNormals;
                    //else
                    //    postProcessSteps |= PostProcessSteps.GenerateNormals;

                    //context.Scale = parameters.Scale;





                Assimp.Scene scene = context.ImportFile(path, postProcessSteps);

                if (scene.Metadata.TryGetValue("OriginalUpAxis", out var originalUpAxis) && scene.Metadata.TryGetValue("UpAxis", out var upAxis))
                {
                    //We will flip it if the up axis have been inverted...
                    if (!originalUpAxis.Data.Equals(upAxis.Data))
                    {
                        postProcessSteps |= PostProcessSteps.FlipWindingOrder;
                        scene = context.ImportFile(path, postProcessSteps);
                    }
                }

                //if(scene.MeshCount > 0 && scene.Meshes[0].po

                //if (scene.Metadata.TryGetValue("OriginalUnitScaleFactor", out var entry))
                //    Console.WriteLine(entry);

                //Loading meshes.....
                Dictionary<int, int> matIndexes = new Dictionary<int, int>();
                for (int i = 0; i < scene.MeshCount; i++)
                {
                    LoadMesh(scene.Meshes[i], meshDef, ref transformMatrix, matIndexes);
                }

                //Loading textures.....                
                for (int i = 0; i < matIndexes.Count; i++)
                {
                    meshDef.Materials.Add(CreateMaterial(scene.Materials[i], i, meshAssetDef, workingDirectory));
                }


                foreach (var subMesh in meshDef.SubMeshes)
                {
                    subMesh.MaterialIndex = matIndexes[subMesh.MaterialIndex];
                }

            }

            return _assetManager.Context.Renderer.CreateMesh(meshDef);


            //////Resetting ambient color on mats if asked...
            ////if (parameters.ResetMaterialAmbientColor)
            ////{
            ////    foreach (Material m in _mesh.Materials)
            ////        m.AmbientColor = Color3.White;
            ////}

            //return _mesh;

        }


        /// <summary>
        /// Load a mesh info meshDef
        /// </summary>
        private void LoadMesh(Assimp.Mesh mesh, MeshDefinition meshDef, ref Matrix3 transformMatrix, Dictionary<int, int> matIndexes)
        {
            Vector3[] positions = new Vector3[mesh.Vertices.Count];
            Vector3[] normals = new Vector3[mesh.Vertices.Count];
            Vector2[] texCoords = new Vector2[mesh.Vertices.Count];
            int[] indices = new int[mesh.FaceCount * 3];

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                positions[i] = transformMatrix * new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z);

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

            if (!matIndexes.ContainsKey(mesh.MaterialIndex))
                matIndexes.Add(mesh.MaterialIndex, matIndexes.Count);

            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Positions = positions,
                Indices = indices,
                TexCoords = texCoords,
                Normals = normals,
                MaterialIndex = mesh.MaterialIndex
            });

        }

        /// <summary>
        /// Create a material
        /// </summary>
        private Material CreateMaterial(Assimp.Material assmat, int matIndex, MeshAssetDefinition meshAssetDef, string workingDirectory)
        {

            if (matIndex < meshAssetDef.MaterialNames.Count)
            {
                //We have a material name...
                return _assetManager.Get<Material>(meshAssetDef.MaterialNames[matIndex]);
            }
            else
            {
                //Creation of a basic Material
                return _assetManager.Context.Renderer.CreateMaterial(new()
                {
                    DiffuseTexture = GetTexture(Assimp.TextureType.Diffuse, assmat, workingDirectory),
                    Shader = BaseShaders.Unlit
                });
            }

            //Material material = new Material();

            ////Diffuse texture...
            //material.Diffuse = GetTexture(Assimp.TextureType.Diffuse, assmat);

            ////Specular texture...
            //material.Specular = GetTexture(Assimp.TextureType.Shininess, assmat);

            ////Ambient color...
            //if (assmat.HasColorAmbient)
            //    material.AmbientColor = new Color3(assmat.ColorAmbient.R, assmat.ColorAmbient.G, assmat.ColorAmbient.B);

            ////Diffuse color...
            //if (assmat.HasColorDiffuse)
            //    material.DiffuseColor = new Color3(assmat.ColorDiffuse.R, assmat.ColorDiffuse.G, assmat.ColorDiffuse.B);

            ////Specular color...
            //if (assmat.HasColorSpecular)
            //    material.SpecularColor = new Color3(assmat.ColorSpecular.R, assmat.ColorSpecular.G, assmat.ColorSpecular.B);

            //_mesh.SetMaterial(material, matIndex);

        }

        /// <summary>
        /// Get a texture of a type
        /// </summary>
        private Texture2D GetTexture(Assimp.TextureType type, Assimp.Material assmat, string workingDirectory)
        {
            //Diffuse texture...
            if (assmat.GetMaterialTextureCount(type) > 0)
            {
                if (assmat.GetMaterialTexture(type, 0, out TextureSlot assTexture))
                {
                    return _assetManager.Get<Texture2D>(Path.Combine(workingDirectory, assTexture.FilePath));
                }
            }

            return BaseTextures.White;
        }



    }
}
