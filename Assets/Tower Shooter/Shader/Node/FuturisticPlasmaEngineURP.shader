Shader "UI/FuturisticPlasmaEngineDirectURP"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Engine Cycle Progress", Range(0, 1)) = 0.0
        _Overdrive ("High Speed Overdrive State", Range(0, 1)) = 0.0
        
        // Customization properties for powerful visual tuning
        _CoreIntensity ("Core Intensity", Range(0.5, 3.0)) = 1.5
        _PlasmaColor ("Plasma Color", Color) = (0.0, 0.85, 1.0, 1.0)
        _CoreColor ("Core Color", Color) = (0.8, 0.2, 1.0, 1.0)

        // Required UI properties for Stencil and Mask support
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
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
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            float4 _Color;
            float _Progress;
            float _Overdrive;
            float _CoreIntensity;
            float4 _PlasmaColor;
            float4 _CoreColor;

            #define PI 3.14159265359

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = TransformObjectToHClip(OUT.worldPosition.xyz);
                OUT.uv = v.uv;
                OUT.color = v.color * _Color;
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float2 p = (IN.uv - 0.5) * 2.0;
                float r = length(p);
                float angle = atan2(p.y, p.x);

                float3 colWhite = float3(1.0, 1.0, 1.0);
                float3 colGold = float3(1.0, 0.5, 0.1);

                float3 finalColor = float3(0, 0, 0);
                float finalAlpha = 0.0;

                // ====================================================
                // STATE A: NORMAL HIGH-IMPACT CYCLE PROGRESSION
                // ====================================================
                if (_Overdrive < 0.5)
                {
                    float pIntake  = saturate(_Progress / 0.25);
                    float pCharge  = saturate((_Progress - 0.25) / 0.45);
                    float pBurst   = saturate((_Progress - 0.7) / 0.3);

                    // 1. High-velocity multi-tier intake vortex
                    if (_Progress < 0.7)
                    {
                        float ringDist = abs(r - 0.72);
                        float ringMask = smoothstep(0.1, 0.0, ringDist) * smoothstep(1.0, 0.15, r);
                        float spiral = sin(angle * 14.0 + r * 22.0 - _Progress * 45.0);
                        float secondarySpiral = cos(angle * 8.0 - r * 15.0 + _Progress * 30.0);
                        float intakeGlow = ringMask * max(0.0, spiral + secondarySpiral * 0.5) * pIntake;

                        finalColor += _PlasmaColor.rgb * intakeGlow * 2.5 * _CoreIntensity;
                        finalAlpha += intakeGlow * 0.8;
                    }

                    // 2. Powerful magnetic compression & hyper-core
                    if (_Progress >= 0.2 && _Progress < 0.95)
                    {
                        float coilDist = abs(r - 0.42);
                        float coilMask = smoothstep(0.06, 0.0, coilDist);
                        float coilPulse = sin(angle * 10.0 + _Progress * 60.0);

                        float coreRadius = 0.3 * (1.0 - 0.3 * pCharge);
                        float coreGlow = 1.0 - smoothstep(0.0, coreRadius, r);
                        float corePulse = coreGlow * (1.0 + 0.5 * sin(_Progress * 70.0));

                        float3 coreCol = lerp(_CoreColor.rgb, _PlasmaColor.rgb, pCharge) * corePulse * 2.0 * _CoreIntensity;
                        coreCol += colWhite * coilMask * max(0.0, coilPulse) * pCharge * 3.5;

                        finalColor += coreCol;
                        finalAlpha += (coreGlow * 1.3 + coilMask) * saturate(pCharge * 2.0);
                    }

                    // 3. Explosive power generation discharge burst
                    if (pBurst > 0.0)
                    {
                        float waveRadius = pBurst * 1.4;
                        float waveDist = abs(r - waveRadius);
                        float waveMask = smoothstep(0.2, 0.0, waveDist) * smoothstep(1.4, 0.0, r);
                        float flash = (1.0 - smoothstep(0.0, 0.5, r)) * pBurst;

                        float3 burstCol = lerp(_PlasmaColor.rgb, colWhite, pBurst) * (waveMask * 5.0 + flash * 4.0) * _CoreIntensity;
                        finalColor += burstCol;
                        finalAlpha += (waveMask + flash) * pBurst * 1.5;
                    }
                }
                // ====================================================
                // STATE B: HIGH-SPEED OVERDRIVE / RAPID INTERVAL STATE
                // (Features high-intensity idle animation driven by fractional progress)
                // ====================================================
                else
                {
                    // Derive smooth high-speed idle animation entirely from progress fraction
                    float idleAnim = frac(_Progress * 15.0) * 2.0 * PI;

                    float coreGlow = 1.0 - smoothstep(0.0, 0.45, r);
                    float vortex = sin(angle * 16.0 - r * 35.0 + idleAnim * 3.0);
                    float ringDist = abs(r - 0.52);
                    float ringMask = smoothstep(0.08, 0.0, ringDist);
                    float electricArcs = step(0.75, sin(angle * 12.0 + idleAnim * 5.0));

                    float3 overdriveCol = lerp(_PlasmaColor.rgb, colGold, abs(vortex)) * coreGlow * 3.0 * _CoreIntensity;
                    overdriveCol += colWhite * ringMask * (1.5 + electricArcs * 2.0);

                    finalColor += overdriveCol;
                    finalAlpha += (coreGlow * 1.5 + ringMask * 1.0);
                }

                // Powerful global reactor containment aura
                float outerGlow = (1.0 - smoothstep(0.0, 0.9, r)) * 0.3;
                finalAlpha = max(finalAlpha, outerGlow);

                // Global clamp & UI integration
                finalColor = saturate(finalColor);
                finalAlpha = saturate(finalAlpha * IN.color.a);

                float4 output = float4(finalColor * IN.color.rgb, finalAlpha);

                #if defined(UNITY_UI_CLIP_RECT)
                output.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #if defined(UNITY_UI_ALPHACLIP)
                clip(output.a - 0.001);
                #endif

                return output;
            }
            ENDHLSL
        }
    }
}