using MiniEngine.Assets;
using MiniEngine.Rendering.Vulkan;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.Drivers.Vulkan.Windows;
using MiniEngine.PrimitiveMeshes;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

            Shader shader = new Shader(@"#version 450

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
layout(location = 2) in vec3 inTexCoord;

layout(location = 0) out vec3 fragColor;

void main() {
    gl_Position = PushConstants.render_matrix * vec4(inPosition, 1.0);
    fragColor = vec3(PushConstants.render_matrix * vec4(inPosition, 1.0));
    fragColor = inColor;
}
", @"#version 450

layout(location = 0) in vec3 fragColor;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(fragColor, 1.0);
}
");


            Renderer = new VkRenderer("Test", new MiniEngine.Drivers.Vulkan.VkVersion(1, 0, 0), CreateSurface, DebugCallback);

            //AssetManager assetManager = new AssetManager();
            ////var texture = assetManager.GetTexture2DFromFile(TEXTURE_PATH);
            ////_texture = new VulkanTextureBinder(texture, this);
            ////_texture.Init();

            //var mesh = assetManager.GetMeshFromFile(MODEL_PATH, new MeshImportationParameters() { InverseFaces = true });
            //var mesh = new CubeMesh();
            //var mesh = new PlaneMesh();
            var mesh = new TriangleMesh();
            foreach (var mat in mesh.Materials)
                mat.Shader = shader;

            Scene.Add(mesh);

            Scene.Camera.Location.Z = -3f;


        }

        private SurfaceKhr CreateSurface(VkInstance vk)
        {
            return vk.CreateWin32SurfaceKHR(
                new Win32SurfaceCreateInfoKhr
                {
                    Hwnd = Handle,
                    Hinstance = Process.GetCurrentProcess().Handle
                });
        }


        private bool DebugCallback(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, int messageCode, string message)
        {
            if (flags == DebugReportFlagsExt.Error)
                throw new Exception($"Vulkan error: {message}");

            Debug.WriteLine($"{flags}: {message}");
            return true;
        }

        private void VulkanForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Renderer?.Dispose();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            Renderer.Render(Scene);
        }
    }
}