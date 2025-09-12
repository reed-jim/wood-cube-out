Shader "Hidden/App UI/Box" {
	Properties {
		_Ratio ("Ratio", Float) = 1
		_Rect ("Rect", Vector) = (0,0,1,1)
		_Color ("Color", Vector) = (1,1,1,1)
		_BackgroundImage ("Background Image", 2D) = "black" {}
		_BackgroundImageTransform ("Background Image Transform", Vector) = (0,0,1,1)
		_BackgroundSize ("Background Size", Float) = 0
		_Radiuses ("Radiuses", Vector) = (0,0,0.5,0.25)
		_AA ("Anti Aliasing", Float) = 0.0012
		_Phase ("Phase", Vector) = (0,0,0,0)
		[Header(Border)] [Toggle(ENABLE_BORDER)] _EnableBorder ("Enable", Float) = 1
		_BorderThickness ("Border Thickness", Range(0, 1)) = 0.012
		_BorderColor ("Border Color", Vector) = (1,1,1,1)
		_BorderStyle ("Border Style", Float) = 0
		_BorderDotFactor ("Border Dot Factor", Float) = 3
		_BorderSpeed ("Border Speed", Float) = 0
		[Header(Shadow)] [Toggle(ENABLE_SHADOW)] _EnableShadow ("Enable", Float) = 1
		_ShadowOffset ("Shadow Offset", Vector) = (0.1,0.1,0.02,0.005)
		_ShadowColor ("Shadow Color", Vector) = (0,0,0,1)
		_ShadowInset ("Shadow Inset Mode", Float) = 0
		[Header(Outline)] [Toggle(ENABLE_OUTLINE)] _EnableOutline ("Enable", Float) = 1
		_OutlineThickness ("Outline Thickness", Range(0, 1)) = 0.012
		_OutlineColor ("Outline Color", Vector) = (1,1,1,1)
		_OutlineOffset ("Outline Offset", Range(0, 1)) = 0
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