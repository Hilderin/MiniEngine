using MiniEngine.PrimitiveMeshes;
using MiniEngine.Rendering.OpenGL;

namespace MiniEngine.Tests
{
    [TestClass]
    public class RendererTests
    {
        private IRenderer CreateRendererTest()
        {
            return new OpenGLRenderer()
            {
                ShouldSwapBuffer = false
            };
        }

        /// <summary>
        /// Basic cube with a centered camera at FOV 60 (default)
        /// </summary>
        [TestMethod]
        public void BasicScene_CubeCenterFOV60()
        {
            using (Context context = new Context(CreateRendererTest()))
            {
                context.Scene.AmbientLight.Intensity = 1f;
                context.Scene.Camera.Location.Z = -3f;

                context.Scene.Add(new CubeMesh());

                context.TestScreenshot("BasicScene_CubeCenterFOV60");

            }

        }

        /// <summary>
        /// Basic cube with a centered camera at FOV 90
        /// </summary>
        [TestMethod]
        public void BasicScene_CubeCenterFOV90()
        {

            using (Context context = new Context(CreateRendererTest()))
            {
                context.Scene.AmbientLight.Intensity = 1f;
                context.Scene.Camera.Location.Z = -3f;
                context.Scene.Camera.FOV = 90;

                context.Scene.Add(new CubeMesh());

                context.TestScreenshot("BasicScene_CubeCenterFOV90");

            }

        }
    }
}