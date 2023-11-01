using MiniEngine.Rendering.Vulkan;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.Drivers.Vulkan.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;
using System.Collections.Generic;

namespace MiniEngine.Labs.Renderer
{
    public partial class VulkanForm : Form
    {
        //const string MODEL_PATH = @"C:\Projects\MiniEngine\CSharp\MiniEngine.Tutorials\Assets\viking_room.obj";
        //const string TEXTURE_PATH = @"C:\Projects\MiniEngine\CSharp\MiniEngine.Tutorials\Assets\viking_room.png";


        private VkRenderer renderer;

        public VulkanForm()
        {
            InitializeComponent();
        }

        private void VulkanForm_Load(object sender, EventArgs e)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            renderer = new VkRenderer("Test", "1.0.0")
                                .SetWindow32Handle(this.Handle)
                                .EnableDebug(DebugCallback);
#pragma warning restore CA2000 // Dispose objects before losing scope

            renderer.Init();

            //AssetManager assetManager = new AssetManager();
            ////var texture = assetManager.GetTexture2DFromFile(TEXTURE_PATH);
            ////_texture = new VulkanTextureBinder(texture, this);
            ////_texture.Init();

            //var mesh = assetManager.GetMeshFromFile(MODEL_PATH, new MeshImportationParameters() { InverseFaces = true });
            //var mesh = new CubeMesh();
            //var mesh = new PlaneMesh();
            var mesh = renderer.CreateMesh(Primitives.CreateTriangleMeshDefinition());
            var transform = new WorldTransform()
            {
                Location = new Vector3(0f, 0f, 3f)
            };

            var materials = new List<Material>()
            { 
                renderer.CreateMaterial(new()
                {
                    DiffuseTexture = BaseTextures.White,
                    Shader = BaseShaders.Unlit
                })
            };

            renderer.AddMesh(mesh, materials, transform);

        }

        private void DebugCallback(DebugLevel level, int messageCode, string message)
        {
            if (level == DebugLevel.Error)
                throw new Exception($"Vulkan error: {message}");

            Debug.WriteLine($"{level}: {message}");

        }

        private void VulkanForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            renderer?.Dispose();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            renderer.Render();
        }
    }
}