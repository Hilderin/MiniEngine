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
                using (var renderer = new VkRenderer(TITLE, "1.0.0"))
                using (var window = new GlfwWindow(WIDTH, HEIGHT, TITLE, context))
                {
                    var game = new Game(context);

                    context.EnableDebug()
                           .SetRenderer(renderer)
                           .SetWindow(window)
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

    }
}