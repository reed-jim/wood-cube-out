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
        [IntRange] _Power ("Power Level (1-6)", Range(1, 6)) = 1
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

            half4 renderVortexNode(float2 st, float isUnlocked, int powerLevel)
            {
                float dist = length(st);
                float angle = atan2(st.y, st.x);

                float activeProgress = isUnlocked > 0.5 ? _Progress : 0.0;
                float hyperBlend = saturate(_SpeedMode);
                float timeVal = activeProgress * TWO_PI * 10.0;

                float3 finalColor = float3(0,0,0);
                float alpha = 0;

                if (isUnlocked > 0.5)
                {
                    float coreVoid = smoothstep(_EventHorizon - 0.02, _EventHorizon + 0.05, dist);
                    float photonRim = smoothstep(0.02, 0.0, abs(dist - _EventHorizon));

                    float3 normalStateColor = float3(0,0,0);

                    // --- 6 DISTINCT POWER EFFECTS (NORMAL STATE) ---
                    if (powerLevel == 1)
                    {
                        // Power 1: Concentric Pulsing Rings (Subdued Energy)
                        float rings = sin(dist * (_Layers * 1.5) - timeVal) * 0.5 + 0.5;
                        normalStateColor = lerp(_Color.rgb, _RimColor.rgb, rings) * (1.0 - coreVoid) * 0.9;
                        normalStateColor += _RimColor.rgb * photonRim * 1.2;
                    }
                    else if (powerLevel == 2)
                    {
                        // Power 2: Dual Spiral Arms (Standard Vortex)
                        float spiral = angle * 2.0 + (dist * _Layers) - timeVal;
                        float arms = sin(spiral) * 0.5 + 0.5;
                        normalStateColor = lerp(_Color.rgb, _RimColor.rgb, arms) * (1.0 - coreVoid) * 1.5;
                        normalStateColor += _RimColor.rgb * photonRim * 2.5;
                    }
                    else if (powerLevel == 3)
                    {
                        // Power 3: Geometric Starburst / Multi-Node Lattice Matrix
                        float star = abs(sin(angle * 4.0 + timeVal)) * (1.0 - dist * 2.0);
                        float ripple = cos(dist * 25.0 - timeVal * 1.5) * 0.5 + 0.5;
                        float lattice = saturate(star + ripple);
                        normalStateColor = lerp(_Color.rgb, float3(1.0, 0.8, 0.2), lattice) * (1.0 - coreVoid) * 2.0;
                        normalStateColor += float3(1.0, 0.9, 0.4) * photonRim * 4.0;
                    }
                    else if (powerLevel == 4)
                    {
                        // Power 4: Triangular Prismatic Hex-Grid Lattice
                        float prism = sin(angle * 3.0 - timeVal * 2.0) * cos(dist * 15.0 + timeVal);
                        float prismPattern = saturate(prism * 0.5 + 0.5);
                        normalStateColor = lerp(_RimColor.rgb, float3(0.2, 1.0, 0.5), prismPattern) * (1.0 - coreVoid) * 1.8;
                        normalStateColor += float3(0.4, 1.0, 0.6) * photonRim * 3.5;
                    }
                    else if (powerLevel == 5)
                    {
                        // Power 5: High-Frequency Quantum Ripple Waves
                        float rippleWave = sin(dist * 35.0 - timeVal * 3.0) * 0.5 + 0.5;
                        float angularPulse = cos(angle * 6.0 + timeVal) * 0.5 + 0.5;
                        float wavePattern = saturate(rippleWave * angularPulse);
                        normalStateColor = lerp(_Color.rgb, float3(0.1, 0.6, 1.0), wavePattern) * (1.0 - coreVoid) * 2.2;
                        normalStateColor += float3(0.3, 0.8, 1.0) * photonRim * 4.5;
                    }
                    else 
                    {
                        // Power 6 (Max): Overclocked Singularity Core with Multi-Layer Shear
                        float s1 = sin(angle * 4.0 + dist * 10.0 - timeVal * 2.0);
                        float s2 = cos(angle * -4.0 + dist * 10.0 - timeVal * 2.0);
                        float singularityPattern = saturate(s1 * s2 + 0.5);
                        normalStateColor = lerp(float3(1.0, 0.2, 0.5), _Color.rgb, singularityPattern) * (1.0 - coreVoid) * 2.5;
                        normalStateColor += float3(1.0, 0.4, 0.7) * photonRim * 5.0;
                    }

                    // --- UNIFORM HIGH-SPEED STATE (ACROSS ALL POWERS) ---
                    float hyperspaceTunnel = sin(dist * 40.0 - (timeVal * 3.0)) * 0.5 + 0.5;
                    float whiteHotCore = smoothstep(0.3, 0.0, dist);
                    float3 highSpeedColor = lerp(_RimColor.rgb, float3(1.0, 1.0, 1.0), hyperspaceTunnel);
                    highSpeedColor += highSpeedColor * (1.5 + whiteHotCore * 2.5);

                    // --- SMOOTH TRANSITION BETWEEN STATES ---
                    finalColor = lerp(normalStateColor, highSpeedColor, hyperBlend);
                    alpha = smoothstep(0.48, 0.45, dist) * saturate((1.0 - coreVoid) + photonRim + 0.2);
                }
                else
                {
                    // --- DORMANT LOCKED NODE ---
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