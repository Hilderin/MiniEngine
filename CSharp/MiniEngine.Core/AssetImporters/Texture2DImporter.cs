using MiniEngine.AssetDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniEngine.AssetImporters
{
    /// <summary>
    /// Texture2D importer
    /// </summary>
    public class Texture2DImporter : IAssetImporter
    {
        /// <summary>
        /// Supported extensions
        /// </summary>
        public static readonly string[] SUPPORTED_EXTENSIONS = new string[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff", ".gif" };


        private AssetManager _assetManager;

        /// <summary>
        /// Cache materials
        /// </summary>
        private Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Texture2D importer
        /// </summary>
        public Texture2DImporter(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }


        /// <summary>
        /// Import a texture...
        /// </summary>
        public object Import(string name, string workingFolderUri)
        {

            if (!_cache.TryGetValue(name, out Texture2D texture))
            {
                string assetPath;

                try
                {
                    if(!_assetManager.TryFindAssetUri(name, workingFolderUri, false, out assetPath))
                        throw new FileNotFoundException($"Texture not found '{name}'.");


                    texture = Renderer.Current.CreateTexture2D(new()
                    {
                        Data = _assetManager.GetBytes(assetPath),
                        Type = TextureType.RGBA
                    });

                    texture.Name = Path.GetFileName(name);

                }
                catch (Exception ex)
                {
                    Debug.Error(ex);
                }
                finally
                {
                    //Texture not found...
                    texture ??= BaseTextures.Magenta;
                }

                _cache.Add(name, texture);

            }

            return texture;

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
