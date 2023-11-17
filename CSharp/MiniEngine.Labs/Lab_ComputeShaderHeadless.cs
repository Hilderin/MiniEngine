using MiniEngine.Drivers.Vulkan;
using MiniEngine.Rendering.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace MiniEngine.Labs
{
    /// <summary>
    /// Test headless compute shader
    /// </summary>
    internal class Lab_ComputeShaderHeadless
    {
        public void Test()
        {
            try
            {
                using (var renderer = new VkRenderer("Lab_ComputeShaderHeadless", "1.0.0")
                                            .EnableDebug()
                                            .EnableHeadless()
                                            .Init())
                {

                    if (!RenderDoc.Load(@"C:\Program Files\RenderDoc\renderdoc.dll", out var rendererDoc))
                        Console.WriteLine("Impossible to load renderdoc.");

                    

                    string shaderCode = @"#version 450

layout(binding = 0) buffer Pos {
   uint values[ ];
};

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout (constant_id = 0) const uint BUFFER_ELEMENTS = 16;

uint fibonacci(uint n) {
	if(n <= 1){
		return n;
	}
	uint curr = 1;
	uint prev = 1;
	for(uint i = 2; i < n; ++i) {
		uint temp = curr;
		curr += prev;
		prev = temp;
	}
	return curr;
}

void main() 
{
	uint index = gl_GlobalInvocationID.x;
	if (index >= BUFFER_ELEMENTS) 
		return;	
	values[index] = fibonacci(values[index]);
    //values[index] = values[index] + 1;
}
";

                    var shader = renderer.CreateShader()
                                         .Load(new()
                    {
                        StageCodes = { { ShaderStage.Compute, shaderCode } }
                    });

                    int[] fibonacci = new int[32];
                    for (int i = 0; i < fibonacci.Length; i++)
                    {
                        fibonacci[i] = i;
                    }

                    var pipeline = renderer.CreatePipelineWrapper(shader)
                                            .SetSpecializationValue("BUFFER_ELEMENTS", fibonacci.Length)
                                            .Build();

                    

                    //Must be TransferDst and Src to copy to and from GPU
                    var buffer = renderer.CreateBufferWrapper(fibonacci, BufferUsageFlags.StorageBuffer | BufferUsageFlags.TransferDst | BufferUsageFlags.TransferSrc, Drivers.Vulkan.MemoryPropertyFlags.DeviceLocal);

                    var descriptorSet = pipeline.CreateDescriptorSet()
                                                .Set("Pos", buffer);

                    var computeQueue = new QueueWrapper(renderer.Device, renderer.ComputeQueueIndex, 0, false);

                    

                    

                    int cpt = 0;
                    while (true)
                    {
                        

                        rendererDoc?.StartFrameCapture();

                        int nbGroup = fibonacci.Length;

                        computeQueue.ExecuteAndWait(cb =>
                        {
                            cb.CmdBindPipeline(PipelineBindPoint.Compute, pipeline);
                            cb.CmdBindDescriptorSets(PipelineBindPoint.Compute, pipeline, 0, descriptorSet, null);
                            cb.CmdDispatch((uint)nbGroup, 1, 1);

                        });

                        int[] fibonacciResult = new int[fibonacci.Length];
                        buffer.CopyTo(fibonacciResult);

                        Console.WriteLine("Frame captured: ");
                        for (int i = 0; i < fibonacciResult.Length; i++)
                        {
                            Console.WriteLine($"  {i} = {fibonacciResult[i]}");
                        }

                        rendererDoc?.EndFrameCapture();

                        Console.WriteLine("");
                        Console.WriteLine("Press enter to capture the next renderdoc frame.");
                        Console.ReadLine();

                        cpt++;

                    }

                    

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur: " + ex.ToString());
            }
            Console.ReadLine();

        }

    }
}
