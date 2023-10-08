using MiniEngine.PrimitiveMeshes;
using MiniEngine.Rendering.OpenGL;

namespace MiniEngine.Tests
{
    [TestClass]
    public class RendererTests
    {
        
        /// <summary>
        /// Basic cube with a centered camera at FOV 60
        /// </summary>
        [TestMethod]
        public void BasicScene_CubeCenterFOV60()
        {

            using (Context context = new Context(new OpenGLRenderer()))
            {
                context.TestScreenshot("BasicScene_CubeCenterFOV60", () =>
                {
                    context.Camera.Location.Z = -3f;

                    context.Add(new CubeMesh());
                });

            }

        }
    }
}