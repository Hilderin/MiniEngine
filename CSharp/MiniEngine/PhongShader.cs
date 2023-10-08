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
        private int _samplerSpecularUniform;
        private int _lightAmbientColorUniform;
        private int _lightAmbientIntensityUniform;
        private int _lightDiffuseColorUniform;
        private int _lightDiffuseIntensityUniform;
        private int _lightDiffuseDirectionUniform;
        private int _gMaterialAmbientColorUniform;
        private int _gMaterialDiffuseColorUniform;
        private int _gMaterialSpecularColorUniform;
        private int _gCameraLocalPosUniform;
        private int _gNumPointLightsUniform;
        private PointLightUniform[] _pointLightUniforms = new PointLightUniform[Renderer.MAX_POINT_LIGHTS];

        private struct PointLightUniform
        {
            public int ColorUniform;
            public int AmbientIntensityUniform;
            public int PositionUniform;
            public int DiffuseIntensityUniform;
            public int AttenConstantUniform;
            public int AttenLinearUniform;
            public int AttenExpUniform;
        }

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
out vec3 LocalPos0;

void main()
{
    gl_Position = gWVP * vec4(Position, 1.0);
    TexCoord0 = TexCoord;
    Normal0 = Normal;
    LocalPos0 = Position;
}
", ShaderType.Vertex);


            //--------------------------------------------
            //Fragment shader...
            Add(@"#version 330

const int MAX_POINT_LIGHTS = {MAX_POINT_LIGHTS};

in vec2 TexCoord0;
in vec3 Normal0;
in vec3 LocalPos0;

out vec4 FragColor;

struct BaseLight
{
    vec3 Color;
    float AmbientIntensity;
    float DiffuseIntensity;
};

struct DirectionalLight
{
    BaseLight Base;
    vec3 Direction;
};

struct Attenuation
{
    float Constant;
    float Linear;
    float Exp;
};

struct PointLight
{
    BaseLight Base;
    vec3 LocalPos;
    Attenuation Atten;
};


struct Material
{
    vec3 AmbientColor;
    vec3 DiffuseColor;
    vec3 SpecularColor;
};

uniform DirectionalLight gDirectionalLight;
uniform int gNumPointLights;
uniform PointLight gPointLights[MAX_POINT_LIGHTS];
uniform Material gMaterial;
uniform sampler2D gSampler;
uniform sampler2D gSamplerSpecularExponent;
uniform vec3 gCameraLocalPos;

vec4 CalcLightInternal(BaseLight Light, vec3 LightDirection, vec3 Normal)
{
    vec4 AmbientColor = vec4(Light.Color, 1.0f) *
                        Light.AmbientIntensity *
                        vec4(gMaterial.AmbientColor, 1.0f);

    float DiffuseFactor = dot(Normal, -LightDirection);

    vec4 DiffuseColor = vec4(0, 0, 0, 0);
    vec4 SpecularColor = vec4(0, 0, 0, 0);

    if (DiffuseFactor > 0) {
        DiffuseColor = vec4(Light.Color, 1.0f) *
                       Light.DiffuseIntensity *
                       vec4(gMaterial.DiffuseColor, 1.0f) *
                       DiffuseFactor;

        vec3 PixelToCamera = normalize(gCameraLocalPos - LocalPos0);
        vec3 LightReflect = normalize(reflect(LightDirection, Normal));
        float SpecularFactor = dot(PixelToCamera, LightReflect);
        if (SpecularFactor > 0) {
            float SpecularExponent = texture2D(gSamplerSpecularExponent, TexCoord0).r * 255.0;
            SpecularFactor = pow(SpecularFactor, SpecularExponent);
            SpecularColor = vec4(Light.Color, 1.0f) *
                            Light.DiffuseIntensity * // using the diffuse intensity for diffuse/specular
                            vec4(gMaterial.SpecularColor, 1.0f) *
                            SpecularFactor;
        }
    }

    return (AmbientColor + DiffuseColor + SpecularColor);
}


vec4 CalcDirectionalLight(vec3 Normal)
{
    return CalcLightInternal(gDirectionalLight.Base, gDirectionalLight.Direction, Normal);
}

vec4 CalcPointLight(int Index, vec3 Normal)
{
    vec3 LightDirection = LocalPos0 - gPointLights[Index].LocalPos;
    float Distance = length(LightDirection);
    LightDirection = normalize(LightDirection);

    vec4 Color = CalcLightInternal(gPointLights[Index].Base, LightDirection, Normal);
    float Attenuation =  gPointLights[Index].Atten.Constant +
                         gPointLights[Index].Atten.Linear * Distance;
    if(Attenuation == 0) {
        return Color;
    }
    else 
    {
        return Color / Attenuation;
    }
}


void main()
{
    vec3 Normal = normalize(Normal0);
    vec4 TotalLight = CalcDirectionalLight(Normal);

    for (int i = 0 ; i < gNumPointLights ; i++) {
        TotalLight += CalcPointLight(i, Normal);
    }

    FragColor = texture2D(gSampler, TexCoord0.xy) * TotalLight;
}

".Replace("{MAX_POINT_LIGHTS}", Renderer.MAX_POINT_LIGHTS.ToString())
, ShaderType.Fragment);


            //uniform DirectionalLight gDirectionalLight;
            //uniform int gNumPointLights;
            //uniform PointLight gPointLights[MAX_POINT_LIGHTS];
            //uniform Material gMaterial;
            //uniform sampler2D gSampler;
            //uniform sampler2D gSamplerSpecularExponent;
            //uniform vec3 gCameraLocalPos;

            _wvpUniform = GetUniformLocation("gWVP");
            _samplerUniform = GetUniformLocation("gSampler");
            _samplerSpecularUniform = GetUniformLocation("gSamplerSpecularExponent");

            _lightAmbientColorUniform = GetUniformLocation("gDirectionalLight.Base.Color");
            _lightAmbientIntensityUniform = GetUniformLocation("gDirectionalLight.Base.AmbientIntensity");

            _lightDiffuseColorUniform = GetUniformLocation("gDirectionalLight.Color");
            _lightDiffuseIntensityUniform = GetUniformLocation("gDirectionalLight.Base.DiffuseIntensity");
            _lightDiffuseDirectionUniform = GetUniformLocation("gDirectionalLight.Direction");

            _gMaterialAmbientColorUniform = GetUniformLocation("gMaterial.AmbientColor");
            _gMaterialDiffuseColorUniform = GetUniformLocation("gMaterial.DiffuseColor");
            _gMaterialSpecularColorUniform = GetUniformLocation("gMaterial.SpecularColor");

            _gCameraLocalPosUniform = GetUniformLocation("gCameraLocalPos");
            _gNumPointLightsUniform = GetUniformLocation("gNumPointLights");
            

            for (int i = 0; i < Renderer.MAX_POINT_LIGHTS; i++)
            {
                _pointLightUniforms[i].ColorUniform = GetUniformLocation(String.Format("gPointLights[{0}].Base.Color", i));
                _pointLightUniforms[i].AmbientIntensityUniform = GetUniformLocation(String.Format("gPointLights[{0}].Base.AmbientIntensity", i));
                _pointLightUniforms[i].PositionUniform = GetUniformLocation(String.Format("gPointLights[{0}].LocalPos", i));
                _pointLightUniforms[i].DiffuseIntensityUniform = GetUniformLocation(String.Format("gPointLights[{0}].Base.DiffuseIntensity", i));
                _pointLightUniforms[i].AttenConstantUniform = GetUniformLocation(String.Format("gPointLights[{0}].Atten.Constant", i));
                _pointLightUniforms[i].AttenLinearUniform = GetUniformLocation(String.Format("gPointLights[{0}].Atten.Linear", i));
                _pointLightUniforms[i].AttenExpUniform = GetUniformLocation(String.Format("gPointLights[{0}].Atten.Exp", i));
            }

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
        /// Set the samplerSpecular
        /// </summary>
        public void SetSamplerSpecular(int samplerSpecular)
        {
            SetUniform(_samplerSpecularUniform, samplerSpecular);
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

        /// <summary>
        /// Set the material Specular color
        /// </summary>
        public void SetMaterialSpecularColor(ref Color3 color)
        {
            SetUniform(_gMaterialSpecularColorUniform, color.R, color.G, color.B);
        }

        /// <summary>
        /// Set the camera location position
        /// </summary>
        public void SetCameraLocalPos(ref Vector3 position)
        {
            SetUniform(_gCameraLocalPosUniform, position.X, position.Y, position.Z);
        }

        /// <summary>
        /// Set the point lights
        /// </summary>
        public void SetPointLights(List<PointLight> pointLights, Vector3[] calculatedLocalPosition)
        {

            if (pointLights != null && pointLights.Count > 0)
            {

                SetUniform(_gNumPointLightsUniform, pointLights.Count);


                for (int i = 0; i < pointLights.Count; i++)
                {
                    PointLight light = pointLights[i];

                    SetUniform(_pointLightUniforms[i].ColorUniform, light.Color.R, light.Color.G, light.Color.B);
                    SetUniform(_pointLightUniforms[i].AmbientIntensityUniform, light.AmbientIntensity);
                    SetUniform(_pointLightUniforms[i].DiffuseIntensityUniform, light.DiffuseIntensity);
                    SetUniform(_pointLightUniforms[i].PositionUniform, calculatedLocalPosition[i].X, calculatedLocalPosition[i].Y, calculatedLocalPosition[i].Z);
                    SetUniform(_pointLightUniforms[i].AttenConstantUniform, light.AttenuationConstant);
                    SetUniform(_pointLightUniforms[i].AttenLinearUniform, light.AttenuationLinear);
                    SetUniform(_pointLightUniforms[i].AttenExpUniform, light.AttenuationExponent);

                }
            }
            else
            {
                //No point lights...
                SetUniform(_gNumPointLightsUniform, 0);
            }
        }



    }
}
