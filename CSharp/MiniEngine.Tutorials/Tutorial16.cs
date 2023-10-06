using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.AssertManager;
using MiniEngine.OpenGL;

namespace MiniEngine.Tutorials
{
    internal unsafe class Tutorial16
    {
        private float translation = 0.0f;
        //private float deltaTransalation = 0.01f;

        private float scale = 1.0f;
        //private float deltaScale = 0.005f;

        private float rotation = 0.0f;
        private float deltaRotation = 0.03f;

        private Camera _camera = new Camera();



        private Shader _mat;
        private Mesh _mesh;
        private Texture2D _texture;

        public void Init()
        {

            Context.Current.LockCursor();

            _camera.Location = new Vector3(1.0f, 0.0f, -3.0f);

            _texture = new AssetManager().GetTexture2DFromFile(@"C:\Projects\ogldev\Content\bricks.jpg");

            _mesh = CreateMesh();

            _mat = CreateMaterial();

        }

        private Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();

            Vector2 t00 = new Vector2(0.0f, 0.0f);  // Bottom left
            Vector2 t01 = new Vector2(0.0f, 1.0f);  // Top left
            Vector2 t10 = new Vector2(1.0f, 0.0f);  // Bottom right
            Vector2 t11 = new Vector2(1.0f, 1.0f);  // Top right

            Vertex[] vertices = new Vertex[8];
            vertices[0] = new Vertex(0.5f, 0.5f, 0.5f, t00);
            vertices[1] = new Vertex(-0.5f, 0.5f, -0.5f, t01);
            vertices[2] = new Vertex(-0.5f, 0.5f, 0.5f, t10);
            vertices[3] = new Vertex(0.5f, -0.5f, -0.5f, t11);
            vertices[4] = new Vertex(-0.5f, -0.5f, -0.5f, t00);
            vertices[5] = new Vertex(0.5f, 0.5f, -0.5f, t10);
            vertices[6] = new Vertex(0.5f, -0.5f, 0.5f, t01);
            vertices[7] = new Vertex(-0.5f, -0.5f, 0.5f, t11);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = new Vector3(Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f));
            }

            mesh.SetVertices(vertices);

            mesh.SetIndices(new int[] {
                              0, 1, 2,
                              1, 3, 4,
                              5, 6, 3,
                              7, 3, 6,
                              2, 4, 7,
                              0, 7, 6,
                              0, 5, 1,
                              1, 5, 3,
                              5, 0, 6,
                              7, 4, 3,
                              2, 1, 4,
                              0, 2, 7
            });

            return mesh;
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

            Matrix4 wvpMatrix = _camera.GetMatrix() * worldMatrix;

            _texture.Bind(GL.GL_TEXTURE0);

            _mat.SetUniform("gMVP", wvpMatrix);
            _mat.SetUniform("gSampler", 0);

            _mesh.Render();
        }

        /// <summary>
        /// Creates an extremely basic shader program that is capable of displaying a triangle on screen.
        /// </summary>
        /// <returns>The created shader program. No error checking is performed for this basic example.</returns>
        private Shader CreateMaterial()
        {

            Shader mat = new Shader(
@"#version 330 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec3 color;
layout (location = 2) in vec2 texCoord;

uniform mat4 gMVP;

out vec2 TexCoord0;
out vec4 Color;

void main()
{
    gl_Position = gMVP * vec4(pos, 1.0);
    TexCoord0 = texCoord;
    Color = vec4(color, 1.0f);
}
"
, @"#version 330 core

in vec2 TexCoord0;
in vec4 Color;

out vec4 FragColor;

uniform sampler2D gSampler;

void main()
{
    //FragColor = Color;
    FragColor = texture2D(gSampler, TexCoord0);
}

");
            return mat;
        }






    }
}
