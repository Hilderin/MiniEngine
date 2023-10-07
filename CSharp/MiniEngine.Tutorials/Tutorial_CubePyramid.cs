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
    internal unsafe class Tutorial_CubePyramid : IDisposable
    {
        private float translation = 0.0f;
        //private float deltaTransalation = 0.01f;

        private float scale = 1.0f;
        //private float deltaScale = 0.005f;

        private float rotation = 0.0f;
        private float deltaRotation = 0.03f;

        private Camera _camera = new Camera();


        private Mesh _cube;
        private Mesh _pyramid;
        private Texture2D _texture;
        private Mesh _currentMesh;

        private RenderingContext _renderingContext = new RenderingContext();

        public void Init()
        {

            Context.Current.LockCursor();

            _camera.Location = new Vector3(1.0f, 0.0f, -3.0f);

            _texture = new AssetManager().GetTexture2DFromFile(@"C:\Projects\ogldev\Content\bricks.jpg");
            
            Material mat = new Material()
            {
                Diffuse = _texture
            };

            _cube = new CubeMesh();
            _cube.SetMaterial(mat, 0);

            _pyramid = new PyramidMesh();
            //_pyramid.SetMaterial(mat, 0);

            _currentMesh = _pyramid;

            

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
            if (Context.Current.Input.IsJustKeyPressed(Keys.Space))
            {
                if (_currentMesh == _pyramid)
                    _currentMesh = _cube;
                else
                    _currentMesh = _pyramid;
            }


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
            worldTransform.Location = new Vector3(translation, translation, 2.0f);
            worldTransform.Scale = new Vector3(scale);
            worldTransform.Rotation = new Vector3(rotation, rotation, rotation);

            Matrix4 worldMatrix = worldTransform.GetMatrix();

            _renderingContext.WVPMatrix = _camera.GetMatrix() * worldMatrix;


            _currentMesh.Render(_renderingContext);
        }

        public void Dispose()
        {
            if (_cube != null)
                _cube.Dispose();
            if (_pyramid != null)
                _pyramid.Dispose();
        }
    }
}
