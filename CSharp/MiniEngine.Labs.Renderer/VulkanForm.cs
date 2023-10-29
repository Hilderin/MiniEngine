using MiniEngine.Assets;
using MiniEngine.Rendering.Vulkan;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.Drivers.Vulkan.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;

namespace MiniEngine.Labs.Renderer
{
    public partial class VulkanForm : Form
    {
        const string MODEL_PATH = @"C:\Projects\MiniEngine\CSharp\MiniEngine.Tutorials\Assets\viking_room.obj";
        const string TEXTURE_PATH = @"C:\Projects\MiniEngine\CSharp\MiniEngine.Tutorials\Assets\viking_room.png";


        private VkRenderer Renderer;
        private Scene Scene = new Scene();

        public VulkanForm()
        {
            InitializeComponent();
        }

        private void VulkanForm_Load(object sender, EventArgs e)
        {
            Renderer = new VkRenderer("Test", "1.0.0")
                                .SetWindow32Handle(this.Handle)
                                .EnableDebug(DebugCallback);

            Renderer.Init();

            Shader shader = Renderer.CreateShader(new()
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

            void main() {
                gl_Position = PushConstants.render_matrix * vec4(inPosition, 1.0);
                //fragColor = vec3(PushConstants.render_matrix * vec4(inPosition, 1.0));
                fragColor = inColor;
            }
            ",
                FragmentCode = @"#version 450

            layout(location = 0) in vec3 fragColor;

            layout(location = 0) out vec4 outColor;

            void main() {
                outColor = vec4(fragColor, 1.0);
            }
            "
            });

            //AssetManager assetManager = new AssetManager();
            ////var texture = assetManager.GetTexture2DFromFile(TEXTURE_PATH);
            ////_texture = new VulkanTextureBinder(texture, this);
            ////_texture.Init();

            //var mesh = assetManager.GetMeshFromFile(MODEL_PATH, new MeshImportationParameters() { InverseFaces = true });
            //var mesh = new CubeMesh();
            //var mesh = new PlaneMesh();
            var mesh = new MeshObject()
            {
                Mesh = Renderer.CreateMesh(Primitives.CreateTriangleMeshDefinition())
            };
            mesh.Location = new Vector3(0f, 0f, 0f);

            mesh.Materials.Add(Renderer.CreateMaterial(new()
            {
                DiffuseTexture = Renderer.CreateTexture2D(new()
                {
                    Data = BaseTextures.WhitePixelData
                }),
                Shader = shader
            }));
            Scene.Add(mesh);

            Scene.Camera.Location.Z = -1f;


        }

        private void DebugCallback(DebugLevel level, int messageCode, string message)
        {
            if (level == DebugLevel.Error)
                throw new Exception($"Vulkan error: {message}");

            Debug.WriteLine($"{level}: {message}");

        }

        private void VulkanForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Renderer?.Dispose();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            Renderer.Render(Scene.Camera);
        }
    }
}