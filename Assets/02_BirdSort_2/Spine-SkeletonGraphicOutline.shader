Shader "Spine/SkeletonGraphicOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
        [Toggle(_CANVAS_GROUP_COMPATIBLE)] _CanvasGroupCompatible("CanvasGroup Compatible", Int) = 0
        _Color ("Tint", Color) = (1,1,1,1)

        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 100)) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
        
        LOD 100
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Fog
        {
            Mode Off
        }
        Blend SrcAlpha OneMinusSrcAlpha

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

        Outline vertOutline(VertexData vData, float2 offset)
        {
            Outline ol;
            ol.vertex = UnityObjectToClipPos(vData.vertex);
            ol.vertex.xy += offset * ol.vertex.w * _OutlineWidth / _ScreenParams.xy;
            ol.texcoord = TRANSFORM_TEX(vData.texcoord, _MainTex);
            return ol;
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

        Pass
        {
            CGPROGRAM
            #pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
            #pragma shader_feature _ _CANVAS_GROUP_COMPATIBLE
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"


            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                half2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            VertexOutput vert(VertexInput IN)
            {
                VertexOutput OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color;

                #ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1);
                #endif

                OUT.color = IN.color * float4(_Color.rgb * _Color.a, _Color.a);
                // Combine a PMA version of _Color with vertexColor.

                return OUT;
            }

            fixed4 frag(VertexOutput IN) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, IN.texcoord);

                #if defined(_STRAIGHT_ALPHA_INPUT)
				texColor.rgb *= texColor.a;
                #endif

                half4 color = (texColor + _TextureSampleAdd) * IN.color;
                #ifdef _CANVAS_GROUP_COMPATIBLE
				// CanvasGroup alpha sets vertex color alpha, but does not premultiply it to rgb components.
				color.rgb *= IN.color.a;
                #endif

                color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
