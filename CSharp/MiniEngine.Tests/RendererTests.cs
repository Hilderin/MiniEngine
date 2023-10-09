using MiniEngine.PrimitiveMeshes;
using MiniEngine.Rendering.OpenGL;

namespace MiniEngine.Tests
{
    [TestClass]
    public class RendererTests
    {
        
        /// <summary>
        /// Basic cube with a centered camera at FOV 60 (default)
        /// </summary>
        [TestMethod]
        public void BasicScene_CubeCenterFOV60()
        {

            using (Context context = new Context(new OpenGLRenderer()))
            {
                context.AmbientLight.Intensity = 1f;
                context.Camera.Location.Z = -3f;

                context.Add(new CubeMesh());

                context.TestScreenshot("BasicScene_CubeCenterFOV60");

            }

        }

        /// <summary>
        /// Basic cube with a centered camera at FOV 90
        /// </summary>
        [TestMethod]
        public void BasicScene_CubeCenterFOV90()
        {

            using (Context context = new Context(new OpenGLRenderer()))
            {
                context.AmbientLight.Intensity = 1f;
                context.Camera.Location.Z = -3f;
                context.Camera.FOV = 90;

                context.Add(new CubeMesh());

                context.TestScreenshot("BasicScene_CubeCenterFOV90");

            }

        }
    }
}