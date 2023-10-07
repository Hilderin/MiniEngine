using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Phong shader
    /// </summary>
    public class PhongShader : Shader
    {
        /// <summary>
        /// Uniforms locations
        /// </summary>
        private int _wvpUniform;
        private int _samplerUniform;
        private int _lightColorUniform;
        private int _lightAmbientIntensityUniform;
        private int _gMaterialAmbientColorUniform;

        /// <summary>
        /// Constructor
        /// </summary>
        public PhongShader()
        {
            //--------------------------------------------
            //Vertex shader...
            Add(@"#version 330

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 TexCoord;

uniform mat4 gWVP;

out vec2 TexCoord0;

void main()
{
    gl_Position = gWVP * vec4(Position, 1.0);
    TexCoord0 = TexCoord;
}

", ShaderType.Vertex);


            //--------------------------------------------
            //Fragment shader...
            Add(@"#version 330

in vec2 TexCoord0;

out vec4 FragColor;

struct BaseLight
{
    vec3 Color;
    float AmbientIntensity;
};

struct Material
{
    vec3 AmbientColor;
};

uniform BaseLight gLight;
uniform Material gMaterial;
uniform sampler2D gSampler;

void main()
{
    FragColor = texture2D(gSampler, TexCoord0.xy) *
                vec4(gMaterial.AmbientColor, 1.0f) *
                vec4(gLight.Color, 1.0f) *
                gLight.AmbientIntensity;
    //FragColor = texture2D(gSampler, TexCoord0.xy);
}
", ShaderType.Fragment);



            _wvpUniform = GetUniformLocation("gWVP");
            _samplerUniform = GetUniformLocation("gSampler");
            _lightColorUniform = GetUniformLocation("gLight.Color");
            _lightAmbientIntensityUniform = GetUniformLocation("gLight.AmbientIntensity");
            _gMaterialAmbientColorUniform = GetUniformLocation("gMaterial.AmbientColor");

        }

        /// <summary>
        /// Set the MVP matrix
        /// </summary>
        public void SetWVP(ref Matrix4 wvpMatrix)
        {
            SetUniform(_wvpUniform, ref wvpMatrix);
        }

        /// <summary>
        /// Set the sampler
        /// </summary>
        public void SetSampler(int sampler)
        {
            SetUniform(_samplerUniform, sampler);
        }

        /// <summary>
        /// Set the ambiant color
        /// </summary>
        public void SetAmbiantColor(ref Color3 color)
        {
            SetUniform(_lightColorUniform, color.R, color.G, color.B);
        }

        /// <summary>
        /// Set the ambiant intensity
        /// </summary>
        public void SetAmbientIntensity(float ambiantIntensity)
        {
            SetUniform(_lightAmbientIntensityUniform, ambiantIntensity);
        }

        /// <summary>
        /// Set the material ambiant color
        /// </summary>
        public void SetMaterialAmbientColor(ref Color3 color)
        {
            SetUniform(_gMaterialAmbientColorUniform, color.R, color.G, color.B);
        }


    }
}
