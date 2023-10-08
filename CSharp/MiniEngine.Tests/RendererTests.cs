using MiniEngine.Helpers;
using MiniEngine.PrimitiveMeshes;
using SixLabors.ImageSharp;

namespace MiniEngine.Tests
{
    [TestClass]
    public class RendererTests
    {
        private const int WIDTH = 1200;
        private const int HEIGHT = 800;

        /// <summary>
        /// Basic cube with a centered camera at FOV 60
        /// </summary>
        [TestMethod]
        public void BasicScene_CubeCenterFOV60()
        {

            using (Context context = new Context())
            {
                context.TestScreenshot("BasicScene_CubeCenterFOV60", () =>
                {
                    context.Renderer.Camera.Location.Z = -3f;

                    context.Renderer.Add(new CubeMesh());
                });

            }

        }
    }
}