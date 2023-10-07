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
        private int _lightAmbientColorUniform;
        private int _lightAmbientIntensityUniform;
        private int _lightDiffuseColorUniform;
        private int _lightDiffuseIntensityUniform;
        private int _lightDiffuseDirectionUniform;
        private int _gMaterialAmbientColorUniform;
        private int _gMaterialDiffuseColorUniform;

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
layout (location = 2) in vec3 Normal;

uniform mat4 gWVP;

out vec2 TexCoord0;
out vec3 Normal0;

void main()
{
    gl_Position = gWVP * vec4(Position, 1.0);
    TexCoord0 = TexCoord;
    Normal0 = Normal;
}


", ShaderType.Vertex);


            //--------------------------------------------
            //Fragment shader...
            Add(@"#version 330

in vec2 TexCoord0;
in vec3 Normal0;

out vec4 FragColor;

struct Light
{
    vec3 AmbientColor;
    float AmbientIntensity;
    vec3 DiffuseColor;
    float DiffuseIntensity;
    vec3 DiffuseDirection;
};

struct Material
{
    vec3 AmbientColor;
    vec3 DiffuseColor;
};

uniform Light gLight;
uniform Material gMaterial;
uniform sampler2D gSampler;

void main()
{
    vec4 AmbientColor = vec4(gLight.AmbientColor, 1.0f) *
                        gLight.AmbientIntensity *
                        vec4(gMaterial.AmbientColor, 1.0f);

    float DiffuseFactor = dot(normalize(Normal0), -gLight.DiffuseDirection);

    vec4 DiffuseColor = vec4(0, 0, 0, 0);

    if (DiffuseFactor > 0) {
        DiffuseColor = vec4(gLight.DiffuseColor, 1.0f) *
                       gLight.DiffuseIntensity *
                       vec4(gMaterial.DiffuseColor, 1.0f) *
                       DiffuseFactor;
    }

    //FragColor = texture2D(gSampler, TexCoord0.xy) * (AmbientColor + DiffuseColor);
    FragColor = texture2D(gSampler, TexCoord0.xy) * (AmbientColor + DiffuseColor);
}

", ShaderType.Fragment);



            _wvpUniform = GetUniformLocation("gWVP");
            _samplerUniform = GetUniformLocation("gSampler");
            _lightAmbientColorUniform = GetUniformLocation("gLight.AmbientColor");
            _lightAmbientIntensityUniform = GetUniformLocation("gLight.AmbientIntensity");

            _lightDiffuseColorUniform = GetUniformLocation("gLight.DiffuseColor");
            _lightDiffuseIntensityUniform = GetUniformLocation("gLight.DiffuseIntensity");
            _lightDiffuseDirectionUniform = GetUniformLocation("gLight.DiffuseDirection");

            _gMaterialAmbientColorUniform = GetUniformLocation("gMaterial.AmbientColor");
            _gMaterialDiffuseColorUniform = GetUniformLocation("gMaterial.DiffuseColor");

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
        /// Set the Ambient color
        /// </summary>
        public void SetAmbientColor(ref Color3 color)
        {
            SetUniform(_lightAmbientColorUniform, color.R, color.G, color.B);
        }

        /// <summary>
        /// Set the Ambient intensity
        /// </summary>
        public void SetAmbientIntensity(float AmbientIntensity)
        {
            SetUniform(_lightAmbientIntensityUniform, AmbientIntensity);
        }

        /// <summary>
        /// Set the material Ambient color
        /// </summary>
        public void SetMaterialAmbientColor(ref Color3 color)
        {
            SetUniform(_gMaterialAmbientColorUniform, color.R, color.G, color.B);
        }

        /// <summary>
        /// Set the Diffuse color
        /// </summary>
        public void SetDiffuseColor(ref Color3 color)
        {
            SetUniform(_lightDiffuseColorUniform, color.R, color.G, color.B);
        }

        /// <summary>
        /// Set the Diffuse intensity
        /// </summary>
        public void SetDiffuseIntensity(float DiffuseIntensity)
        {
            SetUniform(_lightDiffuseIntensityUniform, DiffuseIntensity);
        }

        /// <summary>
        /// Set the Diffuse direction
        /// </summary>
        public void SetDiffuseDirection(ref Vector3 direction)
        {
            SetUniform(_lightDiffuseDirectionUniform, direction.X, direction.Y, direction.Z);
        }

        /// <summary>
        /// Set the material Diffuse color
        /// </summary>
        public void SetMaterialDiffuseColor(ref Color3 color)
        {
            SetUniform(_gMaterialDiffuseColorUniform, color.R, color.G, color.B);
        }


    }
}
