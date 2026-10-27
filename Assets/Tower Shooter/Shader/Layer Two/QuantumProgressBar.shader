Shader "UI/Custom/QuantumMultiTierLoopBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Base Tier Color (Low)", Color) = (1.0, 0.25, 0.05, 1.0)
        _RimColor ("Elite Tier Color (High)", Color) = (0.0, 0.9, 1.0, 1.0)

        [Header(Loop Tier Control)]
        _Power ("Power / Tier Level (0 - 100)", Range(0.0, 100.0)) = 50.0
        _ScrollSpeed ("Loop Speed", Float) = 2.0
        _Layers ("Pattern Density", Range(1.0, 20.0)) = 6.0
        _BarThickness ("Bar Height Profile", Range(0.1, 1.0)) = 0.6

        [Header(Look)]
        _Exposure ("Exposure", Range(0.25, 3.0)) = 1.0
        _Tonemap ("Filmic Tonemap Amount", Range(0.0, 1.0)) = 0.6

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
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha   // premultiplied alpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_QuantumMultiTierLoopBar"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 worldPos   : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _ClipRect;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float _Power;
                float _ScrollSpeed;
                float _Layers;
                float _BarThickness;
                float _Exposure;
                float _Tonemap;
            CBUFFER_END

            #define PI 3.14159265359

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPos = input.positionOS;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color;
                return o;
            }

            // Simple hash & noise helpers for procedural plasma effects in higher tiers
            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    v += a * vnoise(p);
                    p = mul(float2x2(1.6, 1.2, -1.2, 1.6), p) + 17.0;
                    a *= 0.5;
                }
                return v / 0.875;
            }

            float3 ACESFilm(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float t = _Time.y;
                float pN = saturate(_Power / 100.0);

                // Tier weights
                float tier1 = smoothstep(0.0, 0.4, pN); // Low-to-Mid transition
                float tier2 = smoothstep(0.4, 0.75, pN); // High activation
                float tier3 = smoothstep(0.75, 1.0, pN); // Elite / Overdrive state

                // Vertical bar boundaries
                float centerY = abs(uv.y - 0.5) * 2.0;
                float barMask = 1.0 - smoothstep(_BarThickness * 0.8, _BarThickness, centerY);
                float trackBg = (1.0 - smoothstep(_BarThickness - 0.05, _BarThickness + 0.02, centerY)) * 0.12;

                // -------------------------------------------------------------------------
                // TIER-BASED VISUAL EFFECT GENERATION
                // -------------------------------------------------------------------------
                
                float3 finalRgb = 0;
                float streamAlpha = 0;

                // --- TIER 1: Standard Smooth Energy Pulse Stream ---
                float speed1 = _ScrollSpeed * 1.5;
                float pattern1 = frac(uv.x * _Layers - t * speed1);
                float pulse1 = pow(abs(sin(pattern1 * PI)), 2.0);
                
                float3 colT1 = _Color.rgb * (pulse1 * 1.5 + 0.2);
                finalRgb = colT1;
                streamAlpha = pulse1;

                // --- TIER 2: Add Cybernetic Digital Data Blocks & Glitch Lines ---
                if (tier1 > 0.001)
                {
                    float speed2 = _ScrollSpeed * 3.5;
                    float pattern2 = frac(uv.x * (_Layers * 2.0) + t * speed2);
                    // Sharp data blocks
                    float dataBlocks = step(0.6, pattern2) * step(uv.y, 0.6) * step(0.4, uv.y);
                    
                    float3 colT2 = lerp(_Color.rgb, _RimColor.rgb, 0.6);
                    finalRgb = lerp(finalRgb, colT2 + dataBlocks * 2.0, tier1);
                    streamAlpha = lerp(streamAlpha, max(streamAlpha, dataBlocks * 1.5), tier1);
                }

                // --- TIER 3 & 4 (OVERDRIVE / ELITE): Turbulent Plasma Noise + Chromatic Aberration ---
                if (tier2 > 0.001)
                {
                    // Introduce intense plasma noise distortion inside the bar
                    float2 noiseUv = uv * float2(8.0, 2.0) + float2(t * 4.0 * pN, 0.0);
                    float plasma = fbm(noiseUv);

                    // Chromatic Aberration offset (splitting RGB channels based on noise)
                    float caOffset = 0.02 * tier3;
                    float plasmaR = fbm(noiseUv + float2(caOffset, 0.0));
                    float plasmaG = plasma;
                    float plasmaB = fbm(noiseUv - float2(caOffset, 0.0));

                    float3 eliteCol = float3(1.0, 0.95, 0.85); // White-hot core
                    float3 plasmaRgb = float3(plasmaR, plasmaG, plasmaB) * eliteCol * 4.0;

                    // High-speed energetic sparks running across
                    float spark = pow(abs(sin(uv.x * (_Layers * 4.0) - t * 8.0)), 8.0) * tier3;

                    // Blend plasma into higher tiers
                    float3 colT3 = lerp(_RimColor.rgb, plasmaRgb, tier3) + spark * 3.0;
                    finalRgb = lerp(finalRgb, colT3, tier2);
                    streamAlpha = lerp(streamAlpha, max(streamAlpha, plasma + spark * 2.0), tier2);
                }

                // -------------------------------------------------------------------------
                // Composition & Post-Processing (HDR Bloom, Tonemapping, UI Clipping)
                // -------------------------------------------------------------------------
                
                float3 composite = (trackBg * _Color.rgb) + (finalRgb * barMask * streamAlpha);
                float finalAlpha = saturate(trackBg + (barMask * (0.3 + streamAlpha * 0.7)));

                // Film Tonemapping to handle extreme HDR glow cleanly
                float mx = max(composite.r, max(composite.g, composite.b));
                float ex = saturate((mx - 1.0) / 6.0);
                composite = lerp(composite, mx.xxx, ex * 0.7);

               composite *= _Exposure;
               composite = lerp(composite, ACESFilm(composite), _Tonemap);

                // UI Vertex Color & Alpha handling
               composite *= input.color.rgb * input.color.a;
                finalAlpha *= input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
               float2 inside = step(_ClipRect.xy, input.worldPos.xy) * step(input.worldPos.xy, _ClipRect.zw);
               composite *= inside.x * inside.y;
               finalAlpha *= inside.x * inside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
               clip(finalAlpha - 0.001);
                #endif

                return half4(composite, finalAlpha);
            }
            ENDHLSL
        }
    }
}