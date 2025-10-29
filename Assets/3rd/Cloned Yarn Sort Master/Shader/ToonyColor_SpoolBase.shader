Shader "ToonyColor_SpoolBase" {
	Properties {
		_MainColor ("Main Color", Vector) = (0,0,0,0)
		[Header(SHADOW)] _ShadowColor ("Shadow Color", Vector) = (1,0.8915094,0.8915094,0)
		_MinShadow ("Min Shadow", Float) = 0.14
		_MaxShadow ("Max Shadow", Float) = 0.61
		_PaintLayer ("Paint Layer", Vector) = (0,0,0,0)
		_TextureSample0 ("Texture Sample 0", 2D) = "white" {}
		[Toggle(_USINGGRADIENT_ON)] _USINGGRADIENT ("USING GRADIENT", Float) = 0
		_Opacity ("Opacity", Range(0, 1)) = 0
		[HDR] _HighlightColor ("Highlight Color", Vector) = (1,1,1,0)
		_HighlightMax ("HighlightMax", Float) = 0
		_HighlightMin ("HighlightMin", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] __dirty ("", Float) = 1
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
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}