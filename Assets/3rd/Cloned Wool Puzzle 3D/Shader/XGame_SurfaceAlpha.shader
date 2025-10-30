Shader "XGame/SurfaceAlpha" {
	Properties {
		[Header(Main Settings)] _MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Tint Color", Vector) = (1,1,1,1)
		[Header(Lighting Settings)] _LightIntensity ("Light Intensity", Range(0, 2)) = 0.5
		_ShininessPinky ("Shininess", Range(1, 100)) = 20
		[Header(Specular Settings)] _Speculars ("Specular Color", Vector) = (1,1,1,1)
		_SpecularStrength ("Specular Strength", Range(0, 1)) = 1
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
	Fallback "Legacy Shaders/Transparent/Diffuse"
}