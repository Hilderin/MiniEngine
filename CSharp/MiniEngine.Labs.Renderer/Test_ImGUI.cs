using System;
using System.Diagnostics;
using ImGuiNET;
using MiniEngine.Rendering.Vulkan;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_ImGUI
    {
       
        private Context Context = Context.Current;

        

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
