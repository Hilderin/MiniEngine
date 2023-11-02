using MiniEngine;
using MiniEngine.Presentations.Glfw;
using MiniEngine.Rendering.Vulkan;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MiniEngine.Labs.Renderer
{
    internal static class Program
    {
        private const string TITLE = "Simple Window";
        public const int WIDTH = 1200;
        public const int HEIGHT = 800;


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ////Testing with Windows Form.....
            //ApplicationConfiguration.Initialize();
            //using (var form = new VulkanForm())
            //{
            //    Application.Run(form);
            //}

            try
            {

                using (Context context = new Context())
                {
                    var t = new Test_ImGUI();

                    using (var render = new VkRenderer(t.GetType().Name, "1.0.0"))
                    using (var window = new GlfwWindow(WIDTH, HEIGHT, TITLE, context))
                    {

                        context.SetRenderer(render)
                               .EnableDebug()
                               .SetWindow(window)
                               .Init(() =>
                               {
                                   t.Init();
                               })
                               .Run(() =>
                               {
                                   t.Update();

                                   if (context.Input.IsKeyDown(Keys.Escape))
                                   {
                                       if (t is IDisposable)
                                           ((IDisposable)t).Dispose();

                                       context.Quit();
                                   }
                               });

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }

            
        }
    }
}