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

                MeshAssetDefinition assetMeshDef = GetMeshAssetDefinition(name);

                mesh = CreateMesh(assetMeshDef);

                _assetManager.AssetPathToWatch(_assetManager.GetAssetPath(name, AssetManager.ASSET_EXTENSION_FILE), () => ReloadMesh(mesh, name));
                _assetManager.AssetPathToWatch(assetMeshDef.MeshFullPath, () => ReloadMesh(mesh, name));


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
        /// Load the mesh asset definition from disk
        /// </summary>
        private MeshAssetDefinition GetMeshAssetDefinition(string name)
        {
            string meshPath;
            string assetDefPath;
            MeshAssetDefinition assetMeshDef = null;


            try
            {
                assetDefPath = _assetManager.GetAssetPath(name, AssetManager.ASSET_EXTENSION_FILE);

                if (File.Exists(assetDefPath))
                {
                    //We have a definition file...
                    assetMeshDef = _assetManager.DeserializeFile<MeshAssetDefinition>(assetDefPath);
                    assetMeshDef.MeshFullPath = Path.Combine(Path.GetDirectoryName(assetDefPath), assetMeshDef.MeshPath);

                    if (String.IsNullOrEmpty(assetMeshDef.MeshPath))
                        throw new FormatException($"MeshPath not set in '{assetDefPath}'");

                    if (!File.Exists(assetMeshDef.MeshFullPath))
                        throw new FileNotFoundException($"Mesh file not found: {assetMeshDef.MeshFullPath}");

                }
                else
                {
                    //Search directly with the extensions...
                    if (!_assetManager.TryFindAssetPath(name, SUPPORTED_EXTENSIONS, out meshPath))
                        throw new FileNotFoundException($"Mesh not found: {name} for supported extensions: {String.Join(", ", SUPPORTED_EXTENSIONS)}");

                    assetDefPath = meshPath + AssetManager.ASSET_EXTENSION_FILE;
                    assetMeshDef = new MeshAssetDefinition();
                    assetMeshDef.MeshPath = Path.GetFileName(meshPath);
                    assetMeshDef.MeshFullPath = meshPath;

                    _assetManager.SerializeFile(assetMeshDef, assetDefPath);

                }



            }
            catch (Exception ex)
            {
                Debug.Print("Erreur: " + ex.ToString());
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
        /// Reload a mesh
        /// </summary>
        private void ReloadMesh(Mesh mesh, string name)
        {
            MeshAssetDefinition assetMeshDef = GetMeshAssetDefinition(name);
            MeshDefinition meshDef = CreateMeshDefinition(assetMeshDef);

            if (!String.IsNullOrEmpty(assetMeshDef.MeshFullPath))
                mesh.Reload(meshDef);
        }

        /// <summary>
        /// Import a mesh from file
        /// </summary>
        private Mesh CreateMesh(MeshAssetDefinition meshAssetDef)
        {
            if (String.IsNullOrEmpty(meshAssetDef.MeshFullPath))
                return Primitives.CreateEmptyMesh();

            MeshDefinition meshDef = CreateMeshDefinition(meshAssetDef);

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
        /// Load mesh definition
        /// </summary>
        private MeshDefinition CreateMeshDefinition(MeshAssetDefinition meshAssetDef)
        {
            MeshDefinition meshDef = new MeshDefinition();

            string workingDirectory = Path.GetDirectoryName(meshAssetDef.MeshFullPath);
            //Matrix3 transformMatrix = Matrix3.FromEulerAnglesXYZ(Math.DegToRad(90), 0f, 0f);
            Matrix4x4 transformMatrix = Matrix4x4.Identity;
            //MeshDefinition meshDef = new MeshDefinition();

            using (AssimpContext context = new AssimpContext())
            {


                //------------------
                //WORKING GOOD:
                PostProcessSteps postProcessSteps = PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.MakeLeftHanded | PostProcessSteps.GenerateNormals | PostProcessSteps.TransformUVCoords;

                //The base unit of fbx is centimeters...
                if (Path.GetExtension(meshAssetDef.MeshFullPath).Equals(".fbx", StringComparison.OrdinalIgnoreCase))
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

                context.Scale = meshAssetDef.Scale;
                context.XAxisRotation = meshAssetDef.RotationX;
                context.YAxisRotation = meshAssetDef.RotationY;
                context.ZAxisRotation = meshAssetDef.RotationZ;

                Assimp.Scene scene = context.ImportFile(meshAssetDef.MeshFullPath, postProcessSteps);

                if (scene.Metadata.TryGetValue("OriginalUpAxis", out var originalUpAxis) && scene.Metadata.TryGetValue("UpAxis", out var upAxis))
                {
                    //We will flip it if the up axis have been inverted...
                    if (!originalUpAxis.Data.Equals(upAxis.Data))
                    {
                        postProcessSteps |= PostProcessSteps.FlipWindingOrder;
                        scene = context.ImportFile(meshAssetDef.MeshFullPath, postProcessSteps);
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
        }


        /// <summary>
        /// Load a mesh info meshDef
        /// </summary>
        private void LoadMesh(Assimp.Mesh mesh, MeshDefinition meshDef, ref Matrix4x4 transformMatrix)
        {
            Vector3[] positions = new Vector3[mesh.Vertices.Count];
            Vector3[] normals = new Vector3[mesh.Vertices.Count];
            Vector2[] texCoords = new Vector2[mesh.Vertices.Count];
            int[] indices = new int[mesh.FaceCount * 3];

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vector = transformMatrix * mesh.Vertices[i];

                //positions[i] = transformMatrix * new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z);
                positions[i] = new Vector3(vector.X, vector.Y, vector.Z);

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

            //if (!matIndexes.ContainsKey(mesh.MaterialIndex))
            //    matIndexes.Add(mesh.MaterialIndex, matIndexes.Count);

            meshDef.SubMeshes.Add(new SubMeshDefinition()
            {
                Positions = positions,
                Indices = indices,
                TexCoords = texCoords,
                Normals = normals,
                MaterialIndex = mesh.MaterialIndex
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
                    string assetDefPath = assTexture.FilePath;
                    if (Path.IsPathRooted(assetDefPath))
                        assetDefPath = Path.GetFileName(assetDefPath);
                    return _assetManager.Get<Texture2D>(Path.Combine(workingDirectory, assetDefPath));
                }
            }

            return BaseTextures.White;
        }



    }
}
