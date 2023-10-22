using MiniEngine.Assets;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.Drivers.Vulkan.Windows;
using MiniEngine.PrimitiveMeshes;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MiniEngine.Tutorials.Drivers.Vulkan
{
    public partial class VulkanForm : Form
    {
        const string MODEL_PATH = @"C:\Projects\MiniEngine\CSharp\MiniEngine.Tutorials\Assets\viking_room.obj";
        const string TEXTURE_PATH = @"C:\Projects\MiniEngine\CSharp\MiniEngine.Tutorials\Assets\viking_room.png";


        private VkRenderer Renderer;
        private VkMeshRenderer MeshRenderer;

        public VulkanForm()
        {
            InitializeComponent();
        }

        private void VulkanForm_Load(object sender, EventArgs e)
        {

            Shader shader = new Shader(@"#version 450

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inColor;
layout(location = 2) in vec3 inTexCoord;

layout(location = 0) out vec3 fragColor;

void main() {
    gl_Position = vec4(inPosition, 1.0);
    fragColor = inColor;
}
", @"#version 450

layout(location = 0) in vec3 fragColor;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = vec4(1.0, 1.0, 1.0, 1.0);
}
");


            Renderer = new VkRenderer("Test", CreateSurface, DebugCallback);

            //AssetManager assetManager = new AssetManager();
            ////var texture = assetManager.GetTexture2DFromFile(TEXTURE_PATH);
            ////_texture = new VulkanTextureBinder(texture, this);
            ////_texture.Init();

            //var mesh = assetManager.GetMeshFromFile(MODEL_PATH, new MeshImportationParameters() { InverseFaces = true });
            var mesh = new CubeMesh();
            foreach (var mat in mesh.Materials)
                mat.Shader = shader;

            MeshRenderer = new VkMeshRenderer(mesh, Renderer);

            Renderer.MeshRenderers.Add(MeshRenderer);


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
            MeshRenderer?.Dispose();
            Renderer?.Dispose();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            Renderer.DrawFrame();
        }
    }
}