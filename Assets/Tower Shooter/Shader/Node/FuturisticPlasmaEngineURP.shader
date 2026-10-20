Shader "UI/FuturisticPlasmaEngineURP"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Engine Cycle Progress", Range(0, 1)) = 0.0

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

            // Properties
            float4 _Color;
            float _Progress;

            #define PI 3.14159265359

            // Hash / Random helpers
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Distance field helpers
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }

            float sdBox(float2 p, float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            float sdSegment(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a, ba = b - a;
                float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
                return length(pa - ba * h);
            }

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
                // Correct aspect ratio so shapes are not stretched
                float2 uv = IN.uv;
                float2 p = (uv - 0.5) * float2(2.0 * 1.7777, 2.0); // Center at (0,0)

                // Phase remapping from _Progress
                float pIngress   = saturate((_Progress - 0.00) / 0.15);
                float pIgnition  = saturate((_Progress - 0.15) / 0.20);
                float pAccel     = saturate((_Progress - 0.35) / 0.25);
                float pPlume     = saturate((_Progress - 0.60) / 0.25);
                float pStable    = saturate((_Progress - 0.85) / 0.15);

                // Loop smoothing factor for continuous wrapping
                float loopFade = 1.0 - smoothstep(0.85, 1.0, _Progress) * (1.0 - smoothstep(0.0, 0.1, _Progress));

                // Color palettes
                float3 colIntake = float3(0.2, 0.8, 1.0);
                float3 colCore = float3(0.1, 0.9, 1.0);
                float3 colPlasma = float3(0.4, 0.2, 1.0);
                float3 colExhaust = float3(0.0, 0.6, 1.0);
                float3 colWhite = float3(1.0, 1.0, 1.0);

                float finalAlpha = 0.0;
                float3 finalColor = float3(0, 0, 0);

                // ----------------------------------------------------
                // 1. PROPELLANT INTAKE
                // ----------------------------------------------------
                if (pIngress > 0.0)
                {
                    float intakeMask = smoothstep(-1.2, -0.6, p.x) * (1.0 - smoothstep(-0.6, -0.5, p.x));
                    intakeMask *= smoothstep(0.4, 0.1, abs(p.y));
                    
                    float streamWave = sin(p.x * 15.0 - _Progress * 40.0);
                    float particles = step(0.7, frac(p.x * 10.0 - _Progress * 15.0)) * intakeMask;
                    
                    float3 intakeCol = colIntake * (particles * 2.0 + abs(streamWave) * 0.5) * pIngress;
                    finalColor += intakeCol;
                    finalAlpha += (intakeMask * 0.3 + particles * 0.7) * pIngress;
                }

                // ----------------------------------------------------
                // 2. REAR ENERGY CHAMBER & CORE
                // ----------------------------------------------------
                float2 chamberCenter = float2(-0.4, 0.0);
                float dChamber = length(p - chamberCenter);
                
                if (pIgnition > 0.0)
                {
                    float ringPattern = sin(dChamber * 30.0 - _Progress * 10.0);
                    float ringMask = smoothstep(0.4, 0.38, dChamber) * smoothstep(0.15, 0.18, dChamber);
                    
                    float coreGlow = 1.0 - smoothstep(0.0, 0.3, dChamber);
                    float pulsatingCore = coreGlow * (0.8 + 0.2 * sin(_Progress * 30.0));

                    float3 chamberCol = lerp(colPlasma, colCore, pulsatingCore) * pIgnition;
                    chamberCol += colWhite * max(0.0, ringPattern) * ringMask * pIgnition * 1.5;

                    finalColor += chamberCol;
                    finalAlpha += (coreGlow + ringMask * 0.5) * pIgnition;
                }

                // ----------------------------------------------------
                // 3 & 4. MAGNETIC CONTAINMENT FIELD & ACCELERATION CHANNEL
                // ----------------------------------------------------
                if (pAccel > 0.0)
                {
                    float2 fieldP = p - float2(-0.1, 0.0);
                    float ellipseDist = length(fieldP * float2(0.8, 2.2));
                    float fieldArc = smoothstep(0.52, 0.5, ellipseDist) * smoothstep(0.46, 0.48, ellipseDist);
                    
                    float angle = atan2(fieldP.y, fieldP.x);
                    float fieldPulse = sin(angle * 6.0 + _Progress * 25.0);
                    
                    float nozzleUpper = 0.35 * (1.0 - 0.5 * max(0.0, p.x / 0.8));
                    float nozzleMask = step(abs(p.y), nozzleUpper) * step(0.0, p.x) * step(p.x, 0.8);
                    
                    float3 fieldCol = colPlasma * (fieldArc * (0.5 + 0.5 * fieldPulse) * 2.0 + nozzleMask * 1.5) * pAccel;
                    finalColor += fieldCol;
                    finalAlpha += (fieldArc * 0.6 + nozzleMask * 0.4) * pAccel;
                }

                // ----------------------------------------------------
                // 5 & 6. EXHAUST PLUME & STABLE OPERATION
                // ----------------------------------------------------
                if (pPlume > 0.0)
                {
                    float xDist = p.x - 0.4;
                    if (xDist > 0.0)
                    {
                        float halfWidth = 0.25 + xDist * 0.35;
                        float plumeMask = smoothstep(halfWidth, 0.0, abs(p.y)) * smoothstep(1.5, 0.3, xDist);
                        
                        float streak = sin(p.y * 20.0 - xDist * 12.0 + _Progress * 50.0);
                        float particleSpark = step(0.8, random(floor(p * 20.0) + _Progress * 5.0)) * plumeMask;

                        float3 plumeCol = lerp(colExhaust, colWhite, smoothstep(0.0, 0.8, xDist)) * pPlume;
                        plumeCol += colCore * abs(streak) * plumeMask * 1.2;
                        plumeCol += colWhite * particleSpark * 2.0;

                        float alphaContribution = plumeMask * pPlume * (0.8 + 0.2 * pStable);
                        finalColor += plumeCol;
                        finalAlpha += alphaContribution;
                    }
                }

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