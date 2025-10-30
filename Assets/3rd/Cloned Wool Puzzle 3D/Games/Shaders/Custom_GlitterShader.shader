Shader "Custom/GlitterShader" {
	Properties {
		_Color ("Tint", Vector) = (0.5,0.5,0.5,1)
		_ShadowColor ("Shadow Color", Vector) = (0,0,0,1)
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_NoiseSize ("Noise Size", Float) = 2
		_ShiningSpeed ("Shining Speed", Float) = 0.1
		_SparkleColor ("sparkle Color", Vector) = (1,1,1,1)
		SparklePower ("sparkle Power", Float) = 10
		_Specular ("Specular", Range(0, 1)) = 0.5
		_Gloss ("Gloss", Range(0, 1)) = 0.5
		_RimColor ("Rim Color", Vector) = (0.17,0.36,0.81,0)
		_RimPower ("Rim Power", Range(0.6, 36)) = 8
		_RimIntensity ("Rim Intensity", Range(0, 100)) = 1
		_specsparkleRate ("Specular sparkle Rate", Float) = 6
		_rimsparkleRate ("Rim sparkle Rate", Float) = 10
		_diffsparkleRate ("Diffuse sparkle Rate", Float) = 1
		_ParallaxMap ("Parallax Map", 2D) = "white" {}
		_HeightFactor ("Height Scale", Range(-1, 1)) = 0.05
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

			fixed4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "VertexLit"
}