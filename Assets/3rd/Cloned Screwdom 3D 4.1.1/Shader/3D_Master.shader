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

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_MatrixMVP;

			struct Vertex_Stage_Input
			{
				float3 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixMVP, float4(input.pos, 1.0));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState _MainTex_sampler;
			fixed4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(_MainTex_sampler, float2(input.uv.x, input.uv.y)) * _Color;
			}

			ENDHLSL
		}
	}
}