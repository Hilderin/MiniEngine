using MiniEngine;
using MiniEngine.Presentations.Glfw;
using MiniEngine.Rendering.Vulkan;
using System;
using System.Diagnostics;

namespace ShipDefense
{
    internal static class Program
    {
        private const string TITLE = "ShipDefense";
        public const int WIDTH = 1200;
        public const int HEIGHT = 800;


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {

                using (Context context = new Context())
                {
                    var game = new Game(context);

                    context.SetRenderer(new VkRenderer(TITLE, "1.0.0")
                                                .EnableDebug(DebugCallback)
                                       )
                           .SetWindow(new GlfwWindow(WIDTH, HEIGHT, TITLE, context))
                           .Init(() =>
                           {
                               game.Init();
                           })
                           .Run(() =>
                           {
                               game.Update();
                           });



                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error: " + ex.ToString());
            }


        }

        private static void DebugCallback(DebugReportLevel level, int messageCode, string message)
        {
            if (level == DebugReportLevel.Error)
                throw new Exception($"Vulkan error: {message}");

            Debug.WriteLine($"{level}: {message}");
        }
    }
}