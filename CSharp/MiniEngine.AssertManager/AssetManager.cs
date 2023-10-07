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
        public Mesh GetMeshFromFile(string path, bool inverseFaces)
        {
            return new MeshImporter().GetMeshFromFile(path, inverseFaces);

        }

        /// <summary>
        /// Import a texture 2d from file
        /// </summary>
        public Texture2D GetTexture2DFromFile(string path)
        {
            return new TextureImporter().GetTexture2DFromFile(path);

        }


    }
}