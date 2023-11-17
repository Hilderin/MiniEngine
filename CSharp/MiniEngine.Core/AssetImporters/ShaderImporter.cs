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
                        string computePath = String.Empty;
                        Dictionary<string, ShaderVariableDefinition> variableDefinitions = null;

                        string extension = Path.GetExtension(assetPath).ToLower();

                        if (extension == AssetManager.ASSET_EXTENSION_FILE)
                        {
                            //We have a definition file...
                            var assetInfo = _assetManager.DeserializeAsset<ShaderAssetDefinition>(assetPath);

                            //if (String.IsNullOrEmpty(assetInfo.VertexCodePath))
                            //    throw new FormatException($"VertexCodePath undefined in asset definition file: '{assetPath}'");
                            //if (String.IsNullOrEmpty(assetInfo.FragmentCodePath))
                            //    throw new FormatException($"FragmentCodePath undefined in asset definition file: '{assetPath}'");


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

                            if (!String.IsNullOrEmpty(assetInfo.ComputeCodePath))
                            {
                                if (!_assetManager.TryFindAssetUri(assetInfo.ComputeCodePath, AssetManager.GetDirectoryName(assetPath), out computePath))
                                    throw new FileNotFoundException($"Compute file not found: {assetInfo.ComputeCodePath}");
                            }

                            variableDefinitions = assetInfo.VariableDefinitions;
                        }
                        else
                        {
                            //File directly...
                            foreach (string shaderExtension in new string[] { ".vert", ".frag", ".comp" })
                            {
                                string pathCheck = AssetManager.ChangeAssetUriExtension(assetPath, shaderExtension);
                                if (_assetManager.TryFindAssetUri(pathCheck, AssetManager.GetDirectoryName(assetPath), out string codePath))
                                {
                                    switch (shaderExtension)
                                    {
                                        case ".vert":
                                            vertPath = codePath;
                                            break;
                                        case ".frag":
                                            fragPath = codePath;
                                            break;
                                        case ".comp":
                                            computePath = codePath;
                                            break;
                                    }
                                }

                            }

                        }



                        ShaderDefinition shaderDefinition = new ShaderDefinition();

                        if(!String.IsNullOrEmpty(vertPath))
                            shaderDefinition.VertexCode = GlslHelper.Expand(_assetManager.GetString(vertPath), AssetManager.GetDirectoryName(vertPath));
                        if (!String.IsNullOrEmpty(fragPath))
                            shaderDefinition.FragmentCode = GlslHelper.Expand(_assetManager.GetString(fragPath), AssetManager.GetDirectoryName(fragPath));
                        if (!String.IsNullOrEmpty(computePath))
                            shaderDefinition.ComputeCode = GlslHelper.Expand(_assetManager.GetString(computePath), AssetManager.GetDirectoryName(computePath));

                        shaderDefinition.VariableDefinitions = variableDefinitions;

                        shader = Renderer.Current.CreateShader(shaderDefinition);

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
