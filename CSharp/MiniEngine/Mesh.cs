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
        //public List<Material> Materials;
        //public List<SubMeshData> SubMeshes;

        /// <summary>
        /// Destruction of the Mesh
        /// </summary>
        protected abstract void Destroy();

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Destroy();
        }
    }

}
