using MiniEngine.Drivers.Vulkan;
using MiniEngine.Rendering.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Tests
{
    /// <summary>
    /// Extensions for testing
    /// </summary>
    public static class ContextExtensionsTest
    {
        private const int WIDTH = 1200;
        private const int HEIGHT = 800;

        public static bool IsRecording = false;

        /// <summary>
        /// Create the test 
        /// </summary>
        public static Context SetupTest(this Context context)
        {
            //TODO: Init a window...
            context.SetRenderer(new VkRenderer("Test", "1.0.0"));

            return context;
        }

        /// <summary>
        /// Test the 
        /// </summary>
        public static void TestScreenshot(this Context context, string testName)
        {
            context.SetupTest()
                       .Init()
                       .RenderOneFramebuffer();

            string pathFileResult = TestHelper.GetPathTestResultFile(testName + ".png");



            using (var actualImage = context.TakeScreenshot(0, 0, WIDTH, HEIGHT))
            {

                if (IsRecording)
                {
                    //Juste saving the image....
                    if(!Directory.Exists(Path.GetDirectoryName(pathFileResult)))
                        Directory.CreateDirectory(Path.GetDirectoryName(pathFileResult));

                    actualImage.SaveAsPng(pathFileResult);
                }
                else
                {
                    //Testing the image...
                    if (!File.Exists(pathFileResult))
                        throw new FileNotFoundException($"Test file result not found: {pathFileResult}");

                    using (var expectedImage = SixLabors.ImageSharp.Image.Load<Rgba32>(pathFileResult))
                    {
                        ImageComparer.AssertImage(expectedImage, actualImage);
                    }
                }


            }


        }
    }
}
