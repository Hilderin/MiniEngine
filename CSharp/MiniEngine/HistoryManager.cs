using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// History manager to trak added and removed objects on scenes
    /// </summary>
    public class HistoryManager
    {

        public List<MeshComponent> AddedMeshes = new List<MeshComponent>();
        public List<MeshComponent> RemovedMeshes = new List<MeshComponent>();

        /// <summary>
        /// Constructor
        /// </summary>
        public HistoryManager()
        {

        }


    }



}
