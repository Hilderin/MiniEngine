using System;
using System.Diagnostics;
using System.IO;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_Model
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {
            //View from up and look down at 45deg
            //Context.Renderer.Camera.Transform.MoveTo(new Vector3(0.0f, 6f, -6f))
            //                                 .RotatePitch(Math.DegToRad(-45));

            //Just move camera back
            Context.Renderer.Camera.Transform.MoveTo(new Vector3(0.0f, 0f, -6f));

            _currentMesh = new MeshObject()
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\VikingRoom\\viking_room.obj")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\VikingRoom\\viking_room.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\VikingRoom\\viking_room.dae")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\HighBox_-ZForward_Yup.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\HighBox_YForward_Zup.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\HighBox_-ZForward_Yup_Triangulated.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\HighBox_-ZForward_Yup_NoSpaceTransform.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\HighBox_-ZForward_Yup_TransformWithApplyUnit0.01.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\HighBox_-ZForward_Yup_TransformWithApplyUnit1.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\Plate_Rectangle.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\Plane.obj")
                                .SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\Plane.fbx")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\Plane.glb")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\SimpleModels\\Plane.dae")
                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\Table\\odesd2_B2_obj.obj")
                                .SetScale(0.03f);


                                //.SetMesh("C:\\Projects\\MiniEngine\\Assets\\Tests\\VikingRoom\\VikingRoom_from_Blender.obj")                                
                                //.SetMaterial("C:\\Projects\\MiniEngine\\Assets\\Tests\\VikingRoom\\viking_room.png", 0);

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
