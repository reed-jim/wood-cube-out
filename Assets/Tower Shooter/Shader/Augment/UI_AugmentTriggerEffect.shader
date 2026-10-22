Shader "UI/Custom/LeagueStyleAugmentShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Core Tint Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _RimColor ("Energy Glow Color", Color) = (1.0, 0.3, 0.9, 1.0)
        _QualityColor ("Augment Quality Color", Color) = (1.0, 0.85, 0.4, 1.0)

        [Header(Shared Control)]
        _TriggerProgress ("Trigger Progress (0 - 1)", Range(0.0, 1.0)) = 0.0
        _Intensity ("Intensity", Range(0.0, 3.0)) = 1.0
        _SoftClip ("Soft Clip (1 = LDR canvas, 0 = HDR + Bloom)", Range(0.0, 1.0)) = 1.0
        _Seed ("Random Seed", Float) = 0.0

        [Header(Effect Toggles)]
        _Effect1Active ("Effect 1: Hextech Arc Burst", Float) = 0.0
        _Effect2Active ("Effect 2: Void Rift Slash", Float) = 0.0
        _Effect3Active ("Effect 3: Astral Spin Blade", Float) = 0.0
        _Effect4Active ("Effect 4: Sunfire Rune Pulse", Float) = 0.0
        _Effect5Active ("Effect 5: Ultimate Nova Shockwave", Float) = 0.0

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
        // Premultiplied: rgb is emissive energy, alpha is "body" coverage.
        // Glow keeps alpha low -> behaves additively, bodies darken what is behind them.
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_LeagueStyleAugmentShader"
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
                float2 posOS      : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float4 _QualityColor;
                float _TriggerProgress;
                float _Intensity;
                float _SoftClip;
                float _Seed;
                float _Effect1Active;
                float _Effect2Active;
                float _Effect3Active;
                float _Effect4Active;
                float _Effect5Active;
            CBUFFER_END

            float4 _ClipRect; // set by RectMask2D

            #define PI  3.14159265359
            #define TAU 6.28318530718
            #define SPARK_COUNT 22

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color;
                o.posOS = input.positionOS.xy;
                return o;
            }

            // =====================================================================
            // Context shared by every effect
            // =====================================================================
            struct FxCtx
            {
                float2 st;     // centered uv (-0.5..0.5)
                float  dist;
                float  angle;
                float  prog;
                float  t;      // time (noise flow only)
                float  aa;     // pixel-size antialias width
                float  seed;
            };

            // =====================================================================
            // Math / noise / easing
            // =====================================================================
            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float Hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 Hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float VNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float FBM(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float2x2 m = float2x2(0.8, -0.6, 0.6, 0.8);
                [unroll] for (int i = 0; i < 4; i++)
                {
                    v += a * VNoise(p);
                    p = mul(m, p) * 2.02 + 1.7;
                    a *= 0.5;
                }
                return v;
            }

            // Noise that tiles seamlessly around the circle (no atan2 seam)
            float PolarFBM(float angle, float r, float scale, float anim)
            {
                return FBM(float2(cos(angle), sin(angle)) * scale + float2(r * scale * 1.3 + anim, anim * 0.6));
            }

            float2 Rot(float2 v, float a)
            {
                float s, c;
                sincos(a, s, c);
                return float2(c * v.x - s * v.y, s * v.x + c * v.y);
            }

            float WrapAngle(float a)
            {
                return a - TAU * floor((a + PI) / TAU);
            }

            float EaseOutCubic(float t) { t = saturate(t); return 1.0 - pow(1.0 - t, 3.0); }
            float EaseOutExpo(float t)  { t = saturate(t); return (1.0 - exp2(-10.0 * t)) / (1.0 - exp2(-10.0)); }

            // Antialiased line: 1 inside width w, soft edge of +-aa
            float Line(float d, float w, float aa)
            {
                return 1.0 - smoothstep(w - aa, w + aa, d);
            }

            // Hexagon signed distance
            float sdHex(float2 p, float r)
            {
                const float3 k = float3(-0.866025404, 0.5, 0.577350269);
                p = abs(p);
                p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
                p -= float2(clamp(p.x, -k.z * r, k.z * r), r);
                return length(p) * sign(p.y);
            }

            // Heat ramp: dark outer -> saturated colour -> mid glow -> white-hot core
            float3 Heat(float h, float3 outer, float3 mid)
            {
                h = saturate(h);
                float3 c = lerp(outer * 0.35, outer, smoothstep(0.0, 0.35, h));
                c = lerp(c, mid, smoothstep(0.3, 0.7, h));
                c = lerp(c, lerp(mid, float3(1, 1, 1), 0.75) * 1.6, smoothstep(0.65, 1.0, h));
                return c;
            }

            // Radial spark particles with streak + soft glow.
            // gravity > 0 falls, < 0 rises.
            float Sparks(FxCtx c, float seed, float minR, float maxR, float size, float streak, float gravity)
            {
                float acc = 0.0;
                [unroll] for (int i = 0; i < SPARK_COUNT; i++)
                {
                    float fi = (float)i + seed * 17.0 + c.seed;
                    float2 h = Hash22(float2(fi, fi * 1.7 + 3.1));
                    float a = h.x * TAU;
                    float2 dir = float2(cos(a), sin(a));
                    float delay = h.y * 0.25;
                    float life = saturate((c.prog - delay) / (1.0 - delay));
                    float travel = EaseOutCubic(life);
                    float r = lerp(minR, maxR * (0.55 + 0.45 * Hash11(fi * 3.7)), travel);
                    float2 pos = dir * r + float2(0.0, -gravity * life * life);
                    float2 rel = c.st - pos;
                    float2 perp = float2(-dir.y, dir.x);
                    float al = dot(rel, dir) / (streak * (1.0 - life * 0.6));
                    float ac = dot(rel, perp);
                    float d = length(float2(al, ac));
                    float s = max(size * (1.0 - life), 1e-4);
                    float fade = (1.0 - life) * smoothstep(0.0, 0.05, life);
                    acc += ((1.0 - smoothstep(0.0, s, d)) + 0.25 * exp(-d / (s * 2.0 + 0.002))) * fade;
                }
                return acc;
            }

            // =====================================================================
            // EFFECT 1: Hextech Arc Burst
            // Counter-rotating hex rings with electric jitter, tapered blade rays,
            // white-hot core flash and sparks.
            // =====================================================================
            float4 Effect1_Hextech(FxCtx c)
            {
                float p = c.prog;
                float eo = EaseOutExpo(p * 1.15);
                float flash = pow(1.0 - p, 3.0);

                float3 cOuter = _Color.rgb;
                float3 cMid = _RimColor.rgb;
                float3 cAccent = _QualityColor.rgb;

                // electric jitter on the outer hex
                float jit = (PolarFBM(c.angle, 0.0, 4.0, c.t * 2.0 + c.seed) - 0.47) * 0.03 * (1.0 - p);

                float2 hp = Rot(c.st, p * 1.4);
                float R = lerp(0.03, 0.36, eo);
                float hd = abs(abs(sdHex(hp, R)) + jit);
                float w = lerp(0.018, 0.004, p);
                float hex = Line(hd, w, c.aa);
                float hexGlow = exp(-hd * 26.0);

                float2 hp2 = Rot(c.st, -p * 2.1 + 0.5);
                float hd2 = abs(sdHex(hp2, R * 0.6));
                float hex2 = Line(hd2, w * 0.6, c.aa) * 0.8;
                float hex2Glow = exp(-hd2 * 34.0) * 0.5;

                // 6 tapered blade rays, alternating long / short
                float k = c.angle * (6.0 / TAU) + p * 0.35;
                float angD = abs(frac(k + 0.5) - 0.5) * (TAU / 6.0);
                float odd = fmod(abs(floor(k + 0.5)), 2.0);
                float L = lerp(0.06, 0.45, eo) * lerp(1.0, 0.55, odd);
                float perp = angD * c.dist;
                float rw = lerp(0.028, 0.0015, saturate(c.dist / max(L, 1e-3)));
                float rayFade = (1.0 - smoothstep(L * 0.65, L, c.dist)) * smoothstep(0.02, 0.07, c.dist);
                float ray = Line(perp, rw, c.aa) * rayFade;
                float rayGlow = exp(-perp * 40.0) * (1.0 - smoothstep(L * 0.5, L, c.dist)) * smoothstep(0.02, 0.08, c.dist) * 0.35;

                float core = exp(-c.dist * 16.0) * flash * 3.0 + (1.0 - smoothstep(0.0, 0.1, c.dist)) * flash;

                float sp = Sparks(c, 3.1, 0.05, 0.44, 0.012, 3.0, 0.0);

                float3 colRing = Heat(0.85 * hex + 0.3, cOuter, cMid);
                float3 col = colRing * (hex * 3.0 + hexGlow * 0.8)
                           + Heat(0.6, cOuter, cMid) * (hex2 * 2.0 + hex2Glow)
                           + lerp(cAccent, float3(1, 1, 1), 0.5) * (ray * 3.5 + rayGlow * 2.0)
                           + lerp(cMid, float3(1, 1, 1), 0.6) * core
                           + lerp(cAccent, float3(1, 1, 1), 0.6) * sp * 3.0;

                float a = saturate(hex + hex2 + ray + core * 0.5 + sp * 0.8 + (hexGlow + hex2Glow) * 0.15);
                return float4(col, a);
            }

            // =====================================================================
            // EFFECT 2: Void Rift Slash
            // Two jagged rifts that sweep open, dark swirling void body, white-hot
            // rim, head flare and shards.
            // =====================================================================
            float4 Effect2_VoidRift(FxCtx c)
            {
                float p = c.prog;
                float3 voidCol = lerp(_RimColor.rgb, float3(0.55, 0.12, 1.0), 0.6);
                float3 hot = lerp(voidCol, float3(1, 1, 1), 0.65);
                float3 col = 0;
                float a = 0;

                [unroll] for (int i = 0; i < 2; i++)
                {
                    float ang = (i == 0) ? 0.62 : -0.55;
                    float start = (i == 0) ? 0.0 : 0.16;
                    float lp = saturate((p - start) / 0.6);
                    float on = step(0.0001, lp);

                    float reveal = EaseOutExpo(lp * 1.5);
                    float2 q = Rot(c.st, -ang);
                    float along = q.x;
                    float halfLen = 0.44;
                    float tap = pow(saturate(1.0 - abs(along) / halfLen), 0.7);
                    float sweep = lerp(-halfLen, halfLen, reveal);
                    float vis = 1.0 - smoothstep(sweep - 0.05, sweep, along);
                    float collapse = 1.0 - smoothstep(0.45, 1.0, lp);

                    float jag = (VNoise(float2(along * 16.0 + i * 7.3 + c.seed, floor(c.t * 9.0) * 0.37)) - 0.5) * 0.03 * tap;
                    float across = q.y + jag;
                    float wv = 0.05 * tap * collapse;
                    float ad = abs(across);

                    float body = (1.0 - smoothstep(wv * 0.85, wv + c.aa, ad)) * vis;
                    float rim = Line(abs(ad - wv), 0.005 + 0.004 * tap, c.aa) * vis * tap;
                    float hair = Line(ad, 0.003 * tap * collapse + 0.0005, c.aa) * vis * tap;
                    float outside = max(ad - wv, 0.0);
                    float glow = exp(-outside * 32.0) * vis * tap * collapse;

                    float sw = FBM(float2(along * 9.0 - c.t * 1.5, across * 18.0) + c.seed);
                    float3 voidBody = voidCol * (0.15 + sw * 0.9) * body;

                    float2 hd = float2(along - sweep, across * 1.6);
                    float headFlare = exp(-length(hd) * 34.0) * (1.0 - lp) * 3.0;

                    col += on * (voidBody + voidCol * glow * 1.2 + hot * (rim * 3.0 + hair * 3.0) + hot * headFlare);
                    float ai = saturate(body * 0.9 + rim + hair + glow * 0.2 + headFlare * 0.3);
                    a = 1.0 - (1.0 - a) * (1.0 - on * ai);
                }

                float sp = Sparks(c, 5.7, 0.02, 0.40, 0.010, 2.5, 0.0);
                col += hot * sp * 2.5;
                a = saturate(a + sp * 0.7);
                return float4(col, a);
            }

            // =====================================================================
            // EFFECT 3: Astral Spin Blade
            // Three comet-tailed crescent blades with star flares at the heads,
            // inner rune ring and a centre star twinkle.
            // =====================================================================
            float4 Effect3_Astral(FxCtx c)
            {
                float p = c.prog;
                float grow = EaseOutCubic(p * 1.6);
                float r0 = lerp(0.10, 0.28, grow);
                float rot = TAU * 1.25 * EaseOutCubic(p);
                float trailLen = lerp(2.2, 0.7, p);

                float3 cOuter = _Color.rgb;
                float3 cMid = _RimColor.rgb;
                float3 cAccent = _QualityColor.rgb;

                float3 col = 0;
                float a = 0;

                [unroll] for (int i = 0; i < 3; i++)
                {
                    float th = rot + (float)i * (TAU / 3.0);
                    float behind = WrapAngle(th - c.angle);      // > 0 means trailing the head
                    float tr = saturate(1.0 - behind / trailLen);
                    tr *= tr;
                    float head = smoothstep(-0.02, 0.03, behind) * step(behind, trailLen);

                    float rr = r0 - behind * 0.012;              // slight inward spiral
                    float d = abs(c.dist - rr);
                    float thick = 0.002 + 0.05 * tr;
                    float blade = Line(d, thick, c.aa) * head;
                    float glow = exp(-d * 30.0) * tr * head * 0.6;

                    float3 bc = Heat(tr, cOuter, cMid);
                    col += bc * (blade * 3.5 + glow * 1.2);
                    a = a + saturate(blade + glow * 0.15) - a * saturate(blade + glow * 0.15);

                    // star flare riding the blade head
                    float2 pos = float2(cos(th), sin(th)) * (r0);
                    float2 rel = c.st - pos;
                    float fl = exp(-abs(rel.x) * 70.0) * exp(-abs(rel.y) * 14.0)
                             + exp(-abs(rel.y) * 70.0) * exp(-abs(rel.x) * 14.0);
                    fl += exp(-length(rel) * 38.0) * 1.5;
                    fl *= (1.0 - smoothstep(0.7, 1.0, p));
                    col += lerp(cAccent, float3(1, 1, 1), 0.6) * fl * 1.6;
                    a = saturate(a + fl * 0.15);
                }

                // inner rune ring with ticks
                float rr2 = r0 * 0.45;
                float ringD = abs(c.dist - rr2);
                float tick = step(0.55, sin(c.angle * 24.0 + rot * 2.0));
                float inner = Line(ringD, 0.004, c.aa) * (0.5 + 0.5 * tick);
                col += lerp(cMid, cAccent, 0.5) * (inner * 2.0 + exp(-ringD * 40.0) * 0.3) * (1.0 - p);
                a = saturate(a + inner * 0.6 * (1.0 - p));

                // centre star twinkle
                float star = exp(-abs(c.st.x) * 50.0) * exp(-abs(c.st.y) * 9.0)
                           + exp(-abs(c.st.y) * 50.0) * exp(-abs(c.st.x) * 9.0);
                star *= pow(1.0 - p, 1.5);
                col += float3(1, 1, 1) * star * 1.8 + cAccent * exp(-c.dist * 20.0) * (1.0 - p) * 1.2;
                a = saturate(a + star * 0.3);

                float sp = Sparks(c, 8.3, 0.1, 0.40, 0.007, 1.5, 0.0);
                col += lerp(cAccent, float3(1, 1, 1), 0.5) * sp * 2.2;
                a = saturate(a + sp * 0.6);
                return float4(col, a);
            }

            // =====================================================================
            // EFFECT 4: Sunfire Rune Pulse
            // Expanding fire ring with noisy flame tongues, rotating segmented
            // rune ring with ticks, sun glyph, rising embers.
            // =====================================================================
            float4 Effect4_Sunfire(FxCtx c)
            {
                float p = c.prog;
                float grow = EaseOutCubic(p);
                float3 gold = lerp(_QualityColor.rgb, float3(1.0, 0.55, 0.1), 0.6);
                float3 amber = float3(1.0, 0.85, 0.4);

                float R = 0.02 + grow * 0.38;
                float off = c.dist - R;
                float ad = abs(off);

                float w = lerp(0.02, 0.004, p);
                float ring = Line(ad, w, c.aa);
                float ringGlow = exp(-ad * 28.0);

                // flame tongues licking outwards
                float n = PolarFBM(c.angle, c.dist, 6.0, -c.t * 1.6 + c.seed);
                float flameH = 0.075 * saturate(n * 1.4) * (1.0 - p);
                float flame = step(0.0, off) * (1.0 - smoothstep(0.0, max(flameH, 1e-3), off));
                float flameHeat = 1.0 - saturate(off / max(flameH, 1e-3));

                // inner heat fill
                float fill = (1.0 - smoothstep(0.0, R, c.dist)) * 0.35 * (1.0 - p);

                // rotating segmented rune ring
                float rot = p * 2.4;
                float r2 = lerp(0.08, 0.27, EaseOutCubic(p * 1.3));
                float rd = abs(c.dist - r2);
                float seg = step(-0.2, sin(c.angle * 8.0 + rot));
                float ticks = step(0.75, sin(c.angle * 36.0 - rot * 1.5));
                float rune = Line(rd, 0.006, c.aa) * seg
                           + Line(abs(c.dist - (r2 + 0.025)), 0.004, c.aa) * ticks * 0.9;

                // sun glyph: disc + 12 rays
                float sunRays = pow(saturate(cos(c.angle * 12.0 + rot * 2.0)), 6.0)
                              * smoothstep(0.03, 0.05, c.dist) * (1.0 - smoothstep(0.06, 0.17, c.dist));
                float sunDisc = Line(abs(c.dist - 0.03), 0.006, c.aa);
                float sun = (sunRays + sunDisc) * (1.0 - smoothstep(0.55, 1.0, p));

                float ember = Sparks(c, 11.9, 0.05, 0.40, 0.008, 1.6, -0.12);

                float3 col = Heat(0.8, gold, amber) * (ring * 3.2 + ringGlow * 0.8)
                           + Heat(flameHeat * 0.9 + 0.1, gold, amber) * flame * 2.6
                           + gold * fill * 1.2
                           + amber * rune * 2.6
                           + lerp(amber, float3(1, 1, 1), 0.6) * sun * 2.4
                           + lerp(amber, float3(1, 1, 1), 0.3) * ember * 2.6;

                float a = saturate(ring + flame * 0.7 + rune * 0.7 + sun * 0.7 + ember * 0.8
                                   + ringGlow * 0.15 + fill * 0.3);
                return float4(col, a);
            }

            // =====================================================================
            // EFFECT 5: Ultimate Nova Shockwave
            // Chromatic-fringed shock ring, trailing ring, volumetric god rays,
            // anamorphic lens streak, white-hot flash, heavy spark burst.
            // =====================================================================
            float ShockProfile(float d, float w, float aa)
            {
                return Line(d, w, aa) + exp(-d * 22.0) * 0.5;
            }

            float4 Effect5_Nova(FxCtx c)
            {
                float p = c.prog;
                float eo = EaseOutExpo(p * 1.2);
                float flash = pow(1.0 - p, 3.0);

                float3 cQuality = _QualityColor.rgb;
                float3 cRim = _RimColor.rgb;
                float3 cCore = _Color.rgb;

                float R = eo * 0.42;
                float w = lerp(0.05, 0.006, eo);
                float dR = lerp(0.012, 0.0, p);

                float3 ring;
                ring.r = ShockProfile(abs(c.dist - R - dR), w, c.aa);
                ring.g = ShockProfile(abs(c.dist - R), w, c.aa);
                ring.b = ShockProfile(abs(c.dist - R + dR), w, c.aa);
                float m = (ring.r + ring.g + ring.b) / 3.0;
                float3 fringe = ring - m;
                float3 ringTint = Heat(0.8, lerp(cQuality, cRim, 0.5), cQuality);

                // trailing secondary ring
                float R2 = EaseOutExpo(p * 1.2 - 0.15) * 0.30;
                float r2d = abs(c.dist - R2);
                float ring2 = Line(r2d, w * 0.5, c.aa) * 0.7 + exp(-r2d * 30.0) * 0.25;

                // god rays inside the shockwave
                float streak = PolarFBM(c.angle, 0.0, 7.0, c.t * 0.5 + c.seed);
                streak = pow(saturate(streak * 1.6 - 0.35), 2.0);
                float rays = streak * (1.0 - smoothstep(0.0, R * 0.95 + 0.01, c.dist))
                           * smoothstep(0.02, 0.10, c.dist) * (1.0 - p);

                // aura fill
                float aura = pow(saturate(1.0 - c.dist / max(R, 1e-3)), 2.0) * 0.5 * (1.0 - p);

                // core flash + anamorphic streak
                float core = exp(-c.dist * 14.0) * flash * 4.0 + (1.0 - smoothstep(0.0, 0.14, c.dist)) * flash * 2.0;
                float ana = exp(-abs(c.st.y) * 55.0) * exp(-abs(c.st.x) * 5.0) * flash * 2.0;
                float anaV = exp(-abs(c.st.x) * 70.0) * exp(-abs(c.st.y) * 12.0) * flash * 0.8;

                float sp = Sparks(c, 2.2, 0.05, 0.45, 0.012, 3.5, 0.0)
                         + Sparks(c, 6.6, 0.05, 0.38, 0.007, 2.0, 0.0);

                float3 col = ringTint * m * 3.2 + fringe * 1.6
                           + lerp(cRim, cQuality, 0.5) * ring2 * 1.8
                           + lerp(cCore, cQuality, 0.5) * rays * 2.4
                           + cRim * aura * 1.6
                           + lerp(cQuality, float3(1, 1, 1), 0.7) * core
                           + lerp(cCore, float3(1, 1, 1), 0.6) * (ana + anaV) * 1.5
                           + lerp(cQuality, float3(1, 1, 1), 0.5) * sp * 2.4;

                float a = saturate(m * 0.85 + ring2 * 0.4 + rays * 0.4 + aura * 0.5 + core * 0.4
                                   + (ana + anaV) * 0.2 + sp * 0.7);
                return float4(col, a);
            }

            // =====================================================================
            // Fragment
            // =====================================================================
            half4 frag(Varyings i) : SV_Target
            {
                FxCtx c;
                c.st = i.uv - 0.5;
                c.dist = length(c.st);
                c.angle = atan2(c.st.y, c.st.x + 1e-5);
                c.prog = _TriggerProgress;
                c.t = _Time.y;
                c.aa = max(fwidth(c.dist) * 1.5, 0.0015);
                c.seed = _Seed;

                if (c.prog <= 0.001 || c.prog >= 0.999)
                    return half4(0, 0, 0, 0);

                float4 acc = float4(0, 0, 0, 0);

                if (_Effect1Active > 0.5) { float4 f = Effect1_Hextech(c);  acc.rgb += f.rgb; acc.a = acc.a + f.a - acc.a * f.a; }
                if (_Effect2Active > 0.5) { float4 f = Effect2_VoidRift(c);  acc.rgb += f.rgb; acc.a = acc.a + f.a - acc.a * f.a; }
                if (_Effect3Active > 0.5) { float4 f = Effect3_Astral(c);    acc.rgb += f.rgb; acc.a = acc.a + f.a - acc.a * f.a; }
                if (_Effect4Active > 0.5) { float4 f = Effect4_Sunfire(c);   acc.rgb += f.rgb; acc.a = acc.a + f.a - acc.a * f.a; }
                if (_Effect5Active > 0.5) { float4 f = Effect5_Nova(c);      acc.rgb += f.rgb; acc.a = acc.a + f.a - acc.a * f.a; }

                // global envelope + soft circular fade so nothing is cut by the quad edge
                float env = smoothstep(0.0, 0.05, c.prog) * (1.0 - smoothstep(0.78, 1.0, c.prog));
                float mask = 1.0 - smoothstep(0.38, 0.5, c.dist);

                float3 rgb = acc.rgb * (env * mask * _Intensity);
                float a = saturate(acc.a * env * mask);

                // optional filmic shoulder for LDR canvases (set _SoftClip = 0 for HDR + Bloom)
                rgb = lerp(rgb, 1.0 - exp(-rgb * 1.8), _SoftClip);

                // Image.color tint / fade (premultiplied)
                rgb *= i.color.rgb * i.color.a;
                a *= i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, i.posOS) * step(i.posOS, _ClipRect.zw);
                float clipV = inside.x * inside.y;
                rgb *= clipV;
                a *= clipV;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(a - 0.001);
                #endif

                return half4(rgb, a);
            }
            ENDHLSL
        }
    }
}
