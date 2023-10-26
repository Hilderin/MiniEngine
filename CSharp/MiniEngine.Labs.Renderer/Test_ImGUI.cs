
using System;
using System.Diagnostics;
using MiniEngine.Assets;
using MiniEngine.PrimitiveMeshes;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_ImGUI
    {
       
        private Context Context = Context.Current;
        private Scene Scene = Context.Current.Scene;
        private Camera Camera = Context.Current.Scene.Camera;

        public void Init()
        {
        }


        public void Update()
        {
            
            System.Threading.Thread.Sleep(3);

        }

    }
}
