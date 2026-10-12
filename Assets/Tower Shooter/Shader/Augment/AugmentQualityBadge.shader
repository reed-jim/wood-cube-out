Shader "UI/AugmentQualityBadge"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [KeywordEnum(Common, Uncommon, Rare, Epic, Legendary, Mythic)]
        _Quality ("Quality Tier", Float) = 0

        _AspectRatio ("Aspect Ratio (Width / Height)", Float) = 0.8
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.12
        _BorderWidth ("Border Thickness", Range(0.0, 0.15)) = 0.035
        _Speed ("Animation Speed", Range(0.1, 5.0)) = 1.0

        // Vignette & Central Darkening for Text Contrast
        _CenterDarkening ("Center Darkening Power", Range(0.0, 1.0)) = 0.75
        _CenterRadius ("Center Dark Region Radius", Range(0.1, 0.6)) = 0.4

        // UI Stencil Support
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
        ZTest [shape]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _QUALITY_COMMON _QUALITY_UNCOMMON _QUALITY_RARE _QUALITY_EPIC _QUALITY_LEGENDARY _QUALITY_MYTHIC

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

            float _Quality;
            float _AspectRatio;
            float _CornerRadius;
            float _BorderWidth;
            float _Speed;
            float _CenterDarkening;
            float _CenterRadius;

            // Rounded Box SDF
            float sdRoundedBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            // Hash & Noise for Procedural FX
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float2x2 rot = float2x2(0.8, 0.6, -0.6, 0.8);
                for (int i = 0; i < 3; i++)
                {
                    v += a * noise(p);
                    p = mul(rot, p) * 2.0;
                    a *= 0.5;
                }
                return v;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _Speed;

                // Center UV & Aspect Correction
                float2 uv = input.uv - 0.5;
                float aspect = max(_AspectRatio, 0.001);
                float2 aspectUV = float2(uv.x * aspect, uv.y);
                float2 boxHalfExtents = float2(0.5 * aspect, 0.5);

                float dist = sdRoundedBox(aspectUV, boxHalfExtents, _CornerRadius);

                float alphaMask = 1.0 - smoothstep(-0.003, 0.003, dist);
                float borderMask = smoothstep(-_BorderWidth - 0.003, -_BorderWidth, dist) * alphaMask;

                if (alphaMask <= 0.001)
                {
                    discard;
                }

                float radius = length(aspectUV);
                float angle = atan2(aspectUV.y, aspectUV.x);

                float3 col = float3(0.08, 0.08, 0.1);
                float3 borderGlow = float3(0.3, 0.3, 0.3);

                // --- 6 QUALITY TIERS ---

                #if defined(_QUALITY_COMMON)
                    // COMMON: Industrial / Slate Gray
                    float stripe = step(0.5, frac((aspectUV.x + aspectUV.y) * 15.0));
                    col = lerp(float3(0.12, 0.14, 0.16), float3(0.18, 0.20, 0.23), stripe);
                    borderGlow = float3(0.45, 0.50, 0.55);

                #elif defined(_QUALITY_UNCOMMON)
                    // UNCOMMON: Clean Forest / Mint Green
                    float diagonal = frac((aspectUV.x - aspectUV.y) * 8.0 + time * 0.1);
                    col = float3(0.06, 0.18, 0.12) + float3(0.0, 0.08, 0.04) * diagonal;
                    borderGlow = float3(0.20, 0.85, 0.40);

                #elif defined(_QUALITY_RARE)
                    // RARE: Deep Azure / Mystic Blue Glow
                    float radialPulse = sin(radius * 20.0 - time * 2.0) * 0.5 + 0.5;
                    col = float3(0.05, 0.12, 0.28) + float3(0.05, 0.15, 0.35) * radialPulse;
                    borderGlow = float3(0.15, 0.55, 1.0);

                #elif defined(_QUALITY_EPIC)
                    // EPIC: Royal Velvet Purple with Subtle Shimmer
                    float f = fbm(aspectUV * 5.0 + time * 0.2);
                    col = lerp(float3(0.18, 0.06, 0.28), float3(0.32, 0.10, 0.45), f);
                    borderGlow = float3(0.70, 0.25, 1.0);

                #elif defined(_QUALITY_LEGENDARY)
                    // LEGENDARY: Radiant Solar Orange / Warm Gold
                    float rays = pow(abs(sin(angle * 6.0 + time * 1.5)), 4.0);
                    float coreGlow = smoothstep(0.5, 0.0, radius);
                    col = lerp(float3(0.35, 0.12, 0.02), float3(0.60, 0.28, 0.04), coreGlow) + float3(0.2, 0.1, 0.0) * rays;
                    borderGlow = float3(1.0, 0.60, 0.10);

                #elif defined(_QUALITY_MYTHIC)
                    // MYTHIC: Iridescent Crimson / Prism Shift Dual Tone
                    float shift = sin(time + aspectUV.x * 5.0 + aspectUV.y * 5.0) * 0.5 + 0.5;
                    float sparkle = pow(hash(aspectUV * 30.0 + floor(time * 2.0)), 20.0);
                    col = lerp(float3(0.35, 0.02, 0.10), float3(0.15, 0.02, 0.30), shift) + sparkle * 2.0;
                    borderGlow = lerp(float3(1.0, 0.15, 0.40), float3(0.6, 0.2, 1.0), sin(time * 2.0) * 0.5 + 0.5);
                #endif

                // --- CENTRAL DARKENING VIGNETTE (FOR TEXT CONTRAST) ---
                float centerDist = length(uv) / max(_CenterRadius, 0.001);
                float vignette = smoothstep(0.0, 1.0, centerDist);
                float darkMask = lerp(1.0 - _CenterDarkening, 1.0, vignette);

                col *= darkMask;

                // --- BORDER COMPOSITING ---
                col = lerp(col, borderGlow, borderMask);

                return half4(col, alphaMask) * input.color;
            }
            ENDHLSL
        }
    }
}