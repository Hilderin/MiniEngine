using MiniEngine.AssetDefinitions;
using MiniEngine.AssetImporters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
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
        /// Extension for asset file
        /// </summary>
        public const string ASSET_EXTENSION_FILE = ".asset";

        /// <summary>
        /// Event when assets have changed
        /// </summary>
        public event Action OnAssetChanged;


        private FileSystemWatcher _rootFsw;
        private Dictionary<string, FileSystemWatcher> _fileWatchers = new Dictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase);
        private Task _taskUpdateContent = null;
        private DateTime _lastUpdatedContent = DateTime.MinValue;
        private Dictionary<string, bool> _updatedPaths = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);


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
        /// Yaml serialization
        /// </summary>
        private ISerializer _serializer = new SerializerBuilder()
                                                    .Build();



        /// <summary>
        /// Reloadable assets
        /// </summary>
        private Dictionary<string, Action> _reloadableAssetsActions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);


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
            _assetImporters.Add(typeof(Mesh), new MeshImporter(this));



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
        /// Serialize a file
        /// </summary>
        public void SerializeFile(object assetObj, string path)
        {

            using (TextWriter writer = File.CreateText(path))
            {
                _serializer.Serialize(writer, assetObj);
            }

        }

        /// <summary>
        /// Add a path to watch
        /// </summary>
        public void AssetPathToWatch(string path, Action reloadAction)
        {
            if (String.IsNullOrEmpty(path))
                return;

            if (!path.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase))
            {
                //Only if we are watching...
                string folder = Path.GetDirectoryName(path);
                if (!_fileWatchers.ContainsKey(folder) && Directory.Exists(folder))
                {
                    FileSystemWatcher fsw = new FileSystemWatcher(folder);
                    //fsw.IncludeSubdirectories = true;

                    fsw.Changed += Fsw_Changed;
                    fsw.Created += Fsw_Changed;
                    fsw.Deleted += Fsw_Changed;
                    fsw.Renamed += Fsw_Renamed;

                    fsw.EnableRaisingEvents = true;

                    _fileWatchers.Add(folder, fsw);
                }

            }



            _reloadableAssetsActions[path] = reloadAction;
        }


        /// <summary>
        /// Get the path for an asset
        /// </summary>
        public string GetAssetPath(string name, string extension)
        {
            string path;

            if (Path.IsPathRooted(name))
                //Absolute path..
                path = name + extension;
            else
                //Relative path...
                path = Path.Combine(RootPath, name + extension);

            //if (!File.Exists(path))
            //{
            //    return String.Empty;
            //}

            return path;
        }

        /// <summary>
        /// Get the path for an asset that can have multiple extensions
        /// </summary>
        public bool TryFindAssetPath(string name, string[] extensions, out string assetPath)
        {
            string extension = Path.GetExtension(name);

            if (!String.IsNullOrEmpty(extension) && extensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                //Extension in the list, we keep it!
                if (!File.Exists(name))
                {
                    assetPath = String.Empty;
                    return false;
                }
                else
                {
                    assetPath = name;
                    return true;
                }
            }

            //Check each extension
            string folder = Path.GetDirectoryName(name);
            if (Directory.Exists(folder))
            {
                foreach (string file in Directory.EnumerateFiles(folder, Path.GetFileName(name) + ".*"))
                {
                    extension = Path.GetExtension(file);

                    if (!String.IsNullOrEmpty(extension) && extensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                    {
                        assetPath = file;
                        return true;
                    }
                }
            }

            assetPath = String.Empty;
            return false;
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
            if (_rootFsw == null)
            {
                if (Directory.Exists(RootPath))
                {
                    _rootFsw = new FileSystemWatcher(RootPath);
                    _rootFsw.IncludeSubdirectories = true;

                    _rootFsw.Changed += Fsw_Changed;
                    _rootFsw.Created += Fsw_Changed;
                    _rootFsw.Deleted += Fsw_Changed;
                    _rootFsw.Renamed += Fsw_Renamed;

                    _rootFsw.EnableRaisingEvents = true;

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
            lock (_updatedPaths)
            {
                _updatedPaths[fullPath] = true;

#pragma warning disable IDE0074 // Use compound assignment
                if (_taskUpdateContent == null)
                    _taskUpdateContent = Task.Factory.StartNew(TaskWaitBeforeForNotification);
#pragma warning restore IDE0074 // Use compound assignment
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

                string[] updatesPaths;
                lock (_updatedPaths)
                    updatesPaths = _updatedPaths.Keys.ToArray();

                foreach (string path in updatesPaths)
                {
                    try
                    {
                        if (_reloadableAssetsActions.TryGetValue(path, out Action reloadableAssetAction))
                            reloadableAssetAction();
                    }
                    catch (Exception ex)
                    {
                        Debug.Error($"Reload asset '{path}' - Error: {ex}");
                    }

                }

                //On indique qu'on a du contenu à reloader
                //ResetCacheAllImporters();

                OnAssetChanged?.Invoke();

            }
            finally
            {
                _taskUpdateContent = null;
            }
        }

        ///// <summary>
        ///// Reset the cache for all importers
        ///// </summary>
        //private void ResetCacheAllImporters()
        //{
        //    foreach (var importer in _assetImporters.Values)
        //        importer.ResetCache();
        //}


    }
}
