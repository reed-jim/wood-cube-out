Shader "XGame/SurfaceFill" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_FillAmount ("Fill Amount", Range(0, 1)) = 1
		_FlipFillAmount ("Flip Fill Amount", Range(0, 1)) = 1
		_MaxValue ("_MaxValue", Range(-100, 100)) = 0
		_MinValue ("_MinValue", Range(-100, 100)) = 0
		_FillDir ("_FillDir", Vector) = (1,1,1,1)
		_Color ("Color Tint", Vector) = (1,1,1,1)
		_LightIntensity ("Light Intensity", Range(0, 2)) = 1
		_Shininess ("Shininess", Range(1, 100)) = 20
		_SpecularColor ("Specular Color", Vector) = (1,1,1,1)
		_NormalStrength ("Normal Strength", Range(0, 5)) = 1
		_NormalLightingInfluence ("Normal Lighting Influence", Range(0, 1)) = 1
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
	Fallback "Diffuse"
}