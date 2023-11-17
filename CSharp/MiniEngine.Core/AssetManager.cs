using MiniEngine.AssetDefinitions;
using MiniEngine.AssetImporters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// Prefix for file uri
        /// </summary>
        public const string PREFIX_URI_FILE = "file://";

        /// <summary>
        /// Prefix for res uri
        /// </summary>
        public const string PREFIX_URI_RESOURCE = "res://";

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
        /// Root path for the current project
        /// </summary>
        private string _rootPathCurrentProject;

        /// <summary>
        /// Root path for the MiniEngineCore
        /// </summary>
        private string _rootPathMiniEngineCore;

        /// <summary>
        /// Current asset manager
        /// </summary>
        public static AssetManager Current { get; protected set; } = new AssetManager();

        /// <summary>
        /// Asset manager
        /// </summary>
        protected AssetManager()
        {

            _rootPathCurrentProject = GetRootPathCurrentProject();
            _rootPathMiniEngineCore = GetRootPathMiniEngineCore();

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
        public T DeserializeAsset<T>(string assetUri)
        {

            using (TextReader reader = new StringReader(GetString(assetUri)))
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

            if (!path.StartsWith(_rootPathCurrentProject, StringComparison.OrdinalIgnoreCase))
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
        /// Change an asset uri extension
        /// </summary>
        public static string ChangeAssetUriExtension(string assetUriSource, string newExtension)
        {
            if (!newExtension.StartsWith("."))
                newExtension = "." + newExtension;

            int index = assetUriSource.LastIndexOf('.');
            if (index >= 0)
                return assetUriSource.Substring(0, index) + newExtension;
            else
                return assetUriSource + newExtension;
        }

        /// <summary>
        /// Remove the last file name in an asset uri
        /// </summary>
        public static string GetDirectoryName(string assetUri)
        {
            assetUri = assetUri.Replace('\\', '/');

            int index = assetUri.LastIndexOf('/');
            if (index >= 0)
                return assetUri.Substring(0, index);
            else
                return String.Empty;
        }

        /// <summary>
        /// Remove the last file name in an asset uri
        /// </summary>
        public static string CombineUri(params string[] segments)
        {
            if (segments.Length == 0)
                return String.Empty;

            if (segments.Length == 1)
                return segments[0];

            string retour = segments[0];

            for (int i = 1; i < segments.Length; i++)
            {
                if (!retour.EndsWith('\\') && !retour.EndsWith('/'))
                    retour += "/";

                string newSegment = segments[i];
                while (newSegment.StartsWith('\\') || newSegment.StartsWith('/'))
                    newSegment = newSegment.Substring(1);
                retour += newSegment;
            }

            return retour;
        }

        /// <summary>
        /// Remove the prefix from an uri
        /// </summary>
        public static string RemovePrefix(string assetUri)
        {
            if (String.IsNullOrEmpty(assetUri))
                return assetUri;

            assetUri = assetUri.Replace('\\', '/');
            if (assetUri.StartsWith(PREFIX_URI_FILE))
                assetUri = assetUri.Substring(PREFIX_URI_FILE.Length);
            if (assetUri.StartsWith(PREFIX_URI_RESOURCE))
                assetUri = assetUri.Substring(PREFIX_URI_RESOURCE.Length);
            return assetUri;
        }

        /// <summary>
        /// Get the uri for an asset
        /// </summary>
        public bool TryFindAssetUri(string name, string workingFolder, out string assetUri)
        {
            //Sometimes i pass names that are already with prefix, juste remove it to pass into the normal process...
            name = RemovePrefix(name);
            workingFolder = RemovePrefix(workingFolder);

            //-------------------
            //Absolute path...
            if (Path.IsPathRooted(name))
            {
                //Absolute path..
                if (File.Exists(name))
                {
                    assetUri = PREFIX_URI_FILE + name;
                    return true;
                }
                else
                {
                    //File not found..
                    assetUri = null;
                    return false;
                }
            }




            //-------------------
            //Relative path...
            string path;
            if (!String.IsNullOrEmpty(workingFolder))
            {
                path = Path.Combine(workingFolder, name);
                if (File.Exists(path))
                {
                    assetUri = PREFIX_URI_FILE + path;
                    return true;
                }
            }

            path = Path.Combine(_rootPathCurrentProject, name);
            if (File.Exists(path))
            {
                assetUri = PREFIX_URI_FILE + path;
                return true;
            }

            path = Path.Combine(_rootPathMiniEngineCore, name);
            if (File.Exists(path))
            {
                assetUri = PREFIX_URI_FILE + path;
                return true;
            }

            //Not found... check in resources...
            if (TryFindResource(name, out string resName))
            {
                assetUri = PREFIX_URI_RESOURCE + resName;
                return true;
            }

            assetUri = null;
            return false;
        }

        /// <summary>
        /// Get the content of an asset file in string
        /// </summary>
        public string GetString(string assetUri)
        {
            if (assetUri.StartsWith(PREFIX_URI_FILE))
                return File.ReadAllText(assetUri.Substring(PREFIX_URI_FILE.Length));
            else if (assetUri.StartsWith(PREFIX_URI_RESOURCE))
                return ResourceUtils.GetString(assetUri.Substring(PREFIX_URI_RESOURCE.Length));
            else
            {
                if (!TryFindAssetUri(assetUri, String.Empty, out string newAssetUri))
                    throw new FileNotFoundException($"Asset not found: {assetUri}");
                return GetString(newAssetUri);
            }

        }

        /// <summary>
        /// Get the content of an asset file in bytes
        /// </summary>
        public byte[] GetBytes(string assetUri)
        {
            if (assetUri.StartsWith(PREFIX_URI_FILE))
                return File.ReadAllBytes(assetUri.Substring(PREFIX_URI_FILE.Length));
            else if (assetUri.StartsWith(PREFIX_URI_RESOURCE))
                return ResourceUtils.GetBytes(assetUri.Substring(PREFIX_URI_RESOURCE.Length));
            else
            {
                if (!TryFindAssetUri(assetUri, String.Empty, out string newAssetUri))
                    throw new FileNotFoundException($"Asset not found: {assetUri}");
                return GetBytes(newAssetUri);
            }

        }

        /// <summary>
        /// Get the steam of the content of an asset
        /// </summary>
        public Stream GetStream(string assetUri)
        {
            if (assetUri.StartsWith(PREFIX_URI_FILE))
                return File.OpenRead(assetUri.Substring(PREFIX_URI_FILE.Length));
            else if (assetUri.StartsWith(PREFIX_URI_RESOURCE))
                return ResourceUtils.GetStream(assetUri.Substring(PREFIX_URI_RESOURCE.Length));
            else
            {
                if (!TryFindAssetUri(assetUri, String.Empty, out string newAssetUri))
                    throw new FileNotFoundException($"Asset not found: {assetUri}");
                return GetStream(newAssetUri);
            }
        }

        ///// <summary>
        ///// Get the path for an asset that can have multiple extensions
        ///// </summary>
        //public bool TryFindAssetUri(string name, string[] extensions, out string assetUri)
        //{
        //    string extension = Path.GetExtension(name);

        //    if (!String.IsNullOrEmpty(extension) && extensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        //    {
        //        //Extension in the list, we keep it!
        //        if (!File.Exists(name))
        //        {
        //            assetUri = String.Empty;
        //            return false;
        //        }
        //        else
        //        {
        //            assetUri = name;
        //            return true;
        //        }
        //    }

        //    //Check each extension
        //    string folder = Path.GetDirectoryName(name);
        //    if (Directory.Exists(folder))
        //    {
        //        foreach (string file in Directory.EnumerateFiles(folder, Path.GetFileName(name) + ".*"))
        //        {
        //            extension = Path.GetExtension(file);

        //            if (!String.IsNullOrEmpty(extension) && extensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                assetUri = file;
        //                return true;
        //            }
        //        }
        //    }

        //    assetUri = String.Empty;
        //    return false;
        //}


        /// <summary>
        /// Calculate the default root path
        /// </summary>
        private string GetRootPathCurrentProject()
        {

            string rootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            int cpt = 0;
            while (rootPath.Length > 4)
            {
                string folderName = Path.GetFileName(rootPath);

                if (!(cpt == 0 && folderName.StartsWith("net", StringComparison.OrdinalIgnoreCase))
                   && !folderName.Equals("Debug", StringComparison.OrdinalIgnoreCase)
                   && !folderName.Equals("Release", StringComparison.OrdinalIgnoreCase)
                   && !folderName.Equals("bin", StringComparison.OrdinalIgnoreCase)
                    )
                    //Found it..
                    break;

                //Continue looking parents...
                rootPath = Path.GetDirectoryName(rootPath);
                cpt++;
            }

            return rootPath;

        }

        /// <summary>
        /// Calculate the root path for MiniEngine.Core
        /// </summary>
        private string GetRootPathMiniEngineCore()
        {

            string rootPath = GetRootPathCurrentProject();

            while (rootPath.Length > 4)
            {
                string corePath = Path.GetFullPath(Path.Combine(rootPath, "..\\MiniEngine.Core"));
                if (Directory.Exists(corePath))
                    return corePath;

                //Continue looking parents...
                rootPath = Path.GetDirectoryName(rootPath);
            }

            //Not found...
            return null;

        }


        /// <summary>
        /// Permet de vérifier si du contenu change
        /// </summary>
        public void StartWatchUpdateContent()
        {
            if (_rootFsw == null)
            {
                if (Directory.Exists(_rootPathCurrentProject))
                {
                    _rootFsw = new FileSystemWatcher(_rootPathCurrentProject);
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


        /// <summary>
        /// Check if a resource exists
        /// </summary>
        private bool TryFindResource(string name, out string resName)
        {
            //Not found... check in resources...
            string nameReplaced = name.Replace('\\', '.').Replace('/', '.');

            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!ResourceUtils.IsAssemblyUsable(ass))
                    continue;

                string ns = ass.GetName().Name;
                if (ns == "MiniEngine.Core")
                    ns = "MiniEngine";

                string fullResName = ns + "." + nameReplaced;
                string foundResName = ass.GetManifestResourceNames().FirstOrDefault(n => n.Equals(fullResName, StringComparison.OrdinalIgnoreCase));

                if (foundResName != null)
                {
                    resName = foundResName;
                    return true;
                }
            }

            resName = null;
            return false;
        }



    }
}
