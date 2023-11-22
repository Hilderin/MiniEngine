using Assimp;
using MiniEngine.AssetDefinitions;
using MiniEngine.MeshOptimization;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public object Import(string name, string workingFolderUri)
        {

            if (!_cache.TryGetValue(name, out Mesh mesh))
            {
                try
                {
                    mesh = Renderer.Current.CreateMesh();

                    if (Renderer.Current.SupportAsyncAssetLoading)
                    {
                        LoadMeshAsync(name, workingFolderUri, mesh);
                    }
                    else
                    {
                        MeshAssetDefinition meshAssetDef = GetMeshAssetDefinition(name, workingFolderUri, mesh);
                        MeshDefinition meshDef = CreateMeshDefinition(meshAssetDef);
                        mesh.Load(meshDef);
                    }

                    }
                catch (Exception ex)
                {
                    Debug.Error(ex);
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
        /// Load the mesh async
        /// </summary>
        private void LoadMeshAsync(string name, string workingFolderUri, Mesh mesh)
        {


            ThreadPool.QueueUserWorkItem((arg) =>
            {
                try
                {
                    MeshAssetDefinition meshAssetDef = GetMeshAssetDefinition(name, workingFolderUri, mesh);
                    MeshDefinition meshDef = CreateMeshDefinition(meshAssetDef);
                    mesh.Load(meshDef);
                }
                catch (Exception ex)
                {
                    Debug.Error(ex);
                }
            });


        }

        /// <summary>
        /// Load the mesh asset definition from disk
        /// </summary>
        private MeshAssetDefinition GetMeshAssetDefinition(string name, string workingFolderUri, Mesh mesh)
        {

            string meshAssetPath;
            MeshAssetDefinition assetMeshDef = null;


            try
            {
                if (!_assetManager.TryFindAssetUri(name, workingFolderUri, true, out meshAssetPath))
                    throw new FileNotFoundException($"Mesh not found: {name}");

                string extension = Path.GetExtension(meshAssetPath).ToLower();

                if (extension == AssetManager.ASSET_EXTENSION_FILE)
                {
                    _assetManager.AssetUriToWatch(meshAssetPath, () => LoadMeshAsync(name, workingFolderUri, mesh));

                    //We have a definition file...
                    assetMeshDef = _assetManager.DeserializeAsset<MeshAssetDefinition>(meshAssetPath);

                    if (String.IsNullOrEmpty(assetMeshDef.MeshPath))
                        throw new FormatException($"MeshPath not set in '{meshAssetPath}'");

                    string modelAssetUri;
                    if(!_assetManager.TryFindAssetUri(assetMeshDef.MeshPath, AssetManager.GetDirectoryName(meshAssetPath), false, out modelAssetUri))
                        throw new FileNotFoundException($"Mesh file not found: {assetMeshDef.MeshPath}");

                    assetMeshDef.ModelAssetUri = modelAssetUri;

                    _assetManager.AssetUriToWatch(assetMeshDef.ModelAssetUri, () => LoadMeshAsync(name, workingFolderUri, mesh));
                }
                else
                {
                    //Not a .asset file...                    
                    assetMeshDef = new MeshAssetDefinition();
                    assetMeshDef.MeshPath = Path.GetFileName(meshAssetPath);
                    assetMeshDef.ModelAssetUri = meshAssetPath;

                    _assetManager.AssetUriToWatch(meshAssetPath, () => LoadMeshAsync(name, workingFolderUri, mesh));

                    //Create a .asset file..
                    if (meshAssetPath.StartsWith(AssetManager.PREFIX_URI_FILE))
                    {
                        string assetDefPath = meshAssetPath + AssetManager.ASSET_EXTENSION_FILE;
                        _assetManager.SerializeFile(assetMeshDef, assetDefPath);
                    }

                }



            }
            catch (Exception ex)
            {
                Debug.Error(ex);
            }
            finally
            {
                assetMeshDef ??= new MeshAssetDefinition()
                    {
                        MeshPath = name
                    };
            }

            

            return assetMeshDef;
        }


        /// <summary>
        /// Create the mesh definition from an mesh asset definition
        /// </summary>
        private MeshDefinition CreateMeshDefinition(MeshAssetDefinition meshAssetDef)
        {
    
            if (String.IsNullOrEmpty(meshAssetDef.ModelAssetUri))
                return Primitives.CreateEmptyMeshDefinition();


            MeshDefinition meshDef = new MeshDefinition();

            string workingDirectory = AssetManager.GetDirectoryName(meshAssetDef.ModelAssetUri);
            //Matrix3 transformMatrix = Matrix3.FromEulerAnglesXYZ(Math.DegToRad(90), 0f, 0f);
            Matrix4x4 transformMatrix = Matrix4x4.Identity;
            //MeshDefinition meshDef = new MeshDefinition();

            using (AssimpContext context = new AssimpContext())
            {


                //------------------
                //WORKING GOOD:
                PostProcessSteps postProcessSteps = PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateNormals | PostProcessSteps.TransformUVCoords;
                //PostProcessSteps.MakeLeftHanded | 

                //The base unit of fbx is centimeters...
                if (Path.GetExtension(meshAssetDef.ModelAssetUri).Equals(".fbx", StringComparison.OrdinalIgnoreCase))
                    context.Scale = 0.01f;

                //------------------



                //PostProcessSteps postProcessSteps = PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices;  // | PostProcessSteps.MakeLeftHanded;
                //PostProcessSteps postProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.SplitLargeMeshes | PostProcessSteps.LimitBoneWeights | PostProcessSteps.RemoveRedundantMaterials | PostProcessSteps.SortByPrimitiveType | PostProcessSteps.FindInvalidData | PostProcessSteps.GenerateUVCoords | PostProcessSteps.FindInstances | PostProcessSteps.ValidateDataStructure | PostProcessSteps.OptimizeMeshes;


                //PAS SUR: PostProcessSteps postProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.SplitLargeMeshes | PostProcessSteps.LimitBoneWeights | PostProcessSteps.RemoveRedundantMaterials | PostProcessSteps.SortByPrimitiveType | PostProcessSteps.FindDegenerates | PostProcessSteps.FindInvalidData | PostProcessSteps.GenerateUVCoords | PostProcessSteps.FindInstances | PostProcessSteps.ValidateDataStructure | PostProcessSteps.OptimizeMeshes;
                //PAS SUR: PostProcessSteps postProcessSteps = PostProcessSteps.CalculateTangentSpace | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.LimitBoneWeights | PostProcessSteps.RemoveRedundantMaterials | PostProcessSteps.FindDegenerates | PostProcessSteps.FindInvalidData | PostProcessSteps.GenerateUVCoords | PostProcessSteps.FindInstances | PostProcessSteps.ValidateDataStructure;


                if (meshAssetDef.InverseFaces)
                    postProcessSteps |= PostProcessSteps.FlipWindingOrder;
                if (meshAssetDef.SmoothNormals)
                    postProcessSteps |= PostProcessSteps.GenerateSmoothNormals;

                //if (meshAssetDef.Scale != 1f)
                //    transformMatrix *= Matrix4x4.FromScaling(new Vector3D(meshAssetDef.Scale));
                //if (meshAssetDef.RotationX != 0f)
                //    transformMatrix *= Matrix4x4.FromRotationX(meshAssetDef.RotationX);
                //if (meshAssetDef.RotationY != 0f)
                //    transformMatrix *= Matrix4x4.FromRotationX(meshAssetDef.RotationY);
                //if (meshAssetDef.RotationZ != 0f)
                //    transformMatrix *= Matrix4x4.FromRotationX(meshAssetDef.RotationZ);

                context.Scale = context.Scale * meshAssetDef.Scale;
                context.XAxisRotation = meshAssetDef.RotationX;
                context.YAxisRotation = meshAssetDef.RotationY;
                context.ZAxisRotation = meshAssetDef.RotationZ;

                //Assimp.Scene scene = context.ImportFileFromStream(_assetManager.GetStream(meshAssetDef.MeshFullPath), postProcessSteps);
                Assimp.Scene scene = context.ImportFile(AssetManager.RemovePrefix(meshAssetDef.ModelAssetUri), postProcessSteps);

                if (scene.Metadata.TryGetValue("OriginalUpAxis", out var originalUpAxis) && scene.Metadata.TryGetValue("UpAxis", out var upAxis))
                {
                    //We will flip it if the up axis have been inverted...
                    if (!originalUpAxis.Data.Equals(upAxis.Data))
                    {
                        postProcessSteps |= PostProcessSteps.FlipWindingOrder;
                        scene = context.ImportFile(meshAssetDef.ModelAssetUri, postProcessSteps);
                    }
                }

                //Loading meshes.....                
                ProcessSceneNode(scene, scene.RootNode, meshDef, ref transformMatrix);



                //Loading textures.....                
                for (int i = 0; i < scene.MaterialCount; i++)
                {
                    meshDef.Materials.Add(CreateMaterial(scene.Materials[i], i, meshAssetDef, workingDirectory));
                }


                //foreach (var subMesh in meshDef.SubMeshes)
                //{
                //    subMesh.MaterialIndex = matIndexes[subMesh.MaterialIndex];
                //}

            }

            return meshDef;
            //MeshletGenerator meshletGenerator = new MeshletGenerator();

            //return meshletGenerator.Convert(meshDef);
        }


        /// <summary>
        /// Load a mesh info meshDef
        /// </summary>
        private void LoadMesh(Assimp.Mesh mesh, MeshDefinition meshDef, ref Matrix4x4 transformMatrix)
        {
            Vertex[] vertices = new Vertex[mesh.Vertices.Count];
            //Vector3[] normals = new Vector3[mesh.Vertices.Count];
            //Vector2[] texCoords = new Vector2[mesh.Vertices.Count];
            uint[] indices = new uint[mesh.FaceCount * 3];

            bool hasNormals = mesh.HasNormals;
            bool hasTexCoords = mesh.HasTextureCoords(0);
            List<Vector3D> texCoords = null;
            if (hasTexCoords)
                texCoords = mesh.TextureCoordinateChannels[0];

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vector = transformMatrix * mesh.Vertices[i];

                vertices[i].Pos.X = vector.X;
                vertices[i].Pos.Y = vector.Y;
                vertices[i].Pos.Z = vector.Z;

                if (hasNormals)
                {
                    vertices[i].Normal.X = mesh.Normals[i].X;
                    vertices[i].Normal.Y = mesh.Normals[i].Y;
                    vertices[i].Normal.X = mesh.Normals[i].X;
                }

                if (hasTexCoords)
                {
                    vertices[i].TexCoord.X = texCoords[i].X;
                    vertices[i].TexCoord.Y = -texCoords[i].Y;
                }
            }

            int indexIndice = 0;
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                indices[indexIndice++] = (uint)mesh.Faces[i].Indices[0];
                indices[indexIndice++] = (uint)mesh.Faces[i].Indices[1];
                indices[indexIndice++] = (uint)mesh.Faces[i].Indices[2];
            }

            //if (!matIndexes.ContainsKey(mesh.MaterialIndex))
            //    matIndexes.Add(mesh.MaterialIndex, matIndexes.Count);

            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Vertices = vertices,
                Indices = indices,
                MaterialIndex = (uint)mesh.MaterialIndex
            });

        }


        private void ProcessSceneNode(Scene scene, Node node, MeshDefinition meshDef, ref Matrix4x4 transformMatrix)
        {
            Matrix4x4 transformNodeMatrix = node.Transform * transformMatrix;

            for (int m = 0; m < node.MeshCount; m++)
            {
                var mesh = scene.Meshes[node.MeshIndices[m]];

                

                LoadMesh(mesh, meshDef, ref transformNodeMatrix);


                //Vector3[] positions = new Vector3[mesh.Vertices.Count];
                //Vector3[] normals = new Vector3[mesh.Vertices.Count];
                //Vector2[] texCoords = new Vector2[mesh.Vertices.Count];
                ////int[] indices = new int[mesh.FaceCount * 3];
                //List<int> indices = new List<int>(mesh.FaceCount * 3);
                //var vertexMap = new Dictionary<int, int>();

                    //int indexVertice = 0;
                    //bool hasTexCoords = mesh.HasTextureCoords(0);
                    //bool hasNormals = mesh.HasNormals;

                    //for (int f = 0; f < mesh.FaceCount; f++)
                    //{
                    //    var face = mesh.Faces[f];

                    //    for (int i = 0; i < face.IndexCount; i++)
                    //    {
                    //        int index = face.Indices[i];

                    //        var position = mesh.Vertices[index];
                    //        positions[indexVertice].X = position.X;
                    //        positions[indexVertice].Y = position.Y;
                    //        positions[indexVertice].Z = position.Z;


                    //        if (hasNormals)
                    //        {
                    //            var normal = mesh.Normals[i];
                    //            normals[i].X = normal.X;
                    //            normals[i].Y = normal.Y;
                    //            normals[i].Z = normal.Z;
                    //        }


                    //        if (hasTexCoords)
                    //        {
                    //            var textureCoord = mesh.TextureCoordinateChannels[0][index];

                    //            texCoords[indexVertice].X = textureCoord.X;
                    //            texCoords[indexVertice].Y = textureCoord.Y;
                    //        }



                    //        int hash = BytesHelper.CombineHash(BytesHelper.CombineHash(positions[indexVertice].GetHashCode(), normals[i].GetHashCode()), texCoords[indexVertice].GetHashCode());
                    //        if (vertexMap.TryGetValue(hash, out var newIndexVertice))
                    //        {
                    //            indices.Add(newIndexVertice);
                    //        }
                    //        else
                    //        {
                    //            indices.Add(indexVertice);
                    //            vertexMap[hash] = indexVertice;
                    //            indexVertice++;
                    //        }
                    //    }
                    //}
            }

            for (int c = 0; c < node.ChildCount; c++)
            {
                ProcessSceneNode(scene, node.Children[c], meshDef, ref transformNodeMatrix);
            }
        }

        /// <summary>
        /// Create a material
        /// </summary>
        private Material CreateMaterial(Assimp.Material assmat, int matIndex, MeshAssetDefinition meshAssetDef, string workingDirectory)
        {

            if (matIndex < meshAssetDef.MaterialNames.Count)
            {
                //We have a material name...
                return _assetManager.Get<Material>(meshAssetDef.MaterialNames[matIndex], workingDirectory);
            }
            else
            {
                //Creation of a basic Material
                return GetMaterial(Assimp.TextureType.Diffuse, assmat, workingDirectory);
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
        private Material GetMaterial(Assimp.TextureType type, Assimp.Material assmat, string workingDirectory)
        {
            //Diffuse texture...
            if (assmat.GetMaterialTextureCount(type) > 0)
            {
                if (assmat.GetMaterialTexture(type, 0, out TextureSlot assTexture))
                {
                    string assetDefPath = assTexture.FilePath;
                    if (Path.IsPathRooted(assetDefPath))
                        assetDefPath = Path.GetFileName(assetDefPath);
                    return _assetManager.Get<Material>(AssetManager.CombineUri(workingDirectory, assetDefPath), workingDirectory);
                }
            }

            return BaseMaterials.Default;
        }



    }
}
