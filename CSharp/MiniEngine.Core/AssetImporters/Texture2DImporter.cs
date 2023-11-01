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
        public object Import(string name)
        {

            if (!_cache.TryGetValue(name, out Texture2D texture))
            {
                try
                {
                    string assetPath = _assetManager.GetAssetPath(name, SUPPORTED_EXTENSIONS);

                    if (String.IsNullOrEmpty(assetPath))
                        throw new FileNotFoundException($"Texture not found '{name}' for supported extensions: {String.Join(", ", SUPPORTED_EXTENSIONS)}");

                        
                    texture = _assetManager.Context.Renderer.CreateTexture2D(new()
                    {
                        Data = File.ReadAllBytes(assetPath),
                        Type = TextureType.RGBA
                    });

                }
                catch (Exception ex)
                {
                    //TODO: Ajouter un warning dans l'engine
                    Debug.Print("Erreur: " + ex.ToString());
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
