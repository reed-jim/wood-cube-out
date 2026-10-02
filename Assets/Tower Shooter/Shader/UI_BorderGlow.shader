Shader "UI/URP/BorderGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)

        [Header(Glow Control)]
        _EffectAmount ("Glow Intensity", Range(0, 1)) = 1.0

        [Header(Border Dimensions)]
        _BorderWidth ("Border Thickness", Range(0.001, 0.2)) = 0.04
        _GlowSoftness ("Glow Edge Softness", Range(0.001, 0.3)) = 0.08
        _CornerRadius ("Corner Roundness", Range(0.0, 0.5)) = 0.1

        [Header(Glow Colors)]
        [HDR] _GlowColor ("Glow Color", Color) = (0.2, 1.8, 3.5, 1)
        _FillAlpha ("Inner Fill Opacity", Range(0.0, 1.0)) = 1.0

        [Header(Pulse Dynamics)]
        _PulseSpeed ("Pulse Speed", Range(0.0, 10.0)) = 2.0
        _PulseMin ("Pulse Min Scale", Range(0.2, 1.0)) = 0.6

        // UI Canvas Masking Support
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
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _GlowColor;
                float _EffectAmount;
                float _BorderWidth;
                float _GlowSoftness;
                float _CornerRadius;
                float _FillAlpha;
                float _PulseSpeed;
                float _PulseMin;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            // Signed Distance Function (SDF) for rounded rectangles
            float sdRoundedBox(float2 p, float2 b, float4 r)
            {
                r.xy = (p.x > 0.0) ? r.xy : r.zw;
                r.x  = (p.y > 0.0) ? r.x  : r.y;
                float2 q = abs(p) - b + r.x;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Base UI Texture
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // 2. Pulse Animation Factor
                float pulse = 1.0;
                if (_PulseSpeed > 0.0)
                {
                    pulse = lerp(_PulseMin, 1.0, (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5));
                }

                // 3. Rounded Box Distance Field
                float2 centeredPos = input.uv - 0.5;
                float2 boxSize = float2(0.5, 0.5);
                float dist = sdRoundedBox(centeredPos, boxSize, float4(_CornerRadius, _CornerRadius, _CornerRadius, _CornerRadius));

                // 4. Calculate Inner & Outer Border Glow Mask
                float outerGlow = smoothstep(_GlowSoftness, -_BorderWidth, dist);
                float innerGlow = smoothstep(-_BorderWidth - _GlowSoftness, -_BorderWidth, dist);
                float borderMask = outerGlow - innerGlow;

                // 5. Composite Output
                float3 activeGlowColor = _GlowColor.rgb * pulse * saturate(_EffectAmount);
                
                // Combine main image interior with border glow
                float3 glowedRGB = lerp(mainTex.rgb * _FillAlpha, activeGlowColor, borderMask);
                glowedRGB += activeGlowColor * borderMask * 0.5; // Extra additive highlight on glow peak

                // Alpha mask keeps rounded corners clean
                float shapeAlpha = smoothstep(0.01, -0.01, dist) * mainTex.a;

                // Smooth blend between normal UI Image and Border Glow state
                float3 finalRGB = lerp(mainTex.rgb, glowedRGB, saturate(_EffectAmount));

                return float4(finalRGB, shapeAlpha);
            }
            ENDHLSL
        }
    }
}
