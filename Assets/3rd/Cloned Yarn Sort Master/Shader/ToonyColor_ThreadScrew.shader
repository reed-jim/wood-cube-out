Shader "ToonyColor_ThreadScrew" {
	Properties {
		[Header(BASE COLOR)] _Texture ("Texture", 2D) = "white" {}
		[Header(ROUGHNESS)] _RoughnessMap ("RoughnessMap", 2D) = "white" {}
		[Header(AMBIENT OCCLUSION)] _AO ("AO", 2D) = "white" {}
		[Header(NORMAL)] _NormalMap ("Normal Map", 2D) = "bump" {}
		_VertexOffset ("Vertex Offset", Range(-0.5, 0)) = 0
		_Tiling ("Tiling", Float) = 0
		_MainColor ("Main Color", Vector) = (0,0,0,0)
		_DarkThreadColor ("DarkThread Color", Vector) = (0,0,0,0)
		_AOcolor ("AO color", Vector) = (0,0,0,0)
		_Metallic ("Metallic", Float) = 0
		_RoughnessMultiplier ("Roughness Multiplier", Float) = 0
		[Header(SHADOW)] _ShadowColor ("Shadow Color", Vector) = (1,0.8915094,0.8915094,0)
		_MinShadow ("Min Shadow", Float) = 0.14
		_MaxShadow ("Max Shadow", Float) = 0.61
		[Header(HIGHLIGHT)] _Smoothness ("Highlight Smoothness", Range(0, 1)) = 0.5516343
		[HDR] _HighlightColor ("Highlight Color", Vector) = (1,1,1,0)
		_PaintLayer ("Paint Layer", Vector) = (0,0,0,0)
		[Header(ANIMATION)] _ScrollAlongV ("ScrollAlongV", Range(0, 5)) = 0
		_AOSHADOW ("AO SHADOW", Range(0, 1)) = 0
		[Toggle] _ThreadAnim ("Thread Anim", Float) = 0
		[Header(DISSOLVE)] _DissolveNoise ("Dissolve Noise", 2D) = "white" {}
		_Dissolve ("Dissolve", Range(0, 1)) = 0
		_Cutoff ("Mask Clip Value", Float) = 0.5
		[HideInInspector] _texcoord2 ("", 2D) = "white" {}
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