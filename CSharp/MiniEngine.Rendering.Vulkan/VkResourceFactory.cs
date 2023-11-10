using MiniEngine;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.ResourceDefinitions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Factory for Vulkan resources
    /// </summary>
    public class VkResourceFactory: IDisposable
    {
        private VkRenderer _renderer;

        private Dictionary<int, IDisposable> _resources = new Dictionary<int, IDisposable>();


        /// <summary>
        /// Constructor
        /// </summary>
        public VkResourceFactory(VkRenderer renderer)
        {
            _renderer = renderer;
        }

        /// <summary>
        /// Create a Texture2D
        /// </summary>
        public VkTexture2D CreateTexture2D(Texture2DDefinition texDef)
        {
            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(texDef.Data))
            {
                byte[] pixelData = new byte[image.Width * image.Height * image.PixelType.BitsPerPixel / 8];

                image.CopyPixelDataTo(pixelData);
                return new VkTexture2D(pixelData, image.Width, image.Height, Format.R8G8B8A8Srgb, _renderer, this);
            }
        }

        /// <summary>
        /// Create a Material
        /// </summary>
        public VkMaterial CreateMaterial(MaterialDefinition matDef)
        {
            return new VkMaterial(matDef, this);
        }

        /// <summary>
        /// Create a Mesh
        /// </summary>
        public VkMesh CreateMesh()
        {
            return new VkMesh(_renderer, this);

        }


        /// <summary>
        /// Create a shader
        /// </summary>
        public VkShader CreateShader(ShaderDefinition shaderDef)
        {
            Dictionary<string, SpirvVariableDefinition> variableDefinitions = null;

            if (shaderDef.VariableDefinitions != null && shaderDef.VariableDefinitions.Count > 0)
            {
                variableDefinitions = new Dictionary<string, SpirvVariableDefinition>();

                foreach (var kv in shaderDef.VariableDefinitions)
                {
                    SpirvVariableDefinition spirvDef = new SpirvVariableDefinition();

                    var varDef = kv.Value;

                    if (!String.IsNullOrEmpty(varDef.Format))
                    {
                        if (!Enum.TryParse<Format>(varDef.Format, true, out Format format))
                            throw new FormatException($"Invalid format for variable {kv.Key}: {varDef.Format}");

                        spirvDef.Format = format;
                    }

                    spirvDef.Count = varDef.Count;
                    spirvDef.Bindless = varDef.Bindless;
                    

                    variableDefinitions.Add(kv.Key, spirvDef);
                }

            }

            var shader = new ShaderWrapper(_renderer);

            if (variableDefinitions != null)
                shader.SetVariableDefinitions(variableDefinitions);

            if (!String.IsNullOrEmpty(shaderDef.VertexCode))
                shader.SetCode(ShaderStageFlags.Vertex, shaderDef.VertexCode);

            if (!String.IsNullOrEmpty(shaderDef.FragmentCode))
                shader.SetCode(ShaderStageFlags.Fragment, shaderDef.FragmentCode);

            if (!String.IsNullOrEmpty(shaderDef.ComputeCode))
                shader.SetCode(ShaderStageFlags.Compute, shaderDef.ComputeCode);

            return new VkShader(shader);

        }


        /// <summary>
        /// Remove an object from the resources list
        /// </summary>
        public void Add(IDisposable resource)
        {
            _resources.Add(resource.GetHashCode(), resource);
        }

        /// <summary>
        /// Remove an object from the resources list
        /// </summary>
        public void Remove(IDisposable resource)
        {
            _resources.Remove(resource.GetHashCode());
        }

        /// <summary>
        /// Dispose the resources
        /// </summary>
        public void Dispose()
        {
            foreach (IDisposable disposable in _resources.Values)
            {
                disposable.Dispose();
            }
        }
    }
}
