using MiniEngine.Drivers.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniEngine.Rendering.Vulkan
{
    /// <summary>
    /// Parser for spirv
    /// </summary>
    public static class SpirvParser
    {
        /// <summary>
        /// Default bindless array count. (65535)
        /// </summary>
        private const uint DEFAULT_BINDLESS_COUNT = ushort.MaxValue;

        /// <summary>
        /// Variable names that are always bindless
        /// </summary>
        private static readonly string[] BINDLESS_VARIABLE_NAMES = new string[] { ShaderVariableNames.SamplerDiffuse, ShaderVariableNames.DrawCallsBuffers };

        ///// <summary>
        ///// Parse dthe spirv codes
        ///// </summary>
        //public static void ParseUpdateShader(ShaderWrapper shader, Dictionary<string, SpirvVariableDefinition> variableDefinitions = null)
        //{
        //    ParseBytes(shader.VertexSpirv, shader, variableDefinitions);
        //    ParseBytes(shader.FragmentSpirv, shader, variableDefinitions);

        //}

        /// <summary>
        /// Parse the byte codes for a shader
        /// </summary>
        public unsafe static void ParseBytes(ShaderStageFlags stage, byte[] dataBytes, ShaderWrapper shader, Dictionary<string, SpirvVariableDefinition> variableDefinitions)
        {
            if (dataBytes.Length % 4 != 0)
                throw new ArgumentException("Invalid spirv, length % 4 != 0");

            int nbWord = dataBytes.Length / 4;
            uint[] data = new uint[nbWord];

            fixed (byte* ptrDataBytes = &dataBytes[0])
            fixed (uint* ptrData = &data[0])
            {
                System.Buffer.MemoryCopy(ptrDataBytes, ptrData, dataBytes.Length, dataBytes.Length);
            }

            uint magicNumber = data[0];
            if (magicNumber != 0x07230203)
                throw new ArgumentException("Invalid spirv, first byte should be 0x07230203");


            uint id_bound = data[3];
            Id[] ids = new Id[id_bound];
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = new Id();
            }

            //ShaderStageFlags stage = 0;
            uint id_index;
            Id id;
            SpvDecoration decoration;
            uint member_index;
            Member member;
            List<uint> entry_variables_index = new List<uint>();

            //------------------------
            //Loop on words...
            int word_index = 5;
            while (word_index < nbWord)
            {
                SpvOp op = (SpvOp)(data[word_index] & 0xFF);
                ushort word_count = (ushort)(data[word_index] >> 16);


                switch (op)
                {

                    case SpvOp.SpvOpEntryPoint:

                        string entrypoint_name = GetStringFromData(dataBytes, (word_index + 3) * 4);

                        SpvExecutionModel model = (SpvExecutionModel)data[word_index + 1];
                        ShaderStageFlags entrypointStage;
                        switch (model)
                        {
                            case SpvExecutionModel.SpvExecutionModelVertex:
                                entrypointStage = ShaderStageFlags.Vertex;
                                break;
                            case SpvExecutionModel.SpvExecutionModelGeometry:
                                entrypointStage = ShaderStageFlags.Geometry;
                                break;
                            case SpvExecutionModel.SpvExecutionModelFragment:
                                entrypointStage = ShaderStageFlags.Fragment;
                                break;
                            case SpvExecutionModel.SpvExecutionModelKernel:
                            case SpvExecutionModel.SpvExecutionModelGLCompute:
                                entrypointStage = ShaderStageFlags.Compute;
                                break;
                            //case SpvExecutionModel.SpvExecutionModelMeshNV:
                            //    stage = ShaderStageFlags.VK_SHADER_STAGE_MESH_BIT_NV;
                            //    break;
                            //case SpvExecutionModel.SpvExecutionModelTaskNV:
                            //    stage = ShaderStageFlags.VK_SHADER_STAGE_TASK_BIT_NV;
                            //    break;

                            //case SpvExecutionModel.SpvExecutionModelRayGenerationKHR:
                            //    stage = ShaderStageFlags.VK_SHADER_STAGE_RAYGEN_BIT_KHR;
                            //    break;

                            //case SpvExecutionModel.SpvExecutionModelClosestHitKHR:
                            //    stage = ShaderStageFlags.VK_SHADER_STAGE_CLOSEST_HIT_BIT_KHR;
                            //    break;

                            //case SpvExecutionModel.SpvExecutionModelAnyHitKHR:
                            //    stage = ShaderStageFlags.VK_SHADER_STAGE_ANY_HIT_BIT_KHR;
                            //    break;

                            //case SpvExecutionModel.SpvExecutionModelMissKHR:
                            //    stage = ShaderStageFlags.VK_SHADER_STAGE_MISS_BIT_KHR;
                            //    break;

                            default:
                                throw new InvalidOperationException($"SpvOpEntryPoint: Invalid SpvExecutionModel '{model}'");
                        }


                        if (stage != entrypointStage)
                            throw new InvalidOperationException($"Trying to set a shader for the wrong stage. Spirv stage: {entrypointStage}, current stage {stage}.");

                        shader[stage].Entrypoint = entrypoint_name;

                        //When take the entrypoints variables only for vertex shaders...
                        if (stage == ShaderStageFlags.Vertex)
                        {
                            //Round up on 4 digits (including the ending zero)
                            int entrypoint_name_len = ((entrypoint_name.Length + 1) / 4) + ((entrypoint_name.Length + 1) % 4);

                            for (int var_index = word_index + 3 + entrypoint_name_len; var_index < word_index + word_count; var_index++)
                            {
                                entry_variables_index.Add(data[var_index]);
                            }
                        }

                        break;



                    case SpvOp.SpvOpDecorate:

                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;

                        decoration = (SpvDecoration)data[word_index + 2];
                        switch (decoration)
                        {
                            case SpvDecoration.SpvDecorationLocation:
                                id.location = data[word_index + 3];
                                break;

                            case SpvDecoration.SpvDecorationBinding:
                                id.binding = data[word_index + 3];
                                break;

                            case SpvDecoration.SpvDecorationDescriptorSet:
                                id.set = data[word_index + 3];
                                break;

                            case SpvDecoration.SpvDecorationSpecId:
                                id.specid = data[word_index + 3];
                                break;

                            case SpvDecoration.SpvDecorationBuiltIn:
                                //Builtin variable
                                id.builtin = (SpvBuiltin)data[word_index + 3];
                                id.name = "gl_" + id.builtin.ToString();
                                break;
                            //default:
                            //    //
                            //    Debug.Info("Decorate not supported: " + decoration);
                            //    break;
                        }

                        break;

                    case SpvOp.SpvOpMemberDecorate:

                        id_index = data[word_index + 1];

                        id = ids[id_index];

                        member_index = data[word_index + 2];

                        id.members ??= new List<Member>();

                        while (id.members.Count <= member_index)
                            id.members.Add(new Member());

                        member = id.members[(int)member_index];

                        decoration = (SpvDecoration)data[word_index + 3];
                        switch (decoration)
                        {
                            case SpvDecoration.SpvDecorationOffset:
                                member.offset = data[word_index + 4];
                                break;
                        }

                        break;

                    case SpvOp.SpvOpName:

                        id_index = data[word_index + 1];

                        id = ids[id_index];

                        id.name = GetStringFromData(dataBytes, (word_index + 2) * 4);

                        break;

                    case SpvOp.SpvOpMemberName:

                        id_index = data[word_index + 1];
                        id = ids[id_index];

                        member_index = data[word_index + 2];

                        id.members ??= new List<Member>();
                        while (id.members.Count <= member_index)
                            id.members.Add(new Member());

                        member = id.members[(int)member_index];

                        member.name = GetStringFromData(dataBytes, (word_index + 3) * 4);

                        break;

                    case SpvOp.SpvOpTypeInt:

                        id_index = data[word_index + 1];

                        id = ids[id_index];

                        id.op = op;
                        id.width = (byte)(data[word_index + 2] / 8);
                        id.sign = (byte)data[word_index + 3];

                        break;

                    case SpvOp.SpvOpTypeFloat:
                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;
                        id.width = (byte)(data[word_index + 2] / 8);

                        break;

                    case SpvOp.SpvOpTypeVector:

                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;
                        id.type_index = data[word_index + 2];
                        id.count = data[word_index + 3];

                        break;

                    case SpvOp.SpvOpTypeMatrix:
                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;
                        id.type_index = data[word_index + 2];
                        id.count = data[word_index + 3];

                        break;


                    case SpvOp.SpvOpTypeSampler:
                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;

                        break;

                    case SpvOp.SpvOpTypeImage:
                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;

                        break;

                    case SpvOp.SpvOpTypeSampledImage:

                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;

                        break;


                    case SpvOp.SpvOpTypeArray:
                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;
                        id.type_index = data[word_index + 2];
                        id.count = data[word_index + 3];

                        break;

                    case SpvOp.SpvOpTypeRuntimeArray:

                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;
                        id.type_index = data[word_index + 2];

                        break;

                    case SpvOp.SpvOpTypeStruct:

                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;

                        if (word_count > 2)
                        {
                            uint members_count = (uint)word_count - 2;
                            id.count = members_count;
                            id.width = 0;

                            for (int mi = 0; mi < members_count; ++mi)
                            {
                                id.members[mi].id_index = data[word_index + mi + 2];

                                Id member_id = ids[id.members[mi].id_index];

                                member_id.width = GetMemberWidth(member_id, ids);
                                id.width += member_id.width;

                            }

                            // Round up to multiple of 4
                            int size_difference = 4 - (id.width % 4);
                            id.width += (byte)size_difference;

                            //// Round up to multiple of 16
                            //int size_difference = 16 - (id.width % 16);
                            //id.width += (byte)size_difference;
                        }

                        break;

                    case SpvOp.SpvOpTypePointer:

                        id_index = data[word_index + 1];

                        id = ids[id_index];
                        id.op = op;
                        id.type_index = data[word_index + 3];

                        break;

                    case SpvOp.SpvOpConstant:
                        //returntype;returnid;value
                        id_index = data[word_index + 2];

                        id = ids[id_index];

                        ////We received the SpvOpSpecConstant 2 times, one in the op SpvOpSpecConstant and one in SpvOpConstant
                        ////if we use the SpvOpConstant, we wil override important data from the SpvOpSpecConstant op.
                        //if (id.op != SpvOp.SpvOpSpecConstant && id.op != SpvOp.SpvOpSpecConstantTrue && id.op != SpvOp.SpvOpSpecConstantFalse && id.op != SpvOp.SpvOpSpecConstantComposite && id.op != SpvOp.SpvOpSpecConstantOp)
                        //{
                        id.op = op;
                        id.type_index = data[word_index + 1];
                        id.value = data[word_index + 3]; // NOTE(marco: we assume all constants to have maximum 32bit width
                        //}

                        break;

                    case SpvOp.SpvOpSpecConstant:
                    case SpvOp.SpvOpSpecConstantTrue:
                    case SpvOp.SpvOpSpecConstantFalse:
                    case SpvOp.SpvOpSpecConstantOp:
                        //Spec constant:
                        //Exemple: layout (constant_id = 0) const uint BUFFER_ELEMENTS = 32;
                        //returntype;returnid;value
                        id_index = data[word_index + 2];

                        id = ids[id_index];
                        id.op = op; 
                        id.type_index = data[word_index + 1];
                        id.value = data[word_index + 3]; // Default value

                        break;


                    case SpvOp.SpvOpSpecConstantComposite:
                        //Composite of multiple SpecConstant..

                        //returntype;returnid;constituantsid...
                        Id return_type = ids[data[word_index + 1]];
                        id = ids[data[word_index + 2]];

                        for (int i = 0; i < return_type.count; i++)
                        {
                            Id idconstituant = ids[data[word_index + 3 + i]];

                            if (!String.IsNullOrEmpty(id.name))
                            {
                                if (return_type.op == SpvOp.SpvOpTypeVector && i == 0)
                                    idconstituant.name = id.name + ".x";
                                else if (return_type.op == SpvOp.SpvOpTypeVector && i == 1)
                                    idconstituant.name = id.name + ".y";
                                else if (return_type.op == SpvOp.SpvOpTypeVector && i == 2)
                                    idconstituant.name = id.name + ".z";
                                else if (i == 0)
                                    idconstituant.name = id.name;
                            }

                        }

                        
                        break;

                    case SpvOp.SpvOpVariable:

                        id_index = data[word_index + 2];

                        id = ids[id_index];
                        id.op = op;
                        id.type_index = data[word_index + 1];
                        id.storage_class = (SpvStorageClass)data[word_index + 3];

                        break;

                    //case SpvOp.SpvOpCapability:
                    //case SpvOp.SpvOpExtInstImport:
                    //case SpvOp.SpvOpMemoryModel:
                    //case SpvOp.SpvOpExecutionMode:
                    //case SpvOp.SpvOpSource:
                    //case SpvOp.SpvOpSourceExtension:
                    //case SpvOp.SpvOpTypeVoid:
                    //case SpvOp.SpvOpTypeFunction:
                    //    //Rien à faire!
                    //    break;

                    //default:
                    //    Debug.Info("SpvOp not supported: " + op);
                    //    break;
                }

                word_index += word_count;
            }


            //=======================================================================
            List<Constant> constants = new List<Constant>();
            if (shader.Constants != null)
                constants.AddRange(shader.Constants);
            List<List<DescriptorSetLayoutBinding>> bindingSets = new List<List<DescriptorSetLayoutBinding>>();
            if (shader.BindingSets != null)
            {
                for (int iSet = 0; iSet < shader.BindingSets.Length; iSet++)
                    bindingSets.Add(new List<DescriptorSetLayoutBinding>(shader.BindingSets[iSet]));
            }
            List<VertexInputBindingDescription> vertexBindings = new List<VertexInputBindingDescription>();
            if (shader.VertexBindings != null)
                vertexBindings.AddRange(shader.VertexBindings);
            List<VertexInputAttributeDescription> vertexInputAttributes = new List<VertexInputAttributeDescription>();
            if (shader.VertexInputAttributes != null)
                vertexInputAttributes.AddRange(shader.VertexInputAttributes);
            List<SpecializationConstant> specConstants = new List<SpecializationConstant>();
            if (shader.SpecializationConstants != null)
                specConstants.AddRange(shader.SpecializationConstants);

            //--------------------------------------
            //Vertex buffer...
            UpdateVertexBindings(ids, entry_variables_index, vertexBindings, vertexInputAttributes, variableDefinitions);



            //----------------------------------------------
            //Uniforms and constants...
            for (id_index = 0; id_index < ids.Length; ++id_index)
            {
                id = ids[id_index];

                switch (id.op)
                {
                    // Parse specialization constants
                    case SpvOp.SpvOpSpecConstant:
                    case SpvOp.SpvOpSpecConstantTrue:
                    case SpvOp.SpvOpSpecConstantFalse:
                    case SpvOp.SpvOpSpecConstantOp:

                        Id id_spec_type = ids[id.type_index];

                        //uint specOffset = 0;
                        //if (specConstants.Count > 0)
                        //    specOffset = specConstants[specConstants.Count - 1].Offset + specConstants[specConstants.Count - 1].Size;

                        if (!String.IsNullOrEmpty(id.name))
                        {
                            SpecializationConstant specConstant = new SpecializationConstant()
                            {
                                Stage = stage,
                                Name = id.name,
                                ConstantId = id.specid,
                                //Offset = specOffset,
                                Size = id_spec_type.width,
                                DefaultValue = id.value
                            };
                            specConstants.Add(specConstant);
                        }

                        break;



                    case SpvOp.SpvOpVariable:
                        {
                            switch (id.storage_class)
                            {
                                case SpvStorageClass.SpvStorageClassUniform:
                                case SpvStorageClass.SpvStorageClassUniformConstant:

                                    Id uniform_type = ids[ids[id.type_index].type_index];

                                    while (bindingSets.Count <= id.set)
                                        bindingSets.Add(new List<DescriptorSetLayoutBinding>());

                                    var bindings = bindingSets[(int)id.set];


                                    var binding = bindings.FirstOrDefault(b => b.Binding == id.binding);
                                    if (binding == null)
                                    {
                                        binding = new DescriptorSetLayoutBinding();
                                        binding.Binding = id.binding;
                                        binding.Name = id.name;
                                        binding.Size = uniform_type.width;

                                        UpdateBindingDescriptorType(binding, uniform_type, ids);

                                        if (binding.IsArray)
                                        {
                                            if (variableDefinitions != null && variableDefinitions.TryGetValue(binding.Name, out var varDef))
                                            {
                                                binding.DescriptorCount = (uint)varDef.Count;
                                                binding.Bindless = varDef.Bindless;
                                            }
                                            else if (BINDLESS_VARIABLE_NAMES.Contains(binding.Name))
                                            {
                                                //Default bindless...
                                                binding.DescriptorCount = DEFAULT_BINDLESS_COUNT;
                                                binding.Bindless = true;
                                            }
                                        }

                                        bindings.Add(binding);
                                    }


                                    binding.StageFlags |= stage;



                                    break;


                                case SpvStorageClass.SpvStorageClassPushConstant:

                                    Id push_constants_type = ids[ids[id.type_index].type_index];

                                    for (int i = 0; i < push_constants_type.members.Count; i++)
                                    {
                                        Member mc = push_constants_type.members[i];
                                        if (mc == null)
                                            continue;

                                        Id member_id = ids[mc.id_index];

                                        Constant pushConstant = constants.FirstOrDefault(c => c.Offset == mc.offset);
                                        if (pushConstant == null)
                                        {
                                            pushConstant = new Constant();
                                            pushConstant.Name = mc.name;
                                            pushConstant.Size = member_id.width;
                                            pushConstant.Offset = mc.offset;


                                            constants.Add(pushConstant);
                                        }
                                        else if (mc.name != pushConstant.Name)
                                        {
                                            //Multiple constants with different name at the same offset...
                                            throw new InvalidOperationException($"Multiple constants with different name at the same offset {mc.offset}, constant '{pushConstant.Name}' and '{mc.name}'");
                                        }


                                        pushConstant.Stage |= stage;



                                    }
                                    break;
                            }
                        }
                        break;
                }


            }


            //-----------------------------
            //Update the shader with arrays....
            shader.Constants = constants.ToArray();
            shader.VertexBindings = vertexBindings.ToArray();
            shader.VertexInputAttributes = vertexInputAttributes.ToArray();
            shader.BindingSets = new DescriptorSetLayoutBinding[bindingSets.Count][];
            for (int iSet = 0; iSet < bindingSets.Count; iSet++)
            {
                shader.BindingSets[iSet] = bindingSets[iSet].ToArray();
            }
            shader.SpecializationConstants = specConstants.OrderBy(s => s.ConstantId).ToArray();


        }

        /// <summary>
        /// Update the bindings for a desccriptor type
        /// </summary>
        private static unsafe void UpdateBindingDescriptorType(DescriptorSetLayoutBinding binding, Id uniform_type, Id[] ids)
        {
            switch (uniform_type.op)
            {
                case SpvOp.SpvOpTypeStruct:
                    binding.Name = uniform_type.name;

                    if (uniform_type.members != null && uniform_type.members.Count > 0)
                    {
                        Id member = ids[uniform_type.members[0].id_index];
                        if (member.op == SpvOp.SpvOpTypeRuntimeArray)
                        {
                            //it's an array...
                            binding.DescriptorType = DescriptorType.StorageBuffer;
                            binding.DescriptorCount = 1;
                            break;
                        }
                    }
                    binding.DescriptorType = DescriptorType.UniformBuffer;
                    binding.DescriptorCount = 1;
                    break;

                case SpvOp.SpvOpTypeSampledImage:
                    binding.DescriptorType = DescriptorType.CombinedImageSampler;
                    binding.DescriptorCount = 1;
                    break;

                case SpvOp.SpvOpTypeImage:
                    binding.DescriptorType = DescriptorType.SampledImage;
                    binding.DescriptorCount = 1;
                    break;

                case SpvOp.SpvOpTypeSampler:
                    binding.DescriptorType = DescriptorType.Sampler;
                    binding.DescriptorCount = 1;
                    break;

                case SpvOp.SpvOpTypeRuntimeArray:
                case SpvOp.SpvOpTypeArray:

                    Id array_type = ids[uniform_type.type_index];

                    binding.IsArray = true;

                    UpdateBindingDescriptorType(binding, array_type, ids);

                    break;

                default:
                    throw new InvalidOperationException($"Binding '{binding.Name}' type not supported: {uniform_type.op}");
            }
        }

        /// <summary>
        /// Calculate the vertex bindings
        /// </summary>
        private static unsafe void UpdateVertexBindings(
            Id[] ids,
            List<uint> entry_variables_index,
            List<VertexInputBindingDescription> vertexBindings,
            List<VertexInputAttributeDescription> vertexInputAttributes,
            Dictionary<string, SpirvVariableDefinition> variableDefinitions)
        {


            List<BindingSet> vertex_bindings = new List<BindingSet>();
            List<EntrypointMember> entry_members = new List<EntrypointMember>();
            foreach (uint entry_variable_index in entry_variables_index)
            {
                Id id = ids[entry_variable_index];

                //We take only the SpvStorageClassInput... which means vertexbuffer...
                if (id.storage_class == SpvStorageClass.SpvStorageClassInput)
                {
                    //Internal variable to glsl...
                    if (id.name.StartsWith("gl_"))
                        continue;

                    Id typeid = ids[id.type_index];

                    // If the type is a pointer, resolve it
                    if (typeid.op == SpvOp.SpvOpTypePointer)
                    {
                        typeid = ids[typeid.type_index];
                    }
                    var new_entry = new EntrypointMember()
                    {
                        Set = id.set,
                        Binding = id.binding,
                        Location = id.location,
                        Format = GetMemberFormat(typeid),
                        Size = GetMemberWidth(typeid, ids)
                    };

                    if (variableDefinitions != null && variableDefinitions.TryGetValue(id.name, out var varDef) && varDef.Format != Format.Undefined)
                    {
                        new_entry.Format = varDef.Format;
                        new_entry.Size = (uint)(FormatHelper.GetFormatSizeBits(varDef.Format) / 8);
                    }


                    entry_members.Add(new_entry);
                }
            }

            //Important that the entries are in the right order to calculate the offsets...
            foreach (var entry_member in entry_members.OrderBy(e => e.Binding).ThenBy(e => e.Location))
            {

                while (vertex_bindings.Count <= entry_member.Binding)
                    vertex_bindings.Add(new BindingSet());

                BindingSet bindingSet = vertex_bindings[(int)entry_member.Set];

                VertexInputAttributeDescription vertexInputAttribute = new VertexInputAttributeDescription();
                vertexInputAttribute.Location = entry_member.Location;
                vertexInputAttribute.Binding = entry_member.Binding;
                vertexInputAttribute.Offset = bindingSet.Stride;
                vertexInputAttribute.Format = entry_member.Format;
                vertexInputAttributes.Add(vertexInputAttribute);


                bindingSet.Stride += entry_member.Size;
                bindingSet.Binding = entry_member.Binding;


            }


            for (int i = 0; i < vertex_bindings.Count; i++)
            {
                if (!vertexBindings.Any(b => b.Binding == vertex_bindings[i].Binding))
                {
                    vertexBindings.Add(new VertexInputBindingDescription()
                    {
                        Binding = vertex_bindings[i].Binding,
                        Stride = vertex_bindings[i].Stride
                    });
                }
            }

        }

        /// <summary>
        /// Calculate the width of a member
        /// </summary>
        private static byte GetMemberWidth(Id id, Id[] ids)
        {

            switch (id.op)
            {

                case SpvOp.SpvOpTypeInt:                
                case SpvOp.SpvOpTypeFloat:
                    return id.width;

                case SpvOp.SpvOpTypeArray:
                case SpvOp.SpvOpTypeVector:
                case SpvOp.SpvOpTypeMatrix:
                case SpvOp.SpvOpConstant:
                case SpvOp.SpvOpSpecConstant:
                    Id type_id = ids[id.type_index];
                    byte len = GetMemberWidth(type_id, ids);
                    return (byte)(len * id.count);


            }

            return 0;
        }


        /// <summary>
        /// Calculate the Format of a member
        /// </summary>
        private static Format GetMemberFormat(Id id)
        {

            switch (id.op)
            {

                case SpvOp.SpvOpTypeInt:
                    return Format.R32Sint;
                case SpvOp.SpvOpTypeFloat:
                    switch (id.width)
                    {
                        case 4: return Format.R32Sfloat;
                        default: throw new NotSupportedException($"Type not supported: {id.op} x {id.width}");
                    }

                case SpvOp.SpvOpTypeVector:
                    switch (id.count)
                    {
                        case 1: return Format.R32Sfloat;
                        case 2: return Format.R32G32Sfloat;
                        case 3: return Format.R32G32B32Sfloat;
                        case 4: return Format.R32G32B32A32Sfloat;
                        default: throw new NotSupportedException($"Type not supported: {id.op} x {id.count}");
                    }
                //type_id = ids[id.type_index];
                //switch (type_id.op)
                //{
                //    case SpvOp.SpvOpTypeInt:
                //        switch (id.count)
                //        {
                //            case 1: return Format.R32Sint;
                //            case 2: return Format.R32G32Sint;
                //            case 3: return Format.R32G32B32Sint;
                //            case 4: return Format.R32G32B32A32Sint;
                //            default: throw new NotSupportedException($"Type not supported: {id.op} x {id.count}");
                //        }
                //    case SpvOp.SpvOpTypeFloat:
                //        switch (id.count)
                //        {
                //            case 1: return Format.R32Sfloat;
                //            case 2: return Format.R32G32Sfloat;
                //            case 3: return Format.R32G32B32Sfloat;
                //            case 4: return Format.R32G32B32A32Sfloat;
                //            default: throw new NotSupportedException($"Type not supported: {id.op} x {id.count}");
                //        }
                //    default: throw new NotSupportedException($"Type not supported: {id.op} x {id.count}");
                //}
                default: throw new NotSupportedException($"Type not supported: {id.op}");


            }
        }

        /// <summary>
        /// Get a string form data in bytes..
        /// </summary>
        private static string GetStringFromData(byte[] data, int offset)
        {
            for (int i = offset; i < data.Length; i++)
            {
                if (data[i] == 0)
                    //End of the string...
                    return System.Text.Encoding.UTF8.GetString(data, offset, i - offset);
            }

            throw new InvalidOperationException($"End of string not found, staring at {offset}");
        }



        #region Internal classes

        private class EntrypointMember
        {
            public uint Set;
            public uint Binding;
            public uint Location;
            public uint Size;
            public Format Format;
        }

        private class BindingSet
        {
            public uint Binding;
            public uint Stride;
        }


        private class Member
        {
            public uint id_index;
            public uint offset;

            public string name;
        };

        private class Id
        {
            public SpvOp op;
            public uint set;
            public uint binding;
            public uint location;
            public uint specid;
            public SpvBuiltin builtin = SpvBuiltin.None;

            // For integers and floats
            public byte width;
            public byte sign;

            // For arrays, vectors and matrices
            public uint type_index;
            public uint count;

            // For variables
            public SpvStorageClass storage_class = SpvStorageClass.None;

            // For constants
            public uint value;

            // For structs
            public string name;
            public List<Member> members;
        };


        #endregion

        #region Enums & Consts

        //private const uint k_bindless_texture_binding = 10;

        private enum SpvOp : uint
        {
            SpvOpNop = 0,
            SpvOpUndef = 1,
            SpvOpSourceContinued = 2,
            SpvOpSource = 3,
            SpvOpSourceExtension = 4,
            SpvOpName = 5,
            SpvOpMemberName = 6,
            SpvOpString = 7,
            SpvOpLine = 8,
            SpvOpExtension = 10,
            SpvOpExtInstImport = 11,
            SpvOpExtInst = 12,
            SpvOpMemoryModel = 14,
            SpvOpEntryPoint = 15,
            SpvOpExecutionMode = 16,
            SpvOpCapability = 17,
            SpvOpTypeVoid = 19,
            SpvOpTypeBool = 20,
            SpvOpTypeInt = 21,
            SpvOpTypeFloat = 22,
            SpvOpTypeVector = 23,
            SpvOpTypeMatrix = 24,
            SpvOpTypeImage = 25,
            SpvOpTypeSampler = 26,
            SpvOpTypeSampledImage = 27,
            SpvOpTypeArray = 28,
            SpvOpTypeRuntimeArray = 29,
            SpvOpTypeStruct = 30,
            SpvOpTypeOpaque = 31,
            SpvOpTypePointer = 32,
            SpvOpTypeFunction = 33,
            SpvOpTypeEvent = 34,
            SpvOpTypeDeviceEvent = 35,
            SpvOpTypeReserveId = 36,
            SpvOpTypeQueue = 37,
            SpvOpTypePipe = 38,
            SpvOpTypeForwardPointer = 39,
            SpvOpConstantTrue = 41,
            SpvOpConstantFalse = 42,
            SpvOpConstant = 43,
            SpvOpConstantComposite = 44,
            SpvOpConstantSampler = 45,
            SpvOpConstantNull = 46,
            SpvOpSpecConstantTrue = 48,
            SpvOpSpecConstantFalse = 49,
            SpvOpSpecConstant = 50,
            SpvOpSpecConstantComposite = 51,
            SpvOpSpecConstantOp = 52,
            SpvOpFunction = 54,
            SpvOpFunctionParameter = 55,
            SpvOpFunctionEnd = 56,
            SpvOpFunctionCall = 57,
            SpvOpVariable = 59,
            SpvOpImageTexelPointer = 60,
            SpvOpLoad = 61,
            SpvOpStore = 62,
            SpvOpCopyMemory = 63,
            SpvOpCopyMemorySized = 64,
            SpvOpAccessChain = 65,
            SpvOpInBoundsAccessChain = 66,
            SpvOpPtrAccessChain = 67,
            SpvOpArrayLength = 68,
            SpvOpGenericPtrMemSemantics = 69,
            SpvOpInBoundsPtrAccessChain = 70,
            SpvOpDecorate = 71,
            SpvOpMemberDecorate = 72,
            SpvOpDecorationGroup = 73,
            SpvOpGroupDecorate = 74,
            SpvOpGroupMemberDecorate = 75,
            SpvOpVectorExtractDynamic = 77,
            SpvOpVectorInsertDynamic = 78,
            SpvOpVectorShuffle = 79,
            SpvOpCompositeConstruct = 80,
            SpvOpCompositeExtract = 81,
            SpvOpCompositeInsert = 82,
            SpvOpCopyObject = 83,
            SpvOpTranspose = 84,
            SpvOpSampledImage = 86,
            SpvOpImageSampleImplicitLod = 87,
            SpvOpImageSampleExplicitLod = 88,
            SpvOpImageSampleDrefImplicitLod = 89,
            SpvOpImageSampleDrefExplicitLod = 90,
            SpvOpImageSampleProjImplicitLod = 91,
            SpvOpImageSampleProjExplicitLod = 92,
            SpvOpImageSampleProjDrefImplicitLod = 93,
            SpvOpImageSampleProjDrefExplicitLod = 94,
            SpvOpImageFetch = 95,
            SpvOpImageGather = 96,
            SpvOpImageDrefGather = 97,
            SpvOpImageRead = 98,
            SpvOpImageWrite = 99,
            SpvOpImage = 100,
            SpvOpImageQueryFormat = 101,
            SpvOpImageQueryOrder = 102,
            SpvOpImageQuerySizeLod = 103,
            SpvOpImageQuerySize = 104,
            SpvOpImageQueryLod = 105,
            SpvOpImageQueryLevels = 106,
            SpvOpImageQuerySamples = 107,
            SpvOpConvertFToU = 109,
            SpvOpConvertFToS = 110,
            SpvOpConvertSToF = 111,
            SpvOpConvertUToF = 112,
            SpvOpUConvert = 113,
            SpvOpSConvert = 114,
            SpvOpFConvert = 115,
            SpvOpQuantizeToF16 = 116,
            SpvOpConvertPtrToU = 117,
            SpvOpSatConvertSToU = 118,
            SpvOpSatConvertUToS = 119,
            SpvOpConvertUToPtr = 120,
            SpvOpPtrCastToGeneric = 121,
            SpvOpGenericCastToPtr = 122,
            SpvOpGenericCastToPtrExplicit = 123,
            SpvOpBitcast = 124,
            SpvOpSNegate = 126,
            SpvOpFNegate = 127,
            SpvOpIAdd = 128,
            SpvOpFAdd = 129,
            SpvOpISub = 130,
            SpvOpFSub = 131,
            SpvOpIMul = 132,
            SpvOpFMul = 133,
            SpvOpUDiv = 134,
            SpvOpSDiv = 135,
            SpvOpFDiv = 136,
            SpvOpUMod = 137,
            SpvOpSRem = 138,
            SpvOpSMod = 139,
            SpvOpFRem = 140,
            SpvOpFMod = 141,
            SpvOpVectorTimesScalar = 142,
            SpvOpMatrixTimesScalar = 143,
            SpvOpVectorTimesMatrix = 144,
            SpvOpMatrixTimesVector = 145,
            SpvOpMatrixTimesMatrix = 146,
            SpvOpOuterProduct = 147,
            SpvOpDot = 148,
            SpvOpIAddCarry = 149,
            SpvOpISubBorrow = 150,
            SpvOpUMulExtended = 151,
            SpvOpSMulExtended = 152,
            SpvOpAny = 154,
            SpvOpAll = 155,
            SpvOpIsNan = 156,
            SpvOpIsInf = 157,
            SpvOpIsFinite = 158,
            SpvOpIsNormal = 159,
            SpvOpSignBitSet = 160,
            SpvOpLessOrGreater = 161,
            SpvOpOrdered = 162,
            SpvOpUnordered = 163,
            SpvOpLogicalEqual = 164,
            SpvOpLogicalNotEqual = 165,
            SpvOpLogicalOr = 166,
            SpvOpLogicalAnd = 167,
            SpvOpLogicalNot = 168,
            SpvOpSelect = 169,
            SpvOpIEqual = 170,
            SpvOpINotEqual = 171,
            SpvOpUGreaterThan = 172,
            SpvOpSGreaterThan = 173,
            SpvOpUGreaterThanEqual = 174,
            SpvOpSGreaterThanEqual = 175,
            SpvOpULessThan = 176,
            SpvOpSLessThan = 177,
            SpvOpULessThanEqual = 178,
            SpvOpSLessThanEqual = 179,
            SpvOpFOrdEqual = 180,
            SpvOpFUnordEqual = 181,
            SpvOpFOrdNotEqual = 182,
            SpvOpFUnordNotEqual = 183,
            SpvOpFOrdLessThan = 184,
            SpvOpFUnordLessThan = 185,
            SpvOpFOrdGreaterThan = 186,
            SpvOpFUnordGreaterThan = 187,
            SpvOpFOrdLessThanEqual = 188,
            SpvOpFUnordLessThanEqual = 189,
            SpvOpFOrdGreaterThanEqual = 190,
            SpvOpFUnordGreaterThanEqual = 191,
            SpvOpShiftRightLogical = 194,
            SpvOpShiftRightArithmetic = 195,
            SpvOpShiftLeftLogical = 196,
            SpvOpBitwiseOr = 197,
            SpvOpBitwiseXor = 198,
            SpvOpBitwiseAnd = 199,
            SpvOpNot = 200,
            SpvOpBitFieldInsert = 201,
            SpvOpBitFieldSExtract = 202,
            SpvOpBitFieldUExtract = 203,
            SpvOpBitReverse = 204,
            SpvOpBitCount = 205,
            SpvOpDPdx = 207,
            SpvOpDPdy = 208,
            SpvOpFwidth = 209,
            SpvOpDPdxFine = 210,
            SpvOpDPdyFine = 211,
            SpvOpFwidthFine = 212,
            SpvOpDPdxCoarse = 213,
            SpvOpDPdyCoarse = 214,
            SpvOpFwidthCoarse = 215,
            SpvOpEmitVertex = 218,
            SpvOpEndPrimitive = 219,
            SpvOpEmitStreamVertex = 220,
            SpvOpEndStreamPrimitive = 221,
            SpvOpControlBarrier = 224,
            SpvOpMemoryBarrier = 225,
            SpvOpAtomicLoad = 227,
            SpvOpAtomicStore = 228,
            SpvOpAtomicExchange = 229,
            SpvOpAtomicCompareExchange = 230,
            SpvOpAtomicCompareExchangeWeak = 231,
            SpvOpAtomicIIncrement = 232,
            SpvOpAtomicIDecrement = 233,
            SpvOpAtomicIAdd = 234,
            SpvOpAtomicISub = 235,
            SpvOpAtomicSMin = 236,
            SpvOpAtomicUMin = 237,
            SpvOpAtomicSMax = 238,
            SpvOpAtomicUMax = 239,
            SpvOpAtomicAnd = 240,
            SpvOpAtomicOr = 241,
            SpvOpAtomicXor = 242,
            SpvOpPhi = 245,
            SpvOpLoopMerge = 246,
            SpvOpSelectionMerge = 247,
            SpvOpLabel = 248,
            SpvOpBranch = 249,
            SpvOpBranchConditional = 250,
            SpvOpSwitch = 251,
            SpvOpKill = 252,
            SpvOpReturn = 253,
            SpvOpReturnValue = 254,
            SpvOpUnreachable = 255,
            SpvOpLifetimeStart = 256,
            SpvOpLifetimeStop = 257,
            SpvOpGroupAsyncCopy = 259,
            SpvOpGroupWaitEvents = 260,
            SpvOpGroupAll = 261,
            SpvOpGroupAny = 262,
            SpvOpGroupBroadcast = 263,
            SpvOpGroupIAdd = 264,
            SpvOpGroupFAdd = 265,
            SpvOpGroupFMin = 266,
            SpvOpGroupUMin = 267,
            SpvOpGroupSMin = 268,
            SpvOpGroupFMax = 269,
            SpvOpGroupUMax = 270,
            SpvOpGroupSMax = 271,
            SpvOpReadPipe = 274,
            SpvOpWritePipe = 275,
            SpvOpReservedReadPipe = 276,
            SpvOpReservedWritePipe = 277,
            SpvOpReserveReadPipePackets = 278,
            SpvOpReserveWritePipePackets = 279,
            SpvOpCommitReadPipe = 280,
            SpvOpCommitWritePipe = 281,
            SpvOpIsValidReserveId = 282,
            SpvOpGetNumPipePackets = 283,
            SpvOpGetMaxPipePackets = 284,
            SpvOpGroupReserveReadPipePackets = 285,
            SpvOpGroupReserveWritePipePackets = 286,
            SpvOpGroupCommitReadPipe = 287,
            SpvOpGroupCommitWritePipe = 288,
            SpvOpEnqueueMarker = 291,
            SpvOpEnqueueKernel = 292,
            SpvOpGetKernelNDrangeSubGroupCount = 293,
            SpvOpGetKernelNDrangeMaxSubGroupSize = 294,
            SpvOpGetKernelWorkGroupSize = 295,
            SpvOpGetKernelPreferredWorkGroupSizeMultiple = 296,
            SpvOpRetainEvent = 297,
            SpvOpReleaseEvent = 298,
            SpvOpCreateUserEvent = 299,
            SpvOpIsValidEvent = 300,
            SpvOpSetUserEventStatus = 301,
            SpvOpCaptureEventProfilingInfo = 302,
            SpvOpGetDefaultQueue = 303,
            SpvOpBuildNDRange = 304,
            SpvOpImageSparseSampleImplicitLod = 305,
            SpvOpImageSparseSampleExplicitLod = 306,
            SpvOpImageSparseSampleDrefImplicitLod = 307,
            SpvOpImageSparseSampleDrefExplicitLod = 308,
            SpvOpImageSparseSampleProjImplicitLod = 309,
            SpvOpImageSparseSampleProjExplicitLod = 310,
            SpvOpImageSparseSampleProjDrefImplicitLod = 311,
            SpvOpImageSparseSampleProjDrefExplicitLod = 312,
            SpvOpImageSparseFetch = 313,
            SpvOpImageSparseGather = 314,
            SpvOpImageSparseDrefGather = 315,
            SpvOpImageSparseTexelsResident = 316,
            SpvOpNoLine = 317,
            SpvOpAtomicFlagTestAndSet = 318,
            SpvOpAtomicFlagClear = 319,
            SpvOpImageSparseRead = 320,
            SpvOpSizeOf = 321,
            SpvOpTypePipeStorage = 322,
            SpvOpConstantPipeStorage = 323,
            SpvOpCreatePipeFromPipeStorage = 324,
            SpvOpGetKernelLocalSizeForSubgroupCount = 325,
            SpvOpGetKernelMaxNumSubgroups = 326,
            SpvOpTypeNamedBarrier = 327,
            SpvOpNamedBarrierInitialize = 328,
            SpvOpMemoryNamedBarrier = 329,
            SpvOpModuleProcessed = 330,
            SpvOpExecutionModeId = 331,
            SpvOpDecorateId = 332,
            SpvOpGroupNonUniformElect = 333,
            SpvOpGroupNonUniformAll = 334,
            SpvOpGroupNonUniformAny = 335,
            SpvOpGroupNonUniformAllEqual = 336,
            SpvOpGroupNonUniformBroadcast = 337,
            SpvOpGroupNonUniformBroadcastFirst = 338,
            SpvOpGroupNonUniformBallot = 339,
            SpvOpGroupNonUniformInverseBallot = 340,
            SpvOpGroupNonUniformBallotBitExtract = 341,
            SpvOpGroupNonUniformBallotBitCount = 342,
            SpvOpGroupNonUniformBallotFindLSB = 343,
            SpvOpGroupNonUniformBallotFindMSB = 344,
            SpvOpGroupNonUniformShuffle = 345,
            SpvOpGroupNonUniformShuffleXor = 346,
            SpvOpGroupNonUniformShuffleUp = 347,
            SpvOpGroupNonUniformShuffleDown = 348,
            SpvOpGroupNonUniformIAdd = 349,
            SpvOpGroupNonUniformFAdd = 350,
            SpvOpGroupNonUniformIMul = 351,
            SpvOpGroupNonUniformFMul = 352,
            SpvOpGroupNonUniformSMin = 353,
            SpvOpGroupNonUniformUMin = 354,
            SpvOpGroupNonUniformFMin = 355,
            SpvOpGroupNonUniformSMax = 356,
            SpvOpGroupNonUniformUMax = 357,
            SpvOpGroupNonUniformFMax = 358,
            SpvOpGroupNonUniformBitwiseAnd = 359,
            SpvOpGroupNonUniformBitwiseOr = 360,
            SpvOpGroupNonUniformBitwiseXor = 361,
            SpvOpGroupNonUniformLogicalAnd = 362,
            SpvOpGroupNonUniformLogicalOr = 363,
            SpvOpGroupNonUniformLogicalXor = 364,
            SpvOpGroupNonUniformQuadBroadcast = 365,
            SpvOpGroupNonUniformQuadSwap = 366,
            SpvOpCopyLogical = 400,
            SpvOpPtrEqual = 401,
            SpvOpPtrNotEqual = 402,
            SpvOpPtrDiff = 403,
            SpvOpColorAttachmentReadEXT = 4160,
            SpvOpDepthAttachmentReadEXT = 4161,
            SpvOpStencilAttachmentReadEXT = 4162,
            SpvOpTerminateInvocation = 4416,
            SpvOpSubgroupBallotKHR = 4421,
            SpvOpSubgroupFirstInvocationKHR = 4422,
            SpvOpSubgroupAllKHR = 4428,
            SpvOpSubgroupAnyKHR = 4429,
            SpvOpSubgroupAllEqualKHR = 4430,
            SpvOpGroupNonUniformRotateKHR = 4431,
            SpvOpSubgroupReadInvocationKHR = 4432,
            SpvOpTraceRayKHR = 4445,
            SpvOpExecuteCallableKHR = 4446,
            SpvOpConvertUToAccelerationStructureKHR = 4447,
            SpvOpIgnoreIntersectionKHR = 4448,
            SpvOpTerminateRayKHR = 4449,
            SpvOpSDot = 4450,
            SpvOpSDotKHR = 4450,
            SpvOpUDot = 4451,
            SpvOpUDotKHR = 4451,
            SpvOpSUDot = 4452,
            SpvOpSUDotKHR = 4452,
            SpvOpSDotAccSat = 4453,
            SpvOpSDotAccSatKHR = 4453,
            SpvOpUDotAccSat = 4454,
            SpvOpUDotAccSatKHR = 4454,
            SpvOpSUDotAccSat = 4455,
            SpvOpSUDotAccSatKHR = 4455,
            SpvOpTypeCooperativeMatrixKHR = 4456,
            SpvOpCooperativeMatrixLoadKHR = 4457,
            SpvOpCooperativeMatrixStoreKHR = 4458,
            SpvOpCooperativeMatrixMulAddKHR = 4459,
            SpvOpCooperativeMatrixLengthKHR = 4460,
            SpvOpTypeRayQueryKHR = 4472,
            SpvOpRayQueryInitializeKHR = 4473,
            SpvOpRayQueryTerminateKHR = 4474,
            SpvOpRayQueryGenerateIntersectionKHR = 4475,
            SpvOpRayQueryConfirmIntersectionKHR = 4476,
            SpvOpRayQueryProceedKHR = 4477,
            SpvOpRayQueryGetIntersectionTypeKHR = 4479,
            SpvOpImageSampleWeightedQCOM = 4480,
            SpvOpImageBoxFilterQCOM = 4481,
            SpvOpImageBlockMatchSSDQCOM = 4482,
            SpvOpImageBlockMatchSADQCOM = 4483,
            SpvOpGroupIAddNonUniformAMD = 5000,
            SpvOpGroupFAddNonUniformAMD = 5001,
            SpvOpGroupFMinNonUniformAMD = 5002,
            SpvOpGroupUMinNonUniformAMD = 5003,
            SpvOpGroupSMinNonUniformAMD = 5004,
            SpvOpGroupFMaxNonUniformAMD = 5005,
            SpvOpGroupUMaxNonUniformAMD = 5006,
            SpvOpGroupSMaxNonUniformAMD = 5007,
            SpvOpFragmentMaskFetchAMD = 5011,
            SpvOpFragmentFetchAMD = 5012,
            SpvOpReadClockKHR = 5056,
            SpvOpHitObjectRecordHitMotionNV = 5249,
            SpvOpHitObjectRecordHitWithIndexMotionNV = 5250,
            SpvOpHitObjectRecordMissMotionNV = 5251,
            SpvOpHitObjectGetWorldToObjectNV = 5252,
            SpvOpHitObjectGetObjectToWorldNV = 5253,
            SpvOpHitObjectGetObjectRayDirectionNV = 5254,
            SpvOpHitObjectGetObjectRayOriginNV = 5255,
            SpvOpHitObjectTraceRayMotionNV = 5256,
            SpvOpHitObjectGetShaderRecordBufferHandleNV = 5257,
            SpvOpHitObjectGetShaderBindingTableRecordIndexNV = 5258,
            SpvOpHitObjectRecordEmptyNV = 5259,
            SpvOpHitObjectTraceRayNV = 5260,
            SpvOpHitObjectRecordHitNV = 5261,
            SpvOpHitObjectRecordHitWithIndexNV = 5262,
            SpvOpHitObjectRecordMissNV = 5263,
            SpvOpHitObjectExecuteShaderNV = 5264,
            SpvOpHitObjectGetCurrentTimeNV = 5265,
            SpvOpHitObjectGetAttributesNV = 5266,
            SpvOpHitObjectGetHitKindNV = 5267,
            SpvOpHitObjectGetPrimitiveIndexNV = 5268,
            SpvOpHitObjectGetGeometryIndexNV = 5269,
            SpvOpHitObjectGetInstanceIdNV = 5270,
            SpvOpHitObjectGetInstanceCustomIndexNV = 5271,
            SpvOpHitObjectGetWorldRayDirectionNV = 5272,
            SpvOpHitObjectGetWorldRayOriginNV = 5273,
            SpvOpHitObjectGetRayTMaxNV = 5274,
            SpvOpHitObjectGetRayTMinNV = 5275,
            SpvOpHitObjectIsEmptyNV = 5276,
            SpvOpHitObjectIsHitNV = 5277,
            SpvOpHitObjectIsMissNV = 5278,
            SpvOpReorderThreadWithHitObjectNV = 5279,
            SpvOpReorderThreadWithHintNV = 5280,
            SpvOpTypeHitObjectNV = 5281,
            SpvOpImageSampleFootprintNV = 5283,
            SpvOpEmitMeshTasksEXT = 5294,
            SpvOpSetMeshOutputsEXT = 5295,
            SpvOpGroupNonUniformPartitionNV = 5296,
            SpvOpWritePackedPrimitiveIndices4x8NV = 5299,
            SpvOpReportIntersectionKHR = 5334,
            SpvOpReportIntersectionNV = 5334,
            SpvOpIgnoreIntersectionNV = 5335,
            SpvOpTerminateRayNV = 5336,
            SpvOpTraceNV = 5337,
            SpvOpTraceMotionNV = 5338,
            SpvOpTraceRayMotionNV = 5339,
            SpvOpRayQueryGetIntersectionTriangleVertexPositionsKHR = 5340,
            SpvOpTypeAccelerationStructureKHR = 5341,
            SpvOpTypeAccelerationStructureNV = 5341,
            SpvOpExecuteCallableNV = 5344,
            SpvOpTypeCooperativeMatrixNV = 5358,
            SpvOpCooperativeMatrixLoadNV = 5359,
            SpvOpCooperativeMatrixStoreNV = 5360,
            SpvOpCooperativeMatrixMulAddNV = 5361,
            SpvOpCooperativeMatrixLengthNV = 5362,
            SpvOpBeginInvocationInterlockEXT = 5364,
            SpvOpEndInvocationInterlockEXT = 5365,
            SpvOpDemoteToHelperInvocation = 5380,
            SpvOpDemoteToHelperInvocationEXT = 5380,
            SpvOpIsHelperInvocationEXT = 5381,
            SpvOpConvertUToImageNV = 5391,
            SpvOpConvertUToSamplerNV = 5392,
            SpvOpConvertImageToUNV = 5393,
            SpvOpConvertSamplerToUNV = 5394,
            SpvOpConvertUToSampledImageNV = 5395,
            SpvOpConvertSampledImageToUNV = 5396,
            SpvOpSamplerImageAddressingModeNV = 5397,
            SpvOpSubgroupShuffleINTEL = 5571,
            SpvOpSubgroupShuffleDownINTEL = 5572,
            SpvOpSubgroupShuffleUpINTEL = 5573,
            SpvOpSubgroupShuffleXorINTEL = 5574,
            SpvOpSubgroupBlockReadINTEL = 5575,
            SpvOpSubgroupBlockWriteINTEL = 5576,
            SpvOpSubgroupImageBlockReadINTEL = 5577,
            SpvOpSubgroupImageBlockWriteINTEL = 5578,
            SpvOpSubgroupImageMediaBlockReadINTEL = 5580,
            SpvOpSubgroupImageMediaBlockWriteINTEL = 5581,
            SpvOpUCountLeadingZerosINTEL = 5585,
            SpvOpUCountTrailingZerosINTEL = 5586,
            SpvOpAbsISubINTEL = 5587,
            SpvOpAbsUSubINTEL = 5588,
            SpvOpIAddSatINTEL = 5589,
            SpvOpUAddSatINTEL = 5590,
            SpvOpIAverageINTEL = 5591,
            SpvOpUAverageINTEL = 5592,
            SpvOpIAverageRoundedINTEL = 5593,
            SpvOpUAverageRoundedINTEL = 5594,
            SpvOpISubSatINTEL = 5595,
            SpvOpUSubSatINTEL = 5596,
            SpvOpIMul32x16INTEL = 5597,
            SpvOpUMul32x16INTEL = 5598,
            SpvOpConstantFunctionPointerINTEL = 5600,
            SpvOpFunctionPointerCallINTEL = 5601,
            SpvOpAsmTargetINTEL = 5609,
            SpvOpAsmINTEL = 5610,
            SpvOpAsmCallINTEL = 5611,
            SpvOpAtomicFMinEXT = 5614,
            SpvOpAtomicFMaxEXT = 5615,
            SpvOpAssumeTrueKHR = 5630,
            SpvOpExpectKHR = 5631,
            SpvOpDecorateString = 5632,
            SpvOpDecorateStringGOOGLE = 5632,
            SpvOpMemberDecorateString = 5633,
            SpvOpMemberDecorateStringGOOGLE = 5633,
            SpvOpVmeImageINTEL = 5699,
            SpvOpTypeVmeImageINTEL = 5700,
            SpvOpTypeAvcImePayloadINTEL = 5701,
            SpvOpTypeAvcRefPayloadINTEL = 5702,
            SpvOpTypeAvcSicPayloadINTEL = 5703,
            SpvOpTypeAvcMcePayloadINTEL = 5704,
            SpvOpTypeAvcMceResultINTEL = 5705,
            SpvOpTypeAvcImeResultINTEL = 5706,
            SpvOpTypeAvcImeResultSingleReferenceStreamoutINTEL = 5707,
            SpvOpTypeAvcImeResultDualReferenceStreamoutINTEL = 5708,
            SpvOpTypeAvcImeSingleReferenceStreaminINTEL = 5709,
            SpvOpTypeAvcImeDualReferenceStreaminINTEL = 5710,
            SpvOpTypeAvcRefResultINTEL = 5711,
            SpvOpTypeAvcSicResultINTEL = 5712,
            SpvOpSubgroupAvcMceGetDefaultInterBaseMultiReferencePenaltyINTEL = 5713,
            SpvOpSubgroupAvcMceSetInterBaseMultiReferencePenaltyINTEL = 5714,
            SpvOpSubgroupAvcMceGetDefaultInterShapePenaltyINTEL = 5715,
            SpvOpSubgroupAvcMceSetInterShapePenaltyINTEL = 5716,
            SpvOpSubgroupAvcMceGetDefaultInterDirectionPenaltyINTEL = 5717,
            SpvOpSubgroupAvcMceSetInterDirectionPenaltyINTEL = 5718,
            SpvOpSubgroupAvcMceGetDefaultIntraLumaShapePenaltyINTEL = 5719,
            SpvOpSubgroupAvcMceGetDefaultInterMotionVectorCostTableINTEL = 5720,
            SpvOpSubgroupAvcMceGetDefaultHighPenaltyCostTableINTEL = 5721,
            SpvOpSubgroupAvcMceGetDefaultMediumPenaltyCostTableINTEL = 5722,
            SpvOpSubgroupAvcMceGetDefaultLowPenaltyCostTableINTEL = 5723,
            SpvOpSubgroupAvcMceSetMotionVectorCostFunctionINTEL = 5724,
            SpvOpSubgroupAvcMceGetDefaultIntraLumaModePenaltyINTEL = 5725,
            SpvOpSubgroupAvcMceGetDefaultNonDcLumaIntraPenaltyINTEL = 5726,
            SpvOpSubgroupAvcMceGetDefaultIntraChromaModeBasePenaltyINTEL = 5727,
            SpvOpSubgroupAvcMceSetAcOnlyHaarINTEL = 5728,
            SpvOpSubgroupAvcMceSetSourceInterlacedFieldPolarityINTEL = 5729,
            SpvOpSubgroupAvcMceSetSingleReferenceInterlacedFieldPolarityINTEL = 5730,
            SpvOpSubgroupAvcMceSetDualReferenceInterlacedFieldPolaritiesINTEL = 5731,
            SpvOpSubgroupAvcMceConvertToImePayloadINTEL = 5732,
            SpvOpSubgroupAvcMceConvertToImeResultINTEL = 5733,
            SpvOpSubgroupAvcMceConvertToRefPayloadINTEL = 5734,
            SpvOpSubgroupAvcMceConvertToRefResultINTEL = 5735,
            SpvOpSubgroupAvcMceConvertToSicPayloadINTEL = 5736,
            SpvOpSubgroupAvcMceConvertToSicResultINTEL = 5737,
            SpvOpSubgroupAvcMceGetMotionVectorsINTEL = 5738,
            SpvOpSubgroupAvcMceGetInterDistortionsINTEL = 5739,
            SpvOpSubgroupAvcMceGetBestInterDistortionsINTEL = 5740,
            SpvOpSubgroupAvcMceGetInterMajorShapeINTEL = 5741,
            SpvOpSubgroupAvcMceGetInterMinorShapeINTEL = 5742,
            SpvOpSubgroupAvcMceGetInterDirectionsINTEL = 5743,
            SpvOpSubgroupAvcMceGetInterMotionVectorCountINTEL = 5744,
            SpvOpSubgroupAvcMceGetInterReferenceIdsINTEL = 5745,
            SpvOpSubgroupAvcMceGetInterReferenceInterlacedFieldPolaritiesINTEL = 5746,
            SpvOpSubgroupAvcImeInitializeINTEL = 5747,
            SpvOpSubgroupAvcImeSetSingleReferenceINTEL = 5748,
            SpvOpSubgroupAvcImeSetDualReferenceINTEL = 5749,
            SpvOpSubgroupAvcImeRefWindowSizeINTEL = 5750,
            SpvOpSubgroupAvcImeAdjustRefOffsetINTEL = 5751,
            SpvOpSubgroupAvcImeConvertToMcePayloadINTEL = 5752,
            SpvOpSubgroupAvcImeSetMaxMotionVectorCountINTEL = 5753,
            SpvOpSubgroupAvcImeSetUnidirectionalMixDisableINTEL = 5754,
            SpvOpSubgroupAvcImeSetEarlySearchTerminationThresholdINTEL = 5755,
            SpvOpSubgroupAvcImeSetWeightedSadINTEL = 5756,
            SpvOpSubgroupAvcImeEvaluateWithSingleReferenceINTEL = 5757,
            SpvOpSubgroupAvcImeEvaluateWithDualReferenceINTEL = 5758,
            SpvOpSubgroupAvcImeEvaluateWithSingleReferenceStreaminINTEL = 5759,
            SpvOpSubgroupAvcImeEvaluateWithDualReferenceStreaminINTEL = 5760,
            SpvOpSubgroupAvcImeEvaluateWithSingleReferenceStreamoutINTEL = 5761,
            SpvOpSubgroupAvcImeEvaluateWithDualReferenceStreamoutINTEL = 5762,
            SpvOpSubgroupAvcImeEvaluateWithSingleReferenceStreaminoutINTEL = 5763,
            SpvOpSubgroupAvcImeEvaluateWithDualReferenceStreaminoutINTEL = 5764,
            SpvOpSubgroupAvcImeConvertToMceResultINTEL = 5765,
            SpvOpSubgroupAvcImeGetSingleReferenceStreaminINTEL = 5766,
            SpvOpSubgroupAvcImeGetDualReferenceStreaminINTEL = 5767,
            SpvOpSubgroupAvcImeStripSingleReferenceStreamoutINTEL = 5768,
            SpvOpSubgroupAvcImeStripDualReferenceStreamoutINTEL = 5769,
            SpvOpSubgroupAvcImeGetStreamoutSingleReferenceMajorShapeMotionVectorsINTEL = 5770,
            SpvOpSubgroupAvcImeGetStreamoutSingleReferenceMajorShapeDistortionsINTEL = 5771,
            SpvOpSubgroupAvcImeGetStreamoutSingleReferenceMajorShapeReferenceIdsINTEL = 5772,
            SpvOpSubgroupAvcImeGetStreamoutDualReferenceMajorShapeMotionVectorsINTEL = 5773,
            SpvOpSubgroupAvcImeGetStreamoutDualReferenceMajorShapeDistortionsINTEL = 5774,
            SpvOpSubgroupAvcImeGetStreamoutDualReferenceMajorShapeReferenceIdsINTEL = 5775,
            SpvOpSubgroupAvcImeGetBorderReachedINTEL = 5776,
            SpvOpSubgroupAvcImeGetTruncatedSearchIndicationINTEL = 5777,
            SpvOpSubgroupAvcImeGetUnidirectionalEarlySearchTerminationINTEL = 5778,
            SpvOpSubgroupAvcImeGetWeightingPatternMinimumMotionVectorINTEL = 5779,
            SpvOpSubgroupAvcImeGetWeightingPatternMinimumDistortionINTEL = 5780,
            SpvOpSubgroupAvcFmeInitializeINTEL = 5781,
            SpvOpSubgroupAvcBmeInitializeINTEL = 5782,
            SpvOpSubgroupAvcRefConvertToMcePayloadINTEL = 5783,
            SpvOpSubgroupAvcRefSetBidirectionalMixDisableINTEL = 5784,
            SpvOpSubgroupAvcRefSetBilinearFilterEnableINTEL = 5785,
            SpvOpSubgroupAvcRefEvaluateWithSingleReferenceINTEL = 5786,
            SpvOpSubgroupAvcRefEvaluateWithDualReferenceINTEL = 5787,
            SpvOpSubgroupAvcRefEvaluateWithMultiReferenceINTEL = 5788,
            SpvOpSubgroupAvcRefEvaluateWithMultiReferenceInterlacedINTEL = 5789,
            SpvOpSubgroupAvcRefConvertToMceResultINTEL = 5790,
            SpvOpSubgroupAvcSicInitializeINTEL = 5791,
            SpvOpSubgroupAvcSicConfigureSkcINTEL = 5792,
            SpvOpSubgroupAvcSicConfigureIpeLumaINTEL = 5793,
            SpvOpSubgroupAvcSicConfigureIpeLumaChromaINTEL = 5794,
            SpvOpSubgroupAvcSicGetMotionVectorMaskINTEL = 5795,
            SpvOpSubgroupAvcSicConvertToMcePayloadINTEL = 5796,
            SpvOpSubgroupAvcSicSetIntraLumaShapePenaltyINTEL = 5797,
            SpvOpSubgroupAvcSicSetIntraLumaModeCostFunctionINTEL = 5798,
            SpvOpSubgroupAvcSicSetIntraChromaModeCostFunctionINTEL = 5799,
            SpvOpSubgroupAvcSicSetBilinearFilterEnableINTEL = 5800,
            SpvOpSubgroupAvcSicSetSkcForwardTransformEnableINTEL = 5801,
            SpvOpSubgroupAvcSicSetBlockBasedRawSkipSadINTEL = 5802,
            SpvOpSubgroupAvcSicEvaluateIpeINTEL = 5803,
            SpvOpSubgroupAvcSicEvaluateWithSingleReferenceINTEL = 5804,
            SpvOpSubgroupAvcSicEvaluateWithDualReferenceINTEL = 5805,
            SpvOpSubgroupAvcSicEvaluateWithMultiReferenceINTEL = 5806,
            SpvOpSubgroupAvcSicEvaluateWithMultiReferenceInterlacedINTEL = 5807,
            SpvOpSubgroupAvcSicConvertToMceResultINTEL = 5808,
            SpvOpSubgroupAvcSicGetIpeLumaShapeINTEL = 5809,
            SpvOpSubgroupAvcSicGetBestIpeLumaDistortionINTEL = 5810,
            SpvOpSubgroupAvcSicGetBestIpeChromaDistortionINTEL = 5811,
            SpvOpSubgroupAvcSicGetPackedIpeLumaModesINTEL = 5812,
            SpvOpSubgroupAvcSicGetIpeChromaModeINTEL = 5813,
            SpvOpSubgroupAvcSicGetPackedSkcLumaCountThresholdINTEL = 5814,
            SpvOpSubgroupAvcSicGetPackedSkcLumaSumThresholdINTEL = 5815,
            SpvOpSubgroupAvcSicGetInterRawSadsINTEL = 5816,
            SpvOpVariableLengthArrayINTEL = 5818,
            SpvOpSaveMemoryINTEL = 5819,
            SpvOpRestoreMemoryINTEL = 5820,
            SpvOpArbitraryFloatSinCosPiINTEL = 5840,
            SpvOpArbitraryFloatCastINTEL = 5841,
            SpvOpArbitraryFloatCastFromIntINTEL = 5842,
            SpvOpArbitraryFloatCastToIntINTEL = 5843,
            SpvOpArbitraryFloatAddINTEL = 5846,
            SpvOpArbitraryFloatSubINTEL = 5847,
            SpvOpArbitraryFloatMulINTEL = 5848,
            SpvOpArbitraryFloatDivINTEL = 5849,
            SpvOpArbitraryFloatGTINTEL = 5850,
            SpvOpArbitraryFloatGEINTEL = 5851,
            SpvOpArbitraryFloatLTINTEL = 5852,
            SpvOpArbitraryFloatLEINTEL = 5853,
            SpvOpArbitraryFloatEQINTEL = 5854,
            SpvOpArbitraryFloatRecipINTEL = 5855,
            SpvOpArbitraryFloatRSqrtINTEL = 5856,
            SpvOpArbitraryFloatCbrtINTEL = 5857,
            SpvOpArbitraryFloatHypotINTEL = 5858,
            SpvOpArbitraryFloatSqrtINTEL = 5859,
            SpvOpArbitraryFloatLogINTEL = 5860,
            SpvOpArbitraryFloatLog2INTEL = 5861,
            SpvOpArbitraryFloatLog10INTEL = 5862,
            SpvOpArbitraryFloatLog1pINTEL = 5863,
            SpvOpArbitraryFloatExpINTEL = 5864,
            SpvOpArbitraryFloatExp2INTEL = 5865,
            SpvOpArbitraryFloatExp10INTEL = 5866,
            SpvOpArbitraryFloatExpm1INTEL = 5867,
            SpvOpArbitraryFloatSinINTEL = 5868,
            SpvOpArbitraryFloatCosINTEL = 5869,
            SpvOpArbitraryFloatSinCosINTEL = 5870,
            SpvOpArbitraryFloatSinPiINTEL = 5871,
            SpvOpArbitraryFloatCosPiINTEL = 5872,
            SpvOpArbitraryFloatASinINTEL = 5873,
            SpvOpArbitraryFloatASinPiINTEL = 5874,
            SpvOpArbitraryFloatACosINTEL = 5875,
            SpvOpArbitraryFloatACosPiINTEL = 5876,
            SpvOpArbitraryFloatATanINTEL = 5877,
            SpvOpArbitraryFloatATanPiINTEL = 5878,
            SpvOpArbitraryFloatATan2INTEL = 5879,
            SpvOpArbitraryFloatPowINTEL = 5880,
            SpvOpArbitraryFloatPowRINTEL = 5881,
            SpvOpArbitraryFloatPowNINTEL = 5882,
            SpvOpLoopControlINTEL = 5887,
            SpvOpAliasDomainDeclINTEL = 5911,
            SpvOpAliasScopeDeclINTEL = 5912,
            SpvOpAliasScopeListDeclINTEL = 5913,
            SpvOpFixedSqrtINTEL = 5923,
            SpvOpFixedRecipINTEL = 5924,
            SpvOpFixedRsqrtINTEL = 5925,
            SpvOpFixedSinINTEL = 5926,
            SpvOpFixedCosINTEL = 5927,
            SpvOpFixedSinCosINTEL = 5928,
            SpvOpFixedSinPiINTEL = 5929,
            SpvOpFixedCosPiINTEL = 5930,
            SpvOpFixedSinCosPiINTEL = 5931,
            SpvOpFixedLogINTEL = 5932,
            SpvOpFixedExpINTEL = 5933,
            SpvOpPtrCastToCrossWorkgroupINTEL = 5934,
            SpvOpCrossWorkgroupCastToPtrINTEL = 5938,
            SpvOpReadPipeBlockingINTEL = 5946,
            SpvOpWritePipeBlockingINTEL = 5947,
            SpvOpFPGARegINTEL = 5949,
            SpvOpRayQueryGetRayTMinKHR = 6016,
            SpvOpRayQueryGetRayFlagsKHR = 6017,
            SpvOpRayQueryGetIntersectionTKHR = 6018,
            SpvOpRayQueryGetIntersectionInstanceCustomIndexKHR = 6019,
            SpvOpRayQueryGetIntersectionInstanceIdKHR = 6020,
            SpvOpRayQueryGetIntersectionInstanceShaderBindingTableRecordOffsetKHR = 6021,
            SpvOpRayQueryGetIntersectionGeometryIndexKHR = 6022,
            SpvOpRayQueryGetIntersectionPrimitiveIndexKHR = 6023,
            SpvOpRayQueryGetIntersectionBarycentricsKHR = 6024,
            SpvOpRayQueryGetIntersectionFrontFaceKHR = 6025,
            SpvOpRayQueryGetIntersectionCandidateAABBOpaqueKHR = 6026,
            SpvOpRayQueryGetIntersectionObjectRayDirectionKHR = 6027,
            SpvOpRayQueryGetIntersectionObjectRayOriginKHR = 6028,
            SpvOpRayQueryGetWorldRayDirectionKHR = 6029,
            SpvOpRayQueryGetWorldRayOriginKHR = 6030,
            SpvOpRayQueryGetIntersectionObjectToWorldKHR = 6031,
            SpvOpRayQueryGetIntersectionWorldToObjectKHR = 6032,
            SpvOpAtomicFAddEXT = 6035,
            SpvOpTypeBufferSurfaceINTEL = 6086,
            SpvOpTypeStructContinuedINTEL = 6090,
            SpvOpConstantCompositeContinuedINTEL = 6091,
            SpvOpSpecConstantCompositeContinuedINTEL = 6092,
            SpvOpConvertFToBF16INTEL = 6116,
            SpvOpConvertBF16ToFINTEL = 6117,
            SpvOpControlBarrierArriveINTEL = 6142,
            SpvOpControlBarrierWaitINTEL = 6143,
            SpvOpGroupIMulKHR = 6401,
            SpvOpGroupFMulKHR = 6402,
            SpvOpGroupBitwiseAndKHR = 6403,
            SpvOpGroupBitwiseOrKHR = 6404,
            SpvOpGroupBitwiseXorKHR = 6405,
            SpvOpGroupLogicalAndKHR = 6406,
            SpvOpGroupLogicalOrKHR = 6407,
            SpvOpGroupLogicalXorKHR = 6408,
            SpvOpMax = 0x7fffffff,
        }



        private enum SpvExecutionModel : uint
        {
            SpvExecutionModelVertex = 0,
            SpvExecutionModelTessellationControl = 1,
            SpvExecutionModelTessellationEvaluation = 2,
            SpvExecutionModelGeometry = 3,
            SpvExecutionModelFragment = 4,
            SpvExecutionModelGLCompute = 5,
            SpvExecutionModelKernel = 6,
            SpvExecutionModelTaskNV = 5267,
            SpvExecutionModelMeshNV = 5268,
            SpvExecutionModelRayGenerationKHR = 5313,
            SpvExecutionModelRayGenerationNV = 5313,
            SpvExecutionModelIntersectionKHR = 5314,
            SpvExecutionModelIntersectionNV = 5314,
            SpvExecutionModelAnyHitKHR = 5315,
            SpvExecutionModelAnyHitNV = 5315,
            SpvExecutionModelClosestHitKHR = 5316,
            SpvExecutionModelClosestHitNV = 5316,
            SpvExecutionModelMissKHR = 5317,
            SpvExecutionModelMissNV = 5317,
            SpvExecutionModelCallableKHR = 5318,
            SpvExecutionModelCallableNV = 5318,
            SpvExecutionModelTaskEXT = 5364,
            SpvExecutionModelMeshEXT = 5365,
            SpvExecutionModelMax = 0x7fffffff,
        }


        private enum SpvStorageClass : uint
        {
            SpvStorageClassUniformConstant = 0,
            SpvStorageClassInput = 1,
            SpvStorageClassUniform = 2,
            SpvStorageClassOutput = 3,
            SpvStorageClassWorkgroup = 4,
            SpvStorageClassCrossWorkgroup = 5,
            SpvStorageClassPrivate = 6,
            SpvStorageClassFunction = 7,
            SpvStorageClassGeneric = 8,
            SpvStorageClassPushConstant = 9,
            SpvStorageClassAtomicCounter = 10,
            SpvStorageClassImage = 11,
            SpvStorageClassStorageBuffer = 12,
            SpvStorageClassTileImageEXT = 4172,
            SpvStorageClassCallableDataKHR = 5328,
            SpvStorageClassCallableDataNV = 5328,
            SpvStorageClassIncomingCallableDataKHR = 5329,
            SpvStorageClassIncomingCallableDataNV = 5329,
            SpvStorageClassRayPayloadKHR = 5338,
            SpvStorageClassRayPayloadNV = 5338,
            SpvStorageClassHitAttributeKHR = 5339,
            SpvStorageClassHitAttributeNV = 5339,
            SpvStorageClassIncomingRayPayloadKHR = 5342,
            SpvStorageClassIncomingRayPayloadNV = 5342,
            SpvStorageClassShaderRecordBufferKHR = 5343,
            SpvStorageClassShaderRecordBufferNV = 5343,
            SpvStorageClassPhysicalStorageBuffer = 5349,
            SpvStorageClassPhysicalStorageBufferEXT = 5349,
            SpvStorageClassHitObjectAttributeNV = 5385,
            SpvStorageClassTaskPayloadWorkgroupEXT = 5402,
            SpvStorageClassCodeSectionINTEL = 5605,
            SpvStorageClassDeviceOnlyINTEL = 5936,
            SpvStorageClassHostOnlyINTEL = 5937,
            //SpvStorageClassMax = 0x7fffffff,
            None = uint.MaxValue
        }


        private enum SpvDecoration : uint
        {
            SpvDecorationRelaxedPrecision = 0,
            SpvDecorationSpecId = 1,
            SpvDecorationBlock = 2,
            SpvDecorationBufferBlock = 3,
            SpvDecorationRowMajor = 4,
            SpvDecorationColMajor = 5,
            SpvDecorationArrayStride = 6,
            SpvDecorationMatrixStride = 7,
            SpvDecorationGLSLShared = 8,
            SpvDecorationGLSLPacked = 9,
            SpvDecorationCPacked = 10,
            SpvDecorationBuiltIn = 11,
            SpvDecorationNoPerspective = 13,
            SpvDecorationFlat = 14,
            SpvDecorationPatch = 15,
            SpvDecorationCentroid = 16,
            SpvDecorationSample = 17,
            SpvDecorationInvariant = 18,
            SpvDecorationRestrict = 19,
            SpvDecorationAliased = 20,
            SpvDecorationVolatile = 21,
            SpvDecorationConstant = 22,
            SpvDecorationCoherent = 23,
            SpvDecorationNonWritable = 24,
            SpvDecorationNonReadable = 25,
            SpvDecorationUniform = 26,
            SpvDecorationUniformId = 27,
            SpvDecorationSaturatedConversion = 28,
            SpvDecorationStream = 29,
            SpvDecorationLocation = 30,
            SpvDecorationComponent = 31,
            SpvDecorationIndex = 32,
            SpvDecorationBinding = 33,
            SpvDecorationDescriptorSet = 34,
            SpvDecorationOffset = 35,
            SpvDecorationXfbBuffer = 36,
            SpvDecorationXfbStride = 37,
            SpvDecorationFuncParamAttr = 38,
            SpvDecorationFPRoundingMode = 39,
            SpvDecorationFPFastMathMode = 40,
            SpvDecorationLinkageAttributes = 41,
            SpvDecorationNoContraction = 42,
            SpvDecorationInputAttachmentIndex = 43,
            SpvDecorationAlignment = 44,
            SpvDecorationMaxByteOffset = 45,
            SpvDecorationAlignmentId = 46,
            SpvDecorationMaxByteOffsetId = 47,
            SpvDecorationNoSignedWrap = 4469,
            SpvDecorationNoUnsignedWrap = 4470,
            SpvDecorationWeightTextureQCOM = 4487,
            SpvDecorationBlockMatchTextureQCOM = 4488,
            SpvDecorationExplicitInterpAMD = 4999,
            SpvDecorationOverrideCoverageNV = 5248,
            SpvDecorationPassthroughNV = 5250,
            SpvDecorationViewportRelativeNV = 5252,
            SpvDecorationSecondaryViewportRelativeNV = 5256,
            SpvDecorationPerPrimitiveEXT = 5271,
            SpvDecorationPerPrimitiveNV = 5271,
            SpvDecorationPerViewNV = 5272,
            SpvDecorationPerTaskNV = 5273,
            SpvDecorationPerVertexKHR = 5285,
            SpvDecorationPerVertexNV = 5285,
            SpvDecorationNonUniform = 5300,
            SpvDecorationNonUniformEXT = 5300,
            SpvDecorationRestrictPointer = 5355,
            SpvDecorationRestrictPointerEXT = 5355,
            SpvDecorationAliasedPointer = 5356,
            SpvDecorationAliasedPointerEXT = 5356,
            SpvDecorationHitObjectShaderRecordBufferNV = 5386,
            SpvDecorationBindlessSamplerNV = 5398,
            SpvDecorationBindlessImageNV = 5399,
            SpvDecorationBoundSamplerNV = 5400,
            SpvDecorationBoundImageNV = 5401,
            SpvDecorationSIMTCallINTEL = 5599,
            SpvDecorationReferencedIndirectlyINTEL = 5602,
            SpvDecorationClobberINTEL = 5607,
            SpvDecorationSideEffectsINTEL = 5608,
            SpvDecorationVectorComputeVariableINTEL = 5624,
            SpvDecorationFuncParamIOKindINTEL = 5625,
            SpvDecorationVectorComputeFunctionINTEL = 5626,
            SpvDecorationStackCallINTEL = 5627,
            SpvDecorationGlobalVariableOffsetINTEL = 5628,
            SpvDecorationCounterBuffer = 5634,
            SpvDecorationHlslCounterBufferGOOGLE = 5634,
            SpvDecorationHlslSemanticGOOGLE = 5635,
            SpvDecorationUserSemantic = 5635,
            SpvDecorationUserTypeGOOGLE = 5636,
            SpvDecorationFunctionRoundingModeINTEL = 5822,
            SpvDecorationFunctionDenormModeINTEL = 5823,
            SpvDecorationRegisterINTEL = 5825,
            SpvDecorationMemoryINTEL = 5826,
            SpvDecorationNumbanksINTEL = 5827,
            SpvDecorationBankwidthINTEL = 5828,
            SpvDecorationMaxPrivateCopiesINTEL = 5829,
            SpvDecorationSinglepumpINTEL = 5830,
            SpvDecorationDoublepumpINTEL = 5831,
            SpvDecorationMaxReplicatesINTEL = 5832,
            SpvDecorationSimpleDualPortINTEL = 5833,
            SpvDecorationMergeINTEL = 5834,
            SpvDecorationBankBitsINTEL = 5835,
            SpvDecorationForcePow2DepthINTEL = 5836,
            SpvDecorationBurstCoalesceINTEL = 5899,
            SpvDecorationCacheSizeINTEL = 5900,
            SpvDecorationDontStaticallyCoalesceINTEL = 5901,
            SpvDecorationPrefetchINTEL = 5902,
            SpvDecorationStallEnableINTEL = 5905,
            SpvDecorationFuseLoopsInFunctionINTEL = 5907,
            SpvDecorationMathOpDSPModeINTEL = 5909,
            SpvDecorationAliasScopeINTEL = 5914,
            SpvDecorationNoAliasINTEL = 5915,
            SpvDecorationInitiationIntervalINTEL = 5917,
            SpvDecorationMaxConcurrencyINTEL = 5918,
            SpvDecorationPipelineEnableINTEL = 5919,
            SpvDecorationBufferLocationINTEL = 5921,
            SpvDecorationIOPipeStorageINTEL = 5944,
            SpvDecorationFunctionFloatingPointModeINTEL = 6080,
            SpvDecorationSingleElementVectorINTEL = 6085,
            SpvDecorationVectorComputeCallableFunctionINTEL = 6087,
            SpvDecorationMediaBlockIOINTEL = 6140,
            SpvDecorationFPMaxErrorDecorationINTEL = 6170,
            SpvDecorationLatencyControlLabelINTEL = 6172,
            SpvDecorationLatencyControlConstraintINTEL = 6173,
            SpvDecorationConduitKernelArgumentINTEL = 6175,
            SpvDecorationRegisterMapKernelArgumentINTEL = 6176,
            SpvDecorationMMHostInterfaceAddressWidthINTEL = 6177,
            SpvDecorationMMHostInterfaceDataWidthINTEL = 6178,
            SpvDecorationMMHostInterfaceLatencyINTEL = 6179,
            SpvDecorationMMHostInterfaceReadWriteModeINTEL = 6180,
            SpvDecorationMMHostInterfaceMaxBurstINTEL = 6181,
            SpvDecorationMMHostInterfaceWaitRequestINTEL = 6182,
            SpvDecorationStableKernelArgumentINTEL = 6183,
            SpvDecorationMax = 0x7fffffff,
        }

        private enum SpvBuiltin : uint
        {
            Position = 0,
            PointSize = 1,
            ClipDistance = 3,
            CullDistance = 4,
            VertexId = 5,
            InstanceId = 6,
            PrimitiveId = 7,
            InvocationId = 8,
            Layer = 9,
            ViewportIndex = 10,
            TessLevelOuter = 11,
            TessLevelInner = 12,
            TessCoord = 13,
            PatchVertices = 14,
            FragCoord = 15,
            PointCoord = 16,
            FrontFacing = 17,
            SampleId = 18,
            SamplePosition = 19,
            SampleMask = 20,
            FragDepth = 22,
            HelperInvocation = 23,
            NumWorkgroups = 24,
            WorkgroupSize = 25,
            WorkgroupId = 26,
            LocalInvocationId = 27,
            GlobalInvocationId = 28,
            LocalInvocationIndex = 29,
            WorkDim = 30,
            GlobalSize = 31,
            EnqueuedWorkgroupSize = 32,
            GlobalOffset = 33,
            GlobalLinearId = 34,
            SubgroupSize = 36,
            SubgroupMaxSize = 37,
            NumSubgroups = 38,
            NumEnqueuedSubgroups = 39,
            SubgroupId = 40,
            SubgroupLocalInvocationId = 41,
            VertexIndex = 42,
            InstanceIndex = 43,
            SubgroupEqMask = 4416,
            SubgroupEqMaskKHR = 4416,
            SubgroupGeMask = 4417,
            SubgroupGeMaskKHR = 4417,
            SubgroupGtMask = 4418,
            SubgroupGtMaskKHR = 4418,
            SubgroupLeMask = 4419,
            SubgroupLeMaskKHR = 4419,
            SubgroupLtMask = 4420,
            SubgroupLtMaskKHR = 4420,
            BaseVertex = 4424,
            BaseInstance = 4425,
            DrawIndex = 4426,
            PrimitiveShadingRateKHR = 4432,
            DeviceIndex = 4438,
            ViewIndex = 4440,
            ShadingRateKHR = 4444,
            BaryCoordNoPerspAMD = 4992,
            BaryCoordNoPerspCentroidAMD = 4993,
            BaryCoordNoPerspSampleAMD = 4994,
            BaryCoordSmoothAMD = 4995,
            BaryCoordSmoothCentroidAMD = 4996,
            BaryCoordSmoothSampleAMD = 4997,
            BaryCoordPullModelAMD = 4998,
            FragStencilRefEXT = 5014,
            ViewportMaskNV = 5253,
            SecondaryPositionNV = 5257,
            SecondaryViewportMaskNV = 5258,
            PositionPerViewNV = 5261,
            ViewportMaskPerViewNV = 5262,
            FullyCoveredEXT = 5264,
            TaskCountNV = 5274,
            PrimitiveCountNV = 5275,
            PrimitiveIndicesNV = 5276,
            ClipDistancePerViewNV = 5277,
            CullDistancePerViewNV = 5278,
            LayerPerViewNV = 5279,
            MeshViewCountNV = 5280,
            MeshViewIndicesNV = 5281,
            BaryCoordKHR = 5286,
            BaryCoordNV = 5286,
            BaryCoordNoPerspKHR = 5287,
            BaryCoordNoPerspNV = 5287,
            FragSizeEXT = 5292,
            FragmentSizeNV = 5292,
            FragInvocationCountEXT = 5293,
            InvocationsPerPixelNV = 5293,
            LaunchIdNV = 5319,
            LaunchIdKHR = 5319,
            LaunchSizeNV = 5320,
            LaunchSizeKHR = 5320,
            WorldRayOriginNV = 5321,
            WorldRayOriginKHR = 5321,
            WorldRayDirectionNV = 5322,
            WorldRayDirectionKHR = 5322,
            ObjectRayOriginNV = 5323,
            ObjectRayOriginKHR = 5323,
            ObjectRayDirectionNV = 5324,
            ObjectRayDirectionKHR = 5324,
            RayTminNV = 5325,
            RayTminKHR = 5325,
            RayTmaxNV = 5326,
            RayTmaxKHR = 5326,
            InstanceCustomIndexNV = 5327,
            InstanceCustomIndexKHR = 5327,
            ObjectToWorldNV = 5330,
            ObjectToWorldKHR = 5330,
            WorldToObjectNV = 5331,
            WorldToObjectKHR = 5331,
            HitTNV = 5332,
            HitKindNV = 5333,
            HitKindKHR = 5333,
            CurrentRayTimeNV = 5334,
            IncomingRayFlagsNV = 5351,
            IncomingRayFlagsKHR = 5351,
            RayGeometryIndexKHR = 5352,
            WarpsPerSMNV = 5374,
            SMCountNV = 5375,
            WarpIDNV = 5376,
            SMIDNV = 5377,
            CullMaskKHR = 6021,
            None = uint.MaxValue
        }

            #endregion

        }

        /// <summary>
        /// Information on a variable to help parse the spirv
        /// </summary>
        public class SpirvVariableDefinition
    {
        public Format Format = Format.Undefined;
        public int Count = 1;
        public bool Bindless = false;
    }
}
