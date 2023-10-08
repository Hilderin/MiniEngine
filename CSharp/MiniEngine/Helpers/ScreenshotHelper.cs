using MiniEngine.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Helpers
{
    public static class ScreenshotHelper
    {


        /// <summary>
        /// Take a screenshot
        /// </summary>
        public static Image<Rgba32> TakeScreenshot(int x, int y, int width, int height)
        {
            byte[] buffer = new byte[width * height * 4];

            GL.glReadPixels(x, y, width, height, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, buffer);
            GL.CheckError();

            return Image.LoadPixelData<Rgba32>(buffer, width, height);

        }


    }
}
