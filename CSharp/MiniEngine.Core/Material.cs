using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public abstract class Material: IDisposable
    {
        /// <summary>
        /// Name of the material
        /// </summary>
        public string Name { get; set; }

        ///// <summary>
        ///// Not found material (magenta)
        ///// </summary>
        //private static Material _notFoundMaterial = new Material()
        //{
        //    AmbientColor = Color3.Magenta,
        //    Diffuse = Texture2D.Empty
        //};

        ///// <summary>
        ///// Default material (white)
        ///// </summary>
        //private static Material _defaultMaterial = new Material()
        //{
        //    AmbientColor = Color3.White,
        //    Diffuse = Texture2D.Empty
        //};

        ///// <summary>
        ///// Not found material (magenta)
        ///// </summary>
        //public static Material NotFound { get { return _notFoundMaterial; } }

        ///// <summary>
        ///// Default material (white)
        ///// </summary>
        //public static Material Default { get { return _defaultMaterial; } }

        ///// <summary>
        ///// The shader behind this Material
        ///// </summary>
        //public Shader Shader;

        ///// <summary>
        ///// Ambient color: the color used when ambient light hits the material
        ///// </summary>
        //public Color3 AmbientColor = Color3.White;

        ///// <summary>
        ///// Diffuse color: the color used when diffuse light hits the material
        ///// </summary>
        //public Color3 DiffuseColor = Color3.White;

        ///// <summary>
        ///// Scpecular color: the color used when specular light hits the material
        ///// </summary>
        //public Color3 SpecularColor = Color3.White;

        /// <summary>
        /// Diffuse texture (the colors)
        /// </summary>
        public Texture2D Diffuse { get; protected set; }

        ///// <summary>
        ///// Specular texture
        ///// </summary>
        //public Texture2D Specular;

        /// <summary>
        /// Constructor
        /// </summary>
        public Material()
        {

        }

        /// <summary>
        /// Destruction of the Material
        /// </summary>
        protected abstract void Destroy();

        /// <summary>
        /// Dispose the Material
        /// </summary>
        public void Dispose()
        {
            Destroy();   
        }
    }


}
