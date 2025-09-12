Shader "Hidden/App UI/ColorWheel" {
	Properties {
		_CheckerColor1 ("Color 1", Vector) = (1,1,1,1)
		_CheckerColor2 ("Color 2", Vector) = (1,1,1,1)
		_CheckerSize ("Size", Float) = 10
		_Width ("Width", Float) = 200
		_Height ("Height", Float) = 200
		_InnerRadius ("Inner Radius", Range(0, 0.5)) = 0.4
		_Saturation ("Saturation", Range(0, 1)) = 1
		_Brightness ("Brightness", Range(0, 1)) = 1
		_Opacity ("Opacity", Range(0, 1)) = 1
		_AA ("Anti-Aliasing", Float) = 0.005
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
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

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
}