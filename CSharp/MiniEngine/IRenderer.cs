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
        /// Set the window handle for win32 (Windows)
        /// </summary>
        void SetWindow32Handle(IntPtr handle);

        /// <summary>
        /// Render the scene
        /// </summary>
        void Render(ICamera camera);

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

        /// <summary>
        /// Create a shader
        /// </summary>
        Shader CreateShader(ShaderDefinition shaderDef);

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

    }
}
