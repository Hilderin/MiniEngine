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
                    if (!name.Contains('/') && !name.Contains('\\') && typeof(BaseShaders).GetProperty(name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Static) != null)
                    {
                        shader = (Shader)typeof(BaseShaders).GetProperty(name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Static).GetValue(null);
                    }
                    else
                    {
                        //Shader on disk...

                        string vertPath = String.Empty;
                        string fragPath = String.Empty;
                        Dictionary<string, ShaderVariableDefinition> variableDefinitions = null;


                        string assetPath = _assetManager.GetAssetPath(name, AssetManager.ASSET_EXTENSION_FILE);

                        if (File.Exists(assetPath))
                        {
                            //We have an asset file...
                            var assetInfo = _assetManager.DeserializeFile<ShaderAssetDefinition>(assetPath);

                            if (String.IsNullOrEmpty(assetInfo.VertexCodePath))
                                throw new FormatException($"VertexCodePath undefined in asset definition file: '{assetPath}'");
                            if (String.IsNullOrEmpty(assetInfo.FragmentCodePath))
                                throw new FormatException($"FragmentCodePath undefined in asset definition file: '{assetPath}'");

                            if (!String.IsNullOrEmpty(assetInfo.VertexCodePath))
                                vertPath = Path.GetFullPath(assetInfo.VertexCodePath, Path.GetDirectoryName(assetPath));
                            if (!String.IsNullOrEmpty(assetInfo.FragmentCodePath))
                                fragPath = Path.GetFullPath(assetInfo.FragmentCodePath, Path.GetDirectoryName(assetPath));

                            if (!File.Exists(vertPath))
                                throw new FileNotFoundException($"Vertex file not found: {vertPath}");
                            if (!File.Exists(fragPath))
                                throw new FileNotFoundException($"Fragment file not found: {fragPath}");

                            variableDefinitions = assetInfo.VariableDefinitions;
                        }
                        else
                        {
                            //.asset not found.... check only for .vert et .frag...
                            vertPath = _assetManager.GetAssetPath(name, ".vert");
                            fragPath = _assetManager.GetAssetPath(name, ".frag");

                            if (!File.Exists(vertPath))
                                throw new FormatException($"VertexCode file not found '{name}.vert'");
                            if (!File.Exists(fragPath))
                                throw new FormatException($"FragmentCode file not found '{name}.frag'");
                        }

                        

                        shader = _assetManager.Context.Renderer.CreateShader(new()
                        {
                            VertexCode = File.ReadAllText(vertPath),
                            FragmentCode = File.ReadAllText(fragPath),
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
