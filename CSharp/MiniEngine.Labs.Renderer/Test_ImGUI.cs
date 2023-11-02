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

            Debug.Print("DeltaTime: " + Time.DeltaTime + ", LastFrame: " + Time.LastFrameGenerationTime.TotalMilliseconds);
            Context.Current.SetMaxFramerate(60);

            ImGui.ShowDemoWindow();
            //ImGui.Begin("Test");
            //ImGui.Button("Wow");
            //ImGui.End();

            _currentMesh.RotateYaw(1f * Time.DeltaTime);

            //System.Threading.Thread.Sleep(3);

        }




    }
}
