using System;
using System.Diagnostics;
using System.IO;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_Triangle
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {

            MiniEngine.Renderer.Current.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -1f);

            _currentMesh = PrimitiveObjects.CreateTriangleMeshObject()
                                            .MoveTo(new Vector3(0f, 0f, 0f))
                                            .SetMaterial(Context.Renderer.CreateMaterial(new()
                                                        {
                                                            DiffuseTexture = Context.Renderer.CreateTexture2D(new()
                                                            {
                                                                Data = File.ReadAllBytes(@"C:\Projects\VulkanTutorialOverv\resources\viking_room.png")
                                                            }),
                                                            Shader = BaseShaders.Unlit
                                                        }), 0);

            Scene.Add(_currentMesh);
        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);
            LabHelper.ShowStats();

            System.Threading.Thread.Sleep(3);

        }

    }
}
