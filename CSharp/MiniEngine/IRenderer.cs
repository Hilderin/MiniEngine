using MiniEngine.ResourceDefinitions;
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
        ///// <summary>
        ///// Update the window options specific to the engine
        ///// </summary>
        //void PreInitGlfw();

        /// <summary>
        /// Initialize the rendering engine
        /// </summary>
        void Init();

        /// <summary>
        /// Pass the window to the render when it's created
        /// </summary>
        void SetWindow(IWindow window);

        /// <summary>
        /// Render the scene
        /// </summary>
        void Render(Scene scene);

        /// <summary>
        /// Get the frame buffer 
        /// </summary>
        byte[] GetFramebufferRGBA(int x, int y, int width, int height);

        /// <summary>
        /// Indicate we the buffer sould be swapped each frame
        /// </summary>
        bool ShouldSwapBuffer { get; }

        /// <summary>
        /// Create a new mesh
        /// </summary>
        Mesh CreateMesh(MeshDefinition meshDefinition);

        /// <summary>
        /// Create a Texture2D
        /// </summary>
        Texture2D CreateTexture2D(Texture2DDefinition texDef);

        /// <summary>
        /// Create a Material
        /// </summary>
        Material CreateMaterial(MaterialDefinition matDef);

    }
}
