Shader "XGame/SurfaceV2Fill" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_FillAmount ("Fill Amount", Range(0, 1)) = 1
		_FlipFillAmount ("Flip Fill Amount", Range(0, 1)) = 1
		_MaxValue ("_MaxValue", Range(-100, 100)) = 0
		_MinValue ("_MinValue", Range(-100, 100)) = 0
		_FillDir ("_FillDir", Vector) = (1,1,1,1)
		_Color ("Color Tint", Vector) = (1,1,1,1)
		_Saturation ("Color Tint Saturation", Range(0, 10)) = 1
		_Brightness ("Brightness", Range(0.1, 10)) = 1
		_Ambient ("Ambient Light", Range(0, 1)) = 0.3
		_DiffusePower ("Diffuse Power", Range(0, 2)) = 0.7
		_LightDir ("Light Direction", Vector) = (0.4,1,0.6,0)
		_NormalStrength ("Normal Strength", Range(0, 5)) = 1
		_NormalLightingInfluence ("Normal Lighting Influence", Range(0, 1)) = 0
		_LightingBlend ("Lighting Blend", Range(0, 2)) = 2
		[Header(DISSOLVE)] [Toggle] _EnableDissolve ("Enable Dissolve", Float) = 1
		_DissolveNoise ("Dissolve Noise", 2D) = "gray" {}
		_Dissolve ("Dissolve Threshold", Range(0, 1)) = 0.3
		_DissolveSoftness ("Dissolve Softness", Range(0.001, 0.5)) = 0.08
		_Cutoff ("Mask Clip Value", Range(0, 1)) = 0.35
		_EdgeRegion ("Edge Region Size", Range(0.001, 5)) = 0.5
		_DissolveReveal ("Dissolve Reveal (0..1)", Range(0, 5)) = 0
		_DissolveGap ("Dissolve Gap from Fill", Range(-5, 0.5)) = 0.02
		_EdgeWidth ("Edge Glow Width", Range(0.001, 0.5)) = 0.05
		[HDR] _EdgeColor ("Edge Color", Vector) = (1,0.7,0.2,1)
		_EdgeIntensity ("Edge Intensity", Range(0, 10)) = 2
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