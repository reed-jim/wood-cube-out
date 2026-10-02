Shader "UI/URP/HolographicCard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)

        [Header(Holographic Control)]
        _HoloBlend ("Holo Effect Blend", Range(0, 1)) = 1.0
        _HoloBrightness ("Holo Intensity", Range(0.5, 4.0)) = 2.0

        [Header(Iridescence Spectrum)]
        _Frequency ("Rainbow Spectrum Frequency", Range(1.0, 20.0)) = 6.0
        _MotionSpeed ("Color Shift Speed", Range(0.0, 5.0)) = 1.0
        _FoilAngle ("Shimmer Angle (Degrees)", Range(0, 360)) = 60.0

        [Header(Foil Pattern Lines)]
        _LineDensity ("Holo Stripe Density", Range(5.0, 100.0)) = 35.0
        _LineContrast ("Stripe Sharpness", Range(1.0, 10.0)) = 4.0

        [Header(Edge Glow)]
        [HDR] _EdgeGlowColor ("Outer Edge Tint", Color) = (2.0, 2.0, 3.5, 1)
        _EdgeWidth ("Edge Glow Width", Range(0.0, 0.3)) = 0.08

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
                float4 _EdgeGlowColor;
                float _HoloBlend;
                float _HoloBrightness;
                float _Frequency;
                float _MotionSpeed;
                float _FoilAngle;
                float _LineDensity;
                float _LineContrast;
                float _EdgeWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            // Spectral iridescence color generator
            float3 SpectralRainbow(float t)
            {
                return 0.5 + 0.5 * cos(6.28318 * (t + float3(0.0, 0.33, 0.67)));
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Base UI Texture
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // 2. Angle Projection for Foil Lines
                float rad = _FoilAngle * (3.14159265 / 180.0);
                float2 centeredUV = input.uv - 0.5;
                float projectedPos = centeredUV.x * cos(rad) - centeredUV.y * sin(rad);

                // 3. Holographic Spectrum Calculation
                float timeOffset = _Time.y * _MotionSpeed;
                float rainbowPhase = (projectedPos * _Frequency) + timeOffset;
                float3 rainbowCol = SpectralRainbow(rainbowPhase) * _HoloBrightness;

                // 4. Micro-Foil Diffraction Lines
                float stripePattern = sin((projectedPos + timeOffset * 0.2) * _LineDensity);
                float foilShimmer = pow(saturate(stripePattern * 0.5 + 0.5), _LineContrast);

                // 5. Border Rim Luminescence
                float distFromCenter = max(abs(input.uv.x - 0.5), abs(input.uv.y - 0.5));
                float rimGlow = smoothstep(0.5 - _EdgeWidth, 0.5, distFromCenter);

                // 6. Direct Screen/Overlay Blend Mode
                // Calculates holographic color independently of base image brightness
                float3 holoLayer = (rainbowCol * foilShimmer) + (_EdgeGlowColor.rgb * rimGlow);
                
                // Screen Blend Equation: 1 - (1 - Base) * (1 - Effect)
                // Ensures rainbow sheen is prominent even on pure white or pure black surfaces
                float3 screenedHolo = 1.0 - (1.0 - mainTex.rgb) * (1.0 - saturate(holoLayer));

                // Additive punch for specular highlight intensity
                float3 finalHoloCard = screenedHolo + (holoLayer * 0.3);

                // Transition smoothly when _HoloBlend > 0
                float blendFactor = saturate(_HoloBlend);
                float3 finalRGB = lerp(mainTex.rgb, finalHoloCard, blendFactor);

                return float4(finalRGB, mainTex.a);
            }
            ENDHLSL
        }
    }
}
