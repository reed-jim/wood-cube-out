Shader "URP/UI_ChevronArrow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _ArrowColor ("Arrow Color", Color) = (0, 0.8, 1, 1)
        _BgColor ("Background Color", Color) = (0, 0, 0, 0.2)
        _Speed ("Animation Speed", Float) = 2.0
        _Scale ("Arrow Scale (Density)", Float) = 5.0
        _Spacing ("Spacing Between Arrows", Range(0.0, 0.9)) = 0.3

        // Required properties for UI Masking / Stencil compatibility
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _ZTest ("ZTest", Float) = 4
        _ZWrite ("ZWrite", Float) = 0
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

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [_ZTest]

        Pass
        {
            Name "UI_ChevronPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ArrowColor;
                float4 _BgColor;
                float _Speed;
                float _Scale;
                float _Spacing;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Scale horizontally and offset by time to scroll
                uv.x = uv.x * _Scale - _Time.y * _Speed;

                // Split horizontal UV into tile fractional part [0..1]
                float tileU = frac(uv.x);

                // Remap the arrow texture into a narrower slice of the tile [0 .. (1 - _Spacing)]
                float arrowWidth = 1.0 - _Spacing;
                float scaledU = tileU / arrowWidth;

                // Sample texture if within the arrow width; otherwise treat as empty gap
                float4 texColor = float4(0, 0, 0, 0);
                if (tileU < arrowWidth)
                {
                    texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(scaledU, uv.y));
                }

                // Blend arrow color based on texture alpha
                float4 finalColor = lerp(_BgColor, _ArrowColor * texColor, texColor.a);

                // Apply vertex tinting (Image component color)
                finalColor *= i.color;

                return finalColor;
            }
            ENDHLSL
        }
    }
}