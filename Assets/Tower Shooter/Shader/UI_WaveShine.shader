Shader "UI/URP/ContinuousWaveShine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)

        [Header(Shine Control)]
        _EffectAmount ("Shine Intensity", Range(0, 1)) = 1.0

        [Header(Continuous Wave Properties)]
        [HDR] _ShineColor ("Shine Color / Tint", Color) = (2.2, 2.0, 1.5, 1)
        _WaveDensity ("Wave Scale / Density", Range(1.0, 15.0)) = 5.0
        _WaveSpeed ("Flow Speed", Range(0.0, 5.0)) = 1.2
        _FlowAngle ("Flow Angle (Degrees)", Range(0, 360)) = 45.0
        
        [Header(Light Softness Contrast)]
        _MinBrightness ("Base Ambient Glow", Range(0.0, 1.0)) = 0.2
        _MaxBrightness ("Peak Wave Glow", Range(0.5, 3.0)) = 1.8
        _Distortion ("Liquid Distortion", Range(0.0, 1.0)) = 0.35

        // Canvas Masking Support
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
                float4 _ShineColor;
                float _EffectAmount;
                float _WaveDensity;
                float _WaveSpeed;
                float _FlowAngle;
                float _MinBrightness;
                float _MaxBrightness;
                float _Distortion;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Base UI Texture
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // 2. Rotate UVs for Angle Direction
                float rad = _FlowAngle * (3.14159265 / 180.0);
                float2 centeredUV = input.uv - 0.5;
                float flowAxis = centeredUV.x * cos(rad) - centeredUV.y * sin(rad);
                float crossAxis = centeredUV.x * sin(rad) + centeredUV.y * cos(rad);

                // 3. Continuous Multi-Layer Liquid Field Calculation
                float time = _Time.y * _WaveSpeed;
                
                // Primary smooth wave
                float wave1 = sin(flowAxis * _WaveDensity + time);
                
                // Secondary smooth cross-wave with warp
                float warp = cos(crossAxis * (_WaveDensity * 0.8) - time * 0.7) * _Distortion;
                float wave2 = cos((flowAxis + warp) * (_WaveDensity * 1.3) - time * 1.2);

                // Combine layers into a 0.0 - 1.0 continuous smooth curve (no zero-gaps)
                float rawWave = (wave1 + wave2) * 0.25 + 0.5; // Normalized to ~0 to 1
                
                // Remap wave from _MinBrightness to _MaxBrightness so light covers 100% of the surface
                float continuousShine = lerp(_MinBrightness, _MaxBrightness, rawWave);

                // 4. Color Compositing (Screen Blend + Additive Specular)
                float3 lightOverlay = _ShineColor.rgb * continuousShine * saturate(_EffectAmount);
                
                // Screen equation preserves white/bright UI regions without washing out
                float3 screenedImage = 1.0 - (1.0 - mainTex.rgb) * (1.0 - saturate(lightOverlay));

                // Additive highlight pass for bright wave crests
                float3 finalRGB = screenedImage + (lightOverlay * 0.25);

                // Seamlessly blend when _EffectAmount > 0
                finalRGB = lerp(mainTex.rgb, finalRGB, saturate(_EffectAmount));

                return float4(finalRGB, mainTex.a);
            }
            ENDHLSL
        }
    }
}
