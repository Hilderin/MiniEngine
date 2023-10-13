using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Assets;
using MiniEngine.PrimitiveMeshes;

namespace MiniEngine.Tutorials
{
    internal unsafe class Tutorial_SpotLights
    {
       
        private Mesh _currentMesh;
        private SpotLight _spotLight;

        private Context Context = Context.Current;
        private Camera Camera = Context.Current.Camera;



        public void Init()
        {

            Context.LockCursor();

            Context.Camera.Location = new Vector3(1.0f, 0.0f, -3.0f);

            _currentMesh = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\antique_ceramic_vase_01_4k.blend\antique_ceramic_vase_01_4k.obj", new MeshImportationParameters()
            {
                Scale = 6f,
                ResetMaterialAmbientColor = true
            });
            _currentMesh.Location = new Vector3(0f, 0f, 0.0f);
            Context.Add(_currentMesh);


            //Mesh mesh2 = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\antique_ceramic_vase_01_4k.blend\antique_ceramic_vase_01_4k.obj", new MeshImportationParameters()
            //{
            //    Scale = 3f,
            //    ResetMaterialAmbientColor = true
            //});
            //mesh2.Location = new Vector3(2f, 2f, 4f);
            //Context.Add(mesh2);


            Context.AmbientLight.Intensity = 0.1f;

            //Context.DirectionalLight = new DirectionalLight()
            //{
            //    Rotation = Rotator3.FromDegrees(45, 90, 0)
            //};


            _spotLight = new SpotLight()
            {
                Location = new Vector3(-8.0f, 0f, 0f),
                Rotation = Rotator3.FromDegrees(0, 90, 0),
                AttenuationLinear = 0.2f
            };
            Context.Add(_spotLight);


            var terrainMesh = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\box_terrain.obj", new MeshImportationParameters()
            {
                Scale = 1f,
                InverseFaces = false,
                SmoothNormals = false
            });
            terrainMesh.Location = new Vector3(0f, -4f, 0.0f);
            Context.Add(terrainMesh);
        }


        public void Update()
        {
            Camera.MoveInDirections(0.1f, Context.Input.GetMovementVector(Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E));

            if (Context.Input.IsKeyPressed(Keys.NumpadAdd))
                Context.AmbientLight.Intensity += 0.01f;
            if (Context.Input.IsKeyPressed(Keys.NumpadSubtract))
                Context.AmbientLight.Intensity -= 0.01f;

            if (Context.Input.IsKeyPressed(Keys.PageUp))
            {
                if (Context.DirectionalLight != null)
                    Context.DirectionalLight.Intensity += 0.01f;
            }
            if (Context.Input.IsKeyPressed(Keys.PageDown))
            {
                if (Context.DirectionalLight != null)
                    Context.DirectionalLight.Intensity -= 0.01f;
            }
            Context.AmbientLight.Intensity = Math.Clamp(Context.AmbientLight.Intensity, 0f, 1f);
            if (Context.DirectionalLight != null)
                Context.DirectionalLight.Intensity = Math.Clamp(Context.DirectionalLight.Intensity, 0f, 1f);

            if (Context.Input.IsJustMouseMoved)
            {
                Vector2 mouseMovement = Context.Input.MouseMovement;
                Camera.RotatePitch(mouseMovement.Y * -0.1f);
                Camera.RotateYaw(mouseMovement.X * 0.1f);
            }

            //_spotLight.RotateY(0.01f);
            //_currentMesh.RotateY(0.01f);

        }

    }
}
