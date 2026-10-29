Shader "UI/Custom/AuroraVeilAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Background Track Color", Color) = (0.03, 0.05, 0.12, 1.0)
        _RimColor ("Active Charge / Aurora Color", Color) = (0.0, 0.85, 1.0, 1.0)

        [Header(Progress Control)]
        _Power ("Progress (0 - 100)", Range(0.0, 100.0)) = 25.0
        _RotationAngle ("Base Angle Offset (radians)", Float) = 0.0
        _HighSpeedTransition ("High-Speed / Overcharge Mode", Range(0.0, 1.0)) = 0.0

        [Header(Shape)]
        _Layers ("Curtain Folds (streak density)", Range(3, 30)) = 14
        _SegmentGap ("Hem Wave Amplitude", Range(0.0, 0.06)) = 0.014
        _EventHorizon ("Core Radius Boundary", Range(0.05, 0.4)) = 0.22
        _RingOuter ("Curtain Outer Radius", Range(0.3, 0.75)) = 0.60

        [Header(Look)]
        _HeatShimmer ("Ripple & Shimmer", Range(0.0, 2.0)) = 1.0
        _Sparks ("Stardust", Range(0.0, 1.0)) = 1.0
        _Bloom ("Fake Bloom Strength", Range(0.0, 2.0)) = 1.0
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
        Blend One OneMinusSrcAlpha   // premultiplied alpha: low alpha + high rgb = additive light
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_AuroraVeilAAA"
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
                float _RotationAngle;
                float _HighSpeedTransition;
                float _Layers;
                float _SegmentGap;
                float _EventHorizon;
                float _RingOuter;
                float _HeatShimmer;
                float _Sparks;
                float _Bloom;
                float _Exposure;
                float _Tonemap;
            CBUFFER_END

            #define PI 3.14159265359
            #define TWO_PI 6.28318530718

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

            // ------------------------------------------------------------
            // Helpers (sin-free hashes: stable on mobile / low precision GPUs)
            // ------------------------------------------------------------
            float sq(float x) { return x * x; }

            float hash11(float x)
            {
                x = frac(x * 0.1031);
                x *= x + 33.33;
                x *= x + x;
                return frac(x);
            }

            float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float max3(float3 v) { return max(v.x, max(v.y, v.z)); }

            float2 rot2(float2 p, float a)
            {
                float s, c;
                sincos(a, s, c);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
                float a = hash12(i);
                float b = hash12(i + float2(1, 0));
                float c = hash12(i + float2(0, 1));
                float d = hash12(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm4(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    v += a * vnoise(p);
                    p = mul(float2x2(1.6, 1.2, -1.2, 1.6), p) + 17.0;
                    a *= 0.5;
                }
                return v / 0.9375;
            }

            float3 ACESFilm(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
            }

            // ------------------------------------------------------------
            // Main gauge
            // ------------------------------------------------------------
            float4 AuroraGauge(float2 p, float px)
            {
                float t = _Time.y;
                float progress = saturate(_Power * 0.01);
                float hs = saturate(_HighSpeedTransition);
                float hasProg = step(0.001, progress);
                float full = step(0.999, progress);
                float speed = 1.0 + 1.5 * hs;

                float r = length(p);
                float2 pr = rot2(p, _RotationAngle);
                float2 dir = pr / max(r, 1e-4);                    // rotated unit direction: seamless noise domain
                float ang = atan2(pr.y, pr.x);
                float na = 1.0 - frac((ang - PI * 0.5) / TWO_PI);  // 0..1 clockwise from top

                // Palette: low = bright hem, high = cooler upper curtain
                float3 baseCol = _RimColor.rgb;
                float3 cLow = lerp(baseCol, float3(0.30, 1.00, 0.60), 0.45);
                float3 cHigh = lerp(baseCol, float3(0.60, 0.30, 1.00), 0.60);
                cLow = lerp(cLow, float3(1.00, 0.80, 0.40), hs * 0.5);
                cHigh = lerp(cHigh, float3(1.00, 0.30, 0.60), hs * 0.5);
                float3 activeCol = lerp(baseCol, float3(1.0, 0.90, 0.70), hs * 0.5);
                float3 hotCol = lerp(activeCol, float3(1.0, 0.97, 0.90), 0.6);
                float3 trackBase = _Color.rgb;
                float3 dimCol = lerp(trackBase, float3(0.5, 0.6, 0.8), 0.5) * 0.35;

                // Radii
                float inner = _EventHorizon;
                float outer = max(_RingOuter, inner + 0.12);
                float coreR = max(inner - 0.04, 0.03);
                float rf = outer + 0.035;                          // frame ring

                // ---------------- Progress / focus (all angular, all bounded by the curtain itself) ----------------
                float d = progress - na;                           // > 0 : already revealed
                float reveal = lerp(smoothstep(0.0, 0.035, d) * hasProg, 1.0, hs);
                float head = frac(t * 0.5);
                float dHead = frac(na - head + 0.5) - 0.5;
                float spin = exp(-sq(dHead / 0.06)) * hs;          // orbiting bright curtain in overcharge
                float front = exp(-sq(d / 0.03)) * hasProg * (1.0 - hs) * (1.0 - full);
                float focus = front + spin;
                float trailN = exp(-max(d, 0.0) * 1.6);
                float trailHS = exp(-frac(head - na) * 1.6);
                float trail = lerp(trailN, trailHS, hs);
                float bright = lerp(0.55, 1.1, trail);

                // ---------------- Plate + stars ----------------
                float plateMask = saturate(0.5 - (r - (rf + 0.02)) / px);
                float plateA = 0.62 * plateMask;
                float3 plateCol = trackBase * (0.30 + 0.45 * saturate(r * 1.2));
                float2 sp2 = p * 14.0;
                float2 sid = floor(sp2);
                float2 sf = frac(sp2) - 0.5;
                float sh1 = hash12(sid + 9.1);
                float2 jit = float2(hash12(sid + 1.7), hash12(sid + 5.3)) - 0.5;
                float2 sdd = sf - jit * 0.6;
                float twinkle = 0.5 + 0.5 * sin(t * (1.0 + 3.0 * sh1) + sh1 * 50.0);
                float stars = exp(-dot(sdd, sdd) / 0.012) * step(0.80, sh1) * (0.35 + 0.65 * twinkle);

                // ---------------- Aurora curtains (3 layers, radial streaks, wavy hem) ----------------
                float bandIn = smoothstep(inner, inner + 0.06, r);
                float amp = (0.015 + _SegmentGap * 1.5) * (0.6 + 0.4 * _HeatShimmer);
                float F = max(_Layers, 3.0) * 0.28;
                float3 aur = 0.0;

                [unroll]
                for (int k = 0; k < 3; k++)
                {
                    float fk = (float)k;
                    float fa = F * (1.0 + 0.55 * fk);
                    float sp = (0.10 + 0.07 * fk) * speed;
                    float2 qn = dir * fa;

                    float hemN = vnoise(qn * 0.6 + float2(t * 0.18 * speed + fk * 7.0, fk * 3.0));
                    float hem = outer - 0.025 - fk * 0.035 + (hemN - 0.5) * 2.0 * amp;
                    float dh = hem - r;                            // > 0 : above the hem (toward the centre)
                    float hemSoft = smoothstep(-0.012, 0.010, dh);

                    float streak = vnoise(qn + float2(t * sp, r * (1.3 + fk * 0.7)));
                    streak = smoothstep(0.22, 0.85, streak);
                    float rays = 0.55 + 0.45 * vnoise(qn * 5.0 + float2(-t * sp * 2.0 * _HeatShimmer, r * 1.5 + fk * 11.0));

                    float hgt = (0.09 + 0.05 * fk) * (0.6 + streak) * (1.0 + 0.9 * focus);
                    float fall = exp(-max(dh, 0.0) / hgt);
                    float I = hemSoft * fall * (0.25 + 1.2 * streak) * rays;

                    float g = saturate(dh / 0.22);
                    float3 ck = lerp(cLow, cHigh, g) + hotCol * exp(-max(dh, 0.0) * 55.0) * 0.5;
                    aur += ck * I * (1.0 - 0.22 * fk);
                }
                aur *= bandIn * bright * (1.0 + 1.2 * focus);
                float3 aurShown = aur * lerp(0.07, 1.0, reveal);     // faint ghost where not yet charged
                float aurLum = max3(aurShown);

                // haze spilling around the curtains (bounded by the plate / vignette)
                float haze = reveal * (0.5 + 0.5 * trail);
                float3 hazeRGB = (cHigh * 0.06 * smoothstep(inner, outer, r)
                                + cLow * 0.10 * exp(-max(r - (outer - 0.03), 0.0) * 30.0) * bandIn) * haze * _Bloom;

                // ---------------- Frame ring with progress arc ----------------
                float dF = abs(r - rf);
                float frameBand = saturate(0.5 - (dF - 0.004) / px);
                float arcF = (progress - na) * TWO_PI * rf;
                float frameLit = lerp(saturate(arcF / px + 0.5) * hasProg, 1.0, hs);
                float3 frameLitCol = lerp(cLow, hotCol, 0.45) * (0.9 + 0.9 * trail);
                float3 frameRGB = lerp(dimCol * 1.2 + trackBase * 0.15, frameLitCol, frameLit) * frameBand;

                float frameSoft = lerp(smoothstep(-0.08, 0.04, arcF) * hasProg, 1.0, hs);
                float dFo = max(dF - 0.004, 0.0);
                float3 frameGlow = cLow * (exp(-dFo * 60.0) * 0.55 + exp(-dFo * 18.0) * 0.14)
                                 * frameSoft * (0.35 + 0.65 * trail) * _Bloom * (1.0 - frameBand);

                // round tip dot / orbiting dot ON the frame
                float s1, c1;
                sincos(PI * 0.5 - progress * TWO_PI - _RotationAngle, s1, c1);
                float2 q1 = p - float2(c1, s1) * rf;
                float tipBlob = exp(-dot(q1, q1) / 0.0006) * hasProg * (1.0 - full) * (1.0 - hs);
                float s2, c2;
                sincos(PI * 0.5 - head * TWO_PI - _RotationAngle, s2, c2);
                float2 q2 = p - float2(c2, s2) * rf;
                float headBlob = exp(-dot(q2, q2) / 0.0006) * hs;
                float3 tipRGB = hotCol * (tipBlob + headBlob) * 2.4;

                // minor ticks just inside the frame
                float tIn = rf - 0.030;
                float tOut = rf - 0.016;
                float mi = floor(na * 48.0 + 0.5);
                float tBand = saturate(0.5 - (abs(r - 0.5 * (tIn + tOut)) - 0.5 * (tOut - tIn)) / px);
                float fm = frac(na * 48.0);
                float dm = min(fm, 1.0 - fm) / 48.0 * TWO_PI * r;
                float tickA = tBand * saturate((0.0028 - dm) / px + 0.5);
                float tickLit = lerp(saturate((progress - mi / 48.0) * 48.0 + 0.5) * hasProg, 1.0, hs);
                float3 ticksRGB = lerp(dimCol, cLow * (0.8 + 0.6 * trail), tickLit) * tickA;

                // ---------------- Core: glass moon lit by the aurora ----------------
                float coreMask = saturate(0.5 - (r - coreR) / px);
                float cz = sqrt(saturate(1.0 - (r * r) / (coreR * coreR)));
                float fres = pow(1.0 - cz, 3.0);
                float2 pd = p + (float2(vnoise(p * 5.0 + float2(0.0, t * 2.5)), vnoise(p * 5.0 + float2(4.3, t * 2.2))) - 0.5)
                                * 0.02 * _HeatShimmer;
                float swirl = fbm4(rot2(pd / coreR, t * (0.8 + 2.5 * hs)) * 2.2 + t * 0.2);
                float coreI = lerp(0.25 + 0.75 * progress, 1.2, hs);
                float pulse = 1.0 + 0.22 * sin(t * lerp(2.5, 14.0, hs)) * _HeatShimmer;
                float3 L = normalize(float3(-0.45, 0.55, 0.70));
                float3 H = normalize(L + float3(0, 0, 1));
                float3 nC = float3(p / coreR, cz);
                float glint = pow(saturate(dot(nC, H)), 60.0) * 0.5;
                float3 coreRGB = (lerp(cLow, cHigh, swirl) * (0.30 + 1.3 * swirl) * (0.35 + 0.9 * pow(cz, 1.5))
                                + hotCol * pow(cz, 6.0) * 1.0
                                + cHigh * fres * 0.9) * coreI * pulse;
                coreRGB = (coreRGB + glint) * coreMask;
                float3 coreGlow = cLow * exp(-max(r - coreR, 0.0) * 22.0) * coreI * 0.45 * (1.0 - coreMask) * _Bloom;

                float collar = saturate(0.5 - (abs(r - (inner - 0.012)) - 0.003) / px);
                float3 collarRGB = lerp(dimCol, cLow * 0.7, saturate(hasProg + hs)) * collar;

                // ---------------- Overcharge shockwave ----------------
                float ph = frac(t * 0.9);
                float swR = lerp(coreR, rf, ph);
                float sw = exp(-sq((r - swR) / 0.022)) * sq(1.0 - ph) * hs;
                float3 swRGB = cLow * sw * 1.4;

                // ---------------- Stardust drifting up through the curtain ----------------
                float3 sparkRGB = 0.0;
                if (_Sparks > 0.001)
                {
                    float SN = 48.0;
                    float cs = na * SN;
                    float id = floor(cs);
                    float h1 = hash11(id);
                    float h2 = hash11(id + 17.3);
                    float h3 = hash11(id + 41.7);
                    float sExists = step(0.4 - 0.25 * hs, hash11(id + 7.7));
                    float life = frac(t * (0.12 + 0.20 * h2) * speed + h1);
                    float ca = (id + 0.2 + 0.6 * h3) / SN;
                    float rr = outer - 0.03 - life * (0.10 + 0.12 * h1);       // rises toward the centre
                    float wob = sin(t * 2.0 + h1 * 20.0) * 0.012 * life;
                    float dx = (na - ca) * TWO_PI * rr + wob;
                    float dy = r - rr;
                    float sz = 0.0045 + 0.0045 * h2;
                    float s = exp(-(dx * dx + dy * dy) / (sz * sz));
                    float flick = 0.6 + 0.4 * sin(t * 18.0 * h3 + h1 * 6.0);
                    float fade = (1.0 - life) * smoothstep(0.0, 0.10, life);
                    float alive = lerp(step(ca, progress) * hasProg, 1.0, hs);
                    sparkRGB = hotCol * s * fade * flick * alive * sExists * bandIn * 2.6 * _Sparks;
                }

                // ---------------- Compose ----------------
                float vig = 1.0 - smoothstep(0.88, 0.99, r);
                float3 starsRGB = float3(0.75, 0.85, 1.0) * stars * 0.55 * plateMask * (1.0 - saturate(aurLum * 2.0));

                float3 soft = (frameGlow + hazeRGB + tipRGB + swRGB + sparkRGB + coreGlow) * vig;
                float3 emis = plateCol * plateA + starsRGB * plateA
                            + aurShown * plateMask
                            + frameRGB + ticksRGB + collarRGB + coreRGB + soft;

                float bodyA = max(max(plateA, frameBand * 0.95), max(tickA * 0.8, max(coreMask * 0.95, collar * 0.6)));
                float alpha = saturate(bodyA + aurLum * 0.30 + max3(soft) * 0.3);

                float edgeFade = 1.0 - smoothstep(0.97, 1.0, r);
                return float4(emis * edgeFade, alpha * edgeFade);
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float px = max(fwidth(p.x), 1e-4);   // pixel size in gauge space (assumes a square Image)

                float4 col = AuroraGauge(p, px);

                // Hot highlights bleed toward white
                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 5.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.75);

                // Exposure + filmic tonemap (set Tonemap to 0 and use HDR + Bloom if you want the post stack to do it)
                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                // Dither against banding in the long gradients
                float dn = hash12(input.positionCS.xy + frac(_Time.y) * 61.0) - 0.5;
                col.rgb += dn / 255.0 * max(col.a, max3(col.rgb));

                // UI vertex colour (premultiplied-correct)
                col.rgb *= input.color.rgb * input.color.a;
                col.a *= input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, input.worldPos.xy) * step(input.worldPos.xy, _ClipRect.zw);
                col *= inside.x * inside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return half4(col);
            }
            ENDHLSL
        }
    }
}
