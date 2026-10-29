Shader "UI/Custom/HexTechReactorAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Background Track Color", Color) = (0.05, 0.08, 0.15, 1.0)
        _RimColor ("Active Charge / Plasma Color", Color) = (0.0, 0.85, 1.0, 1.0)

        [Header(Progress Control)]
        _Power ("Progress (0 - 100)", Range(0.0, 100.0)) = 25.0
        _RotationAngle ("Base Angle Offset (radians)", Float) = 0.0
        _HighSpeedTransition ("High-Speed / Overcharge Mode", Range(0.0, 1.0)) = 0.0

        [Header(Shape)]
        _Layers ("Hex Density (cells across diameter)", Range(6, 30)) = 14
        _SegmentGap ("Cell Gap Width", Range(0.0, 0.04)) = 0.012
        _Bevel ("Cell Bevel", Range(0.0, 1.0)) = 0.6
        _EventHorizon ("Core Hole Radius", Range(0.05, 0.4)) = 0.22
        _RingOuter ("Hex Field Outer Radius", Range(0.3, 0.75)) = 0.60

        [Header(Look)]
        _HeatShimmer ("Energy Shimmer", Range(0.0, 2.0)) = 1.0
        _Sparks ("Data Sparks", Range(0.0, 1.0)) = 1.0
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
        Blend One OneMinusSrcAlpha   // premultiplied alpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_HexTechReactorAAA"
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
                float _Bevel;
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

            // ---- Hex grid (pointy-top, cell width 1). Origin is a cell centre. ----
            float2 hmod(float2 a, float2 b) { return a - b * floor(a / b); }

            // returns xy = offset from cell centre, zw = cell centre
            float4 HexCoords(float2 uv)
            {
                float2 r = float2(1.0, 1.7320508);
                float2 h = r * 0.5;
                float2 a = hmod(uv, r) - h;
                float2 b = hmod(uv - h, r) - h;
                float2 gv = (dot(a, a) < dot(b, b)) ? a : b;
                return float4(gv, uv - gv);
            }

            // 0 at centre, 0.5 at the cell edge
            float HexDist(float2 p)
            {
                p = abs(p);
                return max(dot(p, float2(0.5, 0.8660254)), p.x);
            }

            // ------------------------------------------------------------
            // Main gauge
            // ------------------------------------------------------------
            float4 ReactorGauge(float2 p, float px)
            {
                float t = _Time.y;
                float progress = saturate(_Power * 0.01);
                float hs = saturate(_HighSpeedTransition);
                float hasProg = step(0.001, progress);
                float full = step(0.999, progress);

                float r = length(p);
                float2 pr = rot2(p, _RotationAngle);
                float ang = atan2(pr.y, pr.x);
                float na = 1.0 - frac((ang - PI * 0.5) / TWO_PI);   // 0..1 clockwise from top

                // Palette
                float3 baseCol = _RimColor.rgb;
                float3 activeCol = lerp(baseCol, float3(1.0, 0.90, 0.70), hs * 0.5);
                float3 hotCol = lerp(activeCol, float3(1.0, 0.97, 0.90), 0.6);
                float3 deepCol = pow(max(activeCol, 0.001), 1.7) * 1.25;
                float3 trackBase = _Color.rgb;
                float3 dimCol = lerp(trackBase, float3(0.5, 0.6, 0.8), 0.5) * 0.35;

                float3 L = normalize(float3(-0.45, 0.55, 0.70));
                float3 H = normalize(L + float3(0, 0, 1));

                // Radii
                float inner = _EventHorizon;
                float outer = max(_RingOuter, inner + 0.1);
                float coreR = max(inner - 0.04, 0.03);
                float rf = outer + 0.03;                       // frame ring radius
                float head = frac(t * 0.5);                    // overcharge sweep head (na units)

                // =========================================================
                // HEX FIELD
                // =========================================================
                float cellW = 2.0 * outer / max(_Layers, 6.0);
                float4 hx = HexCoords(p / cellW);
                float2 gv = hx.xy;
                float2 cp = hx.zw * cellW;                     // cell centre in gauge space
                float cr = length(cp);
                float cdist = HexDist(gv);                     // 0..0.5
                float dIn = (0.5 - cdist) * cellW;             // distance to the cell edge (inside)
                float exists = step(inner + 0.58 * cellW, cr) * step(cr, outer - 0.58 * cellW);
                float detail = saturate((cellW / px - 6.0) / 8.0);   // fade fine detail at tiny sizes

                // Per-cell clockwise position (so a cell is lit as a whole, sweep is stepped)
                float2 pc = rot2(cp, _RotationAngle);
                float cna = 1.0 - frac((atan2(pc.y, pc.x) - PI * 0.5) / TWO_PI);
                float rN = saturate((cr - inner) / (outer - inner));
                float cnaEff = cna * 0.94 + 0.06 * rN;          // slight spiral lag toward the rim
                float chash = hash12(hx.zw * 1.37 + 3.1);

                float age = progress - cnaEff;                  // > 0 : swept already
                float ign = saturate(age / 0.012) * hasProg;
                float lit = lerp(ign, 1.0, hs);

                // sweep front flash, cooldown trail, pre-charge shimmer
                float front = exp(-max(age, 0.0) * 22.0) * exp(-pow(min(age, 0.0) / 0.015, 2.0)) * hasProg * (1.0 - hs);
                float trailN = exp(-max(age, 0.0) * 2.5);
                float trailHS = exp(-frac(head - cnaEff) * 2.5);
                float trail = lerp(trailN, trailHS, hs);

                // overcharge: chain wave from centre to rim + random glitch dips
                float wavePh = frac(t * 0.7);
                float wave = exp(-pow((rN - wavePh * 1.3 + 0.15) / 0.18, 2.0)) * hs;
                float glitch = step(0.965, hash11(chash * 91.7 + floor(t * 10.0))) * hs;

                float gain = lerp(0.6, 1.25, trail) + front * 1.5 + wave * 1.3 + hs * 0.35;
                gain *= 1.0 - glitch * 0.7;
                gain *= 1.0 + 0.12 * sin(t * (4.0 + 20.0 * hs) * (0.5 + chash) + chash * 40.0) * _HeatShimmer * hs;

                // Bevel / 3D
                float bevW = cellW * 0.22;
                float hb = saturate(dIn / bevW);
                hb = hb * hb * (3.0 - 2.0 * hb);
                float2 gdir = gv / max(length(gv), 1e-4);
                float3 n = normalize(float3(gdir * (1.0 - hb) * _Bevel * 1.2, 0.6 + 0.4 * hb));
                float diff = saturate(dot(n, L));
                float specG = pow(saturate(dot(n, H)), 40.0);

                // --- dark glass state ---
                float3 glass = trackBase * (0.25 + 0.9 * diff)
                             + specG * 0.16 * float3(0.7, 0.85, 1.0)
                             + trackBase * saturate(dot(p, L.xy) * 0.5 + 0.5) * 0.12;
                float idle = (0.5 + 0.5 * sin(t * 1.3 + chash * 40.0)) * 0.05;
                glass += activeCol * idle * (1.0 - lit);
                // faint ghost outline of the cell, so the empty panel still reads as hex
                float ghostLine = saturate(1.0 - abs(dIn - (_SegmentGap * 0.5 + 0.003)) / (0.003 + px));
                glass += dimCol * ghostLine * 0.8;

                // --- lit state ---
                float centerGlow = pow(saturate(1.0 - cdist * 2.0), 1.6);
                float flow = fbm4(p * 9.0 + float2(0.0, -t * 1.6));
                float energy = lerp(0.75, 1.3, flow);
                energy = lerp(1.0, energy, saturate(_HeatShimmer));
                float3 interior = lerp(deepCol * 0.30, activeCol * 0.85, centerGlow) + hotCol * pow(centerGlow, 3.0) * 0.8;
                interior *= energy * gain;

                float bl = saturate(1.0 - abs(dIn - (_SegmentGap * 0.5 + 0.004)) / (0.004 + px));
                float3 borderRGB = lerp(activeCol, hotCol, 0.5) * bl * 1.3 * gain;

                // circuit detail: inner hex outline + one random trace with a data packet
                float ringL = saturate(1.0 - abs(cdist - 0.27) / (1.3 * (px / cellW)));
                float a6 = floor(chash * 6.0) * (PI / 3.0);
                float2 td = float2(cos(a6), sin(a6));
                float along = dot(gv, td);
                float perp = abs(dot(gv, float2(-td.y, td.x)));
                float traceL = step(0.0, along) * step(along, 0.5) * saturate(1.0 - perp / (1.2 * (px / cellW)));
                float packet = exp(-pow(along * 2.0 - frac(t * (0.5 + 0.8 * hs) + chash), 2.0) / 0.012) * traceL;
                float3 circuit = activeCol * (ringL * 0.5 + traceL * 0.7 + packet * 2.2) * detail * gain;

                float specL = pow(saturate(dot(n, H)), 30.0) * 0.25;
                float3 litRGB = (interior + borderRGB + circuit + specL) * lit;

                float3 cellRGB = glass * (1.0 - lit * 0.85) + litRGB;
                float body = saturate((dIn - _SegmentGap * 0.5) / px + 0.5) * exists;

                // glow bleeding out of lit cell edges (works inside the gaps too)
                float halo = exp(-dIn / max(cellW * 0.12, 1e-3));
                float3 haloRGB = activeCol * halo * lit * gain * 0.22 * exists * _Bloom;

                // background plate (frosted dark glass)
                float plateMask = saturate(0.5 - (r - (outer + 0.02)) / px);
                float3 plateCol = trackBase * (0.30 + 0.25 * saturate(1.0 - r)) ;
                float plateA = 0.62 * plateMask;
                float3 plateRGB = plateCol * plateA;

                float3 emis = plateRGB * (1.0 - body) + cellRGB * body;

                // =========================================================
                // FRAME RING (progress arc, segmented)
                // =========================================================
                float dF = abs(r - rf);
                float frameHW = 0.006;
                float frameBand = saturate(0.5 - (dF - frameHW) / px);
                float fcF = frac(na * 12.0);
                float dEF = min(fcF, 1.0 - fcF) / 12.0 * TWO_PI * rf;
                float segF = saturate((dEF - 0.006) / px + 0.5);
                float arcF = (progress - na) * TWO_PI * rf;
                float frameLit = lerp(saturate(arcF / px + 0.5) * hasProg, 1.0, hs);
                float behindP = saturate(progress - na);
                float trailF = lerp(exp(-behindP * 4.0), exp(-frac(head - na) * 3.0), hs);
                float tF = clamp((r - rf) / frameHW, -1.0, 1.0);
                float hF = sqrt(saturate(1.0 - tF * tF));
                float3 frameLitCol = lerp(deepCol, hotCol, pow(hF, 3.0) * 0.8) * (0.9 + 1.0 * trailF);
                float3 frameDimCol = dimCol * (0.6 + 0.8 * hF) + trackBase * 0.15;
                float3 frameRGB = lerp(frameDimCol, frameLitCol, frameLit) * frameBand * segF;

                // frame glow (circular only)
                float frameSoft = lerp(smoothstep(-0.08, 0.04, arcF) * hasProg, 1.0, hs);
                float dFo = max(dF - frameHW, 0.0);
                float frameGlowShape = exp(-dFo * 55.0) * 0.6 + exp(-dFo * 18.0) * 0.18;
                float3 frameGlow = activeCol * frameGlowShape * frameSoft * (0.35 + 0.65 * trailF) * (0.6 + 0.8 * hs) * _Bloom;
                frameGlow *= (1.0 - frameBand);

                // tip / orbiting head: round blobs ON the frame (cannot become a ray)
                float s1, c1;
                sincos(PI * 0.5 - progress * TWO_PI - _RotationAngle, s1, c1);
                float dT = length(p - float2(c1, s1) * rf);
                float tipBlob = exp(-dT * dT / 0.0006) * hasProg * (1.0 - full) * (1.0 - hs);
                float s2, c2;
                sincos(PI * 0.5 - head * TWO_PI - _RotationAngle, s2, c2);
                float dH = length(p - float2(c2, s2) * rf);
                float headBlob = exp(-dH * dH / 0.0006) * hs;
                float3 tipRGB = hotCol * (tipBlob + headBlob) * 2.4;

                // minor ticks
                float tIn = outer + 0.046;
                float tOut = outer + 0.060;
                float mi = floor(na * 60.0 + 0.5);
                float tBand = saturate(0.5 - (abs(r - 0.5 * (tIn + tOut)) - 0.5 * (tOut - tIn)) / px);
                float fm = frac(na * 60.0);
                float dm = min(fm, 1.0 - fm) / 60.0 * TWO_PI * r;
                float tMask = saturate((0.0030 - dm) / px + 0.5);
                float tickA = tBand * tMask;
                float tickLit = lerp(saturate((progress - mi / 60.0) * 60.0 + 0.5) * hasProg, 1.0, hs);
                float tickNear = exp(-abs(progress - mi / 60.0) * 40.0) * (1.0 - hs) * hasProg;
                float3 tickCol = lerp(dimCol, activeCol * (0.9 + 0.5 * trailF) + hotCol * tickNear * 0.8, tickLit);
                float3 ticksRGB = tickCol * tickA;

                // rotating corner brackets (arcs only)
                float rb = outer + 0.073;
                float rotB = t * 0.04 * (1.0 + 4.0 * hs);
                float dBk = abs(frac(na * 4.0 - rotB * 4.0 + 0.5) - 0.5) / 4.0;
                float brA = saturate(((0.045 - dBk) * TWO_PI * rb) / px + 0.5);
                float brR = saturate(0.5 - (abs(r - rb) - 0.007) / px);
                float bracketA = brA * brR;
                float3 bracketRGB = lerp(dimCol * 1.4, activeCol * 0.9, 0.3 + 0.7 * hs) * bracketA;

                // collar ring between core and field
                float collar = saturate(0.5 - (abs(r - (inner - 0.012)) - 0.004) / px);
                float3 collarRGB = lerp(dimCol, activeCol * 0.7, saturate(hasProg + hs)) * collar;

                // =========================================================
                // CORE
                // =========================================================
                float coreMask = saturate(0.5 - (r - coreR) / px);
                float cz = sqrt(saturate(1.0 - (r * r) / (coreR * coreR)));
                float fres = pow(1.0 - cz, 3.0);
                float2 pd = p + (float2(vnoise(p * 5.0 + float2(0.0, t * 2.5)), vnoise(p * 5.0 + float2(4.3, t * 2.2))) - 0.5)
                                * 0.02 * _HeatShimmer;
                float swirl = fbm4(rot2(pd / coreR, t * (1.5 + 3.0 * hs)) * 2.5 + t * 0.3);
                float coreI = lerp(0.25 + 0.75 * progress, 1.2, hs);
                float pulse = 1.0 + 0.25 * sin(t * lerp(3.0, 16.0, hs)) * _HeatShimmer;
                float3 nC = float3(p / coreR, cz);
                float glint = pow(saturate(dot(nC, H)), 60.0) * 0.5;
                float3 coreRGB = (activeCol * (0.35 + 1.4 * swirl) * (0.35 + 0.9 * pow(cz, 1.5))
                                + hotCol * pow(cz, 6.0) * 1.2
                                + deepCol * fres * 0.9) * coreI * pulse;
                // rotating hex outline inside the core
                float2 ph2 = rot2(p, t * 0.6 * (1.0 + 2.0 * hs));
                float hcd = HexDist(ph2 / (coreR * 1.25));
                float hexLine = saturate(1.0 - abs(hcd - 0.42) * coreR * 1.25 / (px * 1.5));
                coreRGB += hotCol * hexLine * coreI * 0.9;
                coreRGB = (coreRGB + glint) * coreMask;
                float3 coreGlow = activeCol * exp(-max(r - coreR, 0.0) * 22.0) * coreI * 0.45 * (1.0 - coreMask) * _Bloom;

                // =========================================================
                // OVERCHARGE SHOCKWAVE + DATA SPARKS
                // =========================================================
                float ph = frac(t * 0.9);
                float swR = lerp(coreR, rf, ph);
                float sw = exp(-pow((r - swR) / 0.022, 2.0)) * (1.0 - ph) * (1.0 - ph) * hs;
                float3 swRGB = activeCol * sw * 1.5;

                float3 sparkRGB = 0.0;
                if (_Sparks > 0.001)
                {
                    float SN = 56.0;
                    float cs = na * SN;
                    float id = floor(cs);
                    float h1 = hash11(id);
                    float h2 = hash11(id + 17.3);
                    float h3 = hash11(id + 41.7);
                    float sExists = step(0.4 - 0.25 * hs, hash11(id + 7.7));
                    float life = frac(t * (0.25 + 0.35 * h2) + h1);
                    float ca = (id + 0.2 + 0.6 * h3) / SN;
                    float rr = rf + life * (0.06 + 0.05 * h1);
                    float wob = sin(t * 3.0 + h1 * 20.0) * 0.010 * life;
                    float dx = (na - ca) * TWO_PI * rr + wob;
                    float dy = r - rr;
                    float sz = 0.0045 + 0.0045 * h2;
                    float s = exp(-(dx * dx + dy * dy) / (sz * sz));
                    float flick = 0.6 + 0.4 * sin(t * 20.0 * h3 + h1 * 6.0);
                    float fade = (1.0 - life) * smoothstep(0.0, 0.08, life);
                    float alive = lerp(step(ca, progress) * hasProg, 1.0, hs);
                    sparkRGB = hotCol * s * fade * flick * alive * sExists * 3.0 * _Sparks;
                }

                // =========================================================
                // COMPOSE
                // =========================================================
                float vig = 1.0 - smoothstep(0.86, 0.99, r);
                float3 soft = (frameGlow + haloRGB + tipRGB + swRGB + sparkRGB + coreGlow) * vig;

                emis += frameRGB + ticksRGB + bracketRGB + collarRGB + coreRGB + soft;

                float bodyA = max(max(plateA, body * 0.95), max(max(frameBand * segF, tickA) * 0.9, bracketA * 0.8));
                bodyA = max(bodyA, max(coreMask * 0.95, collar * 0.6));
                float alpha = saturate(bodyA + max3(soft) * 0.3);

                float edgeFade = 1.0 - smoothstep(0.97, 1.0, r);
                return float4(emis * edgeFade, alpha * edgeFade);
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float px = max(fwidth(p.x), 1e-4);   // pixel size in gauge space (assumes a square Image)

                float4 col = ReactorGauge(p, px);

                // Hot highlights bleed toward white
                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 5.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.75);

                // Exposure + filmic tonemap (set Tonemap to 0 and use HDR + Bloom if you want the post stack to do it)
                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                // Dither against banding
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
