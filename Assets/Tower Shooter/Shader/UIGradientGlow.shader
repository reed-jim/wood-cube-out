Shader "UI/GradientGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _ColorLeft  ("Left Color", Color) = (0.1, 0.4, 1, 1)
        _ColorRight ("Right Color", Color) = (0.8, 0.1, 1, 1)

        _GlowColor ("Glow Color", Color) = (0.2, 0.5, 1, 1)
        _GlowSize ("Glow Size", Range(0, 0.25)) = 0.08
        _GlowStrength ("Glow Strength", Range(0, 5)) = 1.5

        _Brightness ("Brightness", Range(0.1, 3)) = 1

        [Toggle] _UseUIClipRect ("UI Clip Rect", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
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

        Pass
        {
            Name "UIGradientGlow"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

            float4 _ColorLeft;
            float4 _ColorRight;
            float4 _GlowColor;

            float _GlowSize;
            float _GlowStrength;
            float _Brightness;

            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    IN.uv
                );

                // Horizontal gradient
                half3 gradient = lerp(
                    _ColorLeft.rgb,
                    _ColorRight.rgb,
                    IN.uv.x
                );

                gradient *= _Brightness;

                // Approximate glow from the sprite alpha.
                // Works with UI Image sprites with transparent borders.
                float2 pixel = float2(
                    max(fwidth(IN.uv.x), 0.0001),
                    max(fwidth(IN.uv.y), 0.0001)
                );

                float2 offset = pixel * (_GlowSize * 40.0);

                float alpha = tex.a;

                float a1 = SAMPLE_TEXTURE2D(
                    _MainTex, sampler_MainTex,
                    IN.uv + float2(offset.x, 0)
                ).a;

                float a2 = SAMPLE_TEXTURE2D(
                    _MainTex, sampler_MainTex,
                    IN.uv - float2(offset.x, 0)
                ).a;

                float a3 = SAMPLE_TEXTURE2D(
                    _MainTex, sampler_MainTex,
                    IN.uv + float2(0, offset.y)
                ).a;

                float a4 = SAMPLE_TEXTURE2D(
                    _MainTex, sampler_MainTex,
                    IN.uv - float2(0, offset.y)
                ).a;

                float surroundingAlpha =
                    max(max(a1, a2), max(a3, a4));

                // Only show glow outside the button.
                float glow = saturate(
                    surroundingAlpha - alpha
                );

                glow *= _GlowStrength;

                half3 finalColor =
                    gradient * alpha +
                    _GlowColor.rgb * glow;

                half finalAlpha =
                    saturate(alpha + glow * _GlowColor.a);

                return half4(
                    finalColor,
                    finalAlpha
                ) * IN.color;
            }

            ENDHLSL
        }
    }
}
