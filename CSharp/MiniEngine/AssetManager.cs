using MiniEngine.AssetDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace MiniEngine
{
    /// <summary>
    /// Asset manager
    /// </summary>
    public class AssetManager
    {
        /// <summary>
        /// Yaml deserialization
        /// </summary>
        private IDeserializer _deserializer = new DeserializerBuilder()
                                                        .WithAttemptingUnquotedStringTypeDeserialization()
                                                        .Build();
        /// <summary>
        /// Cache shaders
        /// </summary>
        private Dictionary<string, Shader> _cacheShader = new Dictionary<string, Shader>();

        /// <summary>
        /// Context
        /// </summary>
        private Context _context;

        /// <summary>
        /// Root path for the assets
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// Asset manager
        /// </summary>
        public AssetManager(Context context)
        {
            RootPath = GetDefaultRootPath();
        }

        /// <summary>
        /// Get a shader
        /// </summary>
        public Shader GetShader(string name)
        {

            if (!_cacheShader.TryGetValue(name, out Shader shader))
            {
                string vertPath;
                string fragPath;
                Dictionary<string, string> overwrideVariableFormats = null;


                string assetPath = GetAssetPath(name, ".asset", false);

                if (!String.IsNullOrEmpty(assetPath))
                {
                    //We have an asset file...
                    var assetInfo = DeserializeFile<ShaderAssetDefinition>(assetPath);

                    if (String.IsNullOrEmpty(assetInfo.VertexCodePath))
                        throw new FormatException($"VertexCodePath undefined in asset definition file: '{assetPath}'");
                    if (String.IsNullOrEmpty(assetInfo.FragmentCodePath))
                        throw new FormatException($"FragmentCodePath undefined in asset definition file: '{assetPath}'");

                    vertPath = Path.GetFullPath(assetInfo.VertexCodePath, Path.GetDirectoryName(assetPath));
                    fragPath = Path.GetFullPath(assetInfo.FragmentCodePath, Path.GetDirectoryName(assetPath));

                    if (!File.Exists(vertPath))
                        throw new FormatException($"VertexCodePath not found: '{vertPath}'");
                    if (!File.Exists(fragPath))
                        throw new FormatException($"FragmentCodePath not found: '{fragPath}'");

                    overwrideVariableFormats = assetInfo.OverwrideVariableFormats;
                }
                else
                {
                    //.asset not found.... check only for .vert et .frag...
                    vertPath = GetAssetPath(name, ".vert");
                    fragPath = GetAssetPath(name, ".frag");
                }

                shader = _context.Renderer.CreateShader(new()
                {
                    VertexCode = File.ReadAllText(vertPath),
                    FragmentCode = File.ReadAllText(fragPath),
                    OverwrideVariableFormats = overwrideVariableFormats
                });

                _cacheShader.Add(name, shader);
            }

            return shader;
        }

        /// <summary>
        /// Deserialize a file
        /// </summary>
        private T DeserializeFile<T>(string path)
        {

            using (TextReader reader = File.OpenText(path))
            {
                return _deserializer.Deserialize<T>(reader);
            }

        }
        /// <summary>
        /// Calculate the default root path
        /// </summary>
        private string GetDefaultRootPath()
        {

            string rootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            if (rootPath.EndsWith("\\net7.0-windows"))
                rootPath = rootPath.Substring(rootPath.Length - "\\net7.0-windows".Length);

            if (rootPath.EndsWith("\\Debug"))
                rootPath = rootPath.Substring(rootPath.Length - "\\Debug".Length);
            if (rootPath.EndsWith("\\Release"))
                rootPath = rootPath.Substring(rootPath.Length - "\\Release".Length);

            if (rootPath.EndsWith("\\bin"))
                rootPath = rootPath.Substring(rootPath.Length - "\\bin".Length);

            return rootPath;

        }

        /// <summary>
        /// Get the path for an asset
        /// </summary>
        private string GetAssetPath(string name, string extension, bool throwIfNotExists = true)
        {
            string path = Path.Combine(RootPath, name + extension);

            if (!File.Exists(path))
            {
                if (throwIfNotExists)
                    throw new FileNotFoundException($"Asset not found: {path}");
                return String.Empty;
            }

            return path;
        }

    }
}
