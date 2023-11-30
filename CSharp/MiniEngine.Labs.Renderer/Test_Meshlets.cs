using ImGuiNET;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.Rendering.Vulkan;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_Meshlets
    {

        private MeshObject _cube;
        private MeshObject _cube2;
        private List<MeshObject> _cubes = new List<MeshObject>();

        private VkRenderer _renderer;
        private Context Context = Context.Current;
        private Scene Scene = new Scene();
        private PipelineWrapper _cullingPipeline;
        private QueueWrapper _computeQueue;
        private PipelineDescriptorSet _cullingDescriptorSet;
        private int _lastDrawCallsBuffersCount = 0;

        public void Init()
        {
            //Context.SetMaxFramerate(-1);
            Context.Renderer.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -3f);

            _renderer = (VkRenderer)Context.Renderer;





            //Mesh cubeMesh = Primitives.CreateCubeMesh();
            Mesh mesh = Context.Asset.Get<Mesh>(@"..\..\Assets\Tests\AntiqueCeramicVase\antique_ceramic_vase_01_4k.obj");


            _cube = Scene.Add(new MeshObject() { Mesh = mesh }
                                                //.MoveTo(new Vector3(1.5f, 0f, 0f))
                                                .SetMaterial(BaseMaterials.Aqua, 0)
                     );

            _cube2 = Scene.Add(new MeshObject() { Mesh = mesh }
                                                .MoveTo(new Vector3(-1.5f, 0f, 0f))
                                                .SetMaterial(BaseMaterials.White, 0)
                     );

            Scene.Add(PrimitiveObjects.CreateSphereMeshObject());

           //_cubes.Add(Scene.Add(new MeshObject() { Mesh = mesh }));

           int spread = 10;
            for (int i = 0; i < 100; i++)
            {
                _cubes.Add(Scene.Add(new MeshObject() { Mesh = mesh }
                                                .MoveTo(new Vector3(Math.RandomFloat(-spread, spread), Math.RandomFloat(-spread, spread), Math.RandomFloat(-spread, spread)))
                                                //.SetScale(new Vector3(Math.RandomFloat(-.5f, 1.5f), Math.RandomFloat(-.5f, 1.5f), Math.RandomFloat(-.5f, 1.5f)))
                                                .SetMaterial(BaseMaterials.Aqua, 0)
                     //.RotatePitch(10f)
                     //.RotateYaw(11f)
                     //.RotateRoll(12f)
                     //.SetMaterial(mats[Math.RandomInt(0, mats.Count)], 0)
                     ));
            }

            //int spread = 10;
            //for (int i = 0; i < 10000; i++)
            //{
            //    _cubes.Add(Scene.Add(new MeshObject() { Mesh = mesh }
            //                                    .MoveTo(new Vector3(Math.RandomFloat(-spread, spread), Math.RandomFloat(-spread, spread), Math.RandomFloat(-spread, spread)))
            //                                    //.SetScale(new Vector3(Math.RandomFloat(-.5f, 1.5f), Math.RandomFloat(-.5f, 1.5f), Math.RandomFloat(-.5f, 1.5f)))
            //                                    .SetMaterial(BaseMaterials.Aqua, 0)
            //         //.RotatePitch(10f)
            //         //.RotateYaw(11f)
            //         //.RotateRoll(12f)
            //         //.SetMaterial(mats[Math.RandomInt(0, mats.Count)], 0)
            //         ));
            //}

            ThreadPool.QueueUserWorkItem(a => RotateCubes());

        }

        private void RotateCubes()
        {
            while (!Context.Renderer.IsDisposing)
            {
                for (int i = 0; i < _cubes.Count; i++)
                {
                    //if (i % 2 == 0)
                    //    _cubes[i].RotatePitch(((i % 10) + 1));
                    //else
                        _cubes[i].RotateYaw(((i % 10) + 1));
                }

                System.Threading.Thread.Sleep(10);
            }
        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);
            LabHelper.ShowStats();

            _cube?.RotateYaw(1f * Time.DeltaTime);
            _cube2?.RotateYaw(-1f * Time.DeltaTime);

        }

    }
}
