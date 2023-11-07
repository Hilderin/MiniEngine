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
        public object Import(string name)
        {

            if (!_cache.TryGetValue(name, out Material mat))
            {
                try
                {
                    string assetPath = _assetManager.GetAssetPath(name, AssetManager.ASSET_EXTENSION_FILE);

                    if (!File.Exists(assetPath))
                    {
                        //Check directly with a texture...
                        if(_assetManager.TryFindAssetPath(name, Texture2DImporter.SUPPORTED_EXTENSIONS, out assetPath))
                        {
                            var texture = _assetManager.Get<Texture2D>(name);

                            mat = _assetManager.Context.Renderer.CreateMaterial(new()
                            {
                                DiffuseTexture = texture,
                                Shader = BaseShaders.Default,
                            });

                        }
                        else
                            //Asset not found...
                            throw new FileNotFoundException($"Material asset file not found '{name}.amat'");
                    }

                    if (mat == null)
                    {
                        var assetInfo = _assetManager.DeserializeFile<MaterialAssetDefinition>(assetPath);

                        if (String.IsNullOrEmpty(assetInfo.DiffuseTexture))
                            throw new FormatException($"DiffuseTexture not set in '{assetPath}'");
                        if (String.IsNullOrEmpty(assetInfo.Shader))
                            throw new FormatException($"Shader not set in '{assetPath}'");


                        mat = _assetManager.Context.Renderer.CreateMaterial(new()
                        {
                            DiffuseTexture = _assetManager.Get<Texture2D>(assetInfo.DiffuseTexture),
                            Shader = _assetManager.Get<Shader>(assetInfo.Shader),
                        });
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
