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
        /// <summary>
        /// Supported shader extensions
        /// </summary>
        private static readonly string[] SHADER_EXTENSIONS = new string[] { ".vert", ".frag", ".comp", ".tesc", ".tese", ".geom" };

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
                        

                        shader = Renderer.Current.CreateShader();

                        LoadShader(name, shader);


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
        /// Load the shader
        /// </summary>
        private void LoadShader(string name, Shader shader)
        {
            //Shader on disk...
            if (!_assetManager.TryFindAssetUri(name, String.Empty, true, out string assetPath))
                throw new FormatException($"Shader not fould: '{name}'");


            Dictionary<ShaderStage, string> stageUris = new Dictionary<ShaderStage, string>();
            Dictionary<string, ShaderVariableDefinition> variableDefinitions = null;
            List<string> urisToWatch = new List<string>();

            string extension = Path.GetExtension(assetPath).ToLower();

            if (extension == AssetManager.ASSET_EXTENSION_FILE)
            {
                //We have a definition file...
                var assetInfo = _assetManager.DeserializeAsset<ShaderAssetDefinition>(assetPath);

                if (assetInfo.StagePaths == null || assetInfo.StagePaths.Count == 0)
                    throw new FormatException($"Invalid shader definition, no StagePaths found in: '{assetPath}'");

                urisToWatch.Add(assetPath);

                foreach (var kv in assetInfo.StagePaths)
                {
                    if (!_assetManager.TryFindAssetUri(kv.Value, AssetManager.GetDirectoryName(assetPath), false, out string shaderUri))
                        throw new FileNotFoundException($"Shader file not found: {kv.Value}");

                    stageUris.Add(kv.Key, shaderUri);
                    urisToWatch.Add(shaderUri);
                }

                variableDefinitions = assetInfo.VariableDefinitions;
            }
            else
            {
                //File directly...
                foreach (string shaderExtension in SHADER_EXTENSIONS)
                {
                    string pathCheck = AssetManager.ChangeAssetUriExtension(assetPath, shaderExtension);
                    if (_assetManager.TryFindAssetUri(pathCheck, AssetManager.GetDirectoryName(assetPath), false, out string shaderUri))
                    {
                        stageUris.Add(GlslHelper.GetShaderStageFromPath(shaderUri), shaderUri);
                        urisToWatch.Add(shaderUri);
                    }
                }

            }



            ShaderDefinition shaderDefinition = new ShaderDefinition();

            foreach (var kv in stageUris)
            {
                string code = GlslHelper.Expand(_assetManager.GetString(kv.Value), AssetManager.GetDirectoryName(kv.Value));
                shaderDefinition.StageCodes.Add(kv.Key, code);

#if DEBUG
                if (Directory.Exists(@"C:\Projects\Temp\Shaders"))
                    File.WriteAllText(@"C:\Projects\Temp\Shaders\"+ Path.GetFileName(name), code);
#endif
            }

            shaderDefinition.VariableDefinitions = variableDefinitions;

            shader.Load(shaderDefinition);

            foreach (string uri in urisToWatch)
                _assetManager.AssetUriToWatch(uri, () => LoadShader(name, shader));

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
