using System;
using MiniEngine.Assets;
using MiniEngine.PrimitiveMeshes;

namespace MiniEngine.Tutorials
{
    internal unsafe class Tutorial_Cube
    {
       
        private Mesh _currentMesh;

        private Context Context = Context.Current;
        private Scene Scene = Context.Current.Scene;
        private Camera Camera = Context.Current.Scene.Camera;

        public void Init()
        {

            Context.LockCursor();

            Scene.Camera.Location = new Vector3(0.0f, 0.0f, -3.0f);

            _currentMesh = new CubeMesh();
            _currentMesh.Location = new Vector3(0f, 0f, 0.0f);
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

            if (Context.Input.IsKeyPressed(Keys.NumpadAdd))
                Scene.AmbientLight.Intensity += 0.01f;
            if (Context.Input.IsKeyPressed(Keys.NumpadSubtract))
                Scene.AmbientLight.Intensity -= 0.01f;

            if (Context.Input.IsKeyPressed(Keys.PageUp))
            {
                if (Scene.DirectionalLight != null)
                    Scene.DirectionalLight.Intensity += 0.01f;
            }
            if (Context.Input.IsKeyPressed(Keys.PageDown))
            {
                if (Scene.DirectionalLight != null)
                    Scene.DirectionalLight.Intensity -= 0.01f;
            }
            Scene.AmbientLight.Intensity = Math.Clamp(Scene.AmbientLight.Intensity, 0f, 1f);
            if (Scene.DirectionalLight != null)
                Scene.DirectionalLight.Intensity = Math.Clamp(Scene.DirectionalLight.Intensity, 0f, 1f);

            if (Context.Input.IsJustMouseMoved)
            {
                Vector2 mouseMovement = Context.Input.MouseMovement;
                Camera.RotatePitch(mouseMovement.Y * -0.1f);
                Camera.RotateYaw(mouseMovement.X * 0.1f);
            }


            //_currentMesh.RotateY(0.01f);
                        
        }

    }
}
