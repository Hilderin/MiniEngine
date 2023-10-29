using System;
using System.Diagnostics;
using ImGuiNET;
using MiniEngine.Assets;
using MiniEngine.Rendering.Vulkan;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_ImGUI
    {
       
        private Context Context = Context.Current;
        private Scene Scene = Context.Current.Scene;
        private CameraObject Camera = Context.Current.Scene.Camera;

        

        public void Init()
        {
            ((VkRenderer)Context.Renderer).InitGui();
        }


        public void Update()
        {
            ((VkRenderer)Context.Renderer).UpdateImGuiInput(Context.Input);

            ImGui.ShowDemoWindow();
            //ImGui.Begin("Test");
            //ImGui.Button("Wow");
            //ImGui.End();

            

            System.Threading.Thread.Sleep(3);

        }

    }
}
