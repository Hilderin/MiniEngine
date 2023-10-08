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
    internal unsafe class Tutorial_SpecularLighting : IDisposable
    {
        private float translation = 0.0f;
        //private float deltaTransalation = 0.01f;

        private float scale = 1f;
        //private float deltaScale = 0.005f;

        private float rotation = 0.0f;
        private float deltaRotation = 0.001f;

        private Camera _camera = new Camera();

        private Mesh _currentMesh;

        private RenderingContext _renderingContext = new RenderingContext();


        public void Init()
        {

            Context.Current.LockCursor();

            _camera.Location = new Vector3(1.0f, 0.0f, -3.0f);

            _currentMesh = new AssetManager().GetMeshFromFile(@"C:\Projects\ogldev\Content\antique_ceramic_vase_01_4k.blend\antique_ceramic_vase_01_4k.obj", new MeshImportationParameters()
            {
                Scale = 6f
            });


            foreach (Material m in _currentMesh.Materials)
                m.AmbientColor = Color3.White;

            _renderingContext.AmbientIntensity = 0.8f;
            _renderingContext.DiffuseColor = Color3.White;
            _renderingContext.DiffuseIntensity = 1.0f;
        }


        public void Update()
        {

            if (Context.Current.Input.IsKeyPressed(Keys.A))
                _camera.Location.X -= 0.1f;
            if (Context.Current.Input.IsKeyPressed(Keys.D))
                _camera.Location.X += 0.1f;
            if (Context.Current.Input.IsKeyPressed(Keys.W))
                _camera.Location.Z += 0.1f;
            if (Context.Current.Input.IsKeyPressed(Keys.S))
                _camera.Location.Z -= 0.1f;
            if (Context.Current.Input.IsKeyPressed(Keys.Q))
                _camera.Location.Y += 0.1f;
            if (Context.Current.Input.IsKeyPressed(Keys.E))
                _camera.Location.Y -= 0.1f;
            if (Context.Current.Input.IsKeyPressed(Keys.NumpadAdd))
                _renderingContext.AmbientIntensity += 0.01f;
            if (Context.Current.Input.IsKeyPressed(Keys.NumpadSubtract))
                _renderingContext.AmbientIntensity -= 0.01f;
            if (Context.Current.Input.IsKeyPressed(Keys.PageUp))
                _renderingContext.DiffuseIntensity += 0.01f;
            if (Context.Current.Input.IsKeyPressed(Keys.PageDown))
                _renderingContext.DiffuseIntensity -= 0.01f;
            _renderingContext.AmbientIntensity = Math.Clamp(_renderingContext.AmbientIntensity, 0f, 1f);
            _renderingContext.DiffuseIntensity = Math.Clamp(_renderingContext.DiffuseIntensity, 0f, 1f);

            if (Context.Current.Input.IsJustMouseMoved)
            {
                Vector2 mouseMovement = Context.Current.Input.MouseMovement;
                _camera.RotateX(mouseMovement.Y * 0.1f);
                _camera.RotateY(mouseMovement.X * 0.1f);
            }
            //_camera.RotateY(0.01f);
            //Debug.Print(Math.RadToDeg(_camera.Rotation.Y).ToString() + " => " + _camera.Forward.ToString());

            //Debug.Print(camera.Forward + " " + camera.Up);

            //translation += deltaTransalation;
            //if ((translation >= 1.0f) || (translation <= -1.0f))
            //{
            //    deltaTransalation *= -1.0f;
            //}

            //scale += deltaScale;
            //if ((scale >= 1.5f) || (scale <= 0.8f))
            //{
            //    deltaScale *= -1.0f;
            //}

            rotation += deltaRotation;
            //if ((rotation >= LMath.PiOver2) || (rotation <= -LMath.PiOver2))
            //{
            //    deltaRotation *= -1.0f;
            //}


            WorldTransform worldTransform = new WorldTransform();
            worldTransform.Location = new Vector3(translation, translation, 6.0f);
            worldTransform.Scale = new Vector3(scale);
            worldTransform.Rotation = new Rotator3(0f, rotation, 0f);

            Matrix4 worldMatrix = worldTransform.GetMatrix();

            //_renderingContext.AmbientColor = new Color3(1f, 0, 1f);
            //_renderingContext.AmbientIntensity = 2f;
            _renderingContext.WVPMatrix = _camera.GetMatrix() * worldMatrix;

            
            _renderingContext.DiffuseDirection = new Vector3(1.0f, 0.0f, 0f);
            _renderingContext.CalculateDiffuseDirection(ref worldMatrix);
            _renderingContext.CameraLocalPosition = worldTransform.GetLocalPosition(ref _camera.Location);

            _currentMesh.Render(_renderingContext);
        }


        public void Dispose()
        {
            if (_currentMesh != null)
                _currentMesh.Dispose();
        }
    }
}
