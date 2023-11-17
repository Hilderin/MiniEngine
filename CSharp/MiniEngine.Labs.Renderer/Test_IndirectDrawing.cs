using ImGuiNET;
using MiniEngine.Rendering.Vulkan;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_IndirectDrawing
    {

        private MeshObject _cube;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();
        private List<MeshObject> _cubes = new List<MeshObject>();

        public void Init()
        {
            //Context.SetMaxFramerate(-1);
            Context.Renderer.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -3f);

            var shader = Context.Asset.Get<Shader>("Shaders/Test_IndirectDrawing.vert");

            var matWhite = Context.Renderer.CreateMaterial(new()
            {
                DiffuseTexture = BaseTextures.White,
                Shader = shader
            });

            var matAqua = Context.Renderer.CreateMaterial(new()
            {
                DiffuseTexture = BaseTextures.Aqua,
                Shader = shader
            });

            var matMagenta = Context.Renderer.CreateMaterial(new()
            {
                DiffuseTexture = BaseTextures.Magenta,
                Shader = shader
            });

            List<Material> mats = new List<Material>();
            mats.Add(matWhite);
            mats.Add(matAqua);
            mats.Add(matMagenta);

            Scene.Add(PrimitiveObjects.CreateTriangleMeshObject()
                                           .MoveTo(new Vector3(0f, 0f, 0f))
                                           .SetMaterial(matWhite, 0)
                     );

            Mesh cubeMesh = Primitives.CreateCubeMesh();


            _cube = Scene.Add(new MeshObject() { Mesh = cubeMesh }
                                                .MoveTo(new Vector3(1f, 0f, 0f))
                                               //.SetScale(new Vector3(7f, 8f, 9f))
                                               //.RotatePitch(10f)
                                               //.RotateYaw(11f)
                                               //.RotateRoll(12f)
                                               .SetMaterial(matAqua, 0)
                     );

            int spread = 10;
            for (int i = 0; i < 100; i++)
            {
                _cubes.Add(Scene.Add(new MeshObject() { Mesh = cubeMesh }
                                                .MoveTo(new Vector3(Math.RandomFloat(-spread, spread), Math.RandomFloat(-spread, spread), Math.RandomFloat(-spread, spread)))
                                                .SetScale(new Vector3(Math.RandomFloat(-.5f, 1.5f), Math.RandomFloat(-.5f, 1.5f), Math.RandomFloat(-.5f, 1.5f)))
                                               //.RotatePitch(10f)
                                               //.RotateYaw(11f)
                                               //.RotateRoll(12f)
                                               .SetMaterial(mats[Math.RandomInt(0, mats.Count)], 0)
                     ));
            }

            //Scene.Add(PrimitiveObjects.CreateCubeMeshObject()
            //                               .MoveTo(new Vector3(2f, 0f, 0f))
            //                               .SetMaterial(matAqua, 0)
            //         );

            ThreadPool.QueueUserWorkItem(a => RotateCubes());
        }

        private void RotateCubes()
        {
            while (!Context.Renderer.IsDisposing)
            {
                for (int i = 0; i < _cubes.Count; i++)
                {
                    if (i % 2 == 0)
                        _cubes[i].RotatePitch(((i % 10) + 1) );
                    else
                        _cubes[i].RotateYaw(((i % 10) + 1) );
                }

                System.Threading.Thread.Sleep(1);
            }
        }

        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);
            LabHelper.ShowStats();

            _cube.RotateYaw(1f * Time.DeltaTime);

            //for (int i = 0; i < _cubes.Count; i++)
            //{
            //    if(i % 2 == 0)
            //        _cubes[i].RotatePitch(((i % 10) + 1) * Time.DeltaTime);
            //    else
            //        _cubes[i].RotateYaw(((i % 10) + 1) * Time.DeltaTime);
            //}

            //System.Threading.Thread.Sleep(3);


        }

    }
}
