Shader "Custom/URP_UI_StatUpgrade_AAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        [HDR] _Color ("Core Flare Color", Color) = (1.0, 0.6, 0.1, 1.0)
        [HDR] _RimColor ("Energy Aura Color", Color) = (0.2, 0.8, 1.0, 1.0)
        [HDR] _QualityColor ("Stellar Highlight Color", Color) = (1.0, 0.95, 0.7, 1.0)

        _Progress ("Upgrade Progress", Range(0.0, 1.0)) = 0.0
        _Intensity ("Intensity", Range(0.0, 3.0)) = 1.4
        _SoftClip ("Soft Clip (1 = LDR canvas, 0 = HDR + Bloom)", Range(0.0, 1.0)) = 1.0
        _Additive ("Additive Blend Amount", Range(0.0, 1.0)) = 0.8
        _Aberration ("Chromatic Aberration", Range(0.0, 1.0)) = 0.5
        _RingAmount ("Shockwave Amount", Range(0.0, 2.0)) = 1.0
        _FlareAmount ("Flare + Rays Amount", Range(0.0, 2.0)) = 1.0
        _SparkAmount ("Sparks + Glints Amount", Range(0.0, 2.0)) = 1.0
        _Seed ("Random Seed", Float) = 133.7

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
        // Premultiplied. Alpha < luminance gives partially additive light (glow without muddying the UI).
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIStatUpgrade_AAA_Pass"
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
                float _Progress;
                float _Intensity;
                float _SoftClip;
                float _Additive;
                float _Aberration;
                float _RingAmount;
                float _FlareAmount;
                float _SparkAmount;
                float _Seed;
            CBUFFER_END

            float4 _ClipRect;

            #define PI  3.14159265359
            #define TAU 6.28318530718
            #define IMPACT 0.16   // fraction of _Progress spent charging before the burst

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

            struct FxCtx
            {
                float2 st;     // centered uv (-0.5..0.5)
                float  dist;
                float  angle;
                float  seed;
            };

            // ---------------------------------------------------------------- utils
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

            // Noise that is seamless around the circle (no atan2 seam)
            float AngNoise(float a, float freq, float t)
            {
                float2 q = float2(cos(a), sin(a)) * freq;
                return FBM(q + float2(t, t * 0.37 + 5.3));
            }

            float2 Rot(float2 p, float a)
            {
                float s, c;
                sincos(a, s, c);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            float EaseOutCubic(float t) { t = saturate(t); return 1.0 - pow(1.0 - t, 3.0); }
            float EaseInCubic(float t)  { t = saturate(t); return t * t * t; }
            float EaseOutExpo(float t)  { t = saturate(t); return (1.0 - exp2(-10.0 * t)) / (1.0 - exp2(-10.0)); }

            // Long-tailed bloom-like falloff (much closer to real lens bloom than a gaussian)
            float Lorentz(float d, float r) { return (r * r) / (d * d + r * r); }

            float Max3(float3 v) { return max(v.x, max(v.y, v.z)); }
            float Luma(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); }

            // Asymmetric shockwave: steep leading edge, long glowing trail behind it
            float Shock(float d, float R, float w)
            {
                float x = (d - R) / w;
                float lead = exp(-x * x * 5.0);
                float tail = 0.5 * exp(x * 0.35) + 0.5 * lead;
                return x > 0.0 ? lead : tail;
            }

            // White-hot centre -> stellar -> core colour -> dim ember
            float3 Heat(float h, float3 outer, float3 mid)
            {
                h = saturate(h);
                float3 c = lerp(outer * 0.35, outer, smoothstep(0.0, 0.35, h));
                c = lerp(c, mid, smoothstep(0.3, 0.7, h));
                c = lerp(c, lerp(mid, float3(1, 1, 1), 0.75) * 1.6, smoothstep(0.65, 1.0, h));
                return c;
            }

            // Highlight-preserving filmic curve with bleed-to-white (fake bloom for LDR canvases)
            float3 ToneLDR(float3 x)
            {
                float3 t = 1.0 - exp(-x * 1.5);
                float bleed = saturate(Luma(x) - 1.0) * 0.5;
                return lerp(t, float3(1, 1, 1), bleed);
            }

            // Interleaved gradient noise (cheap, blue-ish) for banding-free gradients
            float IGN(float2 p)
            {
                return frac(52.9829189 * frac(dot(p, float2(0.06711056, 0.00583715))));
            }

            // ---------------------------------------------------------------- particle layers
            // Burst sparks / embers: comet streaks with heat-based colour aging, flicker and gravity
            float3 SparkLayer(FxCtx c, float bt, float seedOff, int count, float maxR, float size,
                              float trailMul, float grav, float delayMax, float ember)
            {
                float3 acc = 0;
                [loop] for (int k = 0; k < count; k++)
                {
                    float fi = (float)k + seedOff + c.seed;
                    float2 h = Hash22(float2(fi, fi * 1.7 + 3.1));
                    float h3 = Hash11(fi * 3.7);
                    float a = h.x * TAU;
                    float2 dir = float2(cos(a), sin(a));
                    float2 perp = float2(-dir.y, dir.x);

                    float delay = h.y * delayMax;
                    float life = saturate((bt - delay) / (1.0 - delay));
                    if (life <= 0.0 || life >= 1.0) continue;

                    float r = maxR * (0.35 + 0.65 * h3) * EaseOutCubic(life);
                    float2 pos = dir * r + float2(0.0, -grav * life * life);
                    float2 rel = c.st - pos;
                    float along  = dot(rel, dir);
                    float across = dot(rel, perp);

                    float s = size * (1.0 - life * 0.7) * (0.7 + 0.6 * h3);
                    float2 q = float2(along < 0.0 ? along / (s * trailMul * (1.0 - life * 0.5)) : along / (s * 1.6),
                                      across / s);
                    float d2 = dot(q, q);
                    float g = exp(-d2 * 1.2) + 0.10 / (1.0 + d2 * 0.4);

                    float flick = 0.75 + 0.25 * sin(fi * 7.0 + bt * 60.0);
                    float fade = pow(1.0 - life, 1.3) * smoothstep(0.0, 0.04, life);
                    float age = saturate(life * (1.0 + ember * 1.5));
                    float3 sc = Heat(1.0 - age, _Color.rgb, _QualityColor.rgb);
                    acc += sc * g * fade * flick;
                }
                return acc;
            }

            // Anticipation: motes spiral inward toward the core
            float3 ChargeMotes(FxCtx c, float chg)
            {
                float3 acc = 0;
                [loop] for (int k = 0; k < 22; k++)
                {
                    float fi = (float)k + 41.0 + c.seed;
                    float2 h = Hash22(float2(fi, fi * 1.3 + 7.7));
                    float h3 = Hash11(fi * 2.9);
                    float delay = h.y * 0.5;
                    float life = saturate((chg - delay) / (1.0 - delay));
                    if (life <= 0.0) continue;

                    float tr = EaseInCubic(life);
                    float r = lerp(0.40 * (0.6 + 0.4 * h3), 0.035, tr);
                    float a = h.x * TAU + tr * 2.5 * (h3 - 0.3);
                    float2 dir = float2(cos(a), sin(a));
                    float2 rel = c.st - dir * r;
                    float along  = dot(rel, dir);          // positive = behind the mote (outward)
                    float across = dot(rel, float2(-dir.y, dir.x));

                    float s = 0.005 * (0.6 + 0.8 * life);
                    float2 q = float2(along > 0.0 ? along / (s * 10.0) : along / (s * 1.5), across / s);
                    float g = exp(-dot(q, q) * 1.2);
                    float fade = smoothstep(0.0, 0.15, life) * (1.0 - smoothstep(0.9, 1.0, life));
                    acc += lerp(_RimColor.rgb, float3(1, 1, 1), 0.5) * g * fade * 1.5;
                }
                return acc;
            }

            // Twinkling 4-point star glints
            float3 Glints(FxCtx c, float bt)
            {
                float3 acc = 0;
                [loop] for (int k = 0; k < 10; k++)
                {
                    float fi = (float)k * 1.37 + 77.0 + c.seed;
                    float2 h = Hash22(float2(fi, fi * 2.1 + 1.9));
                    float t0 = Hash11(fi * 5.1) * 0.35 + 0.04;
                    float tw = saturate((bt - t0) / 0.3);
                    float env = sin(tw * PI);
                    env *= env;
                    if (env <= 0.0) continue;

                    float ang = h.x * TAU;
                    float rad = lerp(0.08, 0.34, h.y);
                    float2 rel = Rot(c.st - float2(cos(ang), sin(ang)) * rad, 0.785 * h.x);
                    float sz = 0.012 * (0.6 + h.y);
                    float2 ar = abs(rel);
                    float star = exp(-ar.x / (sz * 3.5)) * exp(-ar.y / (sz * 0.35))
                               + exp(-ar.y / (sz * 3.5)) * exp(-ar.x / (sz * 0.35));
                    float core = exp(-dot(rel, rel) / (sz * sz * 0.5));
                    acc += lerp(_QualityColor.rgb, float3(1, 1, 1), 0.6) * (star * 0.9 + core) * env;
                }
                return acc;
            }

            // ---------------------------------------------------------------- fragment
            half4 frag(Varyings i) : SV_Target
            {
                float p = _Progress;
                if (p <= 0.001 || p >= 0.999)
                    return half4(0, 0, 0, 0);

                FxCtx c;
                c.st = i.uv - 0.5;
                c.dist = length(c.st);
                c.angle = atan2(c.st.y, c.st.x + 1e-5);
                c.seed = _Seed;

                // ---- timeline
                float chg = saturate(p / IMPACT);
                float bt = saturate((p - IMPACT) / (1.0 - IMPACT));
                float after = smoothstep(IMPACT - 0.004, IMPACT + 0.004, p);
                float flash = pow(1.0 - bt, 2.6) * after;
                float impact = exp(-bt * 14.0) * after;
                float chargeEnv = chg * chg * (1.0 - after);

                float3 rimWhite = lerp(_RimColor.rgb, float3(1, 1, 1), 0.7);
                float3 col = 0;

                // ---- plasma-distorted distance
                float plasma = FBM(c.st * 4.5 - float2(0.0, bt * 3.0) + c.seed * 0.01);
                float dD = c.dist + (plasma - 0.5) * 0.08 * (flash + chargeEnv * 0.5);

                // ================= 1. CHARGE-UP (anticipation)
                float pulse = 0.75 + 0.25 * sin(chg * 50.0);
                float chargeCore = Lorentz(dD, lerp(0.012, 0.045, chg)) * chargeEnv * pulse * 2.5
                                 + Lorentz(dD, 0.12) * chargeEnv * 0.3;
                col += Heat(0.55 + 0.4 * chg, _Color.rgb, _QualityColor.rgb) * chargeCore;

                float Rc = lerp(0.36, 0.05, EaseInCubic(chg));
                float xr = (c.dist - Rc) / 0.01;
                col += rimWhite * exp(-xr * xr) * chargeEnv * smoothstep(0.0, 0.3, chg) * 0.9;

                col += ChargeMotes(c, chg) * (1.0 - after) * _SparkAmount;

                // ================= 2. IMPACT CORE (three-scale bloom + white-hot disc)
                float tight = Lorentz(dD, 0.018 + 0.05 * impact);
                float mid   = Lorentz(dD, 0.09 + 0.05 * flash);
                float wide  = Lorentz(dD, 0.20);
                float coreE = tight * 3.0 + mid * 0.9 + wide * 0.25;
                float heatV = saturate(tight * 1.2 + (plasma - 0.5) * 0.35 + mid * 0.15);
                col += Heat(heatV, _Color.rgb, _QualityColor.rgb) * coreE * (impact * 3.0 + flash * 1.4);

                float disc = (1.0 - smoothstep(0.0, 0.12 * impact + 0.015, dD)) * impact * 3.0;
                col += Heat(1.0, _Color.rgb, _QualityColor.rgb) * disc;

                // ================= 3. SHOCKWAVES (chromatic, broken-up, asymmetric)
                float Rt = EaseOutExpo(bt);
                float R1 = Rt * 0.40;
                float w1 = lerp(0.010, 0.030, Rt);
                float nA = AngNoise(c.angle, 2.2, bt * 2.0);
                float nB = AngNoise(c.angle + 1.7, 3.0, bt * 1.3 + 9.0);
                float dR = c.dist + (nA - 0.5) * 0.05 * (1.0 - Rt * 0.5);
                float brk = lerp(0.4, 1.0, smoothstep(0.2, 0.8, nB));
                float ab = 0.035 * _Aberration;
                float ringE = pow(1.0 - bt, 1.6) * after * (1.0 - smoothstep(0.55, 0.95, bt));

                float3 prof = float3(Shock(dR, R1 * (1.0 + ab), w1),
                                     Shock(dR, R1, w1),
                                     Shock(dR, R1 * (1.0 - ab), w1));
                float3 ringCol = prof * rimWhite * brk * ringE * 2.2;

                float bt2 = saturate((bt - 0.08) / 0.92);
                float R2 = EaseOutCubic(bt2) * 0.30;
                float w2 = lerp(0.015, 0.05, bt2);
                float prof2 = Shock(c.dist + (nB - 0.5) * 0.04, R2, w2);
                ringCol += lerp(_Color.rgb, float3(1, 1, 1), 0.3) * prof2 * pow(1.0 - bt2, 2.0) * after * 0.9;

                col += ringCol * _RingAmount;

                // ================= 4. ENERGY AURA (swirling plasma + electric veins inside the wave)
                float2 sw = Rot(c.st, c.dist * 7.0 - bt * 5.0);
                float an = FBM(sw * 6.0 + float2(0.0, bt * 3.0));
                float vn = FBM(sw * 3.0 + float2(bt * 2.0, 4.0));
                float vein = pow(saturate(1.0 - abs(2.0 * vn - 1.0)), 10.0);
                float auraMask = smoothstep(R1 + 0.02, R1 * 0.15, c.dist) * pow(1.0 - bt, 1.2) * after;
                float aura = pow(an, 2.2) * auraMask * 2.2;
                col += lerp(_RimColor.rgb, _QualityColor.rgb, an * an) * aura;
                col += lerp(_QualityColor.rgb, float3(1, 1, 1), 0.5) * vein * auraMask * 1.4;

                // ================= 5. GOD RAYS + ANAMORPHIC STAR FLARE
                float rayN = AngNoise(c.angle, 3.0, bt * 1.5 + c.seed * 0.02);
                float rays = pow(saturate(rayN * 1.8 - 0.35), 3.0);
                rays *= exp(-c.dist * 6.0) * smoothstep(0.0, 0.05, c.dist) * pow(1.0 - bt, 2.0) * after * 2.5;

                float2 fs = Rot(c.st, (Hash11(c.seed) - 0.5) * 0.25);
                float2 fa = abs(fs);
                float hz = exp(-fa.y * 55.0) * exp(-fa.x * 4.0) + 0.6 * exp(-fa.y * 260.0) * exp(-fa.x * 2.2);
                float vt = (exp(-fa.x * 55.0) * exp(-fa.y * 4.0) + 0.6 * exp(-fa.x * 260.0) * exp(-fa.y * 2.2)) * 0.7;
                float2 da = abs(Rot(fs, PI * 0.25));
                float dg = (exp(-da.y * 80.0) * exp(-da.x * 7.0) + exp(-da.x * 80.0) * exp(-da.y * 7.0)) * 0.35;

                float flareEnv = pow(1.0 - bt, 2.0) * after + chargeEnv * 0.25;
                float3 tint = 0.5 + 0.5 * cos(TAU * (fa.x * 1.5 + float3(0.0, 0.33, 0.67)));
                float3 flareCol = lerp(rimWhite, tint, 0.3);
                col += flareCol * (hz + vt + dg) * flareEnv * 2.4 * _FlareAmount;
                col += lerp(_Color.rgb, float3(1, 1, 1), 0.4) * rays * _FlareAmount;

                // ================= 6. SPARKS, EMBERS, GLINTS
                float3 sparks = SparkLayer(c, bt, 3.1, 28, 0.45, 0.006, 12.0, 0.00, 0.20, 0.0) * 1.6;
                sparks += SparkLayer(c, bt, 91.0, 18, 0.38, 0.0045, 5.0, 0.12, 0.20, 1.0) * 1.2;
                sparks += Glints(c, bt) * 1.3;
                col += sparks * _SparkAmount;

                // ================= composition
                float env  = smoothstep(0.0, 0.02, p) * (1.0 - smoothstep(0.9, 1.0, p));
                float edge = max(abs(c.st.x), abs(c.st.y));
                // Guarantees exactly zero energy on the RectTransform border (no clipped rays)
                float mask = (1.0 - smoothstep(0.36, 0.5, edge)) * (1.0 - smoothstep(0.42, 0.62, c.dist));

                col *= env * mask * _Intensity;

                // Tone mapping: LDR canvas -> filmic + bleed to white, HDR canvas -> raw values for bloom
                col = lerp(col, ToneLDR(col), _SoftClip);

                // Dither to kill gradient banding in the long glow falloffs
                float dither = (IGN(i.positionCS.xy) - 0.5) / 255.0;
                col += dither * saturate(Max3(col) * 50.0);
                col = max(col, 0.0);

                // Alpha is only partial coverage; the rest of the light is additive
                float alpha = saturate(Max3(col)) * (1.0 - _Additive);

                // Canvas tint integration
                col   *= i.color.rgb * i.color.a;
                alpha *= i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, i.posOS) * step(i.posOS, _ClipRect.zw);
                float clipV = inside.x * inside.y;
                col   *= clipV;
                alpha *= clipV;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(max(alpha, Max3(col)) - 0.001);
                #endif

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
