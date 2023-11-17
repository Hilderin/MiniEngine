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
        /// <summary>
        /// Get or set the current camera
        /// </summary>
        Camera Camera { get; set; }

        /// <summary>
        /// Get if the renderer is disposing or disposed
        /// </summary>
        bool IsDisposing { get; }

        /// <summary>
        /// Initialize the rendering engine
        /// </summary>
        void Init();

        /// <summary>
        /// Pass the window to the render when it's created
        /// </summary>
        void SetWindow(IWindow window);

        /// <summary>
        /// Set the window handle for win32 (Windows)
        /// </summary>
        void SetWindow32Handle(IntPtr handle);

        /// <summary>
        /// Render the scene
        /// </summary>
        void Render();

        /// <summary>
        /// Get the frame buffer 
        /// </summary>
        byte[] GetFramebufferRGBA(int x, int y, int width, int height);

        /// <summary>
        /// Create a new empty mesh
        /// </summary>
        Mesh CreateMesh();

        /// <summary>
        /// Create a Texture2D
        /// </summary>
        Texture2D CreateTexture2D(Texture2DDefinition texDef);

        /// <summary>
        /// Create a Material
        /// </summary>
        Material CreateMaterial(MaterialDefinition matDef);

        /// <summary>
        /// Create a shader
        /// </summary>
        Shader CreateShader();

        /// <summary>
        /// Enable debugging
        /// </summary>
        void EnableDebug(DebugCallback debugCallback);

        /// <summary>
        /// Add a mesh on the screen
        /// </summary>
        IRenderHandle AddMesh(Mesh mesh, List<Material> materials, WorldTransform transform);

        /// <summary>
        /// Remove a mesh from the screen
        /// </summary>
        void RemoveMesh(IRenderHandle handle);

        /// <summary>
        /// Activate Dear ImGui
        /// </summary>
        void EnableGui();

    }
}
