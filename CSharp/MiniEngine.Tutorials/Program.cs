

using MiniEngine.Rendering.OpenGL;
using System;
using System.Windows.Forms;

namespace MiniEngine.Tutorials
{
    internal unsafe static class Program
    {
        //private static IWindow window;

        private const string TITLE = "Simple Window";
        public const int WIDTH = 800;
        public const int HEIGHT = 800;

        private static void Main(string[] args)
        {
            try
            {

                using (Context context = new Context(new OpenGLRenderer()))
                {
                    var t = new Tutorial_Cube();

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


    }
}