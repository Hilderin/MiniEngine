using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Interface for the renderers
    /// </summary>
    public interface IRenderer: IDisposable
    {
        /// <summary>
        /// Initialize the rendering engine
        /// </summary>
        void Init();

        /// <summary>
        /// Clear the screen (called before a new frame)
        /// </summary>
        void Clear();

        /// <summary>
        /// Render the scene
        /// </summary>
        void Render(Context context);

        /// <summary>
        /// Get the frame buffer 
        /// </summary>
        byte[] GetFramebufferRGBA(int x, int y, int width, int height);
    }
}
