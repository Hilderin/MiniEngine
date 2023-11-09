using ImGuiNET;
using MiniEngine.Rendering.Vulkan;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_IndirectDrawing
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {
            //Context.SetMaxFramerate(-1);
            Context.Renderer.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -3f);

            var shader = Context.Renderer.CreateShader(new()
            {
                VertexCode = @"#version 450

//push constants block
layout( push_constant ) uniform constants
{
	mat4 _matrix_vp;
};

struct object_data
{
    vec3 location;
    vec3 rotation;
    vec3 scale;
    uint textureIndex; 
};

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inColor;
layout(location = 2) in vec2 inTexCoord;

//layout(binding = 2) uniform object_data _objects[];
layout(std430, binding = 2) readonly buffer object_data_array {
    object_data objects[];
};


layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec2 fragTexCoord;
layout(location = 2) flat out uint textureIndex;

float offsets[] = {
    -0.75f,  0.75f,  1.0f, 0.0f, 0.0f,
     0.05f, -0.05f,  0.0f, 1.0f, 0.0f,
    -0.05f, -0.05f,  0.0f, 0.0f, 1.0f,

    -0.05f,  0.05f,  1.0f, 0.0f, 0.0f,
     0.05f, -0.05f,  0.0f, 1.0f, 0.0f,   
     0.05f,  0.05f,  0.0f, 1.0f, 1.0f		    		
};  

void main() {
    gl_Position = _matrix_vp * vec4(inPosition, 1.0);
    gl_Position.x += objects[gl_InstanceIndex].location.x;
    
    fragColor = inColor;
    fragTexCoord = inTexCoord;
    textureIndex = objects[gl_InstanceIndex].textureIndex;
}",
                FragmentCode = @"#version 450
#extension GL_EXT_nonuniform_qualifier : enable

layout(binding = 1) uniform sampler2D _sampler_diffuse[];

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragTexCoord;
layout(location = 2) in flat uint textureIndex;

layout(location = 0) out vec4 outColor;




void main() {
    //outColor = vec4(fragColor, 1.0);
    outColor = texture(_sampler_diffuse[textureIndex], fragTexCoord);
}"
,
                VariableDefinitions = new()
                                        {
                                            { "_sampler_diffuse", new() { Count = 10, Bindless = true } }
                                        }
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

            Scene.Add(PrimitiveObjects.CreateCubeMeshObject()
                                           .MoveTo(new Vector3(4f, 5f, 6f))
                                           .SetMaterial(matAqua, 0)
                     );

            //Scene.Add(PrimitiveObjects.CreateCubeMeshObject()
            //                               .MoveTo(new Vector3(2f, 0f, 0f))
            //                               .SetMaterial(matAqua, 0)
            //         );

        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);


            //_currentMesh.RotateY(0.01f);

            System.Threading.Thread.Sleep(3);

            //var windowSize = Context.Window.ClientSize;

            //ImGui.Begin("FPSCount", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoInputs);
            //ImGui.SetWindowPos(new System.Numerics.Vector2(windowSize.X - 100, 10));
            //ImGui.Text(Time.FramePerSeconds.ToString());
            //ImGui.End();

        }

    }
}
