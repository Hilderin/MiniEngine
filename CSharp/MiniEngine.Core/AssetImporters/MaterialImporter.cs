using MiniEngine.AssetDefinitions;
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
    /// Material importer
    /// </summary>
    public class MaterialImporter : IAssetImporter
    {
        private AssetManager _assetManager;

        /// <summary>
        /// Cache materials
        /// </summary>
        private Dictionary<string, Material> _cache = new Dictionary<string, Material>();

        /// <summary>
        /// Material importer
        /// </summary>
        public MaterialImporter(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }


        /// <summary>
        /// Import a material...
        /// </summary>
        public object Import(string name, string workingFolderUri)
        {

            if (!_cache.TryGetValue(name, out Material mat))
            {
                try
                {
                    string assetPath;

                    if (!_assetManager.TryFindAssetUri(name, workingFolderUri, true, out assetPath))
                        throw new FileNotFoundException($"Material asset file not found '{name}'");

                    string extension = Path.GetExtension(assetPath).ToLower();

                    if (extension == AssetManager.ASSET_EXTENSION_FILE)
                    {
                        var assetInfo = _assetManager.DeserializeAsset<MaterialAssetDefinition>(assetPath);

                        //if (String.IsNullOrEmpty(assetInfo.DiffuseTexture))
                        //    throw new FormatException($"DiffuseTexture not set in '{assetPath}'");
                        if (String.IsNullOrEmpty(assetInfo.Shader))
                            throw new FormatException($"Shader not set in '{assetPath}'");


                        mat = Renderer.Current.CreateMaterial(new()
                        {
                            DiffuseTexture = assetInfo.DiffuseTexture != null ?_assetManager.Get<Texture2D>(assetInfo.DiffuseTexture, workingFolderUri) : null,
                            Shader = _assetManager.Get<Shader>(assetInfo.Shader, AssetManager.GetDirectoryName(assetPath)),
                        });
                    }
                    else if (Texture2DImporter.SUPPORTED_EXTENSIONS.Contains(extension))
                    {
                        //It's only a texture ...
                        var texture = _assetManager.Get<Texture2D>(name, AssetManager.GetDirectoryName(assetPath));

                        mat = Renderer.Current.CreateMaterial(new()
                        {
                            DiffuseTexture = texture,
                            Shader = BaseShaders.Default
                        });
                    }
                    else
                    {
                        throw new NotSupportedException($"Material asset file extension not supported: '{extension}'");
                    }

                    mat.Name = Path.GetFileName(name);

                }
                catch(Exception ex)
                {
                    Debug.Error(ex);
                }
                finally
                {
                    //Material not found...
                    mat ??= BaseMaterials.Magenta;
                }

                _cache.Add(name, mat);
            }

            return mat;

        }

        /// <summary>
        /// Reset the cache
        /// </summary>
        public void ResetCache()
        {
            _cache.Clear();
        }

    }
}
