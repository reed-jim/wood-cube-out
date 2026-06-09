// Spine/Skeleton Tint
// - Two color tint
// - unlit
// - Premultiplied alpha blending (Optional straight alpha input)
// - No depth, no backface culling, no fog.

Shader "Spine/Skeleton Custom Saferio" {
	Properties {
		_Color ("Tint Color", Color) = (1,1,1,1)
		_Black ("Dark Color", Color) = (0,0,0,0)
		_OverlayColor ("Overlay Color", Color) = (1,1,1,1)
		_Saturation ("Saturation", Range(0,2)) = 1
		[NoScaleOffset] _MainTex ("MainTex", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.1
		[Toggle(_DARK_COLOR_ALPHA_ADDITIVE)] _DarkColorAlphaAdditive("Additive DarkColor.A", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass {
			Name "Normal"

			CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma shader_feature _ _DARK_COLOR_ALPHA_ADDITIVE
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "CGIncludes/Spine-Common.cginc"
			sampler2D _MainTex;
			float4 _Color;
			float4 _Black;
			float4 _OverlayColor;
			float _Saturation;

			struct VertexInput {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.vertexColor = PMAGammaToTargetSpace(v.vertexColor) * float4(_Color.rgb * _Color.a, _Color.a); // Combine a PMA version of _Color with vertexColor.
				return o;
			}

			#include "CGIncludes/Spine-Skeleton-Tint-Common.cginc"

			

			float3 OverlayBlend(float3 baseColor, float3 blendColor)
			{
				float3 result;

				result.r = (baseColor.r < 0.5)
					? (2.0 * baseColor.r * blendColor.r)
					: (1.0 - 2.0 * (1.0 - baseColor.r) * (1.0 - blendColor.r));

				result.g = (baseColor.g < 0.5)
					? (2.0 * baseColor.g * blendColor.g)
					: (1.0 - 2.0 * (1.0 - baseColor.g) * (1.0 - blendColor.g));

				result.b = (baseColor.b < 0.5)
					? (2.0 * baseColor.b * blendColor.b)
					: (1.0 - 2.0 * (1.0 - baseColor.b) * (1.0 - blendColor.b));

				return result;
			}

			float3 AdjustSaturation(float3 color, float saturation)
			{
				float luminance = dot(color, float3(0.299, 0.587, 0.114));
				return lerp(luminance.xxx, color, saturation);
			}

			float4 frag (VertexOutput i) : SV_Target {
				float4 texColor = tex2D(_MainTex, i.uv);

				// Overlay blend
				texColor.rgb = lerp(
					texColor.rgb,
					OverlayBlend(texColor.rgb, _OverlayColor.rgb),
					_OverlayColor.a
				);

				// Saturation adjustment
				texColor.rgb = AdjustSaturation(texColor.rgb, _Saturation);

				return fragTintedColor(
					texColor,
					_Black.rgb,
					i.vertexColor,
					_Color.a,
					_Black.a
				);
			}
			ENDCG
		}

		Pass {
			Name "Caster"
			Tags { "LightMode"="ShadowCaster" }
			Offset 1, 1
			ZWrite On
			ZTest LEqual

			Fog { Mode Off }
			Cull Off
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			fixed _Cutoff;

			struct VertexOutput {
				V2F_SHADOW_CASTER;
				float4 uvAndAlpha : TEXCOORD1;
			};

			VertexOutput vert (appdata_base v, float4 vertexColor : COLOR) {
				VertexOutput o;
				o.uvAndAlpha = v.texcoord;
				o.uvAndAlpha.a = vertexColor.a;
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			float4 frag (VertexOutput i) : SV_Target {
				fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
				clip(texcol.a * i.uvAndAlpha.a - _Cutoff);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
