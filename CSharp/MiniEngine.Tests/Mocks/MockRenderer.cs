using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Tests.Mocks
{
    internal class MockRenderer : IRenderer
    {
        public Camera Camera { get; set; } = new Camera();

        public bool IsDisposing { get; set; }

        /// <summary>
        /// Get if the renderer supports async asset loading
        /// </summary>
        public bool SupportAsyncAssetLoading => false;

        public MockRenderer()
        {
            Renderer.Current = this;
        }

        public void AddExtension(IRendererExtension extension)
        {
            throw new NotImplementedException();
        }

        public IRenderHandle AddMesh(Mesh mesh, List<Material> materials, WorldTransform transform)
        {
            throw new NotImplementedException();
        }

        public Material CreateMaterial(MaterialDefinition matDef)
        {
            return new MockMaterial();
        }

        public Mesh CreateMesh()
        {
            return new MockMesh();
        }

        public Shader CreateShader()
        {
            return new MockShader();
        }

        public Texture2D CreateTexture2D(Texture2DDefinition texDef)
        {
            return new MockTexture2D();
        }

        public void Dispose()
        {
        }

        public void EnableDebug(DebugCallback debugCallback)
        {
        }

        public void EnableGui()
        {
        }

        public byte[] GetFramebufferRGBA(int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            
        }

        public void RemoveMesh(IRenderHandle handle)
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
        }

        public void SetWindow(IWindow window)
        {
            throw new NotImplementedException();
        }

        public void SetWindow32Handle(nint handle)
        {
            throw new NotImplementedException();
        }
    }
}
