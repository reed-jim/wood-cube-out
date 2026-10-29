// Custom/URP/ProgressBarSciFi - Unity 6.3 URP, UGUI Image (no sprite/texture)
// Units: bar rect height = 1 (aspect-independent). Chamfered HUD track, octagon nodes,
// segmented energy cells, scanlines, grid. Final milestone = shockwaves + spokes + pillar + arcs.
Shader "Custom/URP/ProgressBarSciFi"
{
    Properties
    {
        _Progress ("Progress", Range(0,1)) = 0.5
        [IntRange] _MilestoneCount ("Milestone Count", Range(2,16)) = 5
        [Toggle] _UseCustomMilestones ("Use Custom Percent Array", Float) = 0
        _NodeSnap ("Node Reach Window", Range(0.001,0.1)) = 0.02

        [Header(Layout)]
        _Aspect ("Aspect Override (0=auto)", Float) = 0
        _StartEndRadius ("Start/End Node Radius", Range(0.05,0.5)) = 0.38
        _MidRadius ("Mid Node Radius", Range(0.05,0.5)) = 0.25
        _BarHeight ("Bar Height", Range(0.02,0.8)) = 0.30
        _FillPadding ("Fill Inset", Range(0,0.1)) = 0.03
        _Fillet ("Track/Node Union", Range(0,0.2)) = 0.03
        _Chamfer ("Track Corner Cut", Range(0,0.3)) = 0.10
        _FillBevel ("Fill Edge Width", Range(0.01,0.3)) = 0.10

        [Header(Colors)]
        [HDR] _FillColor ("Fill Start", Color) = (0.0,0.9,1.4,1)
        [HDR] _FillColorB ("Fill End", Color) = (0.3,0.4,2.0,1)
        [HDR] _BgColor ("Groove", Color) = (0.015,0.03,0.05,1)
        [HDR] _MilestoneBgColor ("Node Socket", Color) = (0.04,0.08,0.12,1)
        [HDR] _PulseColor ("Highlight", Color) = (0.8,1.0,1.0,1)
        [HDR] _AccentColor ("Final Accent", Color) = (1.0,0.25,0.7,1)

        [Header(HUD Detail)]
        _Segments ("Cell Count Density", Range(5,120)) = 45
        _SegGap ("Cell Gap", Range(0.01,0.5)) = 0.14
        _GridScale ("Grid Scale", Range(1,30)) = 10
        _ScanStrength ("Scanlines", Range(0,0.5)) = 0.18
        _NodeRingWidth ("Ring Width", Range(0.005,0.12)) = 0.03
        _CoreSize ("Core Size", Range(0.1,0.8)) = 0.45

        [Header(Animation)]
        _AnimSpeed ("Speed", Range(0,4)) = 1
        _FlowStrength ("Energy Flow", Range(0,1)) = 0.5
        _SheenSpeed ("Flow Speed", Range(0,5)) = 1.5
        _SheenFrequency ("Flow Density", Range(1,60)) = 30
        _PulseWidth ("Leading Edge Glow", Range(0.01,0.6)) = 0.08
        _RippleRadius ("Node Ripple", Range(0,0.3)) = 0.14

        [Header(Glow)]
        _GlowRadius ("Glow Radius", Range(0.005,0.3)) = 0.07
        _GlowIntensity ("Glow Intensity", Range(0,3)) = 0.9

        [Header(Final Milestone)]
        _FinalPower ("Final Power", Range(0,8)) = 3
        _FinalReach ("Shockwave Reach", Range(0.2,3)) = 1.4
        _FinalSpokes ("Spokes", Range(2,24)) = 10

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"
               "PreviewType"="Plane" "CanUseSpriteAtlas"="True" "RenderPipeline"="UniversalPipeline" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Cull Off
        ColorMask [_ColorMask]

        Pass
        {
            Name "ProgressBarSciFi"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; float4 worldPos : TEXCOORD1; };

            CBUFFER_START(UnityPerMaterial)
                float _Progress, _MilestoneCount, _UseCustomMilestones, _NodeSnap;
                float _Aspect, _StartEndRadius, _MidRadius, _BarHeight, _FillPadding, _Fillet, _Chamfer, _FillBevel;
                float4 _FillColor, _FillColorB, _BgColor, _MilestoneBgColor, _PulseColor, _AccentColor;
                float _Segments, _SegGap, _GridScale, _ScanStrength, _NodeRingWidth, _CoreSize;
                float _AnimSpeed, _FlowStrength, _SheenSpeed, _SheenFrequency, _PulseWidth, _RippleRadius;
                float _GlowRadius, _GlowIntensity;
                float _FinalPower, _FinalReach, _FinalSpokes;
            CBUFFER_END

            // material.SetFloatArray("_MilestonePercents", arr); material.SetFloat("_UseCustomMilestones", 1)
            float _MilestonePercents[16];
            float4 _ClipRect;

            float PB_Cov(float d, float px) { return saturate(0.5 - d / px); }

            float PB_SMin(float a, float b, float k)
            {
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * k * 0.25;
            }

            // chamfered box (c = corner cut); with square half-size r and c = 0.9r it is an octagon
            float PB_Cham(float2 q, float2 hs, float c)
            {
                float2 a = abs(q) - hs;
                return max(max(a.x, a.y), (a.x + a.y + c) * 0.7071);
            }

            float PB_Hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float PB_VNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(PB_Hash(i), PB_Hash(i + float2(1, 0)), f.x),
                            lerp(PB_Hash(i + float2(0, 1)), PB_Hash(i + float2(1, 1)), f.x), f.y);
            }

            float PB_Percent(int i, int count)
            {
                return (_UseCustomMilestones > 0.5) ? saturate(_MilestonePercents[i]) : (float)i / (float)(count - 1);
            }

            float PB_Radius(int i, int count) { return (i == 0 || i == count - 1) ? _StartEndRadius : _MidRadius; }

            float PB_Map(float2 p, float ap, float halfBar, int count)
            {
                float dTrack = PB_Cham(p, float2(ap, halfBar), _Chamfer);
                float dN = 1e5;
                [loop] for (int i = 0; i < count; i++)
                {
                    float r = PB_Radius(i, count);
                    dN = min(dN, PB_Cham(p - float2(lerp(-ap, ap, PB_Percent(i, count)), 0.0), float2(r, r), r * 0.9));
                }
                return PB_SMin(dTrack, dN, max(_Fillet, 1e-4));
            }

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color;
                o.worldPos = input.positionOS;
                return o;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // ---------- metrics ----------
                float2 gx = float2(ddx(IN.uv.x), ddy(IN.uv.x));
                float2 gy = float2(ddx(IN.uv.y), ddy(IN.uv.y));
                float lenX = max(length(gx), 1e-6);
                float lenY = max(length(gy), 1e-6);
                float px = lenY;
                float aspect = (_Aspect > 0.0) ? _Aspect : (lenY / lenX);
                float2 p = (IN.uv - 0.5) * float2(aspect, 1.0);
                float W = aspect * 0.5;

                int count = (int)clamp(round(_MilestoneCount), 2.0, 16.0);
                float halfBar = _BarHeight * 0.5;
                float ap = max(W - _StartEndRadius, 1e-3);
                float prog = saturate(_Progress);
                float edgeX = lerp(-ap, ap, prog);
                float t = _Time.y * _AnimSpeed;

                float d = PB_Map(p, ap, halfBar, count);
                float shapeCov = PB_Cov(d, px);
                float tBg = max(-d, 0.0);

                // ---------- groove: grid, scanlines, wall glow ----------
                float3 bgCol = _BgColor.rgb * lerp(0.6, 1.0, saturate(tBg / 0.08));
                float2 gq = abs(frac(p * _GridScale) - 0.5);
                bgCol += _FillColor.rgb * smoothstep(0.46, 0.5, max(gq.x, gq.y)) * 0.05;
                bgCol += _FillColor.rgb * exp(-tBg / 0.012) * 0.22;
                bgCol *= 1.0 - _ScanStrength * (0.5 + 0.5 * sin(p.y * 160.0 - t * 2.0));

                // ---------- nodes ----------
                float dNode = 1e5, dReach = 1e5;
                float ringCov = 0.0, ringReach = 0.0, coreCov = 0.0, dotCov = 0.0;
                float bestPc = -1.0, bestX = 0.0, bestR = 0.0;
                float finR = 0.0, finX = 0.0, finRad = 0.1;
                float snap = max(_NodeSnap, 1e-4);

                [loop] for (int i = 0; i < count; i++)
                {
                    float pc = PB_Percent(i, count);
                    float r = PB_Radius(i, count);
                    float cx = lerp(-ap, ap, pc);
                    float2 q = p - float2(cx, 0.0);
                    float dc = PB_Cham(q, float2(r, r), r * 0.9);
                    dNode = min(dNode, dc);

                    float reach = smoothstep(pc - snap, pc, prog);
                    float eased = 1.0 - (1.0 - reach) * (1.0 - reach);
                    float rf = max(r - _FillPadding, 0.0) * eased;
                    if (reach > 0.001) dReach = min(dReach, PB_Cham(q, float2(rf, rf), rf * 0.9));

                    float rc = PB_Cov(abs(dc + _NodeRingWidth * 0.5) - _NodeRingWidth * 0.5, px);
                    ringCov = max(ringCov, rc);
                    ringReach = max(ringReach, rc * reach);

                    float cr = r * _CoreSize * eased * (0.85 + 0.15 * sin(t * 3.0 + i * 1.7));
                    coreCov = max(coreCov, PB_Cov(PB_Cham(q, float2(cr, cr), cr * 1.6), px) * saturate(reach * 8.0));
                    float dr = r * 0.2;
                    dotCov = max(dotCov, PB_Cov(PB_Cham(q, float2(dr, dr), dr * 1.6), px) * (1.0 - reach));

                    if (reach > 0.999 && pc >= bestPc) { bestPc = pc; bestX = cx; bestR = r; }
                    if (i == count - 1) { finR = reach; finX = cx; finRad = r; }
                }
                float nodeCov = PB_Cov(dNode, px);

                // ---------- fill (slanted tip, segmented cells) ----------
                float dCut = (p.x - edgeX + p.y * 0.5) * 0.894;
                dCut += (1.0 - smoothstep(0.0, 0.004, prog)) * 10.0;
                float dFill = min(max(d + _FillPadding, dCut), dReach);
                float fillCov = PB_Cov(dFill, px);
                float tF = max(-dFill, 0.0);
                float core = smoothstep(0.0, max(_FillBevel, 1e-3), tF);

                float3 baseF = lerp(_FillColor.rgb, _FillColorB.rgb, saturate(IN.uv.x));
                float u = frac((p.x + p.y * 0.5) * _Segments);
                float seg = smoothstep(0.0, _SegGap, u) * smoothstep(0.0, _SegGap, 1.0 - u);
                float3 fillCol = baseF * (0.45 + 0.6 * core) * lerp(0.5, 1.0, seg);

                float flow = pow(0.5 + 0.5 * sin(p.x * _SheenFrequency - p.y * 5.0 - t * _SheenSpeed * 3.0), 6.0);
                float nz = PB_VNoise(float2(p.x * 20.0 - t * 4.0, p.y * 7.0));
                fillCol *= 1.0 + (flow * 1.2 + (nz - 0.5)) * _FlowStrength;
                fillCol += _PulseColor.rgb * exp(-tF / 0.012) * 0.9;

                float glintC = lerp(-W - 0.5, W + 0.5, frac(t * _SheenSpeed * 0.12));
                float gu = (p.x + p.y * 0.7 - glintC) / 0.06;
                fillCol += _PulseColor.rgb * exp(-gu * gu) * 0.5;

                float tip = exp(-abs(edgeX - p.x - p.y * 0.5) / max(_PulseWidth, 1e-3));
                tip *= (0.8 + 0.2 * sin(t * 20.0)) * smoothstep(0.0, 0.01, prog);
                fillCol += _PulseColor.rgb * tip * 0.9;

                // ---------- FINAL MILESTONE: power effect ----------
                float fin = finR;
                float3 fxRGB = 0.0;
                if (fin > 0.001)
                {
                    float2 q = p - float2(finX, 0.0);
                    float lq = length(q);
                    float pw = _FinalPower * fin;
                    float3 hot = lerp(_PulseColor.rgb, _AccentColor.rgb, 0.5);

                    float sw = 0.0;
                    [unroll] for (int k = 0; k < 3; k++)
                    {
                        float ph = frac(t * 0.55 + k / 3.0);
                        float rg = (lq - (finRad + ph * _FinalReach)) / (0.015 + 0.05 * ph);
                        sw += exp(-rg * rg) * (1.0 - ph) * (1.0 - ph);
                    }
                    float ang = atan2(q.y, q.x);
                    float spokes = pow(abs(sin(ang * _FinalSpokes * 0.5 + t * 1.5)), 20.0) * exp(-lq / 0.45);
                    float pillar = exp(-q.x * q.x / 0.0025) * (1.0 - saturate(abs(q.y) * 1.2));
                    float streak = exp(-abs(q.y) / 0.025) * exp(-abs(q.x) / 0.9);
                    float flash = exp(-lq / 0.12) * (0.75 + 0.25 * sin(t * 25.0));
                    float y0 = (PB_VNoise(float2(p.x * 9.0, floor(t * 18.0))) - 0.5) * 0.4;
                    float arc = exp(-abs(p.y - y0) / 0.01) * exp(-abs(q.x) / 0.7);

                    fxRGB = hot * (sw * 1.2 + spokes * 0.8 + pillar * 0.9 + streak * 1.2 + flash * 1.5) * pw
                          + _PulseColor.rgb * arc * pw * 0.7;

                    // supercharge the whole fill
                    fillCol *= 1.0 + fin * 1.5 * (0.7 + 0.3 * sin(t * 12.0));
                    fillCol += hot * fin * 0.25 * seg;
                }

                // ---------- composite ----------
                float3 col = bgCol;
                col = lerp(col, _MilestoneBgColor.rgb, nodeCov);
                col = lerp(col, fillCol, fillCov);

                float3 ringUn = _MilestoneBgColor.rgb * 1.7 + 0.03;
                float3 ringRe = lerp(baseF, _PulseColor.rgb, 0.6);
                col = lerp(col, lerp(ringUn, ringRe, saturate(ringReach / max(ringCov, 1e-4))), ringCov);
                col = lerp(col, _PulseColor.rgb * (0.9 + 0.1 * sin(t * 3.0)), coreCov);
                col = lerp(col, ringUn * 0.8, dotCov);

                // current-node halo + sonar ripple
                float haloVal = 0.0;
                if (bestPc >= 0.0)
                {
                    float lq = length(p - float2(bestX, 0.0));
                    float soft = exp(-max(lq - bestR, 0.0) / 0.06) * (0.35 + 0.25 * (0.5 + 0.5 * sin(t * 3.0)));
                    float ph = frac(t * 0.45);
                    float rg = (lq - (bestR + ph * _RippleRadius)) / 0.02;
                    haloVal = soft + exp(-rg * rg) * (1.0 - ph) * (1.0 - ph) * 0.8;
                }
                float3 haloCol = lerp(_FillColor.rgb, _PulseColor.rgb, 0.5);
                col += haloCol * haloVal * 0.35 * shapeCov;
                col += fxRGB * shapeCov;

                // ---------- outer glow (premultiplied-style, HDR-safe) ----------
                float xMask = (1.0 - smoothstep(edgeX - 0.05, edgeX + 0.25, p.x)) * smoothstep(0.0, 0.01, prog);
                float glowFill = exp(-max(d, 0.0) / (max(_GlowRadius, 1e-3) * (1.0 + fin * 2.0))) * xMask * _GlowIntensity * (1.0 + fin * 2.0);
                float borderFade = smoothstep(0.0, 0.06, 0.5 - abs(p.y)) * smoothstep(0.0, 0.06, W - abs(p.x));
                float3 glowRGB = (baseF * glowFill * 0.5 + haloCol * haloVal * 0.2 + fxRGB) * borderFade;
                float glowA = saturate(max(glowRGB.r, max(glowRGB.g, glowRGB.b)));

                float aOut = shapeCov + (1.0 - shapeCov) * glowA;
                float3 rgbOut = (col * shapeCov + glowRGB * (1.0 - shapeCov)) / max(aOut, 1e-4);
                float4 res = float4(rgbOut, aOut);

                // ---------- UGUI tint / dither / clip ----------
                res.rgb *= IN.color.rgb;
                res.a *= IN.color.a;
                float ign = frac(52.9829189 * frac(dot(IN.positionCS.xy, float2(0.06711056, 0.00583715))));
                res.rgb += (ign - 0.5) / 255.0;

                #if defined(UNITY_UI_CLIP_RECT)
                float2 inside = step(_ClipRect.xy, IN.worldPos.xy) * step(IN.worldPos.xy, _ClipRect.zw);
                res.a *= inside.x * inside.y;
                #endif
                #if defined(UNITY_UI_ALPHACLIP)
                clip(res.a - 0.001);
                #endif

                return res;
            }
            ENDHLSL
        }
    }
}
