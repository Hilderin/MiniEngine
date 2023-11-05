using System;
using System.Diagnostics;
using ImGuiNET;
using MiniEngine.Rendering.Vulkan;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_ImGUI
    {
       
        private Context Context = Context.Current;

        private Scene _scene = new Scene();
        private MeshObject _currentMesh;

        public void Init()
        {
            //Just move camera back
            Context.Renderer.Camera.Transform.MoveTo(new Vector3(0.0f, 0f, -6f));

            _currentMesh = new MeshObject()
                                .SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\VikingRoom\\viking_room.fbx")
                                .SetMaterial("C:\\Projects\\MiniEngine\\Assets\\Tests\\VikingRoom\\viking_room.png", 0)
                                .MoveLeft(-2f)
                                .AddScale(2f);
            

            _scene.Add(_currentMesh);
        }


        public void Update()
        {
            ((VkRenderer)Context.Renderer).UpdateImGuiInput(Context.Input);

            //Debug.Info("DeltaTime: " + Time.DeltaTime + ", LastFrame: " + Time.LastFrameGenerationTime.TotalMilliseconds);
            //Context.Current.SetMaxFramerate(60);

            var windowSize = Context.Window.ClientSize;

            ImGui.Begin("FPSCount", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoInputs);
            ImGui.SetWindowPos(new System.Numerics.Vector2(windowSize.X - 100, 10));
            ImGui.Text(Time.FramePerSeconds.ToString());
            ImGui.End();

            ImGui.ShowDemoWindow();

            //ImGui.Begin("Test");
            //ImGui.Button("Wow");
            //ImGui.End();

            _currentMesh.RotateYaw(1f * Time.DeltaTime);

            //System.Threading.Thread.Sleep(3);

        }




    }
}
