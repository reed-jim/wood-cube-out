Shader "Hidden/App UI/LinearProgress" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_InnerRadius ("Inner Radius", Float) = 0
		_Start ("Start", Float) = 0
		_End ("End", Float) = 0
		_BufferStart ("Buffer Start", Float) = 0
		_BufferEnd ("Buffer End", Float) = 0
		_BufferOpacity ("Buffer Opacity", Float) = 0.1
		_AA ("Anti-Aliasing", Float) = 0.005
		_Phase ("Phase", Vector) = (0,0,0,0)
		_Ratio ("Ratio", Float) = 1
		_Padding ("Padding", Float) = 1
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
}