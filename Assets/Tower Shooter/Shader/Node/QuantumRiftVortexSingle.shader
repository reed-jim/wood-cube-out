Shader "UI/Custom/QuantumRiftVortexSingle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Vortex Core Color", Color) = (0.5, 0.0, 1.0, 1.0)
        _RimColor ("Dimensional Rim Color", Color) = (0.0, 1.0, 0.8, 1.0)
        
        [Header(Vortex Control)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 10.0
        _RotationAngle ("Rotation Angle", Float) = 0.0
        _HighSpeedTransition ("High Speed State Transition", Range(0.0, 1.0)) = 0.0
        _Layers ("Rift Layer Density", Range(3, 30)) = 8
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_QuantumRiftVortexSingle"
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
                float _Power;
                float _RotationAngle;
                float _HighSpeedTransition;
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

            void evaluateTier(int tier, float2 st, float dist, float angle, float autoRotation, float3 baseColor, float3 rimColor, out float pattern, out float3 tierColor)
            {
                pattern = 0.0;
                tierColor = baseColor;

                if (tier == 0) // Tier 0: Trash Tier - Choppy, low-bit rotating mechanical cog/gear teeth
                {
                    float cog = sin((angle * 6.0) + autoRotation * 2.0);
                    pattern = step(0.4, cog) * saturate(1.2 - dist * 3.0);
                    tierColor = baseColor * 0.3;
                } 
                else if (tier == 1) // Tier 1: Slow rusty rotating blades
                {
                    pattern = sin(angle * 3.0 - autoRotation) * 0.5 + 0.5;
                    tierColor = baseColor * 0.5;
                } 
                else if (tier == 2) // Tier 2: Double rotating cross-bars
                {
                    float s = cos(angle * 4.0 + autoRotation) * sin(angle * 2.0 - autoRotation * 0.5);
                    pattern = s * 0.5 + 0.5;
                    tierColor = lerp(baseColor, float3(0.2, 0.8, 0.5), 0.4);
                } 
                else if (tier == 3) // Tier 3: Triangular spinning blades
                {
                    float s1 = sin(angle * 3.0 + autoRotation * 1.5);
                    pattern = saturate(abs(s1) * (1.0 - dist * 2.0));
                    tierColor = lerp(baseColor, float3(1.0, 0.5, 0.1), 0.5);
                } 
                else if (tier == 4) // Tier 4: Rotating spiral segments
                {
                    pattern = sin(angle * 4.0 + dist * 10.0 - autoRotation * 2.0) * 0.5 + 0.5;
                    tierColor = lerp(rimColor, float3(0.0, 1.2, 1.5), 0.6);
                } 
                else if (tier == 5) // Tier 5: Sharp rotating star aperture
                {
                    pattern = abs(sin(angle * 5.0 + autoRotation * 2.0));
                    tierColor = float3(1.5, 0.2, 0.4);
                } 
                else if (tier == 6) // Tier 6: Rotating hexagonal grid spin
                {
                    float hex = sin(angle * 6.0 - dist * 15.0 + autoRotation * 1.5);
                    pattern = hex * 0.5 + 0.5;
                    tierColor = lerp(baseColor, float3(0.8, 0.2, 1.8), 0.7);
                } 
                else if (tier == 7) // Tier 7: Dual-speed rotating gear rings
                {
                    pattern = sin(angle * 8.0 + cos(dist * 10.0) * 2.0 - autoRotation * 3.0) * 0.5 + 0.5;
                    tierColor = float3(1.8, 1.8, 0.2);
                } 
                else if (tier == 8) // Tier 8: Sweeping plasma blade rotation
                {
                    pattern = pow(abs(sin(angle * 6.0 - dist * 20.0 + autoRotation * 2.5)), 0.6);
                    tierColor = float3(0.2, 2.0, 2.0);
                } 
                else if (tier == 9) // Tier 9: Complex multi-slot rotator
                {
                    float sing = sin(angle * 10.0 - autoRotation * 4.0) * cos(dist * 15.0 + autoRotation);
                    pattern = pow(saturate(sing * 0.5 + 0.5), 1.5);
                    tierColor = float3(2.2, 2.2, 2.5);
                }
                else if (tier == 10) // Tier 10: Hyper Spiral Burst
                {
                    float spiral = sin(angle * 14.0 + dist * 40.0 - autoRotation * 4.0);
                    pattern = saturate(pow(abs(spiral), 0.3) * 1.5);
                    tierColor = float3(3.5, 0.5, 1.2);
                }
                else if (tier == 11) // Tier 11: Quantum Lattice Spin
                {
                    float grid = sin(angle * 8.0 + autoRotation * 3.0) * cos(dist * 50.0 - autoRotation * 2.0);
                    pattern = saturate(abs(grid) * 2.0);
                    tierColor = float3(0.5, 3.5, 2.0);
                }
                else if (tier == 12) // Tier 12: Chrono Ring Interference Rotation
                {
                    float ring1 = sin(dist * 50.0 - autoRotation * 3.0);
                    float ring2 = cos(angle * 10.0 + autoRotation * 2.0);
                    pattern = pow(saturate(ring1 * ring2 + 0.5), 1.5);
                    tierColor = float3(3.8, 2.0, 0.3);
                }
                else if (tier == 13) // Tier 13: Solar Ray Flare Spin
                {
                    float rays = abs(cos(angle * 16.0 + autoRotation * 4.0));
                    pattern = pow(rays, 3.0) * smoothstep(0.0, 0.4, dist);
                    tierColor = float3(4.0, 0.8, 0.1);
                }
                else if (tier == 14) // Tier 14: Dimensional Fracture Rotator
                {
                    float wave = sin(angle * 12.0 - autoRotation * 5.0) + cos(dist * 60.0);
                    pattern = saturate(step(0.2, abs(wave)) * wave * 0.5 + 0.5);
                    tierColor = float3(1.2, 0.2, 4.0);
                }
                else if (tier == 15) // Tier 15: Plasma Vortex Pulse Spin
                {
                    float pulse = sin(dist * 70.0 - autoRotation * 6.0) * sin(angle * 16.0 + autoRotation * 2.0);
                    pattern = pow(saturate(pulse * 0.5 + 0.5), 0.4);
                    tierColor = float3(0.2, 4.0, 3.5);
                }
                else if (tier == 16) // Tier 16: Void Nebula Shockwave Rotation
                {
                    float shock = abs(sin(angle * 20.0 + dist * 80.0 - autoRotation * 7.0));
                    pattern = pow(shock, 4.0) * 2.0;
                    tierColor = float3(4.2, 0.2, 3.0);
                }
                else if (tier == 17) // Tier 17: Supernova Resonance Spin
                {
                    float star = sin(angle * 28.0 + autoRotation * 5.0) * cos(dist * 35.0 - autoRotation * 2.0);
                    pattern = saturate(star * star * 3.0);
                    tierColor = float3(4.5, 3.5, 0.5);
                }
                else if (tier == 18) // Tier 18: Singularity Breakdown Spin
                {
                    float noiseSim = sin(angle * 18.0 + autoRotation * 8.0) * cos(dist * 90.0 - autoRotation * 8.0);
                    pattern = saturate(pow(abs(noiseSim), 0.2));
                    tierColor = float3(5.0, 1.0, 5.0);
                }
                else // Tier 19: Apex God Tier
                {
                    float apex = sin(angle * 36.0 - dist * 100.0 + autoRotation * 10.0);
                    pattern = pow(saturate(apex * 0.5 + 0.5), 0.1);
                    tierColor = float3(6.0, 6.0, 6.0); // Blinding HDR God Tier White
                }
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 st = input.uv - 0.5;
                float centerDist = length(st);
                
                float pNorm = saturate(_Power / 100.0);
                float pExp = pow(pNorm, 2.5); 

                // Low power keeps clear rotational geometry without static pixel locking
                if (_Power < 20.0)
                {
                    st *= lerp(0.85, 1.0, pNorm);
                }

                // Tier mapping across 20 tiers (0 to 19)
                float powerTierVal = pNorm * 19.0;
                int tierA = clamp(int(floor(powerTierVal)), 0, 18);
                int tierB = min(tierA + 1, 19);
                float tierMix = frac(powerTierVal);
                tierMix = tierMix * tierMix * (3.0 - 2.0 * tierMix);

                float autoRotation = _RotationAngle;

                // --- DOMAIN WARPING SCALED BY POWER ---
                float warpMag = pExp * 0.25;
                float warpWave = sin(length(st * 15.0) - autoRotation * 2.0) * warpMag;
                float2 warpedSt = st + (st * warpWave);

                float warpedDist = length(warpedSt);
                float angle = atan2(warpedSt.y, warpedSt.x);

                float patA, patB;
                float3 colA, colB;
                evaluateTier(tierA, warpedSt, warpedDist, angle, autoRotation, _Color.rgb, _RimColor.rgb, patA, colA);
                evaluateTier(tierB, warpedSt, warpedDist, angle, autoRotation, _Color.rgb, _RimColor.rgb, patB, colB);

                float pattern = lerp(patA, patB, tierMix);
                float3 tierColor = lerp(colA, colB, tierMix);

                float coreVoid = smoothstep(_EventHorizon - 0.03, _EventHorizon + 0.08, warpedDist);
                float photonRim = smoothstep(0.03, 0.0, abs(warpedDist - _EventHorizon));

                // --- 1. POWER-SCALED VORTEX STATE ---
                float contrastPower = lerp(0.6, 3.5, pExp);
                float highContrastPattern = pow(saturate(pattern), contrastPower);
                
                float brightnessMultiplier = lerp(0.4, 8.0, pExp);
                float3 vortexColor = lerp(_RimColor.rgb * 0.2, tierColor * brightnessMultiplier, highContrastPattern);
                
                vortexColor *= lerp(0.6, 1.0, pNorm) * (1.0 - coreVoid * lerp(0.9, 0.8, pNorm));
                vortexColor += _RimColor.rgb * photonRim * lerp(1.0, 15.0, pExp);

                // --- CHROMATIC ABERRATION AT HIGH POWER ---
                if (_Power > 50.0)
                {
                    float aberrationStrength = ((_Power - 50.0) / 50.0) * 0.02;
                    vortexColor.r += pattern * aberrationStrength * 5.0;
                    vortexColor.b -= pattern * aberrationStrength * 3.0;
                }

                // --- 2. HIGH-SPEED WARP TUNNEL STATE ---
                float warpSpeed = _Time.y * 15.0;
                float radialStreaks = pow(abs(sin(angle * 24.0)), 8.0);
                float tunnelRings = abs(sin(centerDist * 50.0 + warpSpeed));
                float hyperPattern = saturate(radialStreaks * tunnelRings * 2.0);
                
                float3 hyperColor = lerp(_Color.rgb, _RimColor.rgb, sin(centerDist * 30.0 - warpSpeed) * 0.5 + 0.5);
                hyperColor *= hyperPattern * lerp(2.0, 10.0, pExp);
                
                float hyperCore = smoothstep(0.4, 0.0, centerDist);
                hyperColor += _RimColor.rgb * hyperCore * 5.0;

                // --- 3. BLEND STATES ---
                float3 finalColor = lerp(vortexColor, hyperColor, saturate(_HighSpeedTransition));
                
                // --- CLEAN CIRCULAR MASK ---
                float circularMask = smoothstep(0.48, 0.38, centerDist);
                
                float alphaThreshold = lerp(0.3, 0.5, pNorm);
                float alpha = circularMask * saturate(pattern * alphaThreshold + photonRim * 2.0 + (pNorm * 0.3));

                // Premultiply Color with Alpha
                finalColor *= alpha;

                half4 col = half4(finalColor, alpha);

                // Apply UI Vertex Color
                col.rgb *= input.color.rgb;
                col.a *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}