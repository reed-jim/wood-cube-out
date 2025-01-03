Shader "3D/Master" {
	Properties {
		_Directional ("Directional Light", Vector) = (0.32,0.77,-0.56,1)
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_HSV ("Shadow HSV", Vector) = (0,1,0.5,1)
		[Header(Advance)] [Toggle(USE_ADVANCE)] _Advance ("Advance Mode", Float) = 1
		[Toggle(USE_GLOBAL)] _Global ("Global Mode", Float) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_Specular ("Specular Strength", Range(0, 1)) = 1
		[NoScaleOffset] _MetallicCap ("Metallic Capture", 2D) = "white" {}
		_Metallic ("Metallic Strength", Range(0, 1)) = 1
		[Toggle(USE_FRESNEL)] _Fresnel ("Fresnel", Float) = 0
		_FresnelColor ("Fresnel Color", Vector) = (1,1,1,1)
		_FresnelPower ("Fresnel Power", Float) = 10
		[Toggle(USE_HDRI)] _HDRI ("Use HDRI", Float) = 0
		[NoScaleOffset] _CubeMap ("Cubemap   (HDRI)", Cube) = "grey" {}
		_Exposure ("Exposure", Range(0, 8)) = 1
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