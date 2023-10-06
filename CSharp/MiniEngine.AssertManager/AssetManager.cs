using Assimp;

namespace MiniEngine.AssertManager
{
    /// <summary>
    /// Manager for the assets
    /// </summary>
    public class AssetManager
    {
        /// <summary>
        /// Import a mesh from file
        /// </summary>
        public Mesh2 GetMeshFromFile(string path)
        {
            return new MeshImporter().GetMeshFromFile(path);

        }


    }
}