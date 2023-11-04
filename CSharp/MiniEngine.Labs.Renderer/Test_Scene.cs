using System;
using System.Diagnostics;
using System.IO;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_Scene
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {

            //Context.LockCursor();

            //View from up and look down at 45deg
            //Context.Renderer.Camera.Transform.MoveTo(new Vector3(0.0f, 6f, -6f))
            //                                 .RotatePitch(Math.DegToRad(-45));

            //Just move camera back
            Context.Renderer.Camera.Transform.MoveTo(new Vector3(0.0f, 0f, -6f));

            _currentMesh = new MeshObject()
                                .SetMesh("C:\\Projects\\glTF-Sample-Models\\2.0\\Sponza\\glTF\\Sponza.gltf");

            Scene.Add(_currentMesh);
        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);

            System.Threading.Thread.Sleep(3);

        }

    }
}
