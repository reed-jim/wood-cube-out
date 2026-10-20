Shader "UI/Custom/QuantumRiftVortexSingle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Vortex Core Color", Color) = (0.5, 0.0, 1.0, 1.0)
        _RimColor ("Dimensional Rim Color", Color) = (0.0, 1.0, 0.8, 1.0)
        
        [Header(Vortex Control)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 10.0
        _CycleSpeed ("Cycle Rotation Speed", Float) = 1.0
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
        // Premultiplied Alpha Blending to completely eliminate dark halos
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
                float _CycleSpeed;
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

                if (tier == 0) 
                {
                    pattern = sin(angle * 2.0 + dist * _Layers - autoRotation) * 0.5 + 0.5;
                    tierColor = baseColor * 1.5;
                } 
                else if (tier == 1) 
                {
                    pattern = sin(angle * 4.0 - dist * (_Layers * 1.5) + autoRotation * 1.2) * 0.5 + 0.5;
                    tierColor = lerp(baseColor, float3(0.0, 1.8, 0.8), 0.6);
                } 
                else if (tier == 2) 
                {
                    float s = cos(angle * 3.0) * sin(dist * 20.0 - autoRotation);
                    pattern = s * 0.5 + 0.5;
                    tierColor = lerp(rimColor, float3(2.0, 0.2, 1.8), 0.7);
                } 
                else if (tier == 3) 
                {
                    float s1 = sin(angle * 4.0 + autoRotation);
                    float c1 = cos(angle * 4.0 - autoRotation);
                    pattern = saturate(abs(s1 * c1) * sin(dist * 25.0) * 2.0);
                    tierColor = lerp(baseColor, float3(2.5, 1.0, 0.2), 0.7);
                } 
                else if (tier == 4) 
                {
                    pattern = sin(dist * 40.0 - autoRotation * 2.0) * 0.5 + 0.5;
                    tierColor = lerp(rimColor, float3(0.0, 2.2, 2.5), 0.8);
                } 
                else if (tier == 5) 
                {
                    pattern = abs(sin(angle * 8.0 + autoRotation * 1.5));
                    tierColor = float3(2.5, 0.2, 0.6);
                } 
                else if (tier == 6) 
                {
                    float hex = sin(angle * 6.0 - dist * 30.0 + autoRotation) * cos(dist * 15.0);
                    pattern = hex * 0.5 + 0.5;
                    tierColor = lerp(baseColor, float3(1.0, 0.3, 2.5), 0.7);
                } 
                else if (tier == 7) 
                {
                    pattern = sin(angle * 10.0 + sin(dist * 20.0) * 5.0 - autoRotation * 2.0) * 0.5 + 0.5;
                    tierColor = float3(2.2, 2.2, 0.2);
                } 
                else if (tier == 8) 
                {
                    pattern = pow(abs(sin(angle * 5.0 - dist * 40.0 + autoRotation * 3.0)), 0.5);
                    tierColor = float3(0.2, 2.5, 2.5);
                } 
                else 
                {
                    float sing = sin(angle * 12.0 * dist * 15.0 - autoRotation * 5.0) * 0.5 + 0.5;
                    pattern = pow(sing, 2.0);
                    tierColor = float3(3.0, 3.0, 3.5);
                }
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 st = input.uv - 0.5;
                float centerDist = length(st);
                
                // Tier mapping with 100-power safety check
                float normalizedPower = min(_Power / 100.0, 0.9999);
                float powerTierVal = normalizedPower * 9.0;
                
                int tierA = clamp(int(floor(powerTierVal)), 0, 8);
                int tierB = min(tierA + 1, 9);
                float tierMix = frac(powerTierVal);
                tierMix = tierMix * tierMix * (3.0 - 2.0 * tierMix);

                float pNorm = saturate(_Power / 100.0);
                float pExp = pow(pNorm, 4.0);

                // --- CONSTANT ANIMATION SPEED (Decoupled from power tier scaling) ---
                float autoRotation = _Time.y * _CycleSpeed * TWO_PI;

                // --- DOMAIN WARPING ---
                float warpMag = pExp * 0.15;
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

                // --- 1. NORMAL VORTEX STATE ---
                float highContrastPattern = pow(saturate(pattern), 1.8);
                float3 vortexColor = lerp(_RimColor.rgb * 0.2, tierColor * 3.5, highContrastPattern);
                vortexColor *= (1.0 - coreVoid * 0.98) * (1.0 + pNorm * 4.0);
                vortexColor += _RimColor.rgb * photonRim * 8.0;

                // --- 2. TOTALLY UNIQUE HIGH-SPEED WARP TUNNEL STATE ---
                float warpSpeed = _Time.y * 15.0;
                float radialStreaks = pow(abs(sin(angle * 24.0)), 8.0);
                float tunnelRings = abs(sin(centerDist * 50.0 + warpSpeed));
                float hyperPattern = saturate(radialStreaks * tunnelRings * 2.0);
                
                float3 hyperColor = lerp(_Color.rgb, _RimColor.rgb, sin(centerDist * 30.0 - warpSpeed) * 0.5 + 0.5);
                hyperColor *= hyperPattern * 5.0;
                
                float hyperCore = smoothstep(0.4, 0.0, centerDist);
                hyperColor += _RimColor.rgb * hyperCore * 4.0;

                // --- 3. BLEND STATES ---
                float3 finalColor = lerp(vortexColor, hyperColor, saturate(_HighSpeedTransition));
                
                // --- CLEAN CIRCULAR MASK ---
                float circularMask = smoothstep(0.48, 0.38, centerDist);
                float alpha = circularMask * saturate(pattern * 0.8 + photonRim * 2.0 + 0.5);

                // --- PREMULTIPLY COLOR WITH ALPHA ---
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