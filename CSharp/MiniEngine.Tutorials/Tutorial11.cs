using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.OpenGL;

namespace MiniEngine.Tutorials
{
    internal unsafe class Tutorial11
    {

        private uint VBO = 0;
        private uint VAO = 0;
        private uint IBO = 0;
        private uint program;

        private int _transformationUniform;


        private float translation = 0.0f;
        //private float deltaTransalation = 0.01f;

        private float scale = 1.0f;
        //private float deltaScale = 0.005f;

        private float rotation = 0.0f;
        private float deltaRotation = 0.03f;


        private int[] Indices;
        private Vertex[] Vertices;


        public void Init()
        {

            GL.glEnable(GL.GL_CULL_FACE);
            GL.glFrontFace(GL.GL_CW);
            GL.glCullFace(GL.GL_BACK);

            InitVertices();
            CreateVertexBuffer();
            CreateIndexBuffer();
            EnableVertixAttributs();

            program = CreateProgram();

            GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);

        }

        private void InitVertices()
        {
            Vertices = new Vertex[8];
            Vertices[0] = new Vertex(0.5f, 0.5f, 0.5f);
            Vertices[1] = new Vertex(-0.5f, 0.5f, -0.5f);
            Vertices[2] = new Vertex(-0.5f, 0.5f, 0.5f);
            Vertices[3] = new Vertex(0.5f, -0.5f, -0.5f);
            Vertices[4] = new Vertex(-0.5f, -0.5f, -0.5f);
            Vertices[5] = new Vertex(0.5f, 0.5f, -0.5f);
            Vertices[6] = new Vertex(0.5f, -0.5f, 0.5f);
            Vertices[7] = new Vertex(-0.5f, -0.5f, 0.5f);

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Color = new Vector3(Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f), Math.RandomFloat(0.0f, 1.0f));
            }


            Indices = new int[] {
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
            };
        }

        private void EnableVertixAttributs()
        {

            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, VBO);
            GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, IBO);

            //Position...
            GL.glEnableVertexAttribArray(0);
            GL.CheckError();
            GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, false, 6 * sizeof(float), 0);
            GL.CheckError();

            //Color...
            GL.glEnableVertexAttribArray(1);
            GL.CheckError();

            GL.glVertexAttribPointer(1, 3, GL.GL_FLOAT, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.CheckError();
        }


        public void Update()
        {
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


            //Matrix4f translationMatrix = new Matrix4f(1.0f, 0.0f, 0.0f, scale * 2,
            //                                     0.0f, 1.0f, 0.0f, scale,
            //                                     0.0f, 0.0f, 1.0f, 0.0f,
            //                                     0.0f, 0.0f, 0.0f, 1.0f);

            Matrix4 rotationMatrix = Matrix4.CreateRotationMatrixY(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScaleMatrix(scale, scale, scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslationMatrix(translation, translation, 2.0f);
            Matrix4 projectionMatrix = Matrix4.CreateProjection(90.0f, Program.WIDTH, Program.HEIGHT, 1f, 100f);

            //Matrix4f transformMatrix = scaleMatrix * translationMatrix;
            Matrix4 transformMatrix = projectionMatrix * translationMatrix * rotationMatrix * scaleMatrix;

            //GL.glUniform1f(_scaleLocation, scale);
            //GL.glUniformMatrix4fv(_transformationUniform, 1, true, m);
            GL.glUniformMatrix4fv(_transformationUniform, transformMatrix);
            GL.CheckError();

            //GL.glDrawArrays(GL.GL_TRIANGLES, 0, 3);
            GL.glDrawElements(GL.GL_TRIANGLES, Indices.Length, GL.GL_UNSIGNED_INT, nint.Zero);
            //GL.glDrawElements(GL.GL_TRIANGLES, 54, GL.GL_UNSIGNED_INT, IntPtr.Zero);

            GL.CheckError();
        }

        /// <summary>
        /// Creates an extremely basic shader program that is capable of displaying a triangle on screen.
        /// </summary>
        /// <returns>The created shader program. No error checking is performed for this basic example.</returns>
        private uint CreateProgram()
        {
            var vertex = GL.CreateShader(GL.GL_VERTEX_SHADER, @"#version 330 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec3 inColor;

uniform mat4 gTranslation;

out vec4 Color;

void main()
{
    gl_Position = gTranslation * vec4(pos, 1.0);
    //Color = vec4(1.0f,1.0f, 1.0f, 1.0f);
    Color = vec4(inColor, 1.0f);
}
");
            var fragment = GL.CreateShader(GL.GL_FRAGMENT_SHADER, @"#version 330 core

in vec4 Color;
out vec4 FragColor;

void main()
{
    FragColor = Color;
}

");

            var program = GL.glCreateProgram();
            GL.CheckError();

            GL.glAttachShader(program, vertex);
            GL.CheckError();

            GL.glAttachShader(program, fragment);
            GL.CheckError();

            //Linking program...
            GL.glLinkProgram(program);
            GL.CheckError();

            if (!GL.glGetProgramiv(program, GL.GL_LINK_STATUS))
                throw new Exception($"Error linking program: {GL.glGetProgramInfoLog(program)}");

            //Validate program....
            GL.glValidateProgram(program);
            if (!GL.glGetProgramiv(program, GL.GL_VALIDATE_STATUS))
                throw new Exception($"Error validating program: {GL.glGetProgramInfoLog(program)}");


            _transformationUniform = GL.glGetUniformLocation(program, "gTranslation");
            if (_transformationUniform == -1)
                throw new Exception($"Error getting uniform location of 'gTranslation'");


            GL.glDeleteShader(vertex);
            GL.CheckError();

            GL.glDeleteShader(fragment);
            GL.CheckError();

            GL.glUseProgram(program);
            GL.CheckError();


            return program;
        }




        private void CreateVertexBuffer()
        {

            VAO = GL.glGenVertexArrays();
            GL.glBindVertexArray(VAO);
            GL.CheckError();


            VBO = GL.glGenBuffers();
            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, VBO);
            GL.CheckError();

            fixed (Vertex* v = &Vertices[0])
            {
                GL.glBufferData(GL.GL_ARRAY_BUFFER, sizeof(Vertex) * Vertices.Length, v, GL.GL_STATIC_DRAW);
                GL.CheckError();
            }




        }



        private void CreateIndexBuffer()
        {

            IBO = GL.glGenBuffers();
            GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, IBO);
            fixed (int* pointer = &Indices[0])
            {
                GL.glBufferData(GL.GL_ELEMENT_ARRAY_BUFFER, sizeof(int) * Indices.Length, pointer, GL.GL_STATIC_DRAW);
            }

        }


        private struct Vertex
        {
            public Vector3 Pos;
            public Vector3 Color;

            public Vertex(float x, float y)
            {
                Pos = new Vector3(x, y, 0.0f);
                Color = Vector3.Zero;
            }

            public Vertex(float x, float y, float z)
            {
                Pos = new Vector3(x, y, z);
                Color = Vector3.Zero;
            }
        }
    }
}
