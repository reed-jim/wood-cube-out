Shader "UI/Custom/QuantumRiftParticleSpark"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Spark Core Color", Color) = (0.5, 0.0, 1.0, 1.0)
        _RimColor ("Spark Rim Color", Color) = (0.0, 1.0, 0.8, 1.0)
        
        [Header(Particle Animated Controls)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 10.0
        _ParticleSize ("Particle Spread", Range(0.01, 0.5)) = 0.2
        _AnimSpeed ("Animation / Drift Speed", Float) = 3.0
        _Intensity ("Glow Intensity", Range(1.0, 10.0)) = 4.0

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
        // Additive blending for gorgeous glowing plasma stack effects
        Blend SrcAlpha One
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_QuantumRiftParticleSpark"
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
                float _ParticleSize;
                float _AnimSpeed;
                float _Intensity;
            CBUFFER_END

            #define PI 3.14159265359

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

            void evaluateAnimatedParticleTier(int tier, float2 st, float dist, float angle, float time, float3 baseColor, float3 rimColor, out float pattern, out float3 tierColor)
            {
                pattern = 0.0;
                tierColor = baseColor;
                
                // Pulsating soft ember body synchronized with time
                float pulse = sin(time * 4.0 + dist * 10.0) * 0.2 + 0.8;
                float ember = exp(-dist * (8.0 / (_ParticleSize * pulse)));

                if (tier == 0) 
                {
                    pattern = ember;
                    tierColor = baseColor * 1.5;
                } 
                else if (tier == 1) 
                {
                    float rotAngle = angle + time * 2.0;
                    float star = pow(abs(cos(rotAngle * 2.0)), 4.0);
                    pattern = ember * (0.3 + star * 0.7);
                    tierColor = lerp(baseColor, float3(0.0, 1.8, 0.8), 0.6);
                } 
                else if (tier == 2) 
                {
                    float ringProgress = frac(time * 0.5) * _ParticleSize;
                    float microRing = smoothstep(0.03, 0.0, abs(dist - ringProgress));
                    pattern = saturate(ember + microRing * 2.0);
                    tierColor = lerp(rimColor, float3(2.0, 0.2, 1.8), 0.7);
                } 
                else if (tier == 3) 
                {
                    float cross = pow(abs(sin(angle * 4.0 - time * 3.0)), 6.0);
                    pattern = ember * saturate(cross * 2.0 + 0.4);
                    tierColor = lerp(baseColor, float3(2.5, 1.0, 0.2), 0.7);
                } 
                else if (tier == 4) 
                {
                    float trail = saturate(1.0 - (st.y / _ParticleSize + sin(time) * 0.2));
                    pattern = ember * trail * 1.5;
                    tierColor = lerp(rimColor, float3(0.0, 2.2, 2.5), 0.8);
                } 
                else if (tier == 5) 
                {
                    float rays = abs(sin(angle * 6.0 + time * 5.0));
                    pattern = ember * rays;
                    tierColor = float3(2.5, 0.2, 0.6);
                } 
                else if (tier == 6) 
                {
                    float hex = pow(abs(cos(angle * 3.0 - time)), 2.0);
                    pattern = ember * (hex * 1.2 + 0.2);
                    tierColor = lerp(baseColor, float3(1.0, 0.3, 2.5), 0.7);
                } 
                else if (tier == 7) 
                {
                    float jagged = abs(sin(angle * 8.0 - time * 6.0 + dist * 10.0));
                    pattern = ember * jagged;
                    tierColor = float3(2.2, 2.2, 0.2);
                } 
                else if (tier == 8) 
                {
                    float miniSwirl = sin(angle * 3.0 - dist * 15.0 + time * 4.0) * 0.5 + 0.5;
                    pattern = ember * miniSwirl * 1.5;
                    tierColor = float3(0.2, 2.5, 2.5);
                } 
                else 
                {
                    // Tier 9: Refined high-power energy shard (clamped multipliers to prevent harsh popping)
                    float shard = pow(abs(sin(angle * 5.0 + dist * 20.0 - time * 4.0)), 0.7);
                    pattern = ember * saturate(shard * 1.5 + 0.3);
                    tierColor = lerp(rimColor, float3(2.5, 2.5, 3.0), 0.5);
                }
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 st = input.uv - 0.5;
                float dist = length(st);
                float angle = atan2(st.y, st.x);
                float currentTime = _Time.y * _AnimSpeed;

                // --- FIXED TIER MAPPING (Prevents hard-clipping snap at exactly power = 100) ---
                // Clamping input slightly ensures power 100 smoothly resolves into the final blend state without overflowing index bounds.
                float normalizedPower = min(_Power / 100.0, 0.9999);
                float powerTierVal = normalizedPower * 9.0;
                
                int tierA = clamp(int(floor(powerTierVal)), 0, 8);
                int tierB = min(tierA + 1, 9);
                float tierMix = frac(powerTierVal);
                tierMix = tierMix * tierMix * (3.0 - 2.0 * tierMix);

                float patA, patB;
                float3 colA, colB;
                evaluateAnimatedParticleTier(tierA, st, dist, angle, currentTime, _Color.rgb, _RimColor.rgb, patA, colA);
                evaluateAnimatedParticleTier(tierB, st, dist, angle, currentTime, _Color.rgb, _RimColor.rgb, patB, colB);

                float pattern = lerp(patA, patB, tierMix);
                float3 tierColor = lerp(colA, colB, tierMix);

                // Living, breathing point-core flare
                float corePulse = sin(currentTime * 6.0) * 0.5 + 0.5;
                float pointCore = exp(-dist * (20.0 / _ParticleSize)) * (0.7 + corePulse * 0.3);

                // Compose final animated RGB values
                float3 rgb = tierColor * pattern * _Intensity * 2.0;
                rgb += float3(3.0, 3.0, 3.5) * pointCore * _Intensity;

                float alpha = saturate(pattern + pointCore);

                half4 col = half4(rgb, alpha);

                // Multiply by particle vertex color so Unity Particle Systems fade them out over lifetime
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