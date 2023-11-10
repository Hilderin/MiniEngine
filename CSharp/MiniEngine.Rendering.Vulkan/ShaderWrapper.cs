using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Information on a shader for Vulkan
    /// </summary>
    public class ShaderWrapper
    {
        private VkRenderer _renderer;
        private Device _device;

        /// <summary>
        /// Module per stage
        /// </summary>
        private Dictionary<ShaderStageFlags, ShaderStageModule> _stageModules = new Dictionary<ShaderStageFlags, ShaderStageModule>();

        /// <summary>
        /// Variable definitions
        /// </summary>
        private Dictionary<string, SpirvVariableDefinition> _variableDefinitions;

        /// <summary>
        /// Constants
        /// </summary>
        public PushConstantRange[] Constants;

        /// <summary>
        /// Bindings
        /// </summary>
        public DescriptorSetLayoutBinding[][] BindingSets;

        /// <summary>
        /// Bindings for the vertex buffer
        /// </summary>
        public VertexInputBindingDescription[] VertexBindings;

        /// <summary>
        /// Attributes from vertex buffer
        /// </summary>
        public VertexInputAttributeDescription[] VertexInputAttributes;

        /// <summary>
        /// Specialization contants
        /// </summary>
        public SpecializationConstant[] SpecializationConstants;

        /// <summary>
        /// Return true if the shader contains only a compute shader
        /// </summary>
        public bool IsComputeOnly
        {
            get
            {
                return _stageModules.Count == 1 && TryGetStageModule(ShaderStageFlags.Compute, out _);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ShaderWrapper(VkRenderer renderer)
        {
            _renderer = renderer;
            _device = renderer.Device;
        }


        /// <summary>
        /// Indexer
        /// </summary>
        public ShaderStageModule this[ShaderStageFlags stage]
        {
            get
            {
                return _stageModules[stage];
            }
        }

        /// <summary>
        /// Access the different stage modules
        /// </summary>
        public IEnumerable<ShaderStageModule> StageModules
        {
            get { return _stageModules.Values; }
        }


        /// <summary>
        /// Set the variable definitions
        /// </summary>
        public ShaderWrapper SetVariableDefinitions(Dictionary<string, SpirvVariableDefinition> variableDefinitions)
        {
            _variableDefinitions = variableDefinitions;
            return this;
        }

        /// <summary>
        /// Set the definition for a variable
        /// </summary>
        public ShaderWrapper SetVariable(string name, SpirvVariableDefinition definitions)
        {
            if (_variableDefinitions == null)
                _variableDefinitions = new Dictionary<string, SpirvVariableDefinition>();

            if (definitions == null)
                _variableDefinitions.Remove(name);
            else
                _variableDefinitions[name] = definitions;

            return this;
        }

        /// <summary>
        /// Set spirv for a stage
        /// </summary>
        public ShaderWrapper SetSpirv(ShaderStageFlags stage, byte[] spirv)
        {
            if (!_stageModules.TryGetValue(stage, out var module))
            {
                module = new ShaderStageModule()
                {
                    Stage = stage
                };
                _stageModules.Add(stage, module);
            }

            module.Spirv = spirv;

            //Parsing and updating the shader bindings and config...
            SpirvParser.ParseBytes(stage, spirv, this, _variableDefinitions);

            module.Module = _renderer.CreateShaderModule(spirv);

            return this;

        }


        /// <summary>
        /// Set code for a stage
        /// </summary>
        public ShaderWrapper SetCode(ShaderStageFlags stage, string code)
        {
            byte[] spirv = ShaderCompiler.Compile(code, stage);

            SetSpirv(stage, spirv);

            return this;

        }

        ///// <summary>
        ///// Create the modules for the shader
        ///// </summary>
        //public ShaderWrapper CreateModules()
        //{
        //    foreach (var stageModule in _stageModules.Values)
        //    {
        //        SpirvParser.ParseBytes(stageModule.Spirv, this, _variableDefinitions);

        //        stageModule.Module = _renderer.CreateShaderModule(stageModule.Spirv);
        //    }

        //    return this;
        //}

        /// <summary>
        /// Try to get a module by the stage
        /// </summary>
        public bool TryGetStageModule(ShaderStageFlags stage, out ShaderStageModule stageModule)
        {
            return _stageModules.TryGetValue(stage, out stageModule);
        }

    }


    /// <summary>
    /// Shader stage module
    /// </summary>
    public class ShaderStageModule
    {
        public ShaderStageFlags Stage;
        public byte[] Spirv;
        public ShaderModule Module;
        public string Entrypoint;
    }

    /// <summary>
    /// Wrapper for SpecializationMapEntry
    /// </summary>
    public class SpecializationConstant
    {
        public ShaderStageFlags Stage;
        public string Name;
        public uint ConstantId;
        //public uint Offset;
        public uint Size;
        public uint DefaultValue;       //uint or float (4 bytes data)
    }
}
