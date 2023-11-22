using ImGuiNET;
using MiniEngine.ResourceDefinitions;
using System;
using System.Diagnostics;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_ShaderWithVariables
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {
            //Context.SetMaxFramerate(60);
            Context.Renderer.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -3f);

            //var shader = Context.Asset.Get<Shader>("Shaders/unlit_color.asset");
            var mat = Context.Asset.Get<Material>("Materials/mat_unlit_color.asset");


            _currentMesh = PrimitiveObjects.CreateCubeMeshObject(mat);

            _currentMesh.SetShaderVariable("uni_color", 0, Color4.Magenta);

            //    _currentMesh = PrimitiveObjects.CreateCubeMeshObject()
            //                                   .MoveTo(new Vector3(0f, 0f, 0f));
            //                                   //.AddMaterial(Context.Renderer.CreateMaterial(new()
            //                                   //             {
            //                                   //                 DiffuseTexture = BaseTextures.White,
            //                                   //                 Shader = shader
            //                                   //             }));


            Scene.Add(_currentMesh);

        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);
            LabHelper.ShowStats();

            //System.Threading.Thread.Sleep(3);

        }

    }
}
