using MiniEngine.AssetDefinitions;
using MiniEngine.AssetImporters;
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
        /// Event when assets have changed
        /// </summary>
        public event Action OnAssetChanged;


        private FileSystemWatcher _fsw;
        private Task _taskUpdateContent = null;
        private DateTime _lastUpdatedContent = DateTime.MinValue;


        /// <summary>
        /// Asset importer
        /// </summary>
        private Dictionary<Type, IAssetImporter> _assetImporters = new Dictionary<Type, IAssetImporter>();

        /// <summary>
        /// Yaml deserialization
        /// </summary>
        private IDeserializer _deserializer = new DeserializerBuilder()
                                                        .WithAttemptingUnquotedStringTypeDeserialization()
                                                        .Build();

        /// <summary>
        /// Context
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// Root path for the assets
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// Asset manager
        /// </summary>
        public AssetManager(Context context)
        {
            Context = context;

            RootPath = GetDefaultRootPath();

            //Assert importers...
            _assetImporters.Add(typeof(Texture2D), new Texture2DImporter(this));
            _assetImporters.Add(typeof(Material), new MaterialImporter(this));
            _assetImporters.Add(typeof(Shader), new ShaderImporter(this));



        }


        /// <summary>
        /// Get an asset
        /// </summary>
        public T Get<T>(string name)
        {
            if (!_assetImporters.TryGetValue(typeof(T), out IAssetImporter importer))
                throw new InvalidOperationException($"Asset type not supported: {typeof(T).Name}");

            return (T)importer.Import(name);
        }

        /// <summary>
        /// Deserialize a file
        /// </summary>
        public T DeserializeFile<T>(string path)
        {

            using (TextReader reader = File.OpenText(path))
            {
                return _deserializer.Deserialize<T>(reader);
            }

        }


        /// <summary>
        /// Get the path for an asset
        /// </summary>
        public string GetAssetPath(string name, string extension)
        {
            string path = Path.Combine(RootPath, name + extension);

            if (!File.Exists(path))
            {
                return String.Empty;
            }

            return path;
        }

        /// <summary>
        /// Get the path for an asset that can have multiple extensions
        /// </summary>
        public string GetAssetPath(string name, string[] extensions, bool throwIfNotExists = true)
        {
            for (int i = 0; i < extensions.Length; i++)
            {
                string path = GetAssetPath(name, extensions[i]);
                if (!String.IsNullOrEmpty(path))
                    return path;
            }

            return String.Empty;
        }


        /// <summary>
        /// Calculate the default root path
        /// </summary>
        private string GetDefaultRootPath()
        {

            string rootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            if (rootPath.EndsWith("\\net7.0-windows"))
                rootPath = rootPath.Substring(0, rootPath.Length - "\\net7.0-windows".Length);

            if (rootPath.EndsWith("\\Debug"))
                rootPath = rootPath.Substring(0, rootPath.Length - "\\Debug".Length);
            if (rootPath.EndsWith("\\Release"))
                rootPath = rootPath.Substring(0, rootPath.Length - "\\Release".Length);

            if (rootPath.EndsWith("\\bin"))
                rootPath = rootPath.Substring(0, rootPath.Length - "\\bin".Length);

            //Assets are in a subfolder...
            rootPath = Path.Combine(rootPath, "Assets");



            return rootPath;

        }


        /// <summary>
        /// Permet de vérifier si du contenu change
        /// </summary>
        public void StartWatchUpdateContent()
        {
            if (_fsw == null)
            {
                if (Directory.Exists(RootPath))
                {
                    _fsw = new FileSystemWatcher(RootPath);
                    _fsw.IncludeSubdirectories = true;

                    _fsw.Changed += Fsw_Changed;
                    _fsw.Created += Fsw_Changed;
                    _fsw.Deleted += Fsw_Changed;
                    _fsw.Renamed += Fsw_Renamed;

                    _fsw.EnableRaisingEvents = true;

                }
            }

        }

        /// <summary>
        /// Event quand des changements sont lancés
        /// </summary>
        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            ProcessChange(e.FullPath);
        }


        /// <summary>
        /// Event quand des changements sont lancés
        /// </summary>
        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            ProcessChange(e.FullPath);
        }

        /// <summary>
        /// Process le changement
        /// </summary>
        private void ProcessChange(string fullPath)
        {
            lock (_fsw)
            {
                if (_taskUpdateContent == null)
                {
                    _taskUpdateContent = Task.Factory.StartNew(TaskWaitBeforeForNotification);
                }
            }

        }


        /// <summary>
        /// Task that waits a bit before reloading content...
        /// </summary>
        private void TaskWaitBeforeForNotification()
        {
            try
            {
                System.Threading.Thread.Sleep(100);

                //On va surement avoir plus d'un event, on va attendre le dernier event
                while (DateTime.Now.Subtract(_lastUpdatedContent).TotalMilliseconds < 100)
                    System.Threading.Thread.Sleep(10);


                //On indique qu'on a du contenu à reloader
                ResetCacheAllImporters();

                OnAssetChanged?.Invoke();

            }
            finally
            {
                _taskUpdateContent = null;
            }
        }

        /// <summary>
        /// Reset the cache for all importers
        /// </summary>
        private void ResetCacheAllImporters()
        {
            foreach (var importer in _assetImporters.Values)
                importer.ResetCache();
        }

    }
}
