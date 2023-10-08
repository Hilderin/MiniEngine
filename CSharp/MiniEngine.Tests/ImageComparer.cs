using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Tests
{
    /// <summary>
    /// Class to perform simple image comparisons.
    /// </summary>
    public static class ImageComparer
    {
        /// <summary>
        /// Assert that to image are the same
        /// </summary>
        public static void AssertImage(Image<Rgba32> expectedImage, Image<Rgba32> actualImage)
        {

            Assert.AreEqual(expectedImage.Width, actualImage.Width, "Width");
            Assert.AreEqual(expectedImage.Height, actualImage.Height, "Height");

            Assert.AreEqual(expectedImage.Frames.Count, actualImage.Frames.Count, "Frames.Count");

            for (var x = 0; x < expectedImage.Width; x++)
            {
                for (var y = 0; y < expectedImage.Height; y++)
                {
                    Assert.AreEqual(expectedImage[x, y], actualImage[x, y], $"Pixel [{x}, {y}]");
                }
            }

            //byte[] expectedBytes = new byte[expectedImage.Width * expectedImage.Height * 4];
            //expectedImage.cop

            //byte[] pixelBytes = new byte[expectedImage.Width * expectedImage.Height * 4];
            //pixelBytes.CopyPixelDataTo(pixelBytes);

        }
    }
}
