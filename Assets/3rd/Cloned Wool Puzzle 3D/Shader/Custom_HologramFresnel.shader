Shader "Custom/HologramFresnel" {
	Properties {
		_MainTex ("Albedo", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_FresnelPower ("Fresnel Power", Range(0.1, 10)) = 2
		_FresnelColor ("Fresnel Color", Vector) = (1,1,1,1)
		_Threshold ("Threshold", Float) = 0.03
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

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(_MainTex_sampler, float2(input.uv.x, input.uv.y));
			}

			ENDHLSL
		}
	}
	Fallback "Diffuse"
}