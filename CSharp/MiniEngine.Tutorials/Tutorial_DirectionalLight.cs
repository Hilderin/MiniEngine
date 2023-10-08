using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.AssertManager;
using MiniEngine.OpenGL;
using MiniEngine.PrimitiveMeshes;

namespace MiniEngine.Tutorials
{
    internal unsafe class Tutorial_DirectionalLight : IDisposable
    {
       
        private Mesh _currentMesh;

        private Context Context = Context.Current;
        private Renderer Renderer = Context.Current.Renderer;
        private Camera Camera = Context.Current.Renderer.Camera;

        public void Init()
        {

            Context.LockCursor();

            Renderer.Camera.Location = new Vector3(1.0f, 0.0f, -3.0f);

            _currentMesh = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\antique_ceramic_vase_01_4k.blend\antique_ceramic_vase_01_4k.obj", new MeshImportationParameters()
            {
                Scale = 6f,
                ResetMaterialAmbientColor = true
            });
            _currentMesh.Location = new Vector3(0f, 0f, 0.0f);
            Renderer.Add(_currentMesh);


            Mesh mesh2 = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\antique_ceramic_vase_01_4k.blend\antique_ceramic_vase_01_4k.obj", new MeshImportationParameters()
            {
                Scale = 3f,
                ResetMaterialAmbientColor = true
            });
            mesh2.Location = new Vector3(2f, 2f, 4f);
            Renderer.Add(mesh2);


            Renderer.AmbientLight.Intensity = 0.1f;

            Renderer.DirectionalLight = new DirectionalLight()
            {
                Rotation = Rotator3.FromDegrees(45, -90, 0)
            };

            //Renderer.Add(new PointLight()
            //{
            //    Location = new Vector3(-8.0f, 0f, 0f),
            //    AttenuationLinear = 0.2f
            //});


            var terrainMesh = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\box_terrain.obj", new MeshImportationParameters()
            {
                Scale = 1f,
                InverseFaces = false,
                SmoothNormals = false
            });
            terrainMesh.Location = new Vector3(0f, -1f, 0.0f);
            Renderer.Add(terrainMesh);
        }


        public void Update()
        {
            Camera.MoveInDirections(0.1f, Context.Input.GetMovementVector(Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E));

            if (Context.Input.IsKeyPressed(Keys.NumpadAdd))
                Renderer.AmbientLight.Intensity += 0.01f;
            if (Context.Input.IsKeyPressed(Keys.NumpadSubtract))
                Renderer.AmbientLight.Intensity -= 0.01f;

            if (Context.Input.IsKeyPressed(Keys.PageUp))
            {
                if (Renderer.DirectionalLight != null)
                    Renderer.DirectionalLight.Intensity += 0.01f;
            }
            if (Context.Input.IsKeyPressed(Keys.PageDown))
            {
                if (Renderer.DirectionalLight != null)
                    Renderer.DirectionalLight.Intensity -= 0.01f;
            }
            Renderer.AmbientLight.Intensity = Math.Clamp(Renderer.AmbientLight.Intensity, 0f, 1f);
            if (Renderer.DirectionalLight != null)
                Renderer.DirectionalLight.Intensity = Math.Clamp(Renderer.DirectionalLight.Intensity, 0f, 1f);

            if (Context.Input.IsJustMouseMoved)
            {
                Vector2 mouseMovement = Context.Input.MouseMovement;
                Camera.RotatePitch(mouseMovement.Y * -0.1f);
                Camera.RotateYaw(mouseMovement.X * 0.1f);
            }


            //_currentMesh.RotateY(0.01f);
                        
        }


        public void Dispose()
        {
            if (_currentMesh != null)
                _currentMesh.Dispose();
        }
    }
}
