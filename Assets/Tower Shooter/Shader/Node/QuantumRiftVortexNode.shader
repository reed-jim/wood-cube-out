Shader "UI/Custom/QuantumRiftVortexNode"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Vortex Core Color", Color) = (0.5, 0.0, 1.0, 1.0)
        _RimColor ("Dimensional Rim Color", Color) = (0.0, 1.0, 0.8, 1.0)
        _LockedColor ("Dormant Node Color", Color) = (0.15, 0.18, 0.2, 0.4)
        
        [Header(Node Control)]
        _IsUnlocked ("Node Unlocked", Range(0,1)) = 1
        [IntRange] _Power ("Power Level (1-10)", Range(1, 10)) = 1
        _Progress ("Jump Charge Progress", Range(0.0, 1.0)) = 0.0
        _SpeedMode ("Speed Mode Transition", Range(0.0, 1.0)) = 0.0
        _Layers ("Rift Layer Density", Range(3, 15)) = 8
        _EventHorizon ("Rift Core Radius", Range(0.05, 0.4)) = 0.2

        // Stencil / UI Canvas properties
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
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
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
            Name "UI_QuantumRiftVortexNode"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma shader_feature_local _USEUIALPHACLIP_ON

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float4 _LockedColor;
                float _IsUnlocked;
                float _Power;
                float _Progress;
                float _SpeedMode;
                float _Layers;
                float _EventHorizon;
            CBUFFER_END

            #define PI 3.14159265359
            #define TWO_PI 6.28318530718

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            // Evaluates pattern intensity, custom color palettes, and strange-matter distance warping per power level
            void evaluatePowerEffect(float2 st, int powerLevel, float timeVal, float rawDist, float angle, out float pattern, out float warpedDist, out float3 tintA, out float3 tintB)
            {
                pattern = 0.0;
                warpedDist = rawDist;
                tintA = _Color.rgb;
                tintB = _RimColor.rgb;

                if (powerLevel == 1)
                {
                    // 1. Clean Circular Rings (Standard Precise Geometry)
                    float rings = sin(rawDist * (_Layers * 2.0) - timeVal + angle * 2.0) * 0.5 + 0.5;
                    pattern = pow(rings, 3.0);
                    warpedDist = rawDist;
                    tintA = _Color.rgb;
                    tintB = _RimColor.rgb;
                }
                else if (powerLevel == 2)
                {
                    // 2. Clean Circular Spiral Galaxy
                    float spiral = angle * 2.0 + (rawDist * (_Layers * 1.5)) - timeVal;
                    pattern = pow(sin(spiral) * 0.5 + 0.5, 4.0);
                    warpedDist = rawDist;
                    tintA = float3(0.0, 0.3, 0.9);
                    tintB = float3(0.7, 0.9, 1.0);
                }
                else if (powerLevel == 3)
                {
                    // 3. Strange Matter: 6-Fold Crystalline Star Burst (Spills outward into sharp spikes)
                    float starWarp = rawDist + sin(angle * 6.0 - timeVal) * 0.12;
                    float star = abs(cos(angle * 3.0 - timeVal));
                    pattern = smoothstep(0.1, 0.9, saturate(star * (1.0 - starWarp * 1.8)));
                    warpedDist = starWarp;
                    tintA = float3(1.0, 0.4, 0.0);
                    tintB = float3(1.0, 0.9, 0.2);
                }
                else if (powerLevel == 4)
                {
                    // 4. Strange Matter: Hexagonal Fractal Web with Organic Distortion
                    float hexWarp = rawDist + cos(angle * 6.0 - timeVal) * 0.08 * sin(rawDist * 10.0);
                    float hex = cos(angle * 6.0 - timeVal) * sin(rawDist * 18.0 + timeVal * 0.5);
                    pattern = saturate(hex * 2.0 + 0.5);
                    warpedDist = hexWarp;
                    tintA = float3(0.0, 0.6, 0.2);
                    tintB = float3(0.2, 1.0, 0.6);
                }
                else if (powerLevel == 5)
                {
                    // 5. Clean Circular Quantum Radar Sweep
                    float sweep = sin(angle - timeVal * 1.5) * 0.5 + 0.5;
                    float ringWave = cos(rawDist * 25.0 - timeVal * 2.0) * 0.5 + 0.5;
                    pattern = pow(sweep * ringWave, 2.0) * 3.0;
                    warpedDist = rawDist;
                    tintA = float3(0.0, 0.4, 0.8);
                    tintB = float3(0.0, 1.0, 0.9);
                }
                else if (powerLevel == 6)
                {
                    // 6. Strange Matter: Asymmetric Plasma Shear Tear (Torn organic boundary)
                    float tearWarp = rawDist + sin(angle * 4.0 + timeVal) * cos(rawDist * 15.0 - timeVal) * 0.15;
                    float blade1 = sin(angle * 3.0 + rawDist * 12.0 - timeVal * 2.5);
                    float blade2 = cos(angle * -3.0 + rawDist * 12.0 - timeVal * 2.5);
                    pattern = pow(saturate(blade1 * blade2 + 0.5), 3.0);
                    warpedDist = tearWarp;
                    tintA = float3(0.9, 0.0, 0.2);
                    tintB = float3(1.0, 0.3, 0.7);
                }
                else if (powerLevel == 7)
                {
                    // 7. Strange Matter: Multi-Pointed Organic Amorphous Blob
                    float blobWarp = rawDist + sin(angle * 5.0 - timeVal * 2.0) * 0.18;
                    float portalSpikes = abs(sin(angle * 5.0 - timeVal * 2.0)) * saturate(1.2 - blobWarp * 2.0);
                    pattern = smoothstep(0.05, 0.8, portalSpikes);
                    warpedDist = blobWarp;
                    tintA = float3(0.4, 0.0, 1.0);
                    tintB = float3(0.7, 0.4, 1.0);
                }
                else if (powerLevel == 8)
                {
                    // 8. Clean Circular Cybernetic Iris Shutter
                    float irisGear = abs(sin(angle * 8.0 - timeVal * 1.5)) * cos(rawDist * 15.0 - timeVal);
                    pattern = pow(saturate(irisGear * 1.5), 2.0);
                    warpedDist = rawDist;
                    tintA = float3(0.3, 0.8, 0.0);
                    tintB = float3(0.9, 1.0, 0.1);
                }
                else if (powerLevel == 9)
                {
                    // 9. Strange Matter: Chaotic Solar Flares with Jagged Tentacle Fringe
                    float flareWarp = rawDist + sin(angle * 3.0 + sin(rawDist * 20.0 - timeVal)) * 0.2;
                    float flare = sin(angle * 4.0 + sin(flareWarp * 20.0 - timeVal * 3.0));
                    pattern = pow(saturate(flare * 0.5 + 0.5), 3.0);
                    warpedDist = flareWarp;
                    tintA = float3(1.0, 0.3, 0.0);
                    tintB = float3(1.0, 0.8, 0.1);
                }
                else 
                {
                    // 10. Strange Matter: Ultimate Liquid Quantum Singularity (Swirling amorphous void)
                    float voidWarp = rawDist + cos(angle * 7.0 - timeVal * 2.0) * 0.12 + sin(rawDist * 12.0 + timeVal) * 0.08;
                    float v1 = sin(angle * 6.0 + (voidWarp * 16.0) - (timeVal * 3.0));
                    float v2 = cos(angle * -4.0 - (voidWarp * 12.0) + (timeVal * 2.0));
                    pattern = pow(saturate((v1 + v2) * 0.5 + 0.5), 4.0);
                    warpedDist = voidWarp;
                    tintA = float3(1.0, 0.0, 0.5);
                    tintB = float3(0.0, 1.0, 0.9);
                }
            }

            half4 renderVortexNode(float2 st, float isUnlocked, int powerLevel)
            {
                float activeProgress = isUnlocked > 0.5 ? _Progress : 0.0;
                float hyperBlend = saturate(_SpeedMode);
                float timeVal = activeProgress * TWO_PI * 12.0;

                // Dynamic Core Pulsation / Breathing Bloom factor
                float pulse = sin(activeProgress * TWO_PI * 4.0 + _Time.y * 3.0) * 0.5 + 0.5;
                float dynamicHorizon = _EventHorizon + (pulse * 0.02 * (hyperBlend + 0.5));

                // Chromatic Aberration RGB channel splitting vector offsets
                float aberrationStrength = (hyperBlend * 0.04) + (activeProgress * 0.015);
                float2 dir = normalize(st + 0.0001);
                
                float2 stR = st + dir * aberrationStrength;
                float2 stG = st;
                float2 stB = st - dir * aberrationStrength;

                float rawDistR = length(stR), rawDistG = length(stG), rawDistB = length(stB);
                float angleR = atan2(stR.y, stR.x), angleG = atan2(st.y, st.x), angleB = atan2(stB.y, stB.x);

                float3 finalColor = float3(0,0,0);
                float alpha = 0;

                if (isUnlocked > 0.5)
                {
                    // Evaluate unique patterns, warped shapes, and palettes per channel
                    float patR, patG, patB;
                    float warpedDistR, warpedDistG, warpedDistB;
                    float3 tintR_A, tintR_B, tintG_A, tintG_B, tintB_A, tintB_B;
                    
                    evaluatePowerEffect(stR, powerLevel, timeVal, rawDistR, angleR, patR, warpedDistR, tintR_A, tintR_B);
                    evaluatePowerEffect(stG, powerLevel, timeVal, rawDistG, angleG, patG, warpedDistG, tintG_A, tintG_B);
                    evaluatePowerEffect(stB, powerLevel, timeVal, rawDistB, angleB, patB, warpedDistB, tintB_A, tintB_B);

                    float coreVoidG = smoothstep(dynamicHorizon - 0.03, dynamicHorizon + 0.03, warpedDistG);
                    float photonRimG = smoothstep(0.03, 0.0, abs(warpedDistG - dynamicHorizon));

                    // Blend colors per channel using the unique power palettes
                    float3 colR = lerp(tintR_A, tintR_B, patR);
                    float3 colG = lerp(tintG_A, tintG_B, patG);
                    float3 colB = lerp(tintB_A, tintB_B, patB);

                    float3 normalStateColor = float3(colR.r, colG.g, colB.b) * 2.5;
                    normalStateColor += tintG_B * photonRimG * 4.0;

                    // --- UNIFORM HIGH-SPEED STATE (ACROSS ALL POWERS) ---
                    float hyperspaceTunnel = sin(warpedDistG * 40.0 - (timeVal * 3.0)) * 0.5 + 0.5;
                    float whiteHotCore = smoothstep(0.3, 0.0, warpedDistG);
                    float3 highSpeedColor = lerp(_RimColor.rgb, float3(1.0, 1.0, 1.0), hyperspaceTunnel);
                    highSpeedColor += highSpeedColor * (2.0 + whiteHotCore * (3.0 + pulse));

                    // --- SMOOTH TRANSITION BETWEEN STATES ---
                    finalColor = lerp(normalStateColor, highSpeedColor, hyperBlend);
                    alpha = smoothstep(0.48, 0.44, warpedDistG) * saturate((1.0 - coreVoidG) + photonRimG + 0.3);
                }
                else
                {
                    // --- DORMANT LOCKED NODE ---
                    float dist = length(st);
                    float dormantRing = smoothstep(0.46, 0.44, dist) * smoothstep(0.38, 0.4, dist);
                    finalColor = _LockedColor.rgb * dormantRing;
                    alpha = dormantRing * _LockedColor.a;
                }

                return half4(finalColor, alpha);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 localUV = input.uv - 0.5;
                half4 nodeCol = renderVortexNode(localUV, _IsUnlocked, int(_Power));

                float nodeDist = length(localUV);
                if (nodeDist > 0.48) 
                    nodeCol = half4(0,0,0,0);

                nodeCol.rgb *= input.color.rgb;
                nodeCol.a *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(nodeCol.a - 0.001);
                #endif

                return nodeCol;
            }
            ENDHLSL
        }
    }
}