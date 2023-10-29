using System;
using System.Diagnostics;
using MiniEngine.Assets;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_Cube_Vulkan
    {
       
        private MeshObject _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = Context.Current.Scene;
        private Camera Camera = Context.Current.Scene.Camera;

        public void Init()
        {

            Context.LockCursor();



            Shader shader = Context.Renderer.CreateShader(new()
            {
                VertexCode = @"#version 450

layout(binding = 0) uniform UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

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
}
",
                FragmentCode = @"#version 450

layout(binding = 1) uniform sampler2D texSampler;

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragTexCoord;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(fragColor, 1.0);
}
"
            });







            Scene.Camera.Location = new Vector3(0.0f, 0.0f, -1f);

            _currentMesh = Primitives.CreateCubeMeshObject();
            _currentMesh.Location = new Vector3(0f, 0f, 0f);

            _currentMesh.Materials.Add(Context.Renderer.CreateMaterial(new()
            {
                DiffuseTexture = BaseTextures.White,
                Shader = shader
            }));


            Scene.Add(_currentMesh);


            //Mesh mesh2 = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\antique_ceramic_vase_01_4k.blend\antique_ceramic_vase_01_4k.obj", new MeshImportationParameters()
            //{
            //    Scale = 3f,
            //    ResetMaterialAmbientColor = true
            //});
            //mesh2.Location = new Vector3(2f, 2f, 4f);
            //Context.Add(mesh2);


            //Context.AmbientLight.Intensity = 0.1f;

            //Context.DirectionalLight = new DirectionalLight()
            //{
            //    Rotation = Rotator3.FromDegrees(45, 90, 0)
            //};

            //Context.Add(new PointLight()
            //{
            //    Location = new Vector3(-8.0f, 0f, 0f),
            //    AttenuationLinear = 0.2f
            //});


            //var terrainMesh = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\box_terrain.obj", new MeshImportationParameters()
            //{
            //    Scale = 1f,
            //    InverseFaces = false,
            //    SmoothNormals = false
            //});
            //terrainMesh.Location = new Vector3(0f, -1f, 0.0f);
            //Context.Add(terrainMesh);
        }


        public void Update()
        {
            LabHelper.ProcessInputsTest(Context);


            //_currentMesh.RotateY(0.01f);

            System.Threading.Thread.Sleep(3);

        }

    }
}
