using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.AssetImporters
{
    public interface IAssetImporter
    {
        /// <summary>
        /// Import an asset
        /// </summary>
        object Import(string name);

        /// <summary>
        /// Reset the cache
        /// </summary>
        void ResetCache();
    }

}
