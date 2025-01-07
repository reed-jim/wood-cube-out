Shader "FunktronicLabs/Volumetric" {
	Properties {
		[Header(Layer 1)] _Layer0Tex ("Layer 1 Texture", 2D) = "white" {}
		_Layer0Tint ("Layer 1 Tint", Vector) = (1,1,1,1)
		_Layer0SpeedX ("Layer 1 Scroll Speed X", Range(-1, 1)) = 0
		_Layer0SpeedY ("Layer 1 Scroll Speed Y", Range(-1, 1)) = 0
		[Header(Layer 2)] [Toggle(EnableLayer1)] _EnableLayer1 ("Enable", Float) = 1
		_Layer1Tex ("Layer 2 Texture", 2D) = "white" {}
		_Layer1Tint ("Layer 2 Tint", Vector) = (1,1,1,1)
		_Layer1SpeedX ("Layer 2 Scroll Speed X", Range(-1, 1)) = 0
		_Layer1SpeedY ("Layer 2 Scroll Speed Y", Range(-1, 1)) = 0
		[Header(Layer 3)] [Toggle(EnableLayer2)] _EnableLayer2 ("Enable", Float) = 1
		_Layer2Tex ("Layer 3 Texture", 2D) = "white" {}
		_Layer2Tint ("Layer 3 Tint", Vector) = (1,1,1,1)
		_Layer2SpeedX ("Layer 3 Scroll Speed X", Range(-1, 1)) = 0
		_Layer2SpeedY ("Layer 3 Scroll Speed Y", Range(-1, 1)) = 0
		[Header(Layers Global Properties)] _LayerHeightBias ("Layer Height Start Bias", Range(0, 0.2)) = 0.1
		_LayerHeightBiasStep ("Layer Height Step", Range(0, 0.3)) = 0.1
		_LayerDepthFalloff ("Layer Depth Fallofff", Range(0, 1)) = 0.9
		[Header(Volumetric Marble)] _MarbleTex ("Marble Heightmap Texture", 2D) = "black" {}
		_MarbleHeightScale ("Marble Height Scale", Range(0, 0.5)) = 0.1
		_MarbleHeightCausticOffset ("Marble Caustic Offset", Range(-5, 5)) = 0.1
		[Header(Caustic)] [Toggle(EnableCaustic)] _EnableCaustic ("Enable", Float) = 0
		_CausticMap ("Caustic Map", 2D) = "black" {}
		_CausticTint ("Caustic Tint", Vector) = (1,1,1,1)
		_CausticScrollSpeed ("Caustic Scroll Speed X", Range(-5, 5)) = 1
		[Header(Fresnel)] [Toggle(EnableFresnel)] _EnableFresnel ("Enable", Float) = 1
		[Toggle(EnableFresnelUseSkybox)] _EnableFresnelUseSkybox ("Use Skybox Reflection", Float) = 0
		_FresnelTightness ("Fresnel Tightness", Range(0, 10)) = 4
		_FresnelColorInside ("Fresnel Color Inside", Vector) = (1,1,0.5,1)
		_FresnelColorOutside ("Fresnel Color Outside", Vector) = (1,1,1,1)
		[Header(Surface Mask)] [Toggle(EnableSurfaceMask)] _EnableSurfaceMask ("Enable", Float) = 0
		_SurfaceAlphaMaskTex ("Surface Alpha Mask Texture", 2D) = "white" {}
		_SurfaceAlphaColor ("Surface Mask Color", Vector) = (1,1,1,1)
		[Header(Inner Light)] [Toggle(EnableInnerLight)] _EnableInnerLight ("Enable", Float) = 0
		_InnerLightTightness ("Inner Light Tightness", Range(0, 40)) = 20
		_InnerLightColorInside ("Inner Light Color Inside", Vector) = (1,1,1,1)
		_InnerLightColorOutside ("Inner Light Color Outside", Vector) = (1,1,0,1)
		[Header(Specular)] [Toggle(EnableSpecular)] _EnableSpecular ("Enable Specular", Float) = 0
		_SpecularTightness ("Specular Tightness", Range(0, 40)) = 2
		_SpecularBrightness ("Specular Brightness", Range(0, 1)) = 1
		[Header(Fog)] [Toggle(EnableFog)] _EnableFog ("Enable Fog", Float) = 0
		[Header(Refraction)] [Toggle(EnableRefraction)] _EnableRefraction ("Enable Refraction", Float) = 0
		_RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.2
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	Fallback "VertexLit"
}