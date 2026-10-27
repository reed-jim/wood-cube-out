Shader "UI/Custom/RadialHoloHUD"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Background Panel Color", Color) = (0.03, 0.07, 0.12, 1.0)
        _RimColor ("Hologram Color", Color) = (0.0, 0.85, 1.0, 1.0)

        [Header(Progress Control)]
        _Power ("Progress (0 - 100)", Range(0.0, 100.0)) = 25.0
        _RotationAngle ("Base Angle Offset (radians)", Float) = 0.0
        _HighSpeedTransition ("High-Speed / Overcharge Mode", Range(0.0, 1.0)) = 0.0

        [Header(Shape)]
        _Layers ("Segment Count", Range(3, 30)) = 16
        _EventHorizon ("Ring Inner Radius", Range(0.05, 0.4)) = 0.22
        _RingOuter ("Ring Outer Radius", Range(0.3, 0.75)) = 0.60

        [Header(Look)]
        _HeatShimmer ("Glitch & Chromatic Split", Range(0.0, 2.0)) = 1.0
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
            Name "UI_RadialHoloHUD"
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

            // ------------------------------------------------------------
            // Hologram line-art luminance. Pure vector shapes, anti-aliased by px.
            // Evaluated 3x per pixel (centre + two chromatic offsets).
            // ------------------------------------------------------------
            float HoloField(float2 p, float px)
            {
                float t = _Time.y;
                float progress = saturate(_Power * 0.01);
                float hs = saturate(_HighSpeedTransition);
                float hasProg = step(0.001, progress);
                float full = step(0.999, progress);
                float sp = 1.0 + 3.0 * hs;

                float r = length(p);
                float2 pr = rot2(p, _RotationAngle);
                float ang = atan2(pr.y, pr.x);
                float na = 1.0 - frac((ang - PI * 0.5) / TWO_PI);   // 0..1 clockwise from top

                float inner = _EventHorizon;
                float outer = max(_RingOuter, inner + 0.08);
                float rc = 0.5 * (inner + outer);
                float hw = 0.5 * (outer - inner);

                float I = 0.0;

                // ================= Segmented ring (outlined blocks) =================
                float N = max(round(_Layers), 3.0);
                float cell = na * N;
                float id = floor(cell);
                float fc = frac(cell);
                float arcW = TWO_PI * r / N;
                float gap = min(0.008, arcW * 0.25);
                float ax = (fc - 0.5) * arcW;                         // arc offset from segment centre
                float dA = abs(ax) - (arcW * 0.5 - gap);
                float dR = abs(r - rc) - hw;
                float d = max(dA, dR);                                // box SDF in (arc, radial)
                float outline = saturate(1.2 - abs(d) / px);
                float inside = saturate(0.5 - d / px);

                float segFill = saturate(progress * N - id);          // how much of THIS segment is lit
                float litSeg = saturate((segFill - fc) * arcW / px + 0.5) * step(0.001, segFill);
                float lit = lerp(litSeg, 1.0, hs);
                float outlineLit = max(step(0.001, segFill), hs);

                // Overcharge: a brightness wave chases around the segments
                float wave = 0.5 + 0.5 * cos(TWO_PI * (id / N * 2.0 - t * 0.7));
                float segBright = lerp(1.0, 0.35 + 0.9 * wave, hs);

                // Diagonal hatch fill, scrolling slowly
                float hatch = smoothstep(0.35, 0.65, 0.5 + 0.5 * sin((ax + (r - rc)) * 110.0 - t * 2.0 * sp));
                float radialGrad = 0.8 + 0.4 * saturate((r - inner) / (outer - inner));

                I += outline * lerp(0.28, 1.0, outlineLit) * segBright;
                I += inside * lit * (0.30 + 0.30 * hatch) * segBright * radialGrad;
                I += inside * 0.04;

                // Bright growth bar on the segment currently filling
                float isCur = step(abs(id - floor(progress * N)), 0.5);
                float fdist = (fc - segFill) * arcW / 0.010;
                I += exp(-fdist * fdist) * inside * isCur * step(0.001, segFill) * (1.0 - hs) * 1.3;

                // ================= Pointer + beam at the progress tip =================
                float tipPos = lerp(progress, frac(t * 0.6), step(0.5, hs));
                float showTip = max(hasProg * (1.0 - full), step(0.5, hs));
                float dT = (frac(na - tipPos + 0.5) - 0.5) * TWO_PI * r;
                float pTop = outer + 0.075;
                float pw = (pTop - r) * 0.55;
                float tri = saturate((pw - abs(dT)) / px + 0.5)
                          * saturate((r - (outer + 0.02)) / px + 0.5)
                          * saturate((pTop - r) / px + 0.5);
                float beam = saturate(1.2 - abs(dT) / px) * saturate(0.5 - dR / px);
                I += (tri * 1.3 + beam * 0.9) * showTip;

                // ================= Outer hairline + tick ring =================
                float rO1 = outer + 0.045;
                float tickN = 72.0;
                float tf = frac(na * tickN);
                float tdm = min(tf, 1.0 - tf) / tickN * TWO_PI * r;
                float tIdx = round(na * tickN);
                float isMaj = step(fmod(tIdx, 6.0), 0.5);
                float tLen = lerp(0.012, 0.028, isMaj);
                float tk = saturate(1.2 - tdm / px)
                         * saturate((r - rO1) / px + 0.5)
                         * saturate((tLen - (r - rO1)) / px + 0.5);
                float tLit = lerp(saturate((progress - na) * TWO_PI * r / px + 0.5) * hasProg, 1.0, hs);
                I += saturate(1.2 - abs(r - rO1) / px) * lerp(0.25, 0.6, tLit);
                I += tk * lerp(0.28, 1.0, tLit) * lerp(0.8, 1.0, isMaj);

                // ================= Cardinal corner brackets =================
                float rO2 = outer + 0.09;
                float dq = (frac(na * 4.0 + 0.5) - 0.5) / 4.0 * TWO_PI * r;
                float brA = saturate(1.3 - abs(r - rO2) / px) * saturate((0.07 - abs(dq)) / px + 0.5);
                float brC = saturate(1.3 - abs(abs(dq) - 0.07) / px)
                          * saturate((r - (rO2 - 0.02)) / px + 0.5)
                          * saturate((rO2 - r) / px + 0.5);
                I += (brA + brC) * 0.65;

                // ================= Inner counter-rotating dashed rings =================
                float rD1 = inner * 0.82;
                float fd = frac(na * 24.0 + t * 0.3 * sp);
                float dash1 = smoothstep(0.0, 0.04, fd) * (1.0 - smoothstep(0.5, 0.54, fd));
                I += saturate(1.2 - abs(r - rD1) / px) * dash1 * 0.6;

                float rD2 = inner * 0.62;
                float fd2 = frac(na * 40.0 - t * 0.45 * sp);
                float dash2 = smoothstep(0.0, 0.06, fd2) * (1.0 - smoothstep(0.3, 0.36, fd2));
                I += saturate(1.2 - abs(r - rD2) / px) * dash2 * 0.4;

                // ================= Radar sweep =================
                float w = frac(ang / TWO_PI + t * 0.4 * sp);
                float sweep = exp(-w * 5.0);
                float sweepDisc = saturate((rD2 * 0.94 - r) / px + 0.5);
                I += sweep * sweepDisc * (0.4 + 0.6 * r / max(rD2, 1e-3)) * 0.55 * (0.35 + 0.65 * max(progress, hs));

                // ================= Core: crosshair, ring, pulsing dot =================
                float coreR = inner * 0.45;
                float chX = saturate(1.2 - abs(pr.y) / px) * saturate((coreR * 1.5 - abs(pr.x)) / px + 0.5);
                float chY = saturate(1.2 - abs(pr.x) / px) * saturate((coreR * 1.5 - abs(pr.y)) / px + 0.5);
                I += (chX + chY) * 0.35;
                I += saturate(1.2 - abs(r - coreR) / px) * 0.5;
                float pulse = 1.0 + 0.2 * sin(t * lerp(3.0, 14.0, hs));
                I += saturate((coreR * 0.28 * pulse - r) / px + 0.5) * lerp(0.4 + 0.6 * progress, 1.4, hs);

                // ================= Overcharge ping ring =================
                float ph = frac(t * 0.9);
                float pR = lerp(coreR, rO2, ph);
                I += saturate(1.3 - abs(r - pR) / px) * (1.0 - ph) * hs * 0.8;

                return I;
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float px = max(fwidth(p.x), 1e-4);      // pixel size in gauge space (assumes a square Image)

                float t = _Time.y;
                float progress = saturate(_Power * 0.01);
                float hs = saturate(_HighSpeedTransition);
                float gl = saturate(_HeatShimmer);

                float r = length(p);
                float inner = _EventHorizon;
                float outer = max(_RingOuter, inner + 0.08);
                float rc = 0.5 * (inner + outer);
                float hw = 0.5 * (outer - inner);

                // ---------------- Horizontal slice glitch ----------------
                float band = floor((p.y + 1.0) * 9.0);
                float st = floor(t * 7.0);
                float g = hash12(float2(band, st));
                float gate = step(1.0 - (0.05 + 0.25 * hs) * gl, g);
                float2 pG = p;
                pG.x += (hash12(float2(band + 13.0, st)) - 0.5) * 0.10 * gate * (0.5 + hs) * gl;

                // ---------------- Chromatic split (pixel-sized, radial) ----------------
                float2 dirG = pG / max(length(pG), 1e-4);
                float cFr = px * (1.4 + 1.6 * hs + 6.0 * gate) * gl;

                float I0 = HoloField(pG, px);
                float Ip = HoloField(pG + dirG * cFr, px);
                float Im = HoloField(pG - dirG * cFr, px);

                float3 holoCol = lerp(_RimColor.rgb, float3(1.0, 0.35, 0.55), hs * 0.6);   // overcharge = warning hue
                float3 tint = lerp(holoCol, float3(1.0, 1.0, 1.0), 0.5);
                float3 holo = holoCol * I0 * 0.5 + float3(Im * tint.r, I0 * tint.g, Ip * tint.b) * 0.5;

                // ---------------- Scanlines, scan bar, flicker ----------------
                float scan = 0.84 + 0.16 * sin(p.y * 150.0 - t * 3.0);
                float barY = frac(t * 0.22) * 2.6 - 1.3;
                float bd = (barY - p.y) / 0.05;
                float scanBar = exp(-bd * bd) * 0.5;
                float flick = 1.0 - 0.15 * step(0.96, hash11(floor(t * 18.0))) * gl;
                holo *= scan * flick * (1.5 + 0.8 * hs);

                // ---------------- Glass panel + faint grid ----------------
                float panelR = min(outer + 0.14, 0.95);
                float panelMask = saturate((panelR - r) / px + 0.5);
                float radial = saturate(1.0 - r / panelR);

                float2 gf = frac(p * 10.0);
                float2 gd = min(gf, 1.0 - gf) / 10.0;
                float gridLine = saturate(1.2 - min(gd.x, gd.y) / px);
                float panelEdge = saturate(1.2 - abs(r - panelR) / px);

                float panelA = panelMask * (0.35 + 0.25 * radial);
                float3 panelRGB = _Color.rgb * (0.6 + 0.6 * radial) * panelA;
                float3 gridRGB = holoCol * (gridLine * 0.07 * (0.4 + 0.6 * radial) * panelMask + panelEdge * 0.25);
                float3 barRGB = holoCol * scanBar * 0.25 * panelMask;

                // ---------------- Soft halo around the ring ----------------
                float dB = max(abs(r - rc) - hw, 0.0);
                float3 halo = holoCol * exp(-dB * 22.0) * 0.10 * (0.3 + 0.7 * max(progress, hs))
                            * (1.0 - smoothstep(panelR - 0.05, panelR, r));

                float3 rgb = panelRGB + gridRGB + barRGB + holo + halo;
                float lum = max3(holo + halo + gridRGB + barRGB);
                float alpha = saturate(panelA + lum * 0.8);

                float edgeFade = 1.0 - smoothstep(0.97, 1.0, r);
                float4 col = float4(rgb * edgeFade, alpha * edgeFade);

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
