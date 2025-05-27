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