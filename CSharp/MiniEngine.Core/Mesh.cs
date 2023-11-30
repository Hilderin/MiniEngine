using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{

    /// <summary>
    /// Data for the representation of a Model (mesh) that can contains multiple submeshes
    /// </summary>
    public abstract class Mesh: IDisposable
    {
        /// <summary>
        /// Default materials. Represents the slots for the materials for this mesh
        /// </summary>
        public Material[] Materials;

        /// <summary>
        /// Destruction of the Mesh
        /// </summary>
        protected abstract void Destroy();

        /// <summary>
        /// Load or reload the asset
        /// </summary>
        public abstract Mesh Load(MeshDefinition meshDef);


        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Destroy();
        }

        
    }

}
