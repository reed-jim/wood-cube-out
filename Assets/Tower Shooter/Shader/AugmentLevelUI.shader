Shader "UI/SpaceAugmentBadge"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [KeywordEnum(Orbital, Planetary, Stellar, Galactic, Cosmic, Dimensional, Transcendent)]
        _Level ("Augment Level", Float) = 0

        _AspectRatio ("Aspect Ratio (Width / Height)", Float) = 0.6
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.15
        _BorderWidth ("Border Thickness", Range(0.0, 0.15)) = 0.04
        _Speed ("Animation Speed", Range(0.1, 5.0)) = 1.0

        // Vignette & Central Darkening for Text Contrast
        _CenterDarkening ("Center Darkening Power", Range(0.0, 1.0)) = 0.85
        _CenterRadius ("Center Dark Region Radius", Range(0.1, 0.6)) = 0.35

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
            #pragma multi_compile_local _LEVEL_ORBITAL _LEVEL_PLANETARY _LEVEL_STELLAR _LEVEL_GALACTIC _LEVEL_COSMIC _LEVEL_DIMENSIONAL _LEVEL_TRANSCENDENT

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

            float _Level;
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

            // Hash
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Smooth Noise
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

            // FBM Noise
            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float2x2 rot = float2x2(0.8, 0.6, -0.6, 0.8);
                for (int i = 0; i < 4; i++)
                {
                    v += a * noise(p);
                    p = mul(rot, p) * 2.0;
                    a *= 0.5;
                }
                return v;
            }

            // Domain Warping
            float domainWarp(float2 p, float time, out float2 q, out float2 r)
            {
                q = float2(fbm(p + float2(0.0, time * 0.1)), fbm(p + float2(5.2, 1.3 - time * 0.08)));
                r = float2(fbm(p + 4.0 * q + float2(1.7, 9.2 + time * 0.05)), fbm(p + 4.0 * q + float2(8.3, 2.8 - time * 0.1)));
                return fbm(p + 3.0 * r);
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

                // Center UV
                float2 uv = input.uv - 0.5;

                // Aspect Ratio corrected UV
                float aspect = max(_AspectRatio, 0.001);
                float2 aspectUV = float2(uv.x * aspect, uv.y);
                float2 boxHalfExtents = float2(0.5 * aspect, 0.5);

                float dist = sdRoundedBox(aspectUV, boxHalfExtents, _CornerRadius);

                // Edge mask for rounded rectangle
                float alphaMask = 1.0 - smoothstep(-0.003, 0.003, dist);
                float borderMask = smoothstep(-_BorderWidth - 0.003, -_BorderWidth, dist) * alphaMask;

                if (alphaMask <= 0.001)
                {
                    discard;
                }

                float radius = length(aspectUV);
                float angle = atan2(aspectUV.y, aspectUV.x);

                float3 col = float3(0.01, 0.01, 0.03);
                float3 borderGlow = float3(0, 0, 0);

                // --- 7 DISTINCT TIERS WITH UNIQUE PALETTES & PROCEDURAL DESIGNS ---

                #if defined(_LEVEL_ORBITAL)
                    // REMADE ORBITAL: High-Tech Neon Blue Radar Pulse & Orbiting Satellite Nodes
                    // 1. Radar Grid Ring Sweep
                    float radarSweep = frac((angle / 6.28318) - time * 0.5);
                    float gridRings = smoothstep(0.01, 0.0, abs(frac(radius * 12.0) - 0.5) - 0.45);
                    float3 radar = float3(0.0, 0.7, 1.0) * pow(radarSweep, 3.0) * gridRings * 0.8;

                    // 2. Crisp Elliptical Orbit Paths
                    float orbit1 = smoothstep(0.008, 0.0, abs(radius - 0.38));
                    float orbit2 = smoothstep(0.008, 0.0, abs(radius - 0.26));
                    
                    // 3. Orbiting Satellite Dots
                    float sat1Angle = time * 2.0;
                    float2 sat1Pos = float2(cos(sat1Angle), sin(sat1Angle)) * 0.38;
                    float sat1 = smoothstep(0.03, 0.0, length(aspectUV - sat1Pos));

                    float sat2Angle = -time * 1.5 + 2.0;
                    float2 sat2Pos = float2(cos(sat2Angle), sin(sat2Angle)) * 0.26;
                    float sat2 = smoothstep(0.025, 0.0, length(aspectUV - sat2Pos));

                    col += radar + float3(0.0, 0.8, 1.0) * (orbit1 + orbit2) * 0.6 + float3(0.4, 0.95, 1.0) * (sat1 * 2.5 + sat2 * 2.0);
                    borderGlow = float3(0.0, 0.85, 1.0);

                #elif defined(_LEVEL_PLANETARY)
                    // PLANETARY: Emerald / Mint Green Bio-Atmospheric Plasma
                    float2 q, r;
                    float warp = domainWarp(aspectUV * 3.5, time * 0.4, q, r);
                    float aurora = sin(aspectUV.y * 8.0 + warp * 4.0 + time) * 0.5 + 0.5;
                    col += lerp(float3(0.0, 0.25, 0.12), float3(0.02, 0.45, 0.25), r.y) * (aurora * 1.2 + warp * 0.6);
                    borderGlow = float3(0.1, 0.95, 0.45);

                #elif defined(_LEVEL_STELLAR)
                    // STELLAR: Fiery Amber & Solar Flare Core
                    float flare = fbm(float2(angle * 5.0, radius * 8.0 - time * 1.5));
                    float2 q, r;
                    float warp = domainWarp(aspectUV * 4.0, time * 0.3, q, r);
                    col += lerp(float3(0.35, 0.08, 0.0), float3(0.7, 0.3, 0.02), flare) * (warp * 1.1 + 0.3);
                    borderGlow = float3(1.0, 0.55, 0.05);

                #elif defined(_LEVEL_GALACTIC)
                    // GALACTIC: Royal Violet / Deep Purple Spiral Galaxy Dust
                    float spiral = sin(angle * 3.0 - radius * 12.0 + time * 1.5);
                    float dust = fbm(aspectUV * 6.0 + float2(spiral * 0.3, time * 0.2));
                    col += lerp(float3(0.2, 0.02, 0.35), float3(0.08, 0.1, 0.4), spiral * 0.5 + 0.5) * (dust * 1.3 + 0.2);
                    borderGlow = float3(0.75, 0.2, 1.0);

                #elif defined(_LEVEL_COSMIC)
                    // REMADE COSMIC: Vibrant Deep Crimson Nebula & Shimmering Stardust
                    // 1. Swirling Crimson / Rose-Pink Nebula Flow
                    float2 qCosmic, rCosmic;
                    float cosmicWarp = domainWarp(aspectUV * 3.8, time * 0.25, qCosmic, rCosmic);
                    float3 crimsonNebula = lerp(float3(0.3, 0.01, 0.08), float3(0.8, 0.05, 0.25), cosmicWarp);
                    float3 magentaGlow = float3(0.5, 0.02, 0.4) * rCosmic.x;

                    // 2. Cosmic Sparkles / Stardust Specks
                    float2 starGrid = floor((aspectUV + float2(time * 0.02, 0.0)) * 40.0);
                    float starSeed = hash(starGrid);
                    float sparkle = pow(starSeed, 28.0) * (sin(time * 7.0 + starSeed * 50.0) * 0.5 + 0.5);
                    float3 stardust = float3(1.0, 0.7, 0.85) * sparkle * 3.0;

                    col += (crimsonNebula + magentaGlow) * (cosmicWarp * 1.2 + 0.2) + stardust;
                    borderGlow = lerp(float3(1.0, 0.05, 0.35), float3(0.8, 0.1, 0.6), sin(time * 2.0) * 0.5 + 0.5);

                #elif defined(_LEVEL_DIMENSIONAL)
                    // DIMENSIONAL: Prismatic Chromatic Shift Distortion
                    float2 shiftUV = aspectUV + float2(sin(time + aspectUV.y * 6.0), cos(time + aspectUV.x * 6.0)) * 0.04;
                    float rift = fbm(shiftUV * 8.0 + time);
                    float3 chroma = float3(
                        sin(rift * 6.28 + time * 2.0) * 0.4 + 0.5,
                        sin(rift * 6.28 + time * 2.0 + 2.09) * 0.4 + 0.5,
                        sin(rift * 6.28 + time * 2.0 + 4.18) * 0.4 + 0.5
                    );
                    col += chroma * (rift * 0.8 + 0.2);
                    borderGlow = chroma * 1.5;

                #elif defined(_LEVEL_TRANSCENDENT)
                    // TRANSCENDENT: Radiant Celestial Gold Singularity Rays
                    float rays = pow(abs(sin(angle * 8.0 + time * 3.0)), 5.0);
                    float2 q, r;
                    float superWarp = domainWarp(aspectUV * 5.0, time * 0.6, q, r);
                    float3 celestialGold = float3(0.85, 0.65, 0.15);
                    float3 coreWhite = float3(1.0, 0.95, 0.7);
                    col += lerp(celestialGold, coreWhite, superWarp) * (rays * 1.2 + superWarp * 0.8 + 0.3);
                    borderGlow = float3(1.2, 0.95, 0.3) * (1.2 + rays * 0.5);
                #endif

                // --- CENTRAL DARKENING VIGNETTE (TEXT CONTRAST) ---
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
