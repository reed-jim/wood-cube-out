Shader "UI/Custom/RadialMoltenLiquid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Glass Tint Color", Color) = (0.03, 0.07, 0.12, 1.0)
        _RimColor ("Liquid / Lava Color", Color) = (0.0, 0.85, 1.0, 1.0)

        [Header(Progress Control)]
        _Power ("Progress (0 - 100)", Range(0.0, 100.0)) = 25.0
        _RotationAngle ("Base Angle Offset (radians)", Float) = 0.0
        _HighSpeedTransition ("High-Speed / Overcharge Mode", Range(0.0, 1.0)) = 0.0

        [Header(Shape)]
        _Layers ("Segment Count", Range(3, 30)) = 16
        _EventHorizon ("Ring Inner Radius", Range(0.05, 0.4)) = 0.22
        _RingOuter ("Ring Outer Radius", Range(0.3, 0.75)) = 0.60

        [Header(Look)]
        _HeatShimmer ("Surface Wobble & Heat Haze", Range(0.0, 2.0)) = 1.0
        _Exposure ("Exposure", Range(0.25, 3.0)) = 1.0
        _Tonemap ("Filmic Tonemap Amount", Range(0.0, 1.0)) = 0.4

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
            Name "UI_RadialMoltenLiquid"
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
                float _EventHorizon;
                float _RingOuter;
                float _HeatShimmer;
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
            // Helpers
            // ------------------------------------------------------------
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

            float3 ACESFilm(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
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

            float fbm3(float2 p)
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

            float4 MoltenGauge(float2 p, float px)
            {
                float t = _Time.y;
                float progress = saturate(_Power * 0.01);
                float hs = saturate(_HighSpeedTransition);
                float hasProg = step(0.001, progress);
                float full = step(0.999, progress);
                float wob = _HeatShimmer;
                float sp = 1.0 + 2.5 * hs;

                float r = length(p);
                float2 dir = p / max(r, 1e-4);
                float2 pr = rot2(p, _RotationAngle);
                float ang = atan2(pr.y, pr.x);
                float na = 1.0 - frac((ang - PI * 0.5) / TWO_PI);   // 0..1 clockwise from top

                float inner = _EventHorizon;
                float outer = max(_RingOuter, inner + 0.08);
                float rc = 0.5 * (inner + outer);
                float hw = 0.5 * (outer - inner);
                float dBand = abs(r - rc) - hw;
                float band = saturate(0.5 - dBand / px);
                float tr = clamp((r - rc) / hw, -1.0, 1.0);
                float h = sqrt(saturate(1.0 - tr * tr));
                float3 n = float3(dir * tr, h);
                float3 L = normalize(float3(-0.45, 0.55, 0.70));
                float3 H = normalize(L + float3(0, 0, 1));
                float diff = saturate(dot(n, L));

                float3 liqCol = lerp(_RimColor.rgb, float3(1.0, 0.8, 0.45), hs * 0.5);
                float3 hotCol = lerp(liqCol, float3(1.0, 0.95, 0.8), 0.65);
                float3 crustCol = liqCol * 0.12;
                float3 glassTint = _Color.rgb;

                // ---------- Liquid surface: meniscus curve + wobble ----------
                float2 wp = p + (float2(vnoise(p * 7.0 + t * 1.3), vnoise(p * 7.0 + 9.0 - t)) - 0.5) * 0.04 * wob * (0.5 + hs);
                float wobble = (vnoise(float2(na * 40.0, t * 1.2)) - 0.5) * 0.03 * wob * (0.5 + hs);
                float dEdge = (progress - na) * TWO_PI * r + tr * tr * 0.025 + wobble;   // >0 = liquid
                float fill = lerp(saturate(dEdge / px + 0.5) * hasProg, 1.0, hs);
                float behind = saturate(progress - na);
                float heat = lerp(exp(-behind * 3.0), 1.0, hs);                          // newest liquid is hottest

                // ---------- Layer 2: lava body ----------
                float nn = fbm3(rot2(wp, t * 0.2 * sp) * 6.0 + float2(t * 0.4 * sp, 0.0));
                float n2 = vnoise(rot2(wp, -t * 0.15 * sp) * 11.0 + 5.0);
                float cracks = pow(saturate(1.0 - abs(nn * 2.0 - 1.0)), 6.0);
                float crustM = smoothstep(0.55, 0.75, n2) * (1.0 - heat) * (1.0 - hs);
                float3 body = lerp(liqCol * 0.55, liqCol * 1.2, nn);
                body = lerp(body, crustCol, crustM * 0.85);
                body += hotCol * cracks * (0.5 + 0.8 * heat + hs) * (0.4 + 0.6 * (1.0 - crustM));
                body *= (0.55 + 0.6 * h) * lerp(0.8, 1.6, saturate(heat + hs));

                float mens = exp(-pow(dEdge / 0.010, 2.0)) * hasProg * (1.0 - full) * (1.0 - hs);

                // ---------- Layer 1: glass vessel ----------
                float fres = pow(1.0 - h, 2.5);
                float3 glass = glassTint * (0.5 + 1.2 * fres);
                float specG = pow(saturate(dot(n, H)), 30.0) * 0.5;
                float streak = exp(-pow((tr + 0.5) / 0.08, 2.0)) * 0.35 * (0.6 + 0.4 * sin(na * TWO_PI * 3.0 + 1.0));
                float3 hl = float3(0.8, 0.9, 1.0) * (specG + streak);
                float3 rimLine = lerp(glassTint * 3.0, float3(1, 1, 1), 0.6) * saturate(1.2 - abs(dBand) / px) * 0.45;

                // Riveted metal bands (= segments)
                float Nb = max(round(_Layers), 3.0);
                float fcb = frac(na * Nb);
                float dB = min(fcb, 1.0 - fcb) / Nb * TWO_PI * r;
                float bandM = saturate((0.010 - dB) / px + 0.5) * band;
                float3 metal = lerp(float3(0.50, 0.52, 0.56), glassTint * 4.0, 0.3) * (0.3 + 0.9 * diff)
                             + pow(saturate(dot(n, H)), 20.0) * 0.4;
                float rivetD = length(float2(dB, abs(r - rc) - hw * 0.72));
                float rivet = saturate((0.007 - rivetD) / px + 0.5) * band;
                float3 rivetCol = float3(0.85, 0.88, 0.95) * (0.5 + 0.5 * diff);

                // ---------- Layer 3: bubbles ----------
                float BN = 36.0;
                float bi = floor(na * BN);
                float b1 = hash11(bi);
                float b2 = hash11(bi + 17.3);
                float b3 = hash11(bi + 41.7);
                float bex = step(0.35 - 0.3 * hs, hash11(bi + 7.7));
                float bca = (bi + 0.2 + 0.6 * b3) / BN;
                float bph = frac(t * (0.10 + 0.20 * b2) * sp + b1);
                float brr = rc + (bph * 2.0 - 1.0) * hw * 0.7;
                float bsz = 0.007 + 0.008 * b2;
                float2 bd = float2((na - bca) * TWO_PI * r, r - brr);
                float bring = saturate(1.2 - abs(length(bd) - bsz) / px) * 0.8;
                float bhl = saturate((bsz * 0.22 - length(bd - bsz * float2(-0.35, 0.35))) / px + 0.5) * 0.9;
                float bfade = smoothstep(0.0, 0.1, bph) * (1.0 - smoothstep(0.85, 1.0, bph));
                float3 bubRGB = hotCol * (bring + bhl) * bex * bfade * fill;

                // ---------- Compose ring ----------
                float3 bandCol = lerp(glass, body, fill);
                float bandA = band * lerp(0.55, 0.96, fill);
                bandCol += hotCol * mens * 2.2 + bubRGB;
                bandCol = lerp(bandCol, metal, bandM);
                bandCol += rivetCol * rivet;
                bandA = max(bandA, bandM);
                float3 ringRGB = bandCol * bandA + hl * band * (1.0 - bandM * 0.7) + rimLine;

                // ---------- Layer 4: core lava-lamp blobs in a glass bowl ----------
                float coreR = inner * 0.8;
                float bowl = saturate(0.5 - (r - coreR) / px);
                float mv = coreR * 0.25 * (0.4 + 0.6 * progress + hs);
                float rr0 = coreR * 0.34;
                float f = 0.0;
                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    float fi = (float)i;
                    float2 bc = float2(cos(t * 0.7 * sp + fi * 2.1), sin(t * 0.9 * sp + fi * 2.7)) * mv * (0.6 + 0.2 * fi);
                    float2 dq = p - bc;
                    float ri = rr0 * (1.0 - 0.18 * fi);
                    f += ri * ri / (dot(dq, dq) + 1e-5);
                }
                float fw = max(fwidth(f), 1e-3);
                float blob = smoothstep(1.0 - fw, 1.0 + fw, f);
                float coreBright = 0.35 + 0.9 * progress + 0.5 * hs;
                float cpulse = 1.0 + 0.15 * sin(t * lerp(3.0, 12.0, hs));
                float3 lava = lerp(liqCol, hotCol, saturate((f - 1.0) * 0.7)) * coreBright * cpulse;
                float3 bowlRGB = glassTint * 0.7 + liqCol * saturate(f * 0.25) * 0.5 * coreBright;
                float3 coreRGB = lerp(bowlRGB, lava, blob);
                float cz = sqrt(saturate(1.0 - (r * r) / (coreR * coreR)));
                float glint = pow(saturate(dot(float3(p / coreR, cz), H)), 50.0) * 0.5;
                float coreA = bowl * 0.95;
                float3 coreOut = coreRGB * coreA
                               + (float3(0.8, 0.9, 1.0) * glint + pow(saturate(r / coreR), 6.0) * 0.4 * glassTint * 3.0) * bowl
                               + lerp(glassTint * 3.0, float3(1, 1, 1), 0.6) * saturate(1.2 - abs(r - coreR) / px) * 0.45;

                // ---------- Layer 5: heat glow (UNCOUPLED from fill progress) ----------
                float dOut = max(dBand, 0.0);
                float glowShape = exp(-dOut * 34.0) * 0.7 + exp(-dOut * 9.0) * 0.22;
                
                // Note: Removed 'sf' (progress fade) multiplier here so the glow stays fully lit unconditionally
                float3 glow = liqCol * glowShape * (0.5 + 0.5 * hs) * (1.0 - band);
                glow += liqCol * exp(-max(r - coreR, 0.0) * 18.0) * 0.3 * (0.5 + 0.5 * hs) * (1.0 - bowl);
                glow *= 1.0 - smoothstep(0.80, 0.99, r);

                float3 rgb = ringRGB + coreOut + glow;
                float alpha = saturate(bandA + coreA + max3(glow) * 0.3);
                return float4(rgb, alpha);
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float px = max(fwidth(p.x), 1e-4);      // pixel size in gauge space (assumes a square Image)

                float t = _Time.y;
                float4 col = MoltenGauge(p, px);
                float edgeFade = 1.0 - smoothstep(0.97, 1.0, length(p));
                col *= edgeFade;

                // Hot highlights bleed toward white
                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 5.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.75);

                // Exposure + filmic tonemap
                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                // Dither
                float dn = hash12(input.positionCS.xy + frac(t) * 61.0) - 0.5;
                col.rgb += dn / 255.0 * max(col.a, max3(col.rgb));

                // UI vertex colour (premultiplied-correct)
                col.rgb *= input.color.rgb * input.color.a;
                col.a *= input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 clipInside = step(_ClipRect.xy, input.worldPos.xy) * step(input.worldPos.xy, _ClipRect.zw);
                col *= clipInside.x * clipInside.y;
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