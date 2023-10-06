using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.OpenGL;

namespace MiniEngine.Tutorials
{
    internal unsafe class Tutorial10
    {

        private uint VBO = 0;
        private uint VAO = 0;
        private uint IBO = 0;
        private uint program;

        private int _transformationUniform;


        private float translation = -0.5f;
        //private float deltaTransalation = 0.01f;

        private float scale = 0.5f;
        //private float deltaScale = 0.005f;

        private float rotation = 0f;
        private float deltaRotation = 0.03f;



        public void Init()
        {
            CreateVertexBuffer();
            CreateIndexBuffer();
            EnableVertixAttributs();

            program = CreateProgram();

            GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);

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

            int offset = sizeof(Vector3);
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
            //if ((scale >= 1.0f) || (scale <= -1.0f))
            //{
            //    deltaScale *= -1.0f;
            //}

            rotation += deltaRotation;
            if (rotation >= Math.PiOver2 || rotation <= -Math.PiOver2)
            {
                deltaRotation *= -1.0f;
            }


            //Matrix4f translationMatrix = new Matrix4f(1.0f, 0.0f, 0.0f, scale * 2,
            //                                     0.0f, 1.0f, 0.0f, scale,
            //                                     0.0f, 0.0f, 1.0f, 0.0f,
            //                                     0.0f, 0.0f, 0.0f, 1.0f);

