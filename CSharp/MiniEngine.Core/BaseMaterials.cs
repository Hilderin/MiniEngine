using MiniEngine.ResourceDefinitions;
using System;
using System.Diagnostics;

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
        /// Aqua
        /// </summary>
        private static Material _aqua;

        /// <summary>
        /// Default material
        /// </summary>
        public static Material Default => White;

        /// <summary>
        /// Get a base white material
        /// </summary>
        public static Material White
        {
            get
            {
                _white ??= CreateUnlitMaterial(BaseTextures.Default);

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
                _magenta ??= CreateUnlitMaterial(BaseTextures.Magenta);

                return _magenta;
            }
        }

        /// <summary>
        /// Get a base aqua material
        /// </summary>
        public static Material Aqua
        {
            get
            {
                _aqua ??= CreateUnlitMaterial(BaseTextures.Aqua);

                return _aqua;
            }
        }


        /// <summary>
        /// Create a material with base unlit shader
        /// </summary>
        private static Material CreateUnlitMaterial(Texture2D texture)
        {
            if (Renderer.Current == null)
                throw new InvalidOperationException("No current renderer.");

            MaterialDefinition matDef = new MaterialDefinition()
            {
                DiffuseTexture = texture,
                Shader = BaseShaders.Default
            };

            Material mat = Context.Current.Renderer.CreateMaterial(matDef);
            mat.Name = texture.Name;
            return mat;
        }

    }
}
