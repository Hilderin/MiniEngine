using MiniEngine.Rendering.Vulkan;
using MiniEngine.ResourceDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_Bindless
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = new Scene();

        public void Init()
        {
            //Context.SetMaxFramerate(60);
            Context.Renderer.Camera.Transform.Location = new Vector3(0.0f, 0.0f, -1f);

            var shader = Context.Renderer.CreateShader(new()
            {
                VertexCode = @"#version 450

//push constants block
layout( push_constant ) uniform constants
{
	mat4 render_matrix;
} PushConstants;


layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inColor;
layout(location = 2) in vec2 inTexCoord;

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec2 fragTexCoord;

void main() {
    gl_Position = PushConstants.render_matrix * vec4(inPosition, 1.0);
    
    fragColor = inColor;
    fragTexCoord = inTexCoord;
}",
                FragmentCode = @"#version 450
#extension GL_EXT_nonuniform_qualifier : enable

layout(push_constant) uniform _PushConstant {
    layout(offset = 64) int textureRID;
};

layout(binding = 1) uniform sampler2D texSampler[];

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragTexCoord;

layout(location = 0) out vec4 outColor;


void main() {
    //outColor = vec4(fragColor, 1.0);
    outColor = texture(texSampler[textureRID], fragTexCoord);
}"
,
                VariableDefinitions = new()
                                        {
                                            { "texSampler", new() { Count = 10, Bindless = true } }
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


            Scene.Add(PrimitiveObjects.CreateCubeMeshObject()
                                           .MoveTo(new Vector3(-2f, 0f, 0f))
                                           .SetMaterial(matWhite, 0)
                     );

            Scene.Add(PrimitiveObjects.CreateCubeMeshObject()
                                           .MoveTo(new Vector3(2f, 0f, 0f))
                                           .SetMaterial(matAqua, 0)
                     );

        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);


            //_currentMesh.RotateY(0.01f);

            System.Threading.Thread.Sleep(3);

        }

    }
}
