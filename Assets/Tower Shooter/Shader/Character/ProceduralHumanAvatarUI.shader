// ============================================================================
//  UI/Procedural Human Avatar (URP)
//  Fully procedural, texture-less human avatar for UI Image (Canvas) in URP.
//  - Deterministic: the same _Seed always produces the same person.
//  - Mirrors ProceduralHumanAvatarUI.cs: age, 5 expressions, Apollo A7L suit,
//    death / grayscale state.
//  - Look: SDF layout + pseudo-3D height-field normals, wrapped diffuse with
//    subsurface tint, T-zone specular, rim fill, AO in sockets / nose / lips,
//    iris fibres, catch-lights, lashes, strand hair, stubble / beard,
//    wrinkles, age spots, quilted suit fabric with metal neck rings.
//  Use on a UI Image with NO sprite, square rect (aspect is auto-corrected to
//  fit height).
// ============================================================================
Shader "UI/Procedural Human Avatar (URP)"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Identity)]
        _Seed ("Seed", Float) = 12345
        [Enum(Female, 0, Male, 1)] _Gender ("Gender", Float) = 1
        _Age ("Age (0 child, 0.3 youth, 0.6 adult, 1 elder)", Range(0,1)) = 0.6
        [Enum(WarmLeader,0,Inspirational,1,Approachable,2,Determined,3,Confident,4)] _Expression ("Expression", Float) = 0
        [Enum(ShortSide,0,Buzz,1,Receding,2,Long,3,Curly,4,Bald,5)] _HairStyle ("Hair Style", Float) = 0
        [Enum(None,0,Stubble,1,Beard,2)] _FacialHair ("Facial Hair", Float) = 0

        [Header(State)]
        _Dead ("Dead (0..1)", Range(0,1)) = 0
        _Blink ("Blink (0..1)", Range(0,1)) = 0
        _Look ("Look (xy, -1..1)", Vector) = (0,0,0,0)

        [Header(Look)]
        _SuitTint ("Suit Tint", Color) = (1,1,1,1)
        _BgColor ("Background (alpha = opacity)", Color) = (0.07,0.09,0.13,0)

        // --- UI plumbing ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
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
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ClipRect;
                float4 _SuitTint;
                float4 _BgColor;
                float4 _Look;
                float _Seed;
                float _Gender;
                float _Age;
                float _Expression;
                float _HairStyle;
                float _FacialHair;
                float _Dead;
                float _Blink;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 worldPos   : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = IN.positionOS;
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // =====================================================================
            //  Utilities
            // =====================================================================
            #define PI 3.14159265

            static float g_aa = 0.002;

            float3 S(float3 c) { return pow(max(c, 0.0), 2.2); }          // authoring sRGB -> linear
            float Fill(float d) { return 1.0 - smoothstep(-g_aa, g_aa, d); }
            float FillS(float d, float s) { s = max(s, g_aa); return 1.0 - smoothstep(-s, s, d); }
            float Gauss(float x, float w) { x /= w; return exp(-x * x); }
            float Bell(float2 p, float2 c, float2 r) { float2 v = (p - c) / r; return exp(-dot(v, v)); }
            float Bell2(float2 p, float x, float y, float rx, float ry)
            {
                return Bell(p, float2(x, y), float2(rx, ry)) + Bell(p, float2(-x, y), float2(rx, ry));
            }

            uint Pcg(uint v)
            {
                uint s = v * 747796405u + 2891336053u;
                uint w = ((s >> ((s >> 28u) + 4u)) ^ s) * 277803737u;
                return (w >> 22u) ^ w;
            }
            float Rnd(uint seed, uint i)
            {
                return (float)(Pcg(seed ^ Pcg(i * 9781u + 6271u)) & 0xFFFFFFu) * (1.0 / 16777216.0);
            }
            float Hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }
            float Vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            float Fbm(float2 p)
            {
                float v = 0.0, a = 0.5;
                [unroll] for (int i = 0; i < 4; i++) { v += a * Vnoise(p); p = p * 2.03 + 17.3; a *= 0.5; }
                return v;
            }

            float smin(float a, float b, float k)
            {
                float h = saturate(0.5 + 0.5 * (b - a) / k);
                return lerp(b, a, h) - k * h * (1.0 - h);
            }
            float sdEllipse(float2 p, float2 r)
            {
                float k0 = length(p / r);
                float k1 = length(p / (r * r));
                return k0 * (k0 - 1.0) / max(k1, 1e-5);
            }
            float sdRoundBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }
            float sdSegment(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a, ba = b - a;
                float h = saturate(dot(pa, ba) / dot(ba, ba));
                return length(pa - ba * h);
            }
            // Horizontal lens (almond): half length a, half height h
            float sdLens(float2 p, float a, float h)
            {
                h = max(h, 0.0015);
                a = max(a, h + 0.001);
                float r = (h + a * a / h) * 0.5;
                float d = (a * a / h - h) * 0.5;
                float2 pp = abs(p.yx);
                float b = sqrt(max(r * r - d * d, 1e-6));
                return ((pp.y - b) * d > pp.x * b) ? length(pp - float2(0.0, b)) : length(pp - float2(-d, 0.0)) - r;
            }
            float3 Shoulder(float3 c)   // soft highlight roll-off
            {
                float3 hi = 0.8 + 0.2 * (1.0 - exp(-(c - 0.8) / 0.2));
                return float3(c.r < 0.8 ? c.r : hi.r, c.g < 0.8 ? c.g : hi.g, c.b < 0.8 ? c.b : hi.b);
            }

            // =====================================================================
            //  Genetics (all from the seed)
            // =====================================================================
            struct Genes
            {
                float melanin, undertone, masc;
                float3 hair, eye;
                float headW, headH, jawW, jawSq, chin;
                float eyeSpace, eyeSize, eyeScaleY, eyeTilt;
                float browThick, browArch, noseW, noseL, lipW, lipFull;
                float earSize, cheek, hairVol, hairGray, freckles, part;
                int hairStyle, facial;
            };

            float3 PickHair(float r, float mel)
            {
                if (mel > 0.62) r *= 0.40; else if (mel > 0.40) r *= 0.70;
                float3 c;
                if (r < 0.30)      c = float3(0.10, 0.09, 0.085);
                else if (r < 0.55) c = float3(0.23, 0.15, 0.11);
                else if (r < 0.75) c = float3(0.38, 0.25, 0.15);
                else if (r < 0.85) c = float3(0.52, 0.27, 0.15);
                else if (r < 0.95) c = float3(0.72, 0.56, 0.32);
                else               c = float3(0.85, 0.72, 0.46);
                return S(c);
            }
            float3 PickEye(float r, float mel)
            {
                if (mel > 0.55) r *= 0.35; else if (mel > 0.35) r *= 0.60;
                float3 c;
                if (r < 0.35)      c = float3(0.26, 0.14, 0.08);
                else if (r < 0.50) c = float3(0.42, 0.25, 0.12);
                else if (r < 0.62) c = float3(0.48, 0.38, 0.14);
                else if (r < 0.75) c = float3(0.26, 0.45, 0.24);
                else if (r < 0.92) c = float3(0.24, 0.44, 0.68);
                else               c = float3(0.50, 0.57, 0.63);
                return S(c);
            }

            Genes Roll(uint s, float age)
            {
                Genes G;
                G.melanin   = Rnd(s, 0u);
                G.undertone = Rnd(s, 1u) * 2.0 - 1.0;
                G.masc      = (_Gender < -0.5) ? ((Rnd(s, 2u) > 0.5) ? 1.0 : 0.0) : saturate(_Gender);
                G.hair      = PickHair(Rnd(s, 3u), G.melanin);
                G.eye       = PickEye(Rnd(s, 4u), G.melanin);

                G.headW  = lerp(0.95, 1.06, Rnd(s, 5u));
                G.headH  = lerp(0.96, 1.05, Rnd(s, 6u));
                G.jawW   = lerp(0.93, 1.07, Rnd(s, 7u));
                G.jawSq  = saturate(Rnd(s, 8u) * 0.6 + G.masc * 0.4);
                G.chin   = lerp(0.94, 1.08, Rnd(s, 9u));
                G.jawW  *= lerp(0.96, 1.03, G.masc);

                G.eyeSpace  = lerp(-1.0, 1.0, Rnd(s, 10u));
                G.eyeSize   = lerp(0.94, 1.08, Rnd(s, 11u));
                G.eyeScaleY = lerp(0.92, 1.06, Rnd(s, 12u));
                G.eyeTilt   = lerp(-0.03, 0.10, Rnd(s, 13u)) - 0.02 * G.masc;

                G.browThick = lerp(0.8, 1.3, Rnd(s, 14u)) * lerp(0.85, 1.15, G.masc);
                G.browArch  = lerp(0.004, 0.012, Rnd(s, 15u)) * lerp(1.25, 0.75, G.masc);
                G.noseW     = lerp(0.90, 1.12, Rnd(s, 16u)) * lerp(0.97, 1.05, G.masc);
                G.noseL     = lerp(0.90, 1.10, Rnd(s, 17u));
                G.lipW      = lerp(0.94, 1.08, Rnd(s, 18u));
                G.lipFull   = lerp(0.85, 1.25, Rnd(s, 19u)) * lerp(1.10, 0.92, G.masc);
                G.earSize   = lerp(0.90, 1.10, Rnd(s, 20u));
                G.cheek     = lerp(0.85, 1.15, Rnd(s, 21u));
                G.hairVol   = lerp(0.96, 1.06, Rnd(s, 22u));
                G.hairGray  = Rnd(s, 23u);
                G.freckles  = (Rnd(s, 24u) > 0.78) ? lerp(0.4, 1.0, Rnd(s, 25u)) * (1.0 - G.melanin * 0.8) : 0.0;
                G.part      = (Rnd(s, 26u) > 0.5) ? 1.0 : -1.0;

                float hr = Rnd(s, 27u);
                int hs;
                if (G.masc > 0.5)
                {
                    if (hr < 0.35) hs = 0; else if (hr < 0.50) hs = 1; else if (hr < 0.62) hs = 2;
                    else if (hr < 0.78) hs = 4; else if (hr < 0.88) hs = 5; else hs = 3;
                }
                else
                {
                    if (hr < 0.50) hs = 3; else if (hr < 0.72) hs = 0; else hs = 4;
                }
                G.hairStyle = (_HairStyle > -0.5) ? (int)round(_HairStyle) : hs;

                float fr = Rnd(s, 28u);
                int fh = 0;
                if (G.masc > 0.5) { if (fr < 0.35) fh = 0; else if (fr < 0.70) fh = 1; else fh = 2; }
                if (_FacialHair > -0.5) fh = (int)round(_FacialHair);
                if (age < 0.25) fh = 0;
                G.facial = fh;
                return G;
            }

            // =====================================================================
            //  Face layout (age + expression driven)
            // =====================================================================
            struct Face
            {
                float c, e;                         // childness, elderness
                float smile, smirk, open, squint, browLift, browAng;
                float eyeX, eyeY, eyeA, hU, hL, tilt, browY;
                float noseY, mouthY, mouthHW, hUp, hLo;
                float2 look;
            };

            Face Layout(Genes G, float age, float expr)
            {
                Face F;
                F.c = saturate(1.0 - age / 0.3);
                F.e = saturate((age - 0.55) / 0.45);

                float smile = 0, smirk = 0, open = 0, squint = 0, lift = 0, ang = 0;
                int ex = (int)round(expr);
                if (ex == 0)      { smile = 0.55; open = 0.12; lift = 0.004;  ang = 0.04;  squint = 0.12; }
                else if (ex == 1) { smile = 0.90; open = 0.55; lift = 0.012;  ang = 0.08;  squint = 0.20; }
                else if (ex == 2) { smile = 0.40; open = 0.0;  lift = 0.005;  ang = 0.03;  squint = 0.06; }
                else if (ex == 3) { smile = -0.10; open = 0.0; lift = -0.008; ang = -0.16; squint = 0.22; }
                else              { smile = 0.25; smirk = 0.7; open = 0.0;  lift = 0.002;  ang = -0.02; squint = 0.10; }

                float alive = 1.0 - _Dead;
                F.smile = smile * alive;  F.smirk = smirk * alive;  F.open = open * alive;
                F.squint = squint * alive + _Dead * 0.1;
                F.browLift = lift * alive; F.browAng = ang * alive;

                F.eyeX = 0.118 + 0.012 * G.eyeSpace + 0.012 * F.c;
                F.eyeY = 0.035 - 0.055 * F.c;
                float es = G.eyeSize * (1.0 + 0.22 * F.c);
                F.eyeA = 0.060 * es;
                float openAmt = lerp(1.0, 0.22, _Dead) * (1.0 - saturate(_Blink)) * (1.0 - 0.12 * F.e);
                float eh = 0.023 * es * G.eyeScaleY * openAmt;
                F.hU = eh * 1.25 * (1.0 - 0.25 * F.e) * (1.0 - 0.30 * F.squint);
                F.hL = eh * 0.80 * (1.0 - 0.60 * F.squint);
                F.tilt = G.eyeTilt;
                F.browY = F.eyeY + 0.052 + F.browLift + 0.004 * F.c;

                F.noseY = -0.085 + 0.015 * F.c;
                F.mouthY = -0.205 + 0.030 * F.c;
                F.mouthHW = 0.095 * G.lipW * (1.0 + 0.06 * F.smile) * (1.0 - 0.10 * F.c);
                F.hUp = 0.0105 * G.lipFull * (1.0 - 0.35 * F.e) * (1.0 + 0.15 * F.c);
                F.hLo = 0.0150 * G.lipFull * (1.0 - 0.35 * F.e) * (1.0 + 0.15 * F.c);
                F.look = _Look.xy;
                return F;
            }

            float HeadSDF(float2 q, Genes G, float c)
            {
                float cran = sdEllipse(q - float2(0.0, 0.075), float2(0.30 * G.headW * (1.0 + 0.05 * c), 0.315 * G.headH));
                float jw = 0.250 * G.jawW * (1.0 - 0.10 * c);
                float jh = 0.205 * (1.0 - 0.18 * c) * G.chin;
                float rad = min(lerp(0.07, 0.16, saturate(1.0 - G.jawSq + 0.35 * c)), min(jw, jh) * 0.95);
                float jaw = sdRoundBox(q - float2(0.0, -0.325 + jh), float2(jw, jh), rad);
                return smin(cran, jaw, 0.10);
            }

            // Smooth height field -> pseudo 3D normals
            float Height(float2 q, Genes G, Face F)
            {
                float d = HeadSDF(q, G, F.c);
                float depth = saturate(-d / 0.17);
                float h = 0.20 * sqrt(depth * (2.0 - depth));
                float nS = lerp(1.0, 0.72, F.c);
                h += 0.013 * nS * Bell(q, float2(0.0, F.noseY + 0.055), float2(0.016, 0.075));
                h += 0.017 * nS * Bell(q, float2(0.0, F.noseY - 0.004), float2(0.030, 0.024));
                h += 0.008 * nS * Bell2(q, 0.040 * G.noseW, F.noseY - 0.012, 0.018, 0.016);
                h += (0.007 + 0.006 * G.masc) * Bell2(q, F.eyeX + 0.005, F.eyeY + 0.060, 0.080, 0.020);
                h -= 0.010 * Bell2(q, F.eyeX, F.eyeY, 0.072, 0.040);
                h -= 0.004 * F.e * Bell2(q, F.eyeX, F.eyeY - 0.04, 0.06, 0.014);
                h += 0.014 * G.cheek * (1.0 + 0.5 * F.smile) * Bell2(q, 0.165, -0.085, 0.085, 0.075);
                h -= 0.009 * F.e * Bell2(q, 0.16, -0.15, 0.05, 0.06);
                h += 0.009 * G.lipFull * Bell(q, float2(0.0, F.mouthY), float2(F.mouthHW * 0.9, 0.020));
                h -= 0.005 * Bell(q, float2(0.0, F.mouthY - 0.040), float2(0.05, 0.011));
                h -= 0.004 * Bell(q, float2(0.0, F.mouthY + 0.032), float2(0.010, 0.016));
                h += 0.011 * (1.0 + 0.3 * G.masc) * Bell(q, float2(0.0, -0.275 + 0.03 * F.c), float2(0.06, 0.038));
                h += 0.006 * Bell(q, float2(0.0, 0.25), float2(0.15, 0.09));
                h -= 0.004 * Bell2(q, F.mouthHW + 0.004, F.mouthY + F.smile * 0.012, 0.012, 0.012);
                return h;
            }

            // =====================================================================
            //  Lighting
            // =====================================================================
            float3 LightDir() { return normalize(float3(-0.50, 0.62, 0.60)); }

            float3 SuitLight(float3 alb, float3 N, float ao)
            {
                float3 L = LightDir();
                float diff = saturate(dot(N, L) * 0.5 + 0.5);
                diff *= diff;
                float3 amb = lerp(float3(0.10, 0.11, 0.14), float3(0.30, 0.33, 0.40), N.y * 0.5 + 0.5);
                float3 c = alb * (float3(1.0, 0.96, 0.90) * diff + amb) * ao;
                float3 H = normalize(L + float3(0, 0, 1));
                c += pow(saturate(dot(N, H)), 24.0) * 0.06 * ao;
                return c;
            }

            float3 ShadeSkin(float3 alb, float3 N, float ao, float spM, float rough)
            {
                float3 L = LightDir();
                float ndl = dot(N, L);
                float wrap = saturate((ndl + 0.5) / 1.5);
                float sss = saturate(1.0 - abs(ndl - 0.15) * 2.2);
                float3 amb = lerp(float3(0.16, 0.15, 0.17), float3(0.28, 0.30, 0.36), N.y * 0.5 + 0.5);
                float3 c = alb * (float3(1.0, 0.95, 0.88) * wrap * 0.92 + amb) * ao;
                c += alb * float3(1.0, 0.32, 0.18) * sss * 0.28 * ao;
                float3 H = normalize(L + float3(0, 0, 1));
                float nh = saturate(dot(N, H));
                c += (pow(nh, rough) * 0.10 * spM + pow(nh, 6.0) * 0.02) * float3(1.0, 0.96, 0.92) * ao;
                float rim = pow(1.0 - saturate(N.z), 2.5) * saturate(N.x * 0.8 + 0.5);
                c += float3(0.20, 0.30, 0.45) * alb * rim * 0.9 * ao;
                return c;
            }

            float3 SkinAlbedo(Genes G, Face F)
            {
                float m = G.melanin;
                float3 c0 = float3(0.95, 0.76, 0.66);
                float3 c1 = float3(0.82, 0.58, 0.42);
                float3 c2 = float3(0.62, 0.40, 0.27);
                float3 c3 = float3(0.40, 0.25, 0.17);
                float3 c4 = float3(0.24, 0.15, 0.10);
                float3 c;
                if (m < 0.25)      c = lerp(c0, c1, m / 0.25);
                else if (m < 0.50) c = lerp(c1, c2, (m - 0.25) / 0.25);
                else if (m < 0.75) c = lerp(c2, c3, (m - 0.50) / 0.25);
                else               c = lerp(c3, c4, (m - 0.75) / 0.25);
                c.r *= 1.0 + 0.05 * G.undertone;
                c.b *= 1.0 - 0.10 * G.undertone;
                c = lerp(c, dot(c, float3(0.33, 0.33, 0.33)) * float3(1.04, 0.97, 0.94), F.e * 0.20);
                return S(saturate(c));
            }

            // =====================================================================
            //  Face texture-free "maps": blush, wrinkles, AO, freckles, spots
            // =====================================================================
            void FaceMaps(float2 q, Genes G, Face F, float age, inout float3 alb, out float ao, out float spM)
            {
                ao = 1.0;
                float e2 = saturate((age - 0.45) * 1.8);
                float c = F.c;
                float2 aq = float2(abs(q.x), q.y);
                float detail = saturate(1.0 - g_aa * 260.0);

                // colour variation
                alb *= lerp(float3(0.97, 0.93, 0.92), float3(1.03, 1.0, 1.0), Fbm(q * 6.0 + 3.1));
                float blush = Bell(aq, float2(0.16, -0.075), float2(0.085, 0.06)) * 0.20 * (1.0 - 0.55 * G.melanin) * (1.0 + 0.5 * F.smile + 0.4 * c);
                blush += Bell(q, float2(0.0, F.noseY), float2(0.035, 0.04)) * 0.10 * (1.0 - 0.5 * G.melanin);
                alb = lerp(alb, alb * float3(1.18, 0.78, 0.74), saturate(blush));
                float ue = Bell(aq, float2(F.eyeX, F.eyeY - 0.038), float2(0.06, 0.02)) * (0.10 + 0.25 * F.e);
                alb = lerp(alb, alb * float3(0.82, 0.78, 0.90), ue);

                // freckles
                if (G.freckles > 0.0)
                {
                    float fm = Bell(aq, float2(0.09, -0.03), float2(0.17, 0.09));
                    float fr = smoothstep(0.70, 0.80, Vnoise(q * 95.0));
                    alb = lerp(alb, alb * float3(0.72, 0.55, 0.45), fr * fm * G.freckles * 0.8);
                }
                // age spots
                float sp = smoothstep(0.74, 0.80, Vnoise(q * 16.0 + 11.3)) * e2 * 0.35;
                alb = lerp(alb, alb * float3(0.70, 0.55, 0.45), sp);
                // pores
                alb *= 1.0 - 0.035 * detail + 0.07 * detail * Vnoise(q * 310.0);

                // ---- ambient occlusion ----
                ao *= 1.0 - 0.30 * saturate(Bell2(q, F.eyeX, F.eyeY + 0.01, 0.09, 0.055)) * (0.7 + 0.5 * F.e);
                ao *= 1.0 - 0.75 * saturate(Bell2(q, 0.028 * G.noseW, F.noseY - 0.018, 0.011, 0.006));
                ao *= 1.0 - 0.30 * Bell(q, float2(0.006, F.noseY - 0.040), float2(0.040, 0.012));
                ao *= 1.0 - 0.20 * saturate(Bell2(q, 0.068 * G.noseW, F.noseY, 0.012, 0.030));
                ao *= 1.0 - 0.22 * Bell(q, float2(0.0, F.mouthY - 0.052), float2(0.06, 0.014));
                ao *= 1.0 - 0.10 * saturate(Bell2(q, 0.012, F.mouthY + 0.035, 0.005, 0.020));

                // ---- wrinkles ----
                float wr = e2 * 0.20 * (1.0 - c) + saturate(-F.browAng * 2.5) * 0.10;
                [unroll] for (int i = 0; i < 3; i++)
                {
                    float y = 0.215 + 0.036 * (float)i;
                    float wob = 0.004 * sin(q.x * 26.0 + (float)i * 2.1) + 0.003 * (Vnoise(float2(q.x * 18.0, (float)i)) - 0.5);
                    float l = Gauss(q.y - (y + wob), 0.0028) * (1.0 - smoothstep(0.10, 0.16, abs(q.x)));
                    alb *= 1.0 - wr * l;
                }
                float glab = Gauss(abs(q.x) - 0.012, 0.003) * smoothstep(F.browY - 0.01, F.browY + 0.01, q.y) * (1.0 - smoothstep(F.browY + 0.03, F.browY + 0.05, q.y));
                alb *= 1.0 - glab * (e2 * 0.14 + saturate(-F.browAng * 2.5) * 0.12);

                float2 op = float2(F.eyeX + F.eyeA + 0.008, F.eyeY - 0.002);
                float cf = (0.05 + 0.22 * e2 + 0.15 * F.smile) * (1.0 - c);
                [unroll] for (int j = 0; j < 3; j++)
                {
                    float an = -0.45 + 0.45 * (float)j;
                    float2 dir = float2(cos(an), sin(an));
                    float dd = sdSegment(aq, op, op + dir * 0.04);
                    alb *= 1.0 - 0.9 * cf * Gauss(dd, 0.0022);
                }

                float2 na = float2(0.058 * G.noseW, F.noseY - 0.012);
                float2 nb = float2(F.mouthHW + 0.028 + 0.01 * F.smile, F.mouthY + 0.012);
                float2 nm = (na + nb) * 0.5 + float2(0.012, 0.0);
                float dn = min(sdSegment(aq, na, nm), sdSegment(aq, nm, nb));
                float ns = (0.06 + 0.35 * e2 + 0.22 * F.smile) * (1.0 - c * 0.8);
                ao *= 1.0 - ns * Gauss(dn, 0.004 + 0.002 * e2);

                float dm = sdSegment(aq, float2(F.mouthHW * 0.98, F.mouthY - 0.01), float2(F.mouthHW * 0.90, F.mouthY - 0.085));
                ao *= 1.0 - e2 * 0.25 * Gauss(dm, 0.003);

                // specular mask (T-zone)
                spM = 0.5 + 0.8 * Bell(q, float2(0.0, 0.24), float2(0.14, 0.07))
                    + 0.8 * Bell(q, float2(0.0, F.noseY + 0.03), float2(0.03, 0.07))
                    + 0.5 * Bell(aq, float2(0.15, -0.05), float2(0.06, 0.05));
                spM *= 1.0 - 0.35 * F.e;

                // death pallor
                alb = lerp(alb, dot(alb, float3(0.33, 0.33, 0.33)) * float3(0.85, 0.92, 1.05), _Dead * 0.7);
            }

            // =====================================================================
            //  Eyes / brows / mouth
            // =====================================================================
            void DrawEye(float2 q, float side, Genes G, Face F, float alive, inout float3 col)
            {
                float2 e = float2(side * q.x - F.eyeX, q.y - F.eyeY);
                float ct = cos(F.tilt), st = sin(F.tilt);
                float2 et = float2(ct * e.x + st * e.y, -st * e.x + ct * e.y);

                float dU = sdLens(et, F.eyeA, F.hU);
                float dL = sdLens(et, F.eyeA, F.hL);
                float d = (et.y > 0.0) ? dU : dL;
                float isUp = (et.y > 0.0) ? 1.0 : 0.0;

                // lid shadows and crease on the skin around the eye
                col *= 1.0 - 0.40 * exp(-max(d, 0.0) / 0.012) * lerp(0.35, 1.0, isUp) * (1.0 - F.c * 0.3);
                float crease = Gauss(d - (0.020 - 0.008 * F.e), 0.0035) * isUp * step(0.0, d);
                col *= 1.0 - (0.22 + 0.20 * F.e) * crease;
                col *= 1.0 - (0.10 + 0.3 * F.e) * Gauss(d - 0.012, 0.004) * (1.0 - isUp) * step(0.0, d);

                float m = Fill(d);
                if (m > 0.001)
                {
                    float3 sclera = S(float3(0.93, 0.91, 0.89));
                    sclera = lerp(sclera, S(float3(0.86, 0.78, 0.62)), F.e * 0.4);
                    float inner = saturate(-et.x / F.eyeA);
                    sclera = lerp(sclera, S(float3(0.85, 0.42, 0.40)), smoothstep(0.6, 1.0, inner) * 0.6);
                    float topT = saturate(et.y / max(F.hU, 1e-4));
                    sclera *= 1.0 - 0.35 * pow(saturate(abs(et.x) / F.eyeA), 3.0);

                    float2 ic = float2(side * F.look.x * 0.30 * F.eyeA, F.look.y * 0.35 * F.hU);
                    float ri = 0.0245 * G.eyeSize * (1.0 + 0.12 * F.c);
                    float2 id = et - ic;
                    float rd = length(id) / ri;
                    float ia = atan2(id.y, id.x);
                    float fib = Vnoise(float2(ia * 6.0, rd * 5.0)) * 0.5 + Vnoise(float2(ia * 18.0 + 3.0, rd * 9.0)) * 0.5;
                    float3 ir = G.eye * (0.55 + 0.9 * fib);
                    float coll = 1.0 - smoothstep(0.34, 0.62, rd);
                    ir = lerp(ir, ir * 1.6 + S(float3(0.25, 0.15, 0.05)), coll * 0.55);
                    ir = lerp(ir, ir * 0.25, smoothstep(0.70, 1.0, rd));
                    float pr = 0.36 * (1.0 + 0.15 * F.c);
                    float pupil = 1.0 - smoothstep(pr - 0.04, pr + 0.04, rd);
                    ir = lerp(ir, S(float3(0.01, 0.01, 0.012)), pupil);
                    ir += G.eye * 0.25 * smoothstep(0.0, 1.0, -id.y / ri) * (1.0 - pupil);
                    float iM = 1.0 - smoothstep(0.93, 1.0, rd);
                    float3 ec = lerp(sclera, ir, iM);

                    // upper lid shadow over the eyeball
                    ec *= 1.0 - 0.65 * (1.0 - smoothstep(0.0, 0.020, -d)) * isUp;
                    ec *= 1.0 - 0.20 * (1.0 - smoothstep(0.0, 0.010, -d)) * (1.0 - isUp);

                    // catch-lights (window + dot)
                    float2 cl = id / ri;
                    float cat = 1.0 - smoothstep(-0.03, 0.03, sdRoundBox(cl - float2(-0.38, 0.36), float2(0.20, 0.15), 0.06));
                    float cat2 = 1.0 - smoothstep(0.0, 0.05, length(cl - float2(0.42, -0.40)) - 0.07);
                    float cornea = 1.0 - smoothstep(1.0, 1.15, rd);
                    ec = lerp(ec, float3(1.4, 1.4, 1.4), saturate(cat + cat2 * 0.7) * alive * cornea);

                    // tear line
                    ec += Gauss(d + 0.004, 0.0022) * (1.0 - isUp) * 0.30;
                    col = lerp(col, ec, m);
                }

                // lashes
                float lt = (0.0030 + 0.0030 * saturate(et.x / F.eyeA) + 0.0012 * (1.0 - G.masc)) * lerp(0.35, 1.0, isUp);
                float lashA = Fill(abs(d) - lt);
                col = lerp(col, G.hair * 0.25 + S(float3(0.02, 0.018, 0.018)), saturate(lashA) * 0.95);
            }

            void DrawBrow(float2 q, float side, Genes G, Face F, float3 hairC, inout float3 col)
            {
                float bx = side * q.x - (F.eyeX + 0.006);
                float L = 0.082 * (1.0 - 0.1 * F.c) * (1.0 + 0.05 * G.eyeSize);
                float nx = bx / L;
                float yc = F.browY - F.browAng * bx * 1.0
                         + G.browArch * (1.0 - nx * nx)
                         - 0.006 * pow(saturate(nx), 2.0)
                         + ((side > 0.0) ? F.smirk * 0.010 : 0.0);
                float th0 = 0.0055 * G.browThick * (1.0 - 0.3 * F.c) * (1.0 + 0.3 * F.e);
                float th = th0 * lerp(1.1, 0.35, saturate(nx * 0.5 + 0.5)) * sqrt(saturate(1.0 - pow(abs(nx), 4.0) * 0.85));
                float dy = abs(q.y - yc) - th;
                float strand = Vnoise(float2(bx * 60.0, (q.y - yc) * 700.0));
                float edge = 1.0 - smoothstep(0.95, 1.05, abs(nx));
                float a = Fill(dy + (strand - 0.5) * 0.0022) * edge * (0.55 + 0.55 * strand);
                col = lerp(col, hairC * (0.60 + 0.50 * strand), saturate(a) * 0.95);
            }

            float LipShape(float2 q, Face F, out float mline, out float gap, out float hUp, out float hLo, out float mx)
            {
                mx = q.x / F.mouthHW;
                float m2 = saturate(1.0 - mx * mx);
                mline = F.mouthY + F.smile * 0.020 * mx * mx + F.smirk * 0.010 * mx - F.smile * 0.006;
                gap = F.open * 0.011 * m2;
                float cupid = 1.0 - 0.30 * exp(-(mx / 0.16) * (mx / 0.16))
                            + 0.20 * exp(-((abs(mx) - 0.30) / 0.13) * ((abs(mx) - 0.30) / 0.13));
                hUp = F.hUp * pow(m2, 0.55) * cupid;
                hLo = F.hLo * pow(m2, 0.65);
                float yTop = mline + gap + hUp;
                float yBot = mline - gap - hLo;
                float dX = abs(q.x) - F.mouthHW;
                return max(max(q.y - yTop, yBot - q.y), dX);
            }

            void DrawMouth(float2 q, Genes G, Face F, float3 skinLin, float alive, inout float3 col)
            {
                float mline, gap, hUp, hLo, mx;
                float dL = LipShape(q, F, mline, gap, hUp, hLo, mx);
                float aL = FillS(dL, g_aa + 0.0007);

                // corner shadow
                float cd = Bell(float2(abs(q.x), q.y), float2(F.mouthHW + 0.004, F.mouthY + F.smile * 0.014), float2(0.010, 0.008));
                col *= 1.0 - 0.35 * cd;

                float3 lipBase = lerp(S(float3(0.80, 0.46, 0.46)), S(float3(0.44, 0.22, 0.20)), saturate(G.melanin * 1.25));
                lipBase = lerp(lipBase, skinLin * float3(1.0, 0.85, 0.80), saturate(F.e * 0.35 + _Dead * 0.8));
                float up = smoothstep(-0.0015, 0.0015, q.y - mline);
                float3 lc = lerp(lipBase * 1.06, lipBase * 0.86, up);
                float tU = saturate((q.y - mline) / max(hUp + gap, 1e-4));
                float tL = saturate((mline - q.y) / max(hLo + gap, 1e-4));
                lc *= lerp(lerp(1.0, 0.82, tL), lerp(1.05, 0.78, tU), up);
                float wr = Vnoise(float2(q.x * 520.0, q.y * 40.0));
                lc *= 0.93 + 0.14 * wr;
                float spec = Gauss(q.y - (mline - gap - hLo * 0.45), hLo * 0.28 + 1e-4) * pow(saturate(1.0 - mx * mx), 0.8) * 0.45 * (1.0 - up) * alive * (0.6 + 0.8 * wr);
                lc += spec * float3(1.0, 0.95, 0.9) * 0.55;
                float edge = smoothstep(0.0, 0.004, -dL);
                lc = lerp(lerp(lc, skinLin * 0.9, 0.35), lc, edge);
                float m4 = saturate(abs(mx));
                float crease = Gauss(q.y - mline, 0.0016 + 0.0006 * (1.0 - F.open)) * (1.0 - m4 * m4 * m4 * m4) * (1.0 - smoothstep(0.001, 0.003, gap));
                lc = lerp(lc, lc * 0.25 + float3(0.01, 0.0, 0.0), saturate(crease) * 0.85);
                col = lerp(col, lc, aL);

                if (gap > 0.0015)
                {
                    float tv = (q.y - mline) / max(gap, 1e-4);
                    float tl = abs(frac(q.x / 0.0118 + 0.5) - 0.5);
                    float3 teeth = S(float3(0.92, 0.90, 0.84)) * (0.55 + 0.45 * (1.0 - pow(saturate(abs(mx)), 3.0)));
                    teeth *= 1.0 - 0.22 * (1.0 - smoothstep(0.0, 0.06, tl));
                    teeth *= 1.0 - 0.5 * smoothstep(0.6, 1.0, tv);
                    float3 inner = lerp(S(float3(0.16, 0.05, 0.06)), teeth, smoothstep(-0.35, -0.15, tv));
                    col = lerp(col, inner, Fill(abs(q.y - mline) - gap) * aL * smoothstep(0.0015, 0.003, gap));
                }
            }

            // =====================================================================
            //  Hair
            // =====================================================================
            void HairBack(float2 q, Genes G, float3 hairBase, out float alpha, out float3 col)
            {
                alpha = 0.0; col = 0.0;
                if (G.hairStyle != 3) return;
                float vol = G.hairVol;
                float d = sdRoundBox(q - float2(0.0, -0.20), float2(0.375 * vol, 0.62), 0.21);
                float n = Vnoise(float2(q.x * 120.0, q.y * 6.0)) * 0.55 + Vnoise(float2(q.x * 330.0, q.y * 10.0)) * 0.45;
                alpha = Fill(d + (n - 0.5) * 0.02);
                float nx = clamp(q.x / (0.375 * vol), -1.0, 1.0);
                float3 N = normalize(float3(nx * 0.8, 0.2, 0.6));
                col = SuitLight(hairBase * (0.5 + 0.9 * n), N, 0.6);
            }

            void HairFront(float2 q, Genes G, Face F, float age, float3 hairBase, float gray, float3 skinLin, out float alpha, out float3 col)
            {
                int style = G.hairStyle;
                alpha = 0.0; col = 0.0;
                float vol = G.hairVol;
                float2 cc = float2(0.0, 0.085);
                float2 rr = float2(0.318 * G.headW, 0.345 * G.headH) * vol;
                if (style == 4) rr *= float2(1.30, 1.24);
                float ang = atan2(q.y - cc.y, q.x - cc.x);
                float capD = sdEllipse(q - cc, rr);
                if (style == 4) capD -= 0.013 * sin(ang * 11.0 + 1.3) + 0.006 * sin(ang * 23.0);
                if (style == 3)
                {
                    float2 lq = float2(abs(q.x) - 0.285, q.y + 0.10);
                    capD = min(capD, sdRoundBox(lq, float2(0.052, 0.22), 0.05));
                }

                float H0 = 0.25;
                if (style == 0) H0 = 0.22; else if (style == 1) H0 = 0.27; else if (style == 2) H0 = 0.28;
                float yh = H0 + 0.05 * pow(abs(q.x) / 0.24, 2.0) - 0.02 * F.c;
                if (style == 0) yh += 0.03 * q.x * 3.0 * G.part;
                if (style == 2) yh += 0.07 * exp(-pow((abs(q.x) - 0.16) / 0.07, 2.0));
                yh += 0.05 * F.e * G.masc;
                float sideY = (style == 3) ? -0.30 : 0.02;
                yh = lerp(yh, sideY, smoothstep(0.215, 0.255, abs(q.x)));
                float dHl = yh - q.y;
                if (style == 5)
                {
                    capD = max(capD, max(q.y - 0.12, 0.225 - abs(q.x)));
                    dHl = -1.0;
                }

                float rad = length(q);
                float strand;
                if (style == 3) strand = Vnoise(float2(q.x * 120.0, q.y * 6.0)) * 0.55 + Vnoise(float2(q.x * 330.0, q.y * 10.0)) * 0.45;
                else            strand = Vnoise(float2(ang * 26.0, rad * 7.0)) * 0.55 + Vnoise(float2(ang * 85.0, rad * 10.0)) * 0.45;

                float aSil = Fill(capD + (strand - 0.5) * 0.016);
                float aLine = smoothstep(-0.002, 0.012, -dHl + (strand - 0.5) * 0.022);
                alpha = aSil * aLine;
                if (style == 1) alpha *= 0.62 + 0.38 * smoothstep(0.35, 0.65, Vnoise(q * 260.0));
                if (style == 5) alpha *= 0.55 + 0.45 * smoothstep(0.35, 0.65, Vnoise(q * 300.0));
                alpha *= 1.0 - 0.25 * F.e * smoothstep(0.55, 0.75, Vnoise(q * 40.0));   // thinning

                // per-strand greying
                float grayN = Vnoise(float2(ang * 60.0, rad * 3.0)) * 0.5 + 0.5 * Vnoise(float2(ang * 13.0, rad * 20.0));
                float gm = smoothstep(1.0 - gray - 0.12, 1.0 - gray + 0.12, grayN) * smoothstep(0.0, 0.25, gray);
                float3 base = lerp(hairBase, S(float3(0.74, 0.74, 0.76)), saturate(gm));
                base *= 0.55 + 0.9 * strand;

                float2 hq = (q - cc) / rr;
                float z = sqrt(saturate(1.0 - dot(hq, hq)));
                float3 Nh = normalize(float3(hq.x * 0.9, hq.y * 0.9, z + 0.15));
                Nh = normalize(Nh + float3((strand - 0.5) * 0.35, (strand - 0.5) * 0.35, 0.0));
                float3 L = LightDir();
                float diff = saturate(dot(Nh, L) * 0.5 + 0.5);
                diff = diff * diff * 1.2 + 0.1;
                float ao = 0.55 + 0.45 * smoothstep(0.0, 0.07, -capD);
                ao *= lerp(0.7, 1.0, smoothstep(0.0, 0.05, -dHl));
                if (style == 4) ao *= 0.65 + 0.7 * Vnoise(q * 38.0);
                float3 H = normalize(L + float3(0, 0, 1));
                float spec = pow(saturate(dot(Nh, H)), 18.0) * (0.4 + 1.2 * Vnoise(float2(ang * 90.0, rad * 12.0)));
                float3 specC = lerp(float3(1, 1, 1), base * 3.0, 0.5);
                col = base * (diff + 0.18) * ao + spec * 0.25 * specC * ao;

                // parting
                if (style == 0 || style == 3)
                {
                    float partX = 0.07 * G.part;
                    float pd = Gauss(q.x - partX, 0.0035) * step(0.12, q.y);
                    col = lerp(col, skinLin * 0.5, saturate(pd) * 0.6);
                }
                // buzz-cut shows scalp
                if (style == 1 || style == 5) col = lerp(skinLin * 0.55, col, 0.65 + 0.35 * smoothstep(0.35, 0.65, Vnoise(q * 260.0)));
            }

            // =====================================================================
            //  Fragment
            // =====================================================================
            #define OVER(c_, m_) { float mm_ = saturate(m_); float3 cc_ = (c_); \
                if (any(isnan(cc_)) || isnan(mm_)) { mm_ = 0.0; cc_ = 0.0; } \
                P = cc_ * mm_ + P * (1.0 - mm_); A = mm_ + A * (1.0 - mm_); }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float dxu = abs(ddx(uv.x)), dyu = abs(ddy(uv.y));
                float asp = (dxu > 1e-7 && dyu > 1e-7) ? clamp(dyu / dxu, 0.25, 4.0) : 1.0;
                float2 p = (uv - 0.5) * 2.0;
                p.x *= asp;
                g_aa = max(max(abs(ddx(p.x)), abs(ddy(p.y))) * 0.9, 0.0006);

                uint seed = (uint)abs(_Seed);
                float age = _Age;
                Genes G = Roll(seed, age);
                Face F = Layout(G, age, _Expression);
                float alive = 1.0 - _Dead;

                float2 hc = float2(0.0, 0.06);
                float2 q = p - hc;

                float3 skinLin = SkinAlbedo(G, F);
                float gray = saturate((age - 0.42) * 1.9 + G.hairGray * 0.3 * step(0.35, age));
                float3 hairAvg = lerp(G.hair, S(float3(0.72, 0.72, 0.74)), smoothstep(0.35, 1.0, gray));
                float3 suitAlb = S(float3(0.92, 0.93, 0.94) * _SuitTint.rgb);

                float3 P = S(_BgColor.rgb) * (1.0 - 0.3 * dot(p, p) * 0.5) * _BgColor.a;
                float A = _BgColor.a;

                float detail = saturate(1.0 - g_aa * 220.0);

                // ===================== PLSS BACKPACK =====================
                {
                    float d = sdRoundBox(p - float2(0.0, -0.52), float2(0.66, 0.30), 0.12);
                    float m = Fill(d);
                    float3 alb = suitAlb * 0.95 * (0.93 + 0.14 * Vnoise(p * 240.0) * detail + 0.07 * (1.0 - detail));
                    float3 N = normalize(float3(clamp(p.x / 0.7, -1.0, 1.0) * 0.6, 0.35, 0.7));
                    float ao = (0.45 + 0.55 * smoothstep(0.0, 0.05, -d)) * 0.55;
                    OVER(SuitLight(alb, N, ao), m);
                }

                // ===================== SHOULDERS (suit) =====================
                float dSh;
                {
                    float2 sp = p;
                    sp.y += max(abs(p.x) - 0.15, 0.0) * 0.30;
                    dSh = sdRoundBox(sp - float2(0.0, -0.80), float2(0.84, 0.42), 0.28);
                    float m = Fill(dSh);
                    float t = saturate(-dSh / 0.20);
                    float nx = clamp(p.x / 0.9, -1.0, 1.0);
                    float3 N = normalize(float3(nx * 0.7 * (1.0 - t * 0.4), (1.0 - t) * 0.8, 0.5 + 0.5 * t));
                    N.xy += (float2(Fbm(p * 5.0 + 1.7), Fbm(p * 5.0 + 9.1)) - 0.5) * 0.6;
                    N = normalize(N);
                    float3 alb = suitAlb * (0.93 + 0.14 * Vnoise(p * 240.0) * detail + 0.07 * (1.0 - detail));
                    float weave = 0.5 + 0.5 * sin(p.x * 260.0) * sin(p.y * 260.0);
                    alb *= 1.0 - 0.06 * weave * detail;
                    float ao = 0.55 + 0.45 * smoothstep(0.0, 0.06, -dSh);
                    // seams
                    ao *= 1.0 - 0.35 * Gauss(abs(p.x) - 0.24, 0.003) * step(p.y, -0.45);
                    float ringD = abs(length((p - float2(-0.58, -0.64)) / float2(0.11, 0.15)) - 1.0) * 0.11;
                    float ringD2 = abs(length((p - float2(0.58, -0.64)) / float2(0.11, 0.15)) - 1.0) * 0.11;
                    ao *= 1.0 - 0.30 * Gauss(min(ringD, ringD2), 0.003);
                    // head / neck shadow on the suit
                    ao *= 1.0 - 0.45 * Bell(p, float2(0.02, -0.40), float2(0.28, 0.10));
                    OVER(SuitLight(alb, N, ao), m);

                    // flag patch (left shoulder)
                    float2 lp = p - float2(-0.56, -0.60);
                    float cr = cos(0.22), sr = sin(0.22);
                    lp = float2(cr * lp.x - sr * lp.y, sr * lp.x + cr * lp.y);
                    float dP = sdRoundBox(lp, float2(0.065, 0.042), 0.004);
                    float stripe = step(0.5, frac((lp.y / 0.042 * 0.5 + 0.5) * 3.5));
                    float3 pc = lerp(S(float3(0.85, 0.85, 0.88)), S(float3(0.80, 0.12, 0.12)), stripe);
                    float canton = step(lp.x, -0.012) * step(0.0, lp.y);
                    pc = lerp(pc, S(float3(0.12, 0.20, 0.55)), canton);
                    pc *= 0.85 + 0.15 * smoothstep(0.0, 0.01, -dP);
                    OVER(SuitLight(pc, float3(0.0, 0.2, 0.95), 1.0), Fill(dP) * m);

                    // chest control unit
                    float2 cp = p - float2(0.0, -0.76);
                    float dC = sdRoundBox(cp, float2(0.17, 0.12), 0.035);
                    float3 ca = S(float3(0.80, 0.82, 0.85));
                    float cao = 0.75 + 0.25 * smoothstep(0.0, 0.02, -dC);
                    float3 cc3 = SuitLight(ca * (0.95 + 0.05 * Vnoise(p * 150.0)), float3(0.0, 0.1, 0.99), cao);
                    cc3 += Gauss(dC + 0.006, 0.004) * step(0.0, cp.y) * 0.18;
                    OVER(cc3, Fill(dC));

                    // 4 connectors: red (left), blue (right)
                    [unroll] for (int k = 0; k < 4; k++)
                    {
                        float sx = (k < 2) ? -1.0 : 1.0;
                        float sy = (k % 2 == 0) ? -0.72 : -0.80;
                        float2 kp = p - float2(0.095 * sx, sy);
                        float R = 0.030;
                        float dk = length(kp) - R;
                        float2 ln = kp / R;
                        float3 Nk = normalize(float3(ln, sqrt(saturate(1.0 - dot(ln, ln)))));
                        float3 colK = (sx < 0.0) ? S(float3(0.85, 0.15, 0.15)) : S(float3(0.15, 0.40, 0.85));
                        float rim = smoothstep(0.62, 0.78, length(ln));
                        float3 metal = S(float3(0.70, 0.72, 0.75));
                        float3 kb = lerp(colK, metal, rim);
                        float3 kc = SuitLight(kb, Nk, 0.9 + 0.1 * (1.0 - rim));
                        kc += pow(saturate(dot(Nk, normalize(LightDir() + float3(0, 0, 1)))), 50.0) * 0.5;
                        OVER(kc, Fill(dk) * m);
                    }
                }

                // ===================== BACK HAIR (long) =====================
                float3 hairBase = lerp(G.hair, S(float3(0.72, 0.72, 0.74)), 0.0);
                {
                    float ha; float3 hcb;
                    HairBack(q, G, hairBase, ha, hcb);
                    OVER(hcb, ha);
                }

                // ===================== NECK =====================
                float dHead = HeadSDF(q, G, F.c);
                {
                    float2 nq = p - float2(0.0, -0.31);
                    float dN = sdRoundBox(nq, float2(0.108 * (1.0 + 0.10 * G.masc), 0.16), 0.045);
                    float m = Fill(dN);
                    float nx = clamp(nq.x / 0.11, -1.0, 1.0);
                    float3 N = normalize(float3(nx * 0.8, 0.0, 0.6));
                    float ao = lerp(0.25, 1.0, smoothstep(0.0, 0.16, dHead));
                    ao *= 1.0 - 0.12 * Gauss(abs(nq.x) - 0.065, 0.02) * (0.6 + 0.6 * G.masc);
                    float3 alb = skinLin * 0.92;
                    alb *= 1.0 - 0.06 * F.e * Gauss(frac(p.y * 22.0) - 0.5, 0.15);
                    OVER(ShadeSkin(alb, N, ao, 0.3, 20.0), m);
                }

                // ===================== NECK RINGS =====================
                {
                    float2 bp = p - float2(0.0, -0.425);
                    float dB = sdRoundBox(bp, float2(0.20, 0.030), 0.03);
                    float ny = clamp(bp.y / 0.03, -1.0, 1.0);
                    float nx = clamp(bp.x / 0.20, -1.0, 1.0);
                    float3 N = normalize(float3(nx * 0.5, ny * 0.8, 0.6));
                    float3 bc = SuitLight(S(float3(0.18, 0.22, 0.30)), N, 0.8 + 0.2 * smoothstep(0.0, 0.01, -dB));
                    bc += pow(saturate(dot(N, normalize(LightDir() + float3(0, 0, 1)))), 40.0) * 0.6;
                    OVER(bc, Fill(dB));

                    float2 rp = p - float2(0.0, -0.392);
                    float dR = sdRoundBox(rp, float2(0.17, 0.016), 0.016);
                    float rny = clamp(rp.y / 0.016, -1.0, 1.0);
                    float rnx = clamp(rp.x / 0.17, -1.0, 1.0);
                    float3 Nr = normalize(float3(rnx * 0.5, rny * 0.8, 0.6));
                    float3 rb = S(float3(0.10, 0.35, 0.75)) * (0.8 + 0.3 * step(0.5, frac(p.x * 60.0)) * detail);
                    float3 rc = SuitLight(rb, Nr, 0.85);
                    rc += pow(saturate(dot(Nr, normalize(LightDir() + float3(0, 0, 1)))), 40.0) * 0.7;
                    OVER(rc, Fill(dR));
                }

                // ===================== EARS =====================
                {
                    float earY = -0.02;
                    float es = G.earSize * (1.0 + 0.12 * F.e) * (1.0 + 0.10 * F.c);
                    float2 eq = float2(abs(q.x) - 0.292 * G.headW - 0.012, q.y - earY);
                    float2 er = float2(0.030, 0.062) * es;
                    float dE = sdEllipse(eq, er);
                    float sgn = (q.x < 0.0) ? -1.0 : 1.0;
                    float3 N = normalize(float3(sgn * saturate(eq.x / er.x * 0.5 + 0.5) * 0.8, eq.y / er.y * 0.4, 0.6));
                    float dIn = sdEllipse(eq - float2(-0.006, 0.0), float2(0.014, 0.036) * es);
                    float ao = 1.0 - 0.45 * Fill(dIn) - 0.2 * Gauss(dE + 0.006, 0.004);
                    float3 alb = skinLin * float3(1.0, 0.85, 0.80);
                    OVER(ShadeSkin(alb, N, ao, 0.3, 16.0), Fill(dE));
                }

                // ===================== HEAD =====================
                float3 hcol;
                float mHead = Fill(dHead);
                float3 Nhd;
                {
                    float eps = 0.0035;
                    float h0 = Height(q, G, F);
                    float hx = Height(q + float2(eps, 0.0), G, F);
                    float hy = Height(q + float2(0.0, eps), G, F);
                    Nhd = normalize(float3(-(hx - h0) / eps, -(hy - h0) / eps, 1.0));
                    float pn = Vnoise(q * 310.0) - 0.5;
                    Nhd = normalize(Nhd + float3(pn, Vnoise(q * 310.0 + 5.7) - 0.5, 0.0) * 0.06 * detail);

                    float3 alb = skinLin;
                    float ao, spM;
                    FaceMaps(q, G, F, age, alb, ao, spM);
                    ao *= 0.80 + 0.20 * smoothstep(0.0, 0.05, -dHead);
                    // hair cast shadow on forehead
                    float hasFringe = (G.hairStyle == 5) ? 0.0 : 1.0;
                    ao *= 1.0 - 0.30 * hasFringe * Bell(q, float2(0.0, 0.25), float2(0.22, 0.05));
                    hcol = ShadeSkin(alb, Nhd, ao, spM, 38.0);

                    // stubble (inside the head)
                    if (G.facial == 1)
                    {
                        float beardTop = -0.105 + 0.11 * pow(saturate(abs(q.x) / 0.27), 2.0);
                        float mB = (1.0 - smoothstep(-0.015, 0.015, q.y - beardTop)) * (0.7 + 0.6 * Fbm(q * 20.0));
                        float dots = smoothstep(0.52, 0.68, Vnoise(q * 380.0));
                        hcol = lerp(hcol, lerp(hcol * 0.65, hairAvg * 0.6, 0.5), saturate(mB) * (0.30 + 0.55 * dots) * 0.9);
                    }

                    DrawEye(q, -1.0, G, F, alive, hcol);
                    DrawEye(q,  1.0, G, F, alive, hcol);
                    DrawBrow(q, -1.0, G, F, hairAvg * 0.85, hcol);
                    DrawBrow(q,  1.0, G, F, hairAvg * 0.85, hcol);
                    DrawMouth(q, G, F, skinLin, alive, hcol);
                }
                OVER(hcol, mHead);

                // ===================== FULL BEARD =====================
                if (G.facial == 2)
                {
                    float mline, gap, hUp, hLo, mx;
                    float dLip = LipShape(q, F, mline, gap, hUp, hLo, mx);
                    float beardTop = -0.105 + 0.11 * pow(saturate(abs(q.x) / 0.27), 2.0);
                    float n = Vnoise(float2(q.x * 120.0, q.y * 40.0)) * 0.6 + Vnoise(float2(q.x * 300.0, q.y * 100.0)) * 0.4;
                    float dB = max(dHead - 0.025, q.y - beardTop);
                    float a = Fill(dB + (n - 0.5) * 0.012);
                    a *= smoothstep(0.0, 0.010, dLip);
                    a *= 0.85 + 0.15 * n;
                    float3 N = normalize(float3(Nhd.x, Nhd.y - 0.2, Nhd.z));
                    float3 bcol = SuitLight(hairAvg * (0.5 + 0.8 * n), N, 0.85);
                    OVER(bcol, a);
                }

                // ===================== FRONT HAIR =====================
                {
                    float ha; float3 hcf;
                    HairFront(q, G, F, age, hairBase, gray, skinLin, ha, hcf);
                    OVER(hcf, ha);
                }

                // ===================== FINAL =====================
                float3 col = P / max(A, 1e-4);
                float lum = dot(col, float3(0.30, 0.59, 0.11));
                col = lerp(col, lum * float3(0.88, 0.94, 1.04) * 0.85, _Dead * 0.92);
                col = Shoulder(max(col, 0.0));
                col = saturate(col);
                #if defined(UNITY_COLORSPACE_GAMMA)
                    col = LinearToSRGB(col);
                #endif
                // dither against banding
                col += (Hash21(uv * 1733.0) - 0.5) * (1.0 / 255.0);

                float4 res = float4(col, A);
                res *= IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                    float2 inside = step(_ClipRect.xy, IN.worldPos.xy) * step(IN.worldPos.xy, _ClipRect.zw);
                    res.a *= inside.x * inside.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(res.a - 0.001);
                #endif
                return res;
            }
            ENDHLSL
        }
    }
}
