Shader "UI/Custom/NeonTubeSignAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Background / Wall Tint", Color) = (0.05, 0.08, 0.15, 1.0)
        _RimColor ("Neon Color", Color) = (0.0, 0.85, 1.0, 1.0)

        [Header(Progress Control)]
        _Power ("Progress (0 - 100)", Range(0.0, 100.0)) = 25.0
        _RotationAngle ("Base Angle Offset (radians)", Float) = 0.0
        _HighSpeedTransition ("High-Speed / Overcharge Mode", Range(0.0, 1.0)) = 0.0

        [Header(Shape)]
        _Layers ("Tube Rings", Range(1, 6)) = 3
        _SegmentGap ("Tube Break Width (at the top)", Range(0.0, 0.2)) = 0.06
        _EventHorizon ("Inner Radius (bulb area)", Range(0.05, 0.4)) = 0.22
        _RingOuter ("Outer Tube Radius", Range(0.3, 0.75)) = 0.60

        [Header(Look)]
        _HeatShimmer ("Gas Flicker & Buzz", Range(0.0, 2.0)) = 1.0
        _Sparks ("Arc Sparks (overcharge)", Range(0.0, 1.0)) = 1.0
        _Bloom ("Neon Glow Strength", Range(0.0, 2.0)) = 1.0
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
            Name "UI_NeonTubeSignAAA"
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
            float4 NeonGauge(float2 p, float px)
            {
                float t = _Time.y;
                float progress = saturate(_Power * 0.01);
                float hs = saturate(_HighSpeedTransition);
                float hasProg = step(0.001, progress);

                float r = length(p);
                float2 dir = p / max(r, 1e-4);
                float2 pr = rot2(p, _RotationAngle);
                float ang = atan2(pr.y, pr.x);
                float na = 1.0 - frac((ang - PI * 0.5) / TWO_PI);   // 0..1 clockwise from top

                // Palette
                float3 baseCol = _RimColor.rgb;
                float3 activeCol = lerp(baseCol, float3(1.0, 0.90, 0.70), hs * 0.5);
                float3 pinkCol = lerp(float3(1.0, 0.22, 0.60), float3(1.0, 0.55, 0.45), hs * 0.5);
                float3 trackBase = _Color.rgb;

                float3 L = normalize(float3(-0.45, 0.55, 0.70));

                // Radii / layout
                float inner = _EventHorizon;
                float outer = max(_RingOuter, inner + 0.12);
                float K = clamp(round(_Layers), 1.0, 6.0);
                float pitch = (outer - inner) / K;
                float w = pitch * 0.28;                       // tube half width
                float coreR = max(inner - 0.05, 0.03);
                float pr0 = outer + 0.045;                    // sign board radius
                float head = frac(t * 0.5);
                float detail = saturate((w / px - 2.5) / 3.0);

                // ---------------- Tubes (3 nearest rings: shape + glow) ----------------
                float3 tubeAcc = 0.0;
                float shapeAcc = 0.0;
                float3 glowAcc = 0.0;
                float u0 = (r - inner) / pitch;
                float k0 = clamp(floor(u0), 0.0, K - 1.0);

                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    float fk = k0 - 1.0 + (float)i;
                    float valid = step(0.0, fk) * step(fk, K - 1.0);
                    float fkc = clamp(fk, 0.0, K - 1.0);

                    float3 colK = lerp(activeCol, pinkCol, 0.55 * fmod(fkc, 2.0));
                    float3 hotK = lerp(colK, float3(1.0, 0.97, 0.92), 0.6);
                    float3 deepK = pow(max(colK, 0.001), 1.7) * 1.25;

                    float rk = inner + pitch * (fkc + 0.5);
                    float Lk = TWO_PI * rk;
                    float gapH = _SegmentGap * 0.5;
                    float aS = na * Lk - gapH;                // arc length from the tube start
                    float aE = (1.0 - na) * Lk - gapH;        // arc length from the tube end
                    float dx = max(max(-aS, -aE), 0.0);       // beyond the ends -> rounded caps
                    float dr = r - rk;
                    float dd = length(float2(dx, dr));
                    float shape = saturate(0.5 - (dd - w) / px);
                    float tn = dd / w;
                    float tc = dr / w;
                    float h = sqrt(saturate(1.0 - tn * tn));

                    // ignition: inner rings lag a little behind outer ones
                    float ringProg = saturate(progress * (1.0 + 0.06 * (K - 1.0)) - 0.06 * fkc);
                    float hp = step(0.001, ringProg);
                    float d = ringProg - na;                  // > 0 : already lit
                    float litK = lerp(saturate(d * Lk / px + 0.5) * hp, 1.0, hs);
                    float litSoft = lerp(smoothstep(-0.06, 0.03, d) * hp, 1.0, hs);
                    float front = exp(-sq(d / 0.02)) * hp * (1.0 - hs) * (1.0 - step(0.999, ringProg));

                    // neon behaviour: buzz, random dips, stutter just behind the strike point
                    float dip = step(0.965, hash11(floor(t * 9.0) + fkc * 13.1));
                    float fl = 1.0 - lerp(0.12, 0.40, hs) * dip * saturate(_HeatShimmer);
                    float gasN = lerp(1.0, 0.85 + 0.30 * vnoise(float2(na * 30.0 + fkc * 5.0, t * 2.5)), saturate(_HeatShimmer) * 0.5);
                    float stut = lerp(1.0, step(0.35, vnoise(float2(na * 120.0, t * 25.0 + fkc))), exp(-max(d, 0.0) * 40.0) * (1.0 - hs) * hp);
                    float gain = fl * gasN * stut * (1.0 + 1.2 * front) * lerp(1.0, 1.5, hs);

                    // glass tube
                    float spec = exp(-sq((tc + 0.45) / 0.18)) * 0.55 * h;
                    float3 unlit = trackBase * 0.9 + float3(0.10, 0.12, 0.15) * h * 0.6;
                    float3 lit = lerp(deepK, colK, saturate(h * 1.6));
                    lit = lerp(lit, float3(1.0, 0.97, 0.92), pow(h, 5.0) * (0.75 + 0.2 * hs));   // white-hot centre
                    lit *= 0.85 + 0.5 * h;
                    float3 tubeRGB = lerp(unlit, lit * gain, litK) + spec * float3(0.8, 0.9, 1.0) * (0.5 + 0.5 * litK);

                    // brass electrode caps at both tube ends
                    float dEnd = min(aS, aE);
                    float capMask = saturate((w * 1.6 - dEnd) / px + 0.5);
                    float3 brass = float3(0.42, 0.30, 0.18) * (0.30 + 0.9 * h) + spec * 0.6;
                    brass += hotK * litK * 0.25 * exp(-max(dEnd, 0.0) / w);
                    tubeRGB = lerp(tubeRGB, brass, capMask * 0.9);

                    // support clips (4 per ring)
                    float dClip = abs(frac(na * 4.0) - 0.5) / 4.0 * Lk;
                    float clipM = saturate((0.012 - dClip) / px + 0.5)
                                * saturate(0.5 - (abs(dr) - (w + 0.007)) / px) * detail;
                    float3 clipRGB = float3(0.05, 0.05, 0.06) + 0.18 * smoothstep(0.4, 1.0, dr / (w + 0.007));
                    tubeRGB = lerp(tubeRGB, clipRGB, clipM);
                    shape = max(shape, clipM);

                    // glow of this ring (three scales)
                    float dO = max(dd - w, 0.0);
                    float glowShape = exp(-dO * 55.0) * 0.9 + exp(-dO * 20.0) * 0.35 + exp(-dO * 7.0) * 0.10;
                    float3 glowK = colK * glowShape * litSoft * gain * (1.0 + 0.8 * front);

                    tubeAcc += tubeRGB * shape * valid;
                    shapeAcc += shape * valid;
                    glowAcc += glowK * valid;
                }
                shapeAcc = saturate(shapeAcc);
                glowAcc *= _Bloom;

                // ---------------- Wall (dark bricks lit by the neon) ----------------
                float plateMask = saturate(0.5 - (r - pr0) / px);
                float bh = 0.09;
                float bw = 0.2;
                float row = floor(p.y / bh);
                float off = (frac(row * 0.5) > 0.25) ? 0.5 * bw : 0.0;
                float2 bp = float2((p.x + off) / bw, p.y / bh);
                float2 bf = frac(bp);
                float2 bid = floor(bp);
                float dM = min(min(bf.x, 1.0 - bf.x) * bw, min(bf.y, 1.0 - bf.y) * bh);
                float brickMask = saturate((dM - 0.004) / px + 0.5);
                float albedo = lerp(0.25, 1.0, brickMask) * (0.7 + 0.6 * hash12(bid)) * (0.85 + 0.3 * vnoise(p * 40.0));
                albedo = lerp(1.0, albedo, saturate((bh / px - 3.0) / 4.0));
                float3 wallCol = lerp(float3(0.20, 0.12, 0.10), trackBase * 3.0, 0.5);
                float3 wallRGB = wallCol * albedo * (0.10 + 1.8 * glowAcc);

                // metal rim of the sign board
                float rimBand = saturate(0.5 - (abs(r - pr0) - 0.008) / px);
                float rimLight = 0.35 + 0.65 * saturate(dot(dir, L.xy) * 0.5 + 0.5);
                float3 rimRGB = float3(0.16, 0.17, 0.20) * rimLight + glowAcc * 0.8;

                float3 emis = wallRGB * plateMask;
                emis = lerp(emis, rimRGB, rimBand);
                emis = emis * (1.0 - shapeAcc) + tubeAcc;

                // ---------------- Bulb in the centre ----------------
                float coreMask = saturate(0.5 - (r - coreR) / px);
                float cz = sqrt(saturate(1.0 - (r * r) / (coreR * coreR)));
                float fres = pow(1.0 - cz, 3.0);
                float3 hotCol = lerp(activeCol, float3(1.0, 0.97, 0.90), 0.6);
                float3 deepCol = pow(max(activeCol, 0.001), 1.7) * 1.25;
                float coreI = lerp(0.25 + 0.75 * progress, 1.2, hs);
                float pulse = 1.0 + 0.18 * sin(t * lerp(3.0, 18.0, hs)) * saturate(_HeatShimmer);
                float3 nC = float3(p / coreR, cz);
                float3 H = normalize(L + float3(0, 0, 1));
                float glint = pow(saturate(dot(nC, H)), 60.0) * 0.5;
                float3 coreRGB = (activeCol * (0.35 + 0.9 * pow(cz, 1.5))
                                + hotCol * pow(cz, 6.0) * 1.3
                                + deepCol * fres * 0.9) * coreI * pulse;
                coreRGB = (coreRGB + glint) * coreMask;
                float3 coreGlow = activeCol * exp(-max(r - coreR, 0.0) * 22.0) * coreI * 0.45 * (1.0 - coreMask) * _Bloom;
                float collar = saturate(0.5 - (abs(r - (coreR + 0.014)) - 0.005) / px);
                float3 collarRGB = float3(0.07, 0.07, 0.08) * (0.6 + 0.8 * rimLight) * collar;
                emis = emis * (1.0 - collar) + collarRGB;

                // ---------------- Overcharge: arcs jumping between the rings + light pulse ----------------
                float ru = floor(u0 + 0.5);
                float du = abs(u0 - ru);
                float arcZone = step(1.0, ru) * step(ru, K - 1.0) * (1.0 - smoothstep(0.22, 0.34, du));
                float slice = floor(t * 14.0);
                float ca = hash11(slice * 7.3 + ru * 3.1);
                float aex = step(0.55, hash11(slice * 3.7 + ru * 5.7));
                float jag = (vnoise(float2(r * 80.0, slice * 1.3 + ru)) - 0.5) * 0.035;
                float dxA = (na - ca) * TWO_PI * r + jag;
                float bolt = exp(-dxA * dxA / (sq(0.004) + sq(px))) * arcZone * aex * hs * _Sparks;
                float3 arcRGB = lerp(activeCol, float3(0.85, 0.92, 1.0), 0.6) * bolt * 2.2;

                float ph = frac(t * 0.9);
                float swR = lerp(coreR, pr0, ph);
                float sw = exp(-sq((r - swR) / 0.022)) * sq(1.0 - ph) * hs;
                float3 swRGB = activeCol * sw * 1.2 * _Bloom;

                // ---------------- Compose ----------------
                float vig = 1.0 - smoothstep(0.86, 0.99, r);
                float3 soft = (glowAcc * (1.0 - shapeAcc) * 0.7 + coreGlow + arcRGB + swRGB) * vig;
                emis += coreRGB + soft;

                float bodyA = max(max(plateMask * 0.96, shapeAcc), max(coreMask * 0.95, collar * 0.9));
                float alpha = saturate(bodyA + max3(soft) * 0.3);

                float edgeFade = 1.0 - smoothstep(0.97, 1.0, r);
                return float4(emis * edgeFade, alpha * edgeFade);
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float px = max(fwidth(p.x), 1e-4);   // pixel size in gauge space (assumes a square Image)

                float4 col = NeonGauge(p, px);

                // Hot highlights bleed toward white
                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 5.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.75);

                // Exposure + filmic tonemap (set Tonemap to 0 and use HDR + Bloom if you want the post stack to do it)
                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                // Dither against banding in the long glow gradients
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
