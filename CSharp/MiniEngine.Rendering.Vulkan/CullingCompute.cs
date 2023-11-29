using MiniEngine.Drivers.Vulkan;
using MiniEngine.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Compute culling
    /// </summary>
    public class CullingCompute : IRendererExtension
    {
        
        private VkRenderer _renderer;
        private PipelineWrapper _cullingPipeline;
        private QueueWrapper _computeQueue;
        private PipelineDescriptorSet _cullingDescriptorSet;
        private int _lastDrawCallsBuffersCount = 0;
        private uint _workgroupSize;
        private ProfilerInfo _drawCountInfo;


        /// <summary>
        /// Init
        /// </summary>
        public void Init(IRenderer renderer)
        {
            _renderer = (VkRenderer)renderer;
            _drawCountInfo = _renderer.FrameProfiler?.AddInfo("Draw count");

            var cullingShader = (VkShader)AssetManager.Current.Get<Shader>("Shaders/culling.comp");

            if (cullingShader.ShaderWrapper.BindingSets != null)
            {
                _cullingPipeline = _renderer.CreatePipelineWrapper(cullingShader)
                                                .Build();

                _computeQueue = new QueueWrapper(_renderer.Device, _renderer.ComputeQueueIndex, 0, false);


                _cullingDescriptorSet = _cullingPipeline.CreateDescriptorSet();
                _cullingDescriptorSet.SetRendererBuffers();

                _workgroupSize = _renderer.MaxComputeWorkgroupSize[0];


                _renderer.AddActionsBeforeEachFrame(ExecuteCulling);
            }
        }

        /// <summary>
        /// Execute the compute culling
        /// </summary>
        private void ExecuteCulling()
        {
            if (_lastDrawCallsBuffersCount < _renderer.DrawCallsBuffers.Count)
            {
                bool allInitialized = true;
                for (int i = _lastDrawCallsBuffersCount; i < _renderer.DrawCallsBuffers.Count; i++)
                {
                    var drawCallsBuffer = _renderer.DrawCallsBuffers[i];

                    //drawCallsBuffer will be null if not completly initialized...
                    if (drawCallsBuffer != null)
                        _cullingDescriptorSet.Set(ShaderVariableNames.DrawCallsBuffers, drawCallsBuffer, (uint)i);
                    else
                        allInitialized = false;
                }
                if (allInitialized)
                    _lastDrawCallsBuffersCount = _renderer.MeshRenderers.Count;
            }

            if (_lastDrawCallsBuffersCount > 0)
            {
                uint nbGroupX = 1;      // (_renderer.MeshLetInstancesBuffer.Count / _workgroupSize) + 1;

                //if (nbGroupX > 96)
                //    nbGroupX = 96;


                _computeQueue.ExecuteAndWait(cb =>
                {
                    cb.CmdBindPipeline(PipelineBindPoint.Compute, _cullingPipeline);
                    cb.CmdBindDescriptorSets(PipelineBindPoint.Compute, _cullingPipeline, 0, _cullingDescriptorSet, null);
                    cb.CmdDispatch(nbGroupX, 1, 1);
                });



                uint[] counts = new uint[_workgroupSize];
                _renderer.DrawCallsCountsBuffer.CopyTo(counts);

                uint nbVisible = 0;
                for (int i = 0; i < counts.Length; i++)
                {
                    nbVisible += counts[i];
                }
                _drawCountInfo.Update(nbVisible.ToString());
            }

        }

    }


    public static class CullingComputeExtensions
    {
        /// <summary>
        /// Add the culling compute extension
        /// </summary>
        public static T AddCullingCompute<T>(this T renderer) where T : IRenderer
        {
            renderer.AddExtension(new CullingCompute());
            return renderer;
        }
    }
}
