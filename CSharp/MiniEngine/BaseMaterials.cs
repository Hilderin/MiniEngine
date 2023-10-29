using MiniEngine.ResourceDefinitions;

namespace MiniEngine
{
    /// <summary>
    /// Base materials
    /// </summary>
    public static class BaseMaterials
    {
        /// <summary>
        /// White
        /// </summary>
        private static Material _white;

        /// <summary>
        /// Magenta
        /// </summary>
        private static Material _magenta;

        /// <summary>
        /// Get a base white material
        /// </summary>
        public static Material White
        {
            get
            {
                if (_white == null)
                {
                    MaterialDefinition matDef = new MaterialDefinition()
                    {
                        DiffuseTexture = BaseTextures.White,
                        Shader = BaseShaders.Unlit
                    };

                    _white = Context.Current.Renderer.CreateMaterial(matDef);
                }

                return _white;
            }
        }

        /// <summary>
        /// Get a base magenta material
        /// </summary>
        public static Material Magenta
        {
            get
            {
                if (_magenta == null)
                {
                    MaterialDefinition matDef = new MaterialDefinition()
                    {
                        DiffuseTexture = BaseTextures.Magenta,
                        Shader = BaseShaders.Unlit
                    };

                    _magenta = Context.Current.Renderer.CreateMaterial(matDef);
                }

                return _magenta;
            }
        }
    }
}
