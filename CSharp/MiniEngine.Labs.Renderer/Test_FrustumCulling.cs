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
    internal class Test_FrustumCulling
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
            Mesh mesh = Context.Asset.Get<Mesh>(@"C:\Projects\ogldev\Content\antique_ceramic_vase_01_4k.blend\antique_ceramic_vase_01_4k.obj");


            _cube = Scene.Add(new MeshObject() { Mesh = mesh }
                                                .MoveTo(new Vector3(1.5f, 0f, 0f))
                                                .SetMaterial(BaseMaterials.Aqua, 0)
                     );

            _cube2 = Scene.Add(new MeshObject() { Mesh = mesh }
                                                .MoveTo(new Vector3(-1.5f, 0f, 0f))
                                                .SetMaterial(BaseMaterials.White, 0)
                     );

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

            InitCullingCompute();

        }

        private void InitCullingCompute()
        {

            var cullingShader = (VkShader)AssetManager.Current.Get<Shader>("Shaders/Test_FrustumCulling.comp");
            //var cullingShader = _renderer.CreateShader(new()
            //{
            //    ComputeCode = AssetManager.Current.GetString("Shaders/Test_FrustumCulling.comp")
            //});
            _cullingPipeline = _renderer.CreatePipelineWrapper(cullingShader)
                                            .Build();

            _computeQueue = new QueueWrapper(_renderer.Device, _renderer.ComputeQueueIndex, 0, false);


            _cullingDescriptorSet = _cullingPipeline.CreateDescriptorSet();
            _cullingDescriptorSet.SetRendererBuffers();



            _renderer.AddActionsBeforeEachFrame(Culling);
        }

        private void Culling()
        {
            if (_lastDrawCallsBuffersCount < _renderer.DrawCallsBuffers.Count)
            {
                for (int i = _lastDrawCallsBuffersCount; i < _renderer.DrawCallsBuffers.Count; i++)
                {
                    var drawCallsBuffer = _renderer.DrawCallsBuffers[i];
                    _cullingDescriptorSet.Set(ShaderVariableNames.DrawCallsBuffers, drawCallsBuffer, (uint)i);
                }
                _lastDrawCallsBuffersCount = _renderer.MeshRenderers.Count;
            }

            if (_lastDrawCallsBuffersCount > 0)
            {
                uint nbGroupX = _renderer.MeshLetInstancesBuffer.Count;  // (uint)_lastDrawCallsBuffersCount;
                //uint nbGroupY = 2;

                _computeQueue.ExecuteAndWait(cb =>
                {
                    cb.CmdBindPipeline(PipelineBindPoint.Compute, _cullingPipeline);
                    cb.CmdBindDescriptorSets(PipelineBindPoint.Compute, _cullingPipeline, 0, _cullingDescriptorSet, null);
                    _cullingPipeline.UpdatePushConstants(cb);
                    cb.CmdDispatch(nbGroupX, 1, 1);

                });
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
