Shader "KIM/3D/Master Transparent" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
		[Header(Lighting)] _HSV ("Shadow Setting", Vector) = (0,1,0.5,10)
		[Header(Specular Clay)] _SpecularColor ("Specular Clay Color", Vector) = (1,1,1,0.5)
		[Header(HDRI)] [Toggle(USE_HDRI)] _HDRI ("Use HDRI", Float) = 0
		[NoScaleOffset] _CubeMap ("Cubemap   (HDRI)", Cube) = "grey" {}
		_ClayHDRI ("Clay HDRI Strength", Range(0, 1)) = 0.1
		[Header(Advance)] [Toggle(USE_ADVANCE)] _Advance ("Advance Mode", Float) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_Specular ("Specular Strength", Range(0, 1)) = 1
		_Exposure ("HDRI Exposure", Range(0, 2)) = 0.3
		_FresnelColor ("Fresnel Color", Vector) = (1,1,1,1)
		_FresnelPower ("Fresnel Power", Float) = 10
		[NoScaleOffset] _MetallicCap ("Metallic Capture", 2D) = "white" {}
		_Metallic ("Metallic Strength", Range(0, 1)) = 1
		_Alpha ("Alpha", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}