using MiniEngine.Drivers.Vulkan;
using MiniEngine.Rendering.Vulkan;
using System.Diagnostics;
using MiniEngine.Presentations.Glfw;
using System;
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
            //Application.Run(new VulkanForm());

            try
            {
                
                using (Context context = new Context())
                {
                    var t = new Test_ImGUI();

                    context.SetRenderer(new VkRenderer(t.GetType().Name, new MiniEngine.Drivers.Vulkan.VkVersion(1, 0, 0), null, DebugCallback))
                           .SetWindow(new GlfwWindow(WIDTH, HEIGHT, TITLE, context))
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
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }

            
        }

        private static bool DebugCallback(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, int messageCode, string message)
        {
            if (flags == DebugReportFlagsExt.Error)
                throw new Exception($"Vulkan error: {message}");

            Debug.WriteLine($"{flags}: {message}");
            return true;
        }
    }
}