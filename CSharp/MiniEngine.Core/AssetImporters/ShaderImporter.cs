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
    /// Shader importer
    /// </summary>
    public class ShaderImporter : IAssetImporter
    {
        private AssetManager _assetManager;

        /// <summary>
        /// Cache shaders
        /// </summary>
        private Dictionary<string, Shader> _cache = new Dictionary<string, Shader>();

        /// <summary>
        /// Shader importer
        /// </summary>
        public ShaderImporter(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }


        /// <summary>
        /// Import a shader...
        /// </summary>
        public object Import(string name)
        {

            if (!_cache.TryGetValue(name, out Shader shader))
            {
                try
                {
                    //We support some basic 
                    if (!name.Contains('/') && !name.Contains('\\') && !name.Contains('.') && typeof(BaseShaders).GetProperty(name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Static) != null)
                    {
                        shader = (Shader)typeof(BaseShaders).GetProperty(name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Static).GetValue(null);
                    }
                    else
                    {
                        //Shader on disk...
                        if(!_assetManager.TryFindAssetUri(name, String.Empty, out string assetPath))
                            throw new FormatException($"Shader not fould: '{name}'");


                        string vertPath = String.Empty;
                        string fragPath = String.Empty;
                        Dictionary<string, ShaderVariableDefinition> variableDefinitions = null;

                        string extension = Path.GetExtension(assetPath).ToLower();

                        if (extension == AssetManager.ASSET_EXTENSION_FILE)
                        {
                            //We have a definition file...
                            var assetInfo = _assetManager.DeserializeAsset<ShaderAssetDefinition>(assetPath);

                            if (String.IsNullOrEmpty(assetInfo.VertexCodePath))
                                throw new FormatException($"VertexCodePath undefined in asset definition file: '{assetPath}'");
                            if (String.IsNullOrEmpty(assetInfo.FragmentCodePath))
                                throw new FormatException($"FragmentCodePath undefined in asset definition file: '{assetPath}'");


                            if (!String.IsNullOrEmpty(assetInfo.VertexCodePath))
                            {
                                if(!_assetManager.TryFindAssetUri(assetInfo.VertexCodePath, AssetManager.GetDirectoryName(assetPath), out vertPath))
                                    throw new FileNotFoundException($"Vertex file not found: {assetInfo.VertexCodePath}");
                            }

                            if (!String.IsNullOrEmpty(assetInfo.FragmentCodePath))
                            {
                                if (!_assetManager.TryFindAssetUri(assetInfo.FragmentCodePath, AssetManager.GetDirectoryName(assetPath), out fragPath))
                                    throw new FileNotFoundException($"Fragment file not found: {assetInfo.FragmentCodePath}");
                            }

                            variableDefinitions = assetInfo.VariableDefinitions;
                        }
                        else
                        {

                            if (assetPath.EndsWith(".frag", StringComparison.OrdinalIgnoreCase))
                            {
                                fragPath = assetPath;

                                string vertPathCheck = AssetManager.ChangeAssetUriExtension(fragPath, ".vert");
                                if (!_assetManager.TryFindAssetUri(vertPath, AssetManager.GetDirectoryName(assetPath), out vertPath))
                                    throw new FileNotFoundException($"Vertex file not found: {vertPathCheck}");

                            }
                            else if (assetPath.EndsWith(".vert", StringComparison.OrdinalIgnoreCase))
                            {
                                vertPath = assetPath;

                                string fragPathCheck = AssetManager.ChangeAssetUriExtension(vertPath, ".frag");
                                if (!_assetManager.TryFindAssetUri(fragPathCheck, AssetManager.GetDirectoryName(assetPath), out fragPath))
                                    throw new FileNotFoundException($"Fragment file not found: {fragPathCheck}");
                            }

                        }

                        string vertexCode = GlslHelper.Expand(_assetManager.GetString(vertPath), AssetManager.GetDirectoryName(vertPath));
                        string fragCode = GlslHelper.Expand(_assetManager.GetString(fragPath), AssetManager.GetDirectoryName(fragPath));

                        shader = Renderer.Current.CreateShader(new()
                        {
                            VertexCode = vertexCode,
                            FragmentCode = fragCode,
                            VariableDefinitions = variableDefinitions
                        });

                    }
                }
                catch (Exception ex)
                {
                    Debug.Error(ex);
                }
                finally
                {
                    //Shader not found...
                    shader ??= BaseShaders.Default;
                }


                //Adding in cache...
                _cache.Add(name, shader);
            }

            return shader;

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