            Matrix4 rotationMatrix = Matrix4.CreateRotationMatrixZ(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScaleMatrix(scale, scale, scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslationMatrix(translation, 0.0f, 0.0f);

            //Matrix4f transformMatrix = scaleMatrix * translationMatrix;
            Matrix4 transformMatrix = translationMatrix * rotationMatrix * scaleMatrix;

            //GL.glUniform1f(_scaleLocation, scale);
            //GL.glUniformMatrix4fv(_transformationUniform, 1, true, m);
            GL.glUniformMatrix4fv(_transformationUniform, transformMatrix);
            GL.CheckError();


            //GL.glDrawArrays(GL.GL_TRIANGLES, 0, 3);
            GL.glDrawElements(GL.GL_TRIANGLES, 6, GL.GL_UNSIGNED_INT, nint.Zero);
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


            //Vertex[] vertices = new Vertex[19];

            //// Center
            //vertices[0] = new Vertex(0.0f, 0.0f);

            //// Top row
            //vertices[1] = new Vertex(-1.0f, 1.0f);
            //vertices[1] = new Vertex(0.0f, 0.0f);
            //vertices[2] = new Vertex(-0.75f, 1.0f);
            //vertices[3] = new Vertex(-0.50f, 1.0f);
            //vertices[4] = new Vertex(-0.25f, 1.0f);
            //vertices[5] = new Vertex(-0.0f, 1.0f);
            //vertices[6] = new Vertex(0.25f, 1.0f);
            //vertices[7] = new Vertex(0.50f, 1.0f);
            //vertices[8] = new Vertex(0.75f, 1.0f);
            //vertices[9] = new Vertex(1.0f, 1.0f);

            //// Bottom row
            //vertices[10] = new Vertex(-1.0f, -1.0f);
            //vertices[11] = new Vertex(-0.75f, -1.0f);
            //vertices[12] = new Vertex(-0.50f, -1.0f);
            //vertices[13] = new Vertex(-0.25f, -1.0f);
            //vertices[14] = new Vertex(-0.0f, -1.0f);
            //vertices[15] = new Vertex(0.25f, -1.0f);
            //vertices[16] = new Vertex(0.50f, -1.0f);
            //vertices[17] = new Vertex(0.75f, -1.0f);
            //vertices[18] = new Vertex(1.0f, -1.0f);

            //for (int i = 0; i < vertices.Length; i++)
            //    vertices[i].Color = new Vector3(GMath.RandomFloat(0.0f, 1.0f));


            //Vector3[] vectors = new Vector3[3];
            //vectors[0] = new Vector3(-0.5f, -0.5f, 0.0f);
            //vectors[1] = new Vector3(0.5f, -0.5f, 0.0f);
            //vectors[2] = new Vector3(0.0f, 0.5f, 0.0f);

            Vertex[] vertices3 = new Vertex[4];
            vertices3[0] = new Vertex(-0.5f, -0.5f);
            vertices3[1] = new Vertex(0.5f, -0.5f);
            vertices3[2] = new Vertex(0.0f, 1f);
            vertices3[3] = new Vertex(1.0f, 1.0f);
            vertices3[0].Color = new Vector3(1.0f, 0.0f, 0.0f);
            vertices3[1].Color = new Vector3(0.0f, 1.0f, 0.0f);
            vertices3[2].Color = new Vector3(0.0f, 0.0f, 1.0f);
            vertices3[3].Color = new Vector3(1.0f, 1.0f, 1.0f);


            VAO = GL.glGenVertexArrays();
            GL.glBindVertexArray(VAO);
            GL.CheckError();


            VBO = GL.glGenBuffers();
            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, VBO);
            GL.CheckError();

            //var verticesFloat = new[] {
            //    -0.5f, -0.5f, 0.0f,
            //    0.5f, -0.5f, 0.0f,
            //    0.0f,  0.5f, 0.0f
            //};

            //fixed (float* v = &verticesFloat[0])
            //{
            //    GL.glBufferData(GL.GL_ARRAY_BUFFER, sizeof(float) * verticesFloat.Length, v, GL.GL_STATIC_DRAW);
            //    GL.CheckError();
            //}

            //var verticesFloat = new[] {
            //    -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f,
            //    0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f,
            //    0.0f,  0.5f, 0.0f, 1.0f, 0.0f, 0.0f
            //};

            //fixed (float* v = &verticesFloat[0])
            //{
            //    GL.glBufferData(GL.GL_ARRAY_BUFFER, sizeof(float) * verticesFloat.Length, v, GL.GL_STATIC_DRAW);
            //    GL.CheckError();
            //}

            //fixed (Vertex* v = &vertices[0])
            //{
            //    byte* vb = (byte*)v;

            //    for (int i = 0; i < sizeof(Vertex) * vertices.Length; i++)
            //        Debug.WriteLine(vb[i]);

            //    GL.glBufferData(GL.GL_ARRAY_BUFFER, sizeof(Vertex) * vertices.Length, v, GL.GL_STATIC_DRAW);
            //    GL.CheckError();
            //}

            //fixed (Vector3* v = &vectors[0])
            //{
            //    GL.glBufferData(GL.GL_ARRAY_BUFFER, sizeof(Vector3) * vectors.Length, v, GL.GL_STATIC_DRAW);
            //    GL.CheckError();
            //}

            //for (int i = 0; i < vertices3.Length; i++)
            //    vertices3[i].Color = new Vector3(GMath.RandomFloat(0.0f, 1.0f));

            fixed (Vertex* v = &vertices3[0])
            {
                GL.glBufferData(GL.GL_ARRAY_BUFFER, sizeof(Vertex) * vertices3.Length, v, GL.GL_STATIC_DRAW);
                GL.CheckError();
            }




        }



        private void CreateIndexBuffer()
        {
            //int[] Indices = new int[] { // Top triangles
            //                   0, 2, 1,
            //                   0, 3, 2,
            //                   0, 4, 3,
            //                   0, 5, 4,
            //                   0, 6, 5,
            //                   0, 7, 6,
            //                   0, 8, 7,
            //                   0, 9, 8,

            //                   // Bottom triangles
            //                   0, 10, 11,
            //                   0, 11, 12,
            //                   0, 12, 13,
            //                   0, 13, 14,
            //                   0, 14, 15,
            //                   0, 15, 16,
            //                   0, 16, 17,
            //                   0, 17, 18,

            //                   // Left triangle
            //                   0, 1, 10,

            //                   // Right triangle
            //                   0, 18, 9 };

            int[] Indices = new int[] {
                               0, 2, 1,
                               3, 1, 2
                                };


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
        }
    }
}
