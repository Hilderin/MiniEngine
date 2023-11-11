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

            var shader = Context.Renderer.CreateShader(new()
            {
                VertexCode = ResourceUtils.GetString("Shaders.Test_IndirectDrawing.vert"),
                FragmentCode = ResourceUtils.GetString("Shaders.Test_IndirectDrawing.frag"),
                //VariableDefinitions = new()
                //                        {
                //                            { "_sampler_diffuse", new() { Count = 10, Bindless = true } }
                //                        }
            });

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

            for (int i = 0; i < 10000; i++)
            {
                _cubes.Add(Scene.Add(new MeshObject() { Mesh = cubeMesh }
                                                .MoveTo(new Vector3(Math.RandomFloat(-50, 50), Math.RandomFloat(-50, 50), Math.RandomFloat(-50, 50)))
                                               //.SetScale(new Vector3(7f, 8f, 9f))
                                               //.RotatePitch(10f)
                                               //.RotateYaw(11f)
                                               //.RotateRoll(12f)
                                               .SetMaterial(matAqua, 0)
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
            while (true)
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


            _cube.RotateYaw(1f * Time.DeltaTime);

            //for (int i = 0; i < _cubes.Count; i++)
            //{
            //    if(i % 2 == 0)
            //        _cubes[i].RotatePitch(((i % 10) + 1) * Time.DeltaTime);
            //    else
            //        _cubes[i].RotateYaw(((i % 10) + 1) * Time.DeltaTime);
            //}

            //System.Threading.Thread.Sleep(3);

            var windowSize = Context.Window.ClientSize;

            ImGui.Begin("FPSCount", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoInputs);
            ImGui.SetWindowPos(new System.Numerics.Vector2(windowSize.X - 100, 10));
            ImGui.Text(Time.FramePerSeconds.ToString());
            ImGui.End();

        }

    }
}
