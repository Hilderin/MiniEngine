using MiniEngine.Drivers.Vulkan;
using System.Diagnostics;

namespace MiniEngine.Tutorials.Drivers.Vulkan
{
    internal static class Program
    {
        private const string TITLE = "Simple Window";
        public const int WIDTH = 800;
        public const int HEIGHT = 800;


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //// To customize application configuration such as set high DPI settings or default font,
            //// see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //Application.Run(new VulkanForm());

            try
            {
                //VkInstance vk = new VkInstance();

                //vk.InitWindow();
                //vk.InitVulkan();
                //vk.MainLoop();
                //vk.Dispose();

                using (Context context = new Context(new VkRenderer("Test", null, DebugCallback)))
                //using (Context context = new Context(new VulkanRenderer("MiniEngine Tutorial", true)))
                {
                    var t = new Tutorial_Cube_Vulkan();

                    context.OpenWindow(WIDTH, HEIGHT, TITLE)
                           .Init(() =>
                           {
                               t.Init();
                           })
                           .Run(() =>
                           {
                               t.Update();

                               if (context.Input.IsKeyPressed(Keys.Escape))
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