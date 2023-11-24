using ImGuiNET;
using System;
using System.Diagnostics;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_Cube
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {
            //Context.SetMaxFramerate(60);
            Context.Renderer.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -3f);

            _currentMesh = PrimitiveObjects.CreateCubeMeshObject()
                                           .MoveTo(new Vector3(0f, 0f, 0f));
            //.AddMaterial(Context.Renderer.CreateMaterial(new()
            //             {
            //                 DiffuseTexture = BaseTextures.White,
            //                 Shader = shader
            //             }));

            Scene.Add(_currentMesh);

            Scene.Add(PrimitiveObjects.CreateCubeMeshObject()
                                           .MoveTo(new Vector3(-2f, 0f, 0f))
                     );
            //.AddMaterial(Context.Renderer.CreateMaterial(new()
            //             {
            //                 DiffuseTexture = BaseTextures.White,
            //                 Shader = shader
            //             }));


            

        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);
            LabHelper.ShowStats();

            //System.Threading.Thread.Sleep(3);

        }

    }
}
