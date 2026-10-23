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

        [Header(Text Contrast)]
        _CenterDarkening ("Center Darkening Power", Range(0.0, 1.0)) = 0.85
        _CenterRadius ("Center Dark Region Radius", Range(0.1, 0.6)) = 0.35

        [Header(Frame)]
        _Padding ("Inner Padding (room for outer glow)", Range(0.0, 0.15)) = 0.0
        _FrameMetal ("Frame Metallic Strength", Range(0.0, 1.5)) = 1.0
        _FrameGlow ("Frame Energy Glow", Range(0.0, 3.0)) = 1.2
        _GemSize ("Gem Size", Range(0.0, 2.0)) = 1.0
        _OuterGlow ("Outer Glow", Range(0.0, 2.0)) = 0.5

        [Header(Inner Field)]
        _FieldBrightness ("Inner Energy Brightness", Range(0.0, 3.0)) = 1.0
        _MoteDensity ("Floating Motes", Range(0.0, 2.0)) = 1.0
        _ShineStrength ("Gloss Sweep Strength", Range(0.0, 2.0)) = 1.0

        // UI Stencil Support
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #pragma multi_compile_local _LEVEL_ORBITAL _LEVEL_PLANETARY _LEVEL_STELLAR _LEVEL_GALACTIC _LEVEL_COSMIC _LEVEL_DIMENSIONAL _LEVEL_TRANSCENDENT
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define K_PI  3.14159265
            #define K_TAU 6.28318531

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
                #if defined(UNITY_UI_CLIP_RECT)
                float4 worldPosition : TEXCOORD1;
                #endif
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Level;
                float _AspectRatio;
                float _CornerRadius;
                float _BorderWidth;
                float _Speed;
                float _CenterDarkening;
                float _CenterRadius;
                float _Padding;
                float _FrameMetal;
                float _FrameGlow;
                float _GemSize;
                float _OuterGlow;
                float _FieldBrightness;
                float _MoteDensity;
                float _ShineStrength;
                float4 _ClipRect;
            CBUFFER_END

            // ------------------------------------------------------------------
            // Helpers
            // ------------------------------------------------------------------
            float AbSq(float x) { return x * x; }

            float AbSdBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            float AbHash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 AbHash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float AbNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = AbHash12(i);
                float b = AbHash12(i + float2(1.0, 0.0));
                float c = AbHash12(i + float2(0.0, 1.0));
                float d = AbHash12(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // 3-octave normalised fBm, ~0..1
            float AbFbm3(float2 p)
            {
                float v = 0.0, a = 0.5;
                float2x2 rot = float2x2(0.8, 0.6, -0.6, 0.8);
                for (int i = 0; i < 3; i++)
                {
                    v += a * AbNoise(p);
                    p = mul(rot, p) * 2.0;
                    a *= 0.5;
                }
                return v / 0.875;
            }

            // Domain warp (Inigo Quilez style)
            float AbWarp(float2 p, float t, out float2 q, out float2 r)
            {
                q = float2(AbFbm3(p + float2(0.0, t * 0.1)), AbFbm3(p + float2(5.2, 1.3 - t * 0.08)));
                r = float2(AbFbm3(p + 4.0 * q + float2(1.7, 9.2 + t * 0.05)),
                           AbFbm3(p + 4.0 * q + float2(8.3, 2.8 - t * 0.1)));
                return AbFbm3(p + 3.0 * r);
            }

            float3 AbPalette(float t)
            {
                return 0.5 + 0.5 * cos(K_TAU * (t + float3(0.0, 0.33, 0.67)));
            }

            float AbAngDiff(float a, float b)
            {
                float x = frac((a - b) / K_TAU + 0.5) - 0.5;
                return abs(x) * K_TAU;
            }

            // Rising floating motes (embers / spores / dust)
            float AbMotes(float2 p, float t, float scale, float seed)
            {
                float2 g = p * scale + float2(0.0, -t * 0.6);
                float2 id = floor(g);
                float2 f = frac(g) - 0.5;
                float h = AbHash12(id + seed * 17.0);
                if (h < 0.72) return 0.0;
                float2 o = (AbHash22(id + seed * 5.0) - 0.5) * 0.5;
                o.x += sin(t * 1.3 + h * 40.0) * 0.15;
                float d = length(f - o);
                float tw = 0.5 + 0.5 * sin(t * 3.0 + h * 60.0);
                float m = 1.0 - smoothstep(0.0, 0.14, d);
                return m * m * (0.35 + 0.65 * tw);
            }

            // Twinkling star points
            float AbStars(float2 p, float t, float prob)
            {
                float2 id = floor(p);
                float2 f = frac(p) - 0.5;
                float h = AbHash12(id);
                if (h > prob) return 0.0;
                float2 o = (AbHash22(id) - 0.5) * 0.7;
                float d = length(f - o);
                float tw = 0.5 + 0.5 * sin(t * 3.0 + h * 60.0);
                float s = 1.0 - smoothstep(0.0, 0.11, d);
                return s * s * (0.4 + 0.6 * tw) * (0.35 + 0.65 * (h / max(prob, 1e-4)));
            }

            // Occasional shooting star across the badge
            float AbShoot(float2 p, float t)
            {
                float slot = floor(t);
                float f = frac(t);
                if (AbHash12(float2(slot, 3.7)) < 0.5) return 0.0;
                float2 st = float2((AbHash12(float2(slot, 1.3)) - 0.5) * 0.6, 0.55);
                float sgn = (AbHash12(float2(slot, 9.1)) > 0.5) ? 1.0 : -1.0;
                float2 dir = normalize(float2(0.45 * sgn, -1.0));
                float2 head = st + dir * (f * 1.5);
                float2 d = p - head;
                float along = -dot(d, dir);
                float perp = abs(d.x * dir.y - d.y * dir.x);
                float trail = step(0.0, along) * exp(-along * 9.0);
                float core = exp(-(perp * perp) / (0.004 * 0.004)) * trail;
                float hg = exp(-dot(d, d) / (0.012 * 0.012));
                float life = smoothstep(0.0, 0.1, f) * (1.0 - smoothstep(0.7, 1.0, f));
                return (core + hg) * life;
            }

            // Orbiting satellite with a short trail
            float AbSat(float2 p, float rad, float spd, float ph, float t, float size)
            {
                float b = 0.0;
                [unroll]
                for (int k = 0; k < 5; k++)
                {
                    float a = t * spd + ph - sign(spd) * (float)k * 0.12;
                    float2 pos = float2(cos(a), sin(a)) * rad;
                    float d = length(p - pos);
                    float s = size * (1.0 - (float)k * 0.14);
                    float body = 1.0 - smoothstep(0.0, s, d);
                    b += (body * body + exp(-d / (size * 1.6)) * 0.35) * (1.0 - (float)k * 0.2);
                }
                return b;
            }

            // x = F1, y = F2 (distance to nearest two animated points), z = nearest cell id
            float3 AbVoronoi(float2 p, float t)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float f1 = 8.0, f2 = 8.0, id = 0.0;
                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        float2 g = float2((float)i, (float)j);
                        float2 o = AbHash22(ip + g);
                        o = 0.5 + 0.5 * sin(t + K_TAU * o);
                        float2 rr = g + o - fp;
                        float dd = dot(rr, rr);
                        if (dd < f1) { f2 = f1; f1 = dd; id = AbHash12(ip + g); }
                        else if (dd < f2) { f2 = dd; }
                    }
                }
                return float3(sqrt(f1), sqrt(f2), id);
            }

            // Faceted diamond gem. body = color, mask = coverage, glow = additive halo
            void AbGem(float2 lp, float size, float3 cMain, float3 cAcc, float t, float ph,
                       out float3 body, out float mask, out float glow)
            {
                float2 a = abs(lp);
                float dd = a.x + a.y;
                mask = 1.0 - smoothstep(size * 0.8, size, dd);
                float facet = saturate(0.5 + 0.5 * (lp.x - lp.y) / max(size, 1e-4));
                float inner = 1.0 - smoothstep(0.0, size * 0.55, dd);
                float tw = 0.75 + 0.25 * sin(t * 3.0 + ph);
                body = lerp(cMain * 0.55, cAcc, facet * 0.7 + 0.15) * (0.8 + 0.6 * inner) * tw;
                body = lerp(body, float3(1, 1, 1), inner * 0.6);
                body *= 0.7 + 0.3 * smoothstep(size * 0.55, size * 0.8, dd) * 0.0 + 0.3 * (1.0 - smoothstep(size * 0.6, size, dd));
                glow = exp(-length(lp) / max(size * 1.4, 1e-4)) * 0.55 * tw;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;

                #if defined(UNITY_UI_CLIP_RECT)
                output.worldPosition = input.positionOS;
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _Speed;

                // ---------------- Shape (rounded box SDF, height units) ----------------
                float2 uv = input.uv - 0.5;
                float aspect = max(_AspectRatio, 0.001);
                float2 aUV = float2(uv.x * aspect, uv.y);
                float2 ext = float2(0.5 * aspect - _Padding, 0.5 - _Padding);
                float cr = min(_CornerRadius, min(ext.x, ext.y));
                float d = AbSdBox(aUV, ext, cr);
                float aa = max(fwidth(d), 1e-4);

                // ---------------- Tier palette ----------------
                int tier;
                float3 cMain, cAcc, cDark;
                #if defined(_LEVEL_PLANETARY)
                    tier = 1;
                    cMain = float3(0.15, 0.95, 0.50); cAcc = float3(0.75, 1.0, 0.65); cDark = float3(0.0, 0.05, 0.03);
                #elif defined(_LEVEL_STELLAR)
                    tier = 2;
                    cMain = float3(1.0, 0.50, 0.06); cAcc = float3(1.0, 0.85, 0.40); cDark = float3(0.07, 0.01, 0.0);
                #elif defined(_LEVEL_GALACTIC)
                    tier = 3;
                    cMain = float3(0.72, 0.25, 1.0); cAcc = float3(0.45, 0.55, 1.0); cDark = float3(0.03, 0.0, 0.08);
                #elif defined(_LEVEL_COSMIC)
                    tier = 4;
                    cMain = float3(1.0, 0.12, 0.42); cAcc = float3(1.0, 0.45, 0.85); cDark = float3(0.07, 0.0, 0.03);
                #elif defined(_LEVEL_DIMENSIONAL)
                    tier = 5;
                    cMain = AbPalette(time * 0.12) * 1.2;
                    cAcc = AbPalette(time * 0.12 + 0.5) * 1.1 + 0.2;
                    cDark = float3(0.02, 0.015, 0.06);
                #elif defined(_LEVEL_TRANSCENDENT)
                    tier = 6;
                    cMain = float3(1.0, 0.80, 0.28); cAcc = float3(1.0, 0.96, 0.75); cDark = float3(0.08, 0.05, 0.0);
                #else
                    tier = 0;
                    cMain = float3(0.10, 0.80, 1.0); cAcc = float3(0.65, 0.97, 1.0); cDark = float3(0.005, 0.03, 0.09);
                #endif

                // ---------------- Outside the badge: soft tier-colored outer glow ----------------
                if (d > aa)
                {
                    float g = exp(-d * 22.0) * _OuterGlow * (0.75 + 0.25 * sin(time * 2.0));
                    if (g < 0.004) discard;
                    half4 o = half4(cMain * 1.2, saturate(g)) * input.color;
                    #if defined(UNITY_UI_CLIP_RECT)
                    float2 insideO = step(_ClipRect.xy, input.worldPosition.xy) * step(input.worldPosition.xy, _ClipRect.zw);
                    o.a *= insideO.x * insideO.y;
                    #endif
                    return o;
                }

                float alphaMask = 1.0 - smoothstep(-aa, aa, d);
                float bw = _BorderWidth;
                float borderMask = smoothstep(-bw - aa, -bw + aa, d);
                float din = -d - bw;                    // distance inside the inner frame edge

                float radius = length(aUV);
                float angle = atan2(aUV.y, aUV.x);

                // text-contrast vignette (same behaviour as the original)
                float centerDist = length(uv) / max(_CenterRadius, 0.001);
                float vignette = smoothstep(0.0, 1.0, centerDist);
                float darkMask = lerp(1.0 - _CenterDarkening, 1.0, vignette);

                // shared floating motes
                float motes = (AbMotes(aUV, time, 14.0, 1.0) * 0.9 + AbMotes(aUV + 7.3, time * 0.7, 24.0, 2.0) * 0.6) * _MoteDensity;

                // ==================================================================
                // INNER ENERGY FIELD (per tier)
                // ==================================================================
                float3 field = float3(0, 0, 0);

                #if defined(_LEVEL_PLANETARY)
                {
                    // Bio-aurora curtains, spores and a living ground glow
                    float2 q, r;
                    float w = AbWarp(aUV * 3.0, time * 0.4, q, r);
                    float c1 = pow(sin(aUV.x * 9.0 + w * 5.0 + time * 0.5) * 0.5 + 0.5, 3.0);
                    float c2 = pow(sin(aUV.x * 14.0 - w * 4.0 - time * 0.4 + 2.0) * 0.5 + 0.5, 4.0);
                    float hf = smoothstep(-0.5, 0.5, aUV.y + (w - 0.5) * 0.4);
                    float3 aur = lerp(float3(0.0, 0.35, 0.15), float3(0.1, 1.0, 0.5), c1) * c1
                               + float3(0.0, 0.6, 0.7) * c2 * 0.5
                               + float3(0.5, 0.2, 0.9) * smoothstep(0.1, 0.5, aUV.y) * c2 * 0.4;
                    float3 haze = lerp(float3(0.0, 0.12, 0.06), float3(0.02, 0.30, 0.18), r.y) * lerp(0.2, 1.0, w);
                    float ground = exp(-(aUV.y + 0.5) * 6.0);
                    field = aur * (0.5 + hf * 0.8) + haze + cMain * ground * 0.4 + cAcc * motes * 0.9;
                }
                #elif defined(_LEVEL_STELLAR)
                {
                    // Solar plasma: lava veins, prominences toward the rim, rising embers
                    float2 q, r;
                    float w = AbWarp(aUV * 4.0, time * 0.3, q, r);
                    float veins = pow(1.0 - abs(w * 2.0 - 1.0), 4.0);
                    float flare = AbFbm3(float2(angle * 5.0, radius * 8.0 - time * 1.5));
                    float3 magma = lerp(float3(0.25, 0.03, 0.0), float3(0.9, 0.35, 0.02), flare);
                    float prom = pow(flare, 3.0) * smoothstep(0.20, 0.50, radius);
                    field = magma * (w * 1.1 + 0.3) + lerp(cMain, cAcc, veins) * veins * 1.2
                          + cAcc * prom * 0.8 + cMain * motes * 1.3;
                }
                #elif defined(_LEVEL_GALACTIC)
                {
                    // Log-spiral galaxy with dust lanes and star field
                    float lr = log(radius + 0.03);
                    float arm = sin(angle * 2.0 - lr * 6.0 + time * 0.5) * 0.5 + 0.5;
                    float arm2 = arm * arm;
                    float dust = AbFbm3(aUV * 6.0 + float2(arm * 0.3, time * 0.1));
                    float3 gcol = lerp(float3(0.25, 0.03, 0.45), float3(0.08, 0.12, 0.50), arm);
                    float lane = smoothstep(0.35, 0.7, dust);
                    float stars = AbStars(aUV * 28.0 + float2(time * 0.05, 0.0), time, 0.16)
                                + AbStars(aUV * 52.0 + 3.1, time * 1.3, 0.10) * 0.7;
                    field = gcol * (dust * 1.3 + 0.2) * (0.4 + arm2) + cMain * arm2 * lane * 0.35
                          + cAcc * stars * 2.2 + cMain * exp(-radius * 5.0) * 0.5 + cAcc * motes * 0.5;
                }
                #elif defined(_LEVEL_COSMIC)
                {
                    // Crimson nebula, stardust and shooting stars
                    float2 q, r;
                    float w = AbWarp(aUV * 3.8, time * 0.25, q, r);
                    float3 neb = lerp(float3(0.30, 0.01, 0.08), float3(0.90, 0.05, 0.30), w);
                    float3 mag = float3(0.55, 0.03, 0.45) * r.x;
                    float stars = AbStars((aUV + float2(time * 0.02, 0.0)) * 40.0, time, 0.18)
                                + AbStars(aUV * 70.0 + 5.0, time * 1.4, 0.08) * 0.8;
                    float shoot = AbShoot(aUV, time * 0.35);
                    field = (neb + mag) * (w * 1.2 + 0.2) + cAcc * stars * 2.5
                          + float3(1.0, 0.85, 0.95) * shoot * 1.6 + cAcc * motes * 0.5;
                }
                #elif defined(_LEVEL_DIMENSIONAL)
                {
                    // Fractured rift: animated voronoi cracks with thin-film iridescence
                    float3 vor = AbVoronoi(aUV * 5.0 + float2(0.0, time * 0.1), time * 0.6);
                    float edge = vor.y - vor.x;
                    float crack = 1.0 - smoothstep(0.0, 0.12, edge);
                    float crackCore = 1.0 - smoothstep(0.0, 0.03, edge);
                    float2 shiftUV = aUV + 0.04 * float2(sin(time + aUV.y * 6.0), cos(time + aUV.x * 6.0));
                    float rift = AbFbm3(shiftUV * 8.0 + time);
                    float3 irid = AbPalette(rift * 1.5 + time * 0.1 + radius * 1.2);
                    float3 crackCol = AbPalette(edge * 6.0 + time * 0.3 + angle * 0.3) * 1.6;
                    float3 cellCol = AbPalette(vor.z + time * 0.05) * 0.25;
                    field = irid * (rift * 0.5 + 0.1) + cellCol + crackCol * crack * 1.2
                          + float3(1, 1, 1) * crackCore * 0.8 + cAcc * motes * 0.6;
                }
                #elif defined(_LEVEL_TRANSCENDENT)
                {
                    // Radiant singularity: god rays, divine halo with runes, rising gold motes
                    float rays = pow(abs(sin(angle * 8.0 + time * 0.8)), 6.0);
                    float rays2 = pow(abs(sin(angle * 13.0 - time * 0.5)), 8.0);
                    float rayMask = smoothstep(0.05, 0.35, radius);
                    float2 q, r;
                    float w = AbWarp(aUV * 5.0, time * 0.5, q, r);
                    float haloRing = exp(-AbSq((radius - 0.34) / 0.006));
                    float runes = smoothstep(0.7, 1.0, sin(angle * 24.0 - time * 0.5)) * exp(-AbSq((radius - 0.37) / 0.012));
                    float shafts = pow(AbFbm3(float2(aUV.x * 14.0 + time * 0.1, time * 0.05)), 3.0) * smoothstep(0.0, 0.5, aUV.y);
                    field = lerp(cMain * 0.7, cAcc, w) * (rays * 0.9 + rays2 * 0.4 + w * 0.6 + 0.25) * rayMask
                          + cAcc * (haloRing * 0.9 + runes * 0.8) + cMain * shafts * 0.8 + cAcc * motes * 1.5;
                }
                #else
                {
                    // Orbital HUD: radar sweep, range rings, tick marks, orbiting satellites
                    float ringA = exp(-AbSq((radius - 0.40) / 0.004));
                    float ringB = exp(-AbSq((radius - 0.26) / 0.003));
                    float ringC = exp(-AbSq((radius - 0.15) / 0.0025)) * 0.6;
                    float ticks = smoothstep(0.55, 0.9, sin(angle * 60.0)) * exp(-AbSq((radius - 0.425) / 0.012));
                    float majorTick = smoothstep(0.92, 1.0, sin(angle * 12.0)) * exp(-AbSq((radius - 0.44) / 0.02));
                    float sweepP = frac(angle / K_TAU + time * 0.25);
                    float sweepMask = (1.0 - smoothstep(0.40, 0.43, radius)) * smoothstep(0.02, 0.10, radius);
                    float gridRings = 1.0 - smoothstep(0.0, 0.01, abs(frac(radius * 12.0) - 0.5) - 0.45);
                    float trail = exp(-sweepP * 5.0);
                    float radar = (trail * 0.45 + exp(-sweepP * 90.0) * 0.9 + trail * gridRings * 0.5) * sweepMask;
                    float sats = AbSat(aUV, 0.38, 2.0, 0.0, time, 0.022) + AbSat(aUV, 0.26, -1.5, 2.0, time, 0.018) * 0.9;
                    field = cMain * (ringA * 0.9 + ringB * 0.6 + ringC + ticks * 0.8 + majorTick * 1.2 + radar)
                          + cAcc * sats * 2.0 + cAcc * motes * 0.5;
                }
                #endif

                // ---------------- Panel: deep base + field, depth shading, text contrast ----------------
                float3 panel = cDark * (0.8 + 0.4 * AbFbm3(aUV * 3.0 + time * 0.05)) + field * _FieldBrightness;

                // inner shadow just inside the frame gives the panel a recessed look
                panel *= 1.0 - 0.55 * exp(-max(din, 0.0) * 28.0);
                panel *= darkMask;

                // thin energy rim and an inset hairline (double-border look)
                float rimPulse = 0.55 + 0.25 * sin(time * 2.0);
                panel += cMain * exp(-max(din, 0.0) * 70.0) * rimPulse * _FrameGlow;
                panel += cAcc * exp(-AbSq((din - 0.008) / 0.0022)) * 0.7 * _FrameGlow;

                // diagonal gloss sweep + soft top-left clearcoat (kept weak over the text area)
                float2 sdir = normalize(float2(1.0, 0.7));
                float sPos = dot(aUV, sdir);
                float sCenter = lerp(-0.8, 0.8, frac(time * 0.12));
                float sweep = exp(-AbSq((sPos - sCenter) / 0.07)) * 0.5 + exp(-AbSq((sPos - sCenter + 0.12) / 0.025)) * 0.35;
                float shine = sweep * _ShineStrength * lerp(0.3, 1.0, vignette);
                panel += lerp(cAcc, float3(1, 1, 1), 0.5) * shine * 0.35;

                float2 coatCenter = float2(-ext.x * 0.6, ext.y * 0.85);
                float coat = exp(-length((aUV - coatCenter) * float2(1.2, 1.0)) * 5.0) * 0.12;
                panel += float3(1, 1, 1) * coat * lerp(0.4, 1.0, vignette);

                // ==================================================================
                // BEVELED METAL FRAME
                // ==================================================================
                float3 col = panel;

                if (borderMask > 0.001)
                {
                    // SDF gradient = outward direction, used for bevel lighting
                    float eps = 0.0015;
                    float2 gdir = float2(AbSdBox(aUV + float2(eps, 0.0), ext, cr) - d,
                                         AbSdBox(aUV + float2(0.0, eps), ext, cr) - d);
                    gdir = gdir / max(length(gdir), 1e-5);

                    float u = saturate(-d / max(bw, 1e-4));       // 0 outer edge -> 1 inner edge
                    float slope = cos(u * K_PI);                  // + on outer half, - on inner half
                    float2 Ldir = normalize(float2(-0.6, 0.8));   // light from top-left
                    float lightF = dot(gdir, Ldir) * slope;
                    float shade = 0.55 + 0.7 * lightF;

                    float3 steel = float3(0.42, 0.44, 0.50);
                    float3 metalTint = lerp(steel, cMain * 0.9 + 0.1, 0.5);
                    #if defined(_LEVEL_DIMENSIONAL)
                    metalTint = lerp(steel, AbPalette(angle / K_TAU + time * 0.1), 0.55);
                    #endif

                    float grain = AbNoise(aUV * 260.0) * 0.15 + AbNoise(aUV * 90.0) * 0.10;
                    float3 metal = metalTint * (0.85 + grain);
                    float groove = exp(-AbSq((u - 0.5) / 0.07));
                    metal *= 1.0 - 0.55 * groove;

                    float outerHi = 1.0 - smoothstep(0.0, 0.10, u);
                    float spec = pow(saturate(lightF), 3.0);

                    float3 frameCol = metal * shade * _FrameMetal;
                    frameCol += lerp(cAcc, float3(1, 1, 1), 0.5) * (outerHi * (0.35 + 0.4 * saturate(lightF + 0.3)) + spec * 0.6) * _FrameMetal;

                    // running energy lights inside the groove
                    float run = smoothstep(0.55, 1.0, sin(angle * (6.0 + (float)tier * 2.0) - time * 2.0));
                    frameCol += cMain * run * groove * 2.2 * _FrameGlow;

                    // travelling glint(s) around the frame
                    float glint = exp(-AbSq(AbAngDiff(angle, time * 1.0) / 0.22));
                    if (tier >= 4)
                        glint += exp(-AbSq(AbAngDiff(angle, time * 1.0 + K_PI) / 0.22));
                    frameCol += float3(1.0, 0.97, 0.9) * glint * (0.35 + 0.65 * (1.0 - u)) * 0.9;

                    // glowing inner lip
                    float strip = smoothstep(0.82, 1.0, u);
                    frameCol += cMain * strip * 0.9 * _FrameGlow * (0.7 + 0.3 * sin(time * 3.0));

                    col = lerp(panel, frameCol, borderMask);

                    // ---------------- Gems: corners (all tiers), crest (3+), base (5+) ----------------
                    float gs = max(bw, 0.02) * 0.95 * _GemSize * (0.85 + 0.08 * (float)tier);
                    float2 sg = float2((aUV.x >= 0.0) ? 1.0 : -1.0, (aUV.y >= 0.0) ? 1.0 : -1.0);
                    float2 cc = sg * float2(ext.x - cr, ext.y - cr);
                    float2 gp = cc + normalize(sg) * (cr - bw * 0.5);

                    float3 gb; float gm; float gg;
                    AbGem(aUV - gp, gs, cMain, cAcc, time, 0.0, gb, gm, gg);
                    col = lerp(col, gb, gm);
                    col += cMain * gg * _FrameGlow;

                    if (tier >= 3)
                    {
                        float2 cp = float2(0.0, ext.y - bw * 0.5);
                        AbGem(aUV - cp, gs * 1.35, cMain, cAcc, time, 1.7, gb, gm, gg);
                        col = lerp(col, gb, gm);
                        col += cMain * gg * _FrameGlow;
                    }
                    if (tier >= 5)
                    {
                        float2 bp = float2(0.0, -ext.y + bw * 0.5);
                        AbGem(aUV - bp, gs * 1.35, cMain, cAcc, time, 3.1, gb, gm, gg);
                        col = lerp(col, gb, gm);
                        col += cMain * gg * _FrameGlow;
                    }
                }

                // ---------------- Finish ----------------
                col = lerp(col, 1.0 - exp(-col * 1.3), 0.5);                       // soft highlight rolloff
                col += (AbHash12(input.positionCS.xy) - 0.5) * (1.5 / 255.0);       // dither dark gradients

                half4 result = half4(col, alphaMask) * input.color;

                #if defined(UNITY_UI_CLIP_RECT)
                float2 inside = step(_ClipRect.xy, input.worldPosition.xy) * step(input.worldPosition.xy, _ClipRect.zw);
                result.a *= inside.x * inside.y;
                #endif

                return result;
            }
            ENDHLSL
        }
    }
}
