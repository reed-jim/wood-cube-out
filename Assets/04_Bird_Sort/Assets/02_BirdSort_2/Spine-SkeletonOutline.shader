Shader "Spine/SkeletonOutline" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		_OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 80)) = 30
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
	
		CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        float4 _MainTex_ST;
        fixed4 _OutlineColor;
        float _OutlineWidth;
        struct VertexData
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Outline
        {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
        };

        Outline vertOutline(VertexData vData, float2 dir)
        {
            Outline OUT;
            OUT.vertex = UnityObjectToClipPos(vData.vertex);
        	// ol.vertex.xy += offset * _OutlineWidth;
        	OUT.vertex.xy += (dir * _OutlineWidth * OUT.vertex.w) / _ScreenParams.xy;
            OUT.texcoord = TRANSFORM_TEX(vData.texcoord, _MainTex);
            return OUT;
        }

        fixed4 fragOutline(Outline _out) : SV_Target
        {
            fixed4 OUT = _OutlineColor;
            if (_OutlineWidth == 0.0)
                OUT = float4(0,0,0,0);
            OUT.a *= tex2D(_MainTex, _out.texcoord).a;
             
            return OUT;
        }
        ENDCG		
		
		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2( 0, -1));
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(0, 1));
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2( 1,0));
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(-1,0));
            }
            ENDCG
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(0.7071, 0.7071));
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(-0.7071, 0.7071));
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2( 0.7071,-0.7071));
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(-0.7071,-0.7071));
            }
            ENDCG
        }
        
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(-0.414214, 0.91018));
            }
            ENDCG
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(-0.91018, 0.414214));
            }
            ENDCG
        }
        
         Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(0.414214,0.91018));
            }
            ENDCG
        }
        
         Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(0.91018, 0.414214));
            }
            ENDCG
        }
        
         Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(0.91018, -0.414214));
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(0.414214, -0.91018));
            }
            ENDCG
        }
         Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(-0.91018, -0.414214));
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            Outline vert (VertexData vertData)
            {
                return vertOutline(vertData, float2(-0.414214, -0.91018));
            }
            ENDCG
        }
        
		
		Pass {
			Name "Normal"

			CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

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
				o.vertexColor = v.vertexColor;
				return o;
			}

			float4 frag (VertexOutput i) : SV_Target {
				float4 texColor = tex2D(_MainTex, i.uv);

				#if defined(_STRAIGHT_ALPHA_INPUT)
				texColor.rgb *= texColor.a;
				#endif

				return (texColor * i.vertexColor);
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
