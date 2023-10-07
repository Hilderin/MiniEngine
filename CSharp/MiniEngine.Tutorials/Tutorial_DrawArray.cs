using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.OpenGL;

namespace MiniEngine.Tutorials
{
    internal unsafe class Tutorial_DrawArray
    {

        private uint VBO = 0;
        private uint VAO = 0;
        private uint program;

        //private int _transformationUniform;


        private float translation = 0.0f;
        //private float deltaTransalation = 0.01f;

        private float scale = 0.5f;
        //private float deltaScale = 0.005f;

        private float rotation = 0.0f;
        private float deltaRotation = 0.03f;

        public void Init()
        {
            CreateVertexBuffer();

            program = CreateProgram();

            GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);

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
            //if ((rotation >= GMath.PiOver2) || (rotation <= -GMath.PiOver2))
            //{
            //    deltaRotation *= -1.0f;
            //}


            //Matrix4f translationMatrix = new Matrix4f(1.0f, 0.0f, 0.0f, scale * 2,
            //                                     0.0f, 1.0f, 0.0f, scale,
            //                                     0.0f, 0.0f, 1.0f, 0.0f,
            //                                     0.0f, 0.0f, 0.0f, 1.0f);

            Matrix4 rotationMatrix = Matrix4.CreateRotationMatrixZ(rotation);
            Matrix4 scaleMatrix = Matrix4.CreateScaleMatrix(scale, scale, scale);
            Matrix4 translationMatrix = Matrix4.CreateTranslationMatrix(translation, translation, 0.0f);

            //Matrix4f transformMatrix = scaleMatrix * translationMatrix;
            Matrix4 transformMatrix = translationMatrix * rotationMatrix * scaleMatrix;

            //GL.glUniform1f(_scaleLocation, scale);
            //GL.glUniformMatrix4fv(_transformationUniform, 1, true, m);
            //GL.glUniformMatrix4fv(_transformationUniform, transformMatrix);
            //GL.CheckError();

            GL.glDrawArrays(GL.GL_TRIANGLES, 0, 3);
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

out vec4 Color;

const vec4 colors[3] = vec4[3]( vec4(1, 0, 0, 1),
                                vec4(0, 1, 0, 1),
                                vec4(0, 0, 1, 1) );

void main()
{
    gl_Position = vec4(pos, 1.0);
    Color = colors[gl_VertexID];
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


            //_transformationUniform = GL.glGetUniformLocation(program, "gTranslation");
            //if (_transformationUniform == -1)
            //    throw new System.Exception($"Error getting uniform location of 'gTranslation'");


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
            var vertices = new[] {
                -0.5f, -0.5f, 0.0f,
                0.0f,  0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,

            };


            uint vao2 = 0;
            GL.glGenVertexArrays(1, &vao2);
            GL.CheckError();
            VAO = vao2;

            uint vbo2 = 0;
            GL.glGenBuffers(1, &vbo2);
            GL.CheckError();
            VBO = vbo2;

            GL.glBindVertexArray(VAO);
            GL.CheckError();


            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, VBO);
            GL.CheckError();

            fixed (float* v = &vertices[0])
            {
                GL.glBufferData(GL.GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL.GL_STATIC_DRAW);
                GL.CheckError();
            }


            GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, false, 3 * sizeof(float), 0);
            GL.CheckError();

            GL.glEnableVertexAttribArray(0);
            GL.CheckError();


            //Vector3f Vertices[3];
            //Vertices[0] = Vector3f(-1.0f, -1.0f, 0.0f);   // bottom left
            //Vertices[1] = Vector3f(1.0f, -1.0f, 0.0f);    // bottom right
            //Vertices[2] = Vector3f(0.0f, 1.0f, 0.0f);     // top

            //glGenBuffers(1, &VBO);
            //glBindBuffer(GL_ARRAY_BUFFER, VBO);
            //glBufferData(GL_ARRAY_BUFFER, sizeof(Vertices), Vertices, GL_STATIC_DRAW);
        }

    }
}
