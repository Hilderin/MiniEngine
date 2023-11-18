using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Renderer extension
    /// </summary>
    public interface IRendererExtension
    {
        /// <summary>
        /// Initialize the extension
        /// </summary>
        void Init(IRenderer render);
    }
}
