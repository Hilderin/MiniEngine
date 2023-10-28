using System;
using System.Diagnostics;
using MiniEngine.Assets;

namespace MiniEngine.Labs.Renderer
{
    internal class Test_TriangleColor_Vulkan
    {
       
        private MeshActor _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = Context.Current.Scene;
        private Camera Camera = Context.Current.Scene.Camera;

        public void Init()
        {

            Context.LockCursor();



            Shader shader = new Shader(@"#version 450

//push constants block
layout( push_constant ) uniform constants
{
	mat4 render_matrix;
} PushConstants;



layout(binding = 0) uniform UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

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
", @"#version 450


layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragTexCoord;

layout(location = 0) out vec4 outColor;


void main() {
    outColor = vec4(fragColor, 1.0);
}
");







            Scene.Camera.Location = new Vector3(0.0f, 0.0f, -1f);

            _currentMesh = Primitives.CreateTriangleMeshActor();
            _currentMesh.Location = new Vector3(0f, 0f, 0f);

            _currentMesh.Materials.Add(Context.CreateMaterial(new()
            {
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
            Camera.MoveInDirections(0.1f, Context.Input.GetMovementVector(Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E));

            if (Context.Input.IsKeyDown(Keys.NumpadAdd))
                Scene.AmbientLight.Intensity += 0.01f;
            if (Context.Input.IsKeyDown(Keys.NumpadSubtract))
                Scene.AmbientLight.Intensity -= 0.01f;
            if (Context.Input.IsKeyDown(Keys.Z))
                Camera.RotateYaw(-0.1f);
            if (Context.Input.IsKeyDown(Keys.X))
                Camera.RotateYaw(0.1f);
            if (Context.Input.IsKeyDown(Keys.C))
                Camera.RotatePitch(-0.1f);
            if (Context.Input.IsKeyDown(Keys.V))
                Camera.RotatePitch(0.1f);
            if (Context.Input.IsKeyDown(Keys.R))
                Camera.RotateRoll(-0.1f);
            if (Context.Input.IsKeyDown(Keys.F))
                Camera.RotateRoll(0.1f);

            if (Context.Input.IsKeyDown(Keys.PageUp))
            {
                if (Scene.DirectionalLight != null)
                    Scene.DirectionalLight.Intensity += 0.01f;
            }
            if (Context.Input.IsKeyDown(Keys.PageDown))
            {
                if (Scene.DirectionalLight != null)
                    Scene.DirectionalLight.Intensity -= 0.01f;
            }
            Scene.AmbientLight.Intensity = Math.Clamp(Scene.AmbientLight.Intensity, 0f, 1f);
            if (Scene.DirectionalLight != null)
                Scene.DirectionalLight.Intensity = Math.Clamp(Scene.DirectionalLight.Intensity, 0f, 1f);

            //if (Context.Input.IsJustMouseMoved)
            //{
            //    Vector2 mouseMovement = Context.Input.MouseMovement;
            //    Camera.RotatePitch(mouseMovement.Y * -0.1f);
            //    //Camera.RotateYaw(mouseMovement.X * 0.1f);
            //    Debug.Print(mouseMovement.ToString());
            //}


            //_currentMesh.RotateY(0.01f);

            System.Threading.Thread.Sleep(3);

        }

    }
}
