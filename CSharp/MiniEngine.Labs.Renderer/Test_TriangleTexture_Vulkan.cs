using System;
using System.Diagnostics;
using System.IO;
using MiniEngine.Assets;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_TriangleTexture_Vulkan
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {

            Context.LockCursor();



            MiniEngine.Renderer.Current.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -1f);

            _currentMesh = PrimitiveObjects.CreateTriangleMeshObject();
            _currentMesh.Location = new Vector3(0f, 0f, 0f);

            _currentMesh.Materials.Add(Context.Renderer.CreateMaterial(new()
            {
                DiffuseTexture = Context.Renderer.CreateTexture2D(new()
                {
                    Data = File.ReadAllBytes(@"C:\Projects\VulkanTutorialOverv\resources\viking_room.png")
                }),
                Shader = BaseShaders.Unlit
            }));

            Scene.Add(_currentMesh);
        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);

            System.Threading.Thread.Sleep(3);

        }

    }
}
