Shader "UI/URP/CanvasGradientToTextureTransparent"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1,1,1,1)

        _GradientColor ("Gradient Color", Color) = (1,0,0,1)
        _GradientStart ("Gradient Start", Range(0,1)) = 0
        _GradientEnd ("Gradient End", Range(0,1)) = 1

        // 0 = Left -> Right
        // 90 = Bottom -> Top
        // 180 = Right -> Left
        // 270 = Top -> Bottom
        _GradientAngle ("Gradient Angle", Range(0,360)) = 0

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use UI Alpha Clip", Float) = 0

        // UI Stencil
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIGradient"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 2.0

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color       : COLOR;
                float2 uv         : TEXCOORD0;

                #if defined(UNITY_UI_CLIP_RECT)
                float4 worldPosition : TEXCOORD1;
                #endif
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;
                float4 _GradientColor;

                float _GradientStart;
                float _GradientEnd;

                float _GradientAngle;

                float4 _ClipRect;

            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;

                #if defined(UNITY_UI_CLIP_RECT)
                OUT.worldPosition = IN.positionOS;
                #endif

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample original UI texture
                half4 texColor = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    IN.uv
                );

                // --------------------------------------------------
                // Calculate gradient direction
                // --------------------------------------------------

                float angle = radians(_GradientAngle);

                float2 direction = float2(
                    cos(angle),
                    sin(angle)
                );

                float2 centeredUV = IN.uv - 0.5;

                float gradientPosition =
                    dot(centeredUV, direction) + 0.5;

                // --------------------------------------------------
                // Gradient range
                // --------------------------------------------------

                float range = max(
                    _GradientEnd - _GradientStart,
                    0.0001
                );

                float gradient = saturate(
                    (gradientPosition - _GradientStart) / range
                );

                gradient = smoothstep(
                    0.0,
                    1.0,
                    gradient
                );

                // --------------------------------------------------
                // Gradient Color -> Texture
                //
                // gradient = 0:
                //     100% Gradient Color
                //
                // gradient = 1:
                //     100% Texture
                // --------------------------------------------------

                half4 result = lerp(
                    _GradientColor,
                    texColor,
                    gradient
                );

                // Apply UI vertex tint
                result *= IN.color;

                // --------------------------------------------------
                // UI clipping
                // --------------------------------------------------

                #if defined(UNITY_UI_CLIP_RECT)

                float2 inside =
                    step(_ClipRect.xy, IN.worldPosition.xy) *
                    step(IN.worldPosition.xy, _ClipRect.zw);

                result.a *= inside.x * inside.y;

                #endif

                // --------------------------------------------------
                // Alpha clipping
                // --------------------------------------------------

                #if defined(UNITY_UI_ALPHACLIP)

                clip(result.a - 0.001);

                #endif

                return result;
            }

            ENDHLSL
        }
    }
}
