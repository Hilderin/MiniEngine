using MiniEngine.Drivers.Vulkan;
using MiniEngine.ResourceDefinitions;
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
        /// Already loaded?
        /// </summary>
        private bool _loaded = false;

        /// <summary>
        /// Action on reload of the shader
        /// </summary>
        public event Action OnReload;

        /// <summary>
        /// Constants
        /// </summary>
        public Constant[] Constants;

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
        /// Create a shader
        /// </summary>
        public void Load(ShaderDefinition shaderDef)
        {
            if (_loaded)
                //Reloading on next frame...
                _renderer.AddActionsBeforeNextFrame(() => LoadInternal(shaderDef));
            else
                LoadInternal(shaderDef);

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


        /// <summary>
        /// Create a shader
        /// </summary>
        private void LoadInternal(ShaderDefinition shaderDef)
        {
            //Reset...
            Constants = null;
            BindingSets = null;
            VertexBindings = null;
            Constants = null;
            VertexInputAttributes = null;
            SpecializationConstants = null;


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

            SetVariableDefinitions(variableDefinitions);

            _stageModules.Clear();
            foreach (var kv in shaderDef.StageCodes)
            {
                SetCode((ShaderStageFlags)kv.Key, kv.Value);
            }

            _loaded = true;

            OnReload?.Invoke();

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
    /// Wrapper for Constant
    /// </summary>
    public class Constant
    {
        public ShaderStageFlags Stage;
        public string Name;
        public uint Offset;
        public uint Size;
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
