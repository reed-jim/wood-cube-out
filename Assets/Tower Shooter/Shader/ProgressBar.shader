// =============================================================================
//  Custom/URP/ProgressBarAAA
//  Unity 6.3 LTS - URP - for a UGUI Image (Sprite = None, no texture needed)
//
//  Layout units: everything is measured in "bar rect heights" (rect height = 1),
//  so circles stay circles at ANY Image aspect ratio / rotation / canvas scale.
//
//  Features
//   - Pixel-perfect SDF anti-aliasing (1 px, derived from screen derivatives)
//   - Smooth-union fillet between track and milestone nodes
//   - Recessed groove: bevel lighting, inner shadow, wall specular
//   - Liquid/energy fill: tube bevel shading, specular, fresnel rim, gloss band,
//     rounded leading tip, flowing fbm energy, travelling glint, tip hot-glow
//   - Milestone nodes: ring, iris-open fill, glowing core, sonar ripple on the
//     current node, outer neon glow that never clips at the rect border
//   - Gradient dithering (no banding), HDR colors (bloom friendly)
//   - Full UGUI support: vertex color tint, Mask stencil, RectMask2D, alpha clip
// =============================================================================
Shader "Custom/URP/ProgressBarAAA"
{
    Properties
    {
        _Progress ("Progress", Range(0.0, 1.0)) = 0.5
        [IntRange] _MilestoneCount ("Milestone Count", Range(2, 16)) = 5
        [Toggle] _UseCustomMilestones ("Use Custom Percent Array", Float) = 0
        _NodeSnap ("Node Reach Window (progress)", Range(0.001, 0.1)) = 0.02

        [Header(Layout  units are bar rect heights)]
        _Aspect ("Aspect Override (0 = auto)", Float) = 0
        _StartEndRadius ("Start/End Milestone Radius", Range(0.05, 0.5)) = 0.36
        _MidRadius ("Middle Milestone Radius", Range(0.05, 0.5)) = 0.24
        _BarHeight ("Bar Height", Range(0.02, 0.8)) = 0.30
        _FillPadding ("Fill Inset Padding", Range(0.0, 0.1)) = 0.025
        _Fillet ("Track/Node Smooth Union", Range(0.0, 0.3)) = 0.06
        _FillTipRound ("Fill Tip Roundness", Range(0.001, 0.4)) = 0.12

        [Header(Colors)]
        [HDR] _FillColor ("Fill Color (Start)", Color) = (0.0, 0.8, 1.0, 1.0)
        [HDR] _FillColorB ("Fill Color (End)", Color) = (0.45, 0.35, 1.0, 1.0)
        [HDR] _BgColor ("Groove Color", Color) = (0.10, 0.11, 0.14, 1.0)
        [HDR] _MilestoneBgColor ("Milestone Socket Color", Color) = (0.16, 0.17, 0.21, 1.0)
        [HDR] _PulseColor ("Highlight / Pulse Color", Color) = (1.0, 1.0, 1.0, 1.0)

        [Header(Surface Lighting)]
        _LightAngle ("Light Angle (deg, 90 = from top)", Range(0, 360)) = 125
        _GrooveBevel ("Groove Wall Width", Range(0.005, 0.3)) = 0.07
        _GrooveDepth ("Groove Wall Contrast", Range(0.0, 1.5)) = 0.8
        _ShadowLength ("Groove Inner Shadow Length", Range(0.01, 0.3)) = 0.08
        _ShadowStrength ("Groove Inner Shadow Strength", Range(0.0, 1.0)) = 0.55
        _FillBevel ("Fill Tube Bevel Width", Range(0.01, 0.4)) = 0.12
        _SpecPower ("Specular Power", Range(4, 256)) = 48
        _SpecIntensity ("Specular Intensity", Range(0.0, 3.0)) = 0.6
        _RimIntensity ("Fresnel Rim Intensity", Range(0.0, 2.0)) = 0.5
        _GlossIntensity ("Top Gloss Band", Range(0.0, 1.0)) = 0.18

        [Header(Milestone Nodes)]
        _NodeRingWidth ("Ring Width", Range(0.005, 0.12)) = 0.035
        _CoreSize ("Core Size (fraction of radius)", Range(0.1, 0.8)) = 0.42

        [Header(Animation)]
        _AnimSpeed ("Global Animation Speed", Range(0.0, 4.0)) = 1.0
        _FlowStrength ("Energy Flow Strength", Range(0.0, 1.0)) = 0.35
        _SheenSpeed ("Glint Speed", Range(0.0, 5.0)) = 1.5
        _SheenFrequency ("Flow Density", Range(1.0, 50.0)) = 20.0
        _PulseWidth ("Leading Edge Glow Width", Range(0.01, 0.6)) = 0.10
        _RippleRadius ("Node Ripple Reach", Range(0.0, 0.3)) = 0.14

        [Header(Outer Glow)]
        _GlowRadius ("Glow Radius", Range(0.005, 0.3)) = 0.07
        _GlowIntensity ("Glow Intensity", Range(0.0, 3.0)) = 0.8

        // ---- UGUI plumbing ----
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
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
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

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Cull Off
        ColorMask [_ColorMask]

        Pass
        {
            Name "ProgressBarUI"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            CBUFFER_START(UnityPerMaterial)
                float _Progress;
                float _MilestoneCount;
                float _UseCustomMilestones;
                float _NodeSnap;

                float _Aspect;
                float _StartEndRadius;
                float _MidRadius;
                float _BarHeight;
                float _FillPadding;
                float _Fillet;
                float _FillTipRound;

                float4 _FillColor;
                float4 _FillColorB;
                float4 _BgColor;
                float4 _MilestoneBgColor;
                float4 _PulseColor;

                float _LightAngle;
                float _GrooveBevel;
                float _GrooveDepth;
                float _ShadowLength;
                float _ShadowStrength;
                float _FillBevel;
                float _SpecPower;
                float _SpecIntensity;
                float _RimIntensity;
                float _GlossIntensity;

                float _NodeRingWidth;
                float _CoreSize;

                float _AnimSpeed;
                float _FlowStrength;
                float _SheenSpeed;
                float _SheenFrequency;
                float _PulseWidth;
                float _RippleRadius;

                float _GlowRadius;
                float _GlowIntensity;
            CBUFFER_END

            // Set from script: material.SetFloatArray("_MilestonePercents", arr)
            // and material.SetFloat("_UseCustomMilestones", 1)
            float _MilestonePercents[16];

            // Set by RectMask2D
            float4 _ClipRect;

            // -----------------------------------------------------------------
            // Helpers
            // -----------------------------------------------------------------
            float PB_Cov(float d, float px)
            {
                // 1-pixel wide analytic coverage from a signed distance
                return saturate(0.5 - d / px);
            }

            float PB_SMin(float a, float b, float k)
            {
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * k * 0.25;
            }

            // smooth max, wB = blend weight of b (used to blend gradients)
            float PB_SMax(float a, float b, float k, out float wB)
            {
                wB = saturate(0.5 + 0.5 * (b - a) / k);
                return lerp(a, b, wB) + k * wB * (1.0 - wB);
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
                float a = PB_Hash(i);
                float b = PB_Hash(i + float2(1, 0));
                float c = PB_Hash(i + float2(0, 1));
                float d = PB_Hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float PB_FBM(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                [unroll] for (int i = 0; i < 3; i++)
                {
                    v += a * PB_VNoise(p);
                    p = p * 2.03 + 17.1;
                    a *= 0.5;
                }
                return v;
            }

            float PB_Percent(int i, int count)
            {
                float even = (float)i / (float)(count - 1);
                return (_UseCustomMilestones > 0.5) ? saturate(_MilestonePercents[i]) : even;
            }

            float PB_Radius(int i, int count)
            {
                return (i == 0 || i == count - 1) ? _StartEndRadius : _MidRadius;
            }

            // Distance to the whole shape: capsule track smooth-unioned with nodes
            float PB_Map(float2 p, float ap, float halfBar, int count)
            {
                float dTrack = length(float2(p.x - clamp(p.x, -ap, ap), p.y)) - halfBar;
                float dN = 1e5;
                [loop] for (int i = 0; i < count; i++)
                {
                    float cx = lerp(-ap, ap, PB_Percent(i, count));
                    dN = min(dN, length(p - float2(cx, 0.0)) - PB_Radius(i, count));
                }
                return PB_SMin(dTrack, dN, max(_Fillet, 1e-4));
            }

            float PB_Clip(float2 pos, float4 rect)
            {
                float2 inside = step(rect.xy, pos) * step(pos, rect.zw);
                return inside.x * inside.y;
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

            // -----------------------------------------------------------------
            // Fragment
            // -----------------------------------------------------------------
            float4 frag(Varyings IN) : SV_Target
            {
                // ---------- pixel metrics (rotation / scale invariant) ----------
                float2 gx = float2(ddx(IN.uv.x), ddy(IN.uv.x));
                float2 gy = float2(ddx(IN.uv.y), ddy(IN.uv.y));
                float lenX = max(length(gx), 1e-6);
                float lenY = max(length(gy), 1e-6);
                float px = lenY;                                   // 1 pixel in height units
                float aspect = (_Aspect > 0.0) ? _Aspect : (lenY / lenX);

                float2 p = (IN.uv - 0.5) * float2(aspect, 1.0);
                float W = aspect * 0.5;

                int count = (int)clamp(round(_MilestoneCount), 2.0, 16.0);
                float halfBar = _BarHeight * 0.5;
                float ap = max(W - _StartEndRadius, 1e-3);
                float prog = saturate(_Progress);
                float edgeX = lerp(-ap, ap, prog);
                float t = _Time.y * _AnimSpeed;

                // ---------- light ----------
                float la = radians(_LightAngle);
                float2 L2 = float2(cos(la), sin(la));
                float3 L = normalize(float3(L2, 1.0));
                float3 H = normalize(L + float3(0, 0, 1));

                // ---------- base shape distance + gradient ----------
                float d = PB_Map(p, ap, halfBar, count);
                float2 g = float2(PB_Map(p + float2(px, 0), ap, halfBar, count) - d,
                                  PB_Map(p + float2(0, px), ap, halfBar, count) - d);
                g *= rsqrt(max(dot(g, g), 1e-8));                  // outward direction
                float shapeCov = PB_Cov(d, px);

                // ---------- groove (recessed) shading ----------
                float tBg = max(-d, 0.0);
                float sBg = saturate(1.0 - tBg / max(_GrooveBevel, 1e-3));
                float3 nBg = float3(-g * sBg, sqrt(max(1.0 - sBg * sBg, 1e-3)));
                float shadeBg = dot(nBg, L) / L.z;

                float sl = max(_ShadowLength, 1e-3);
                float dS = PB_Map(p + L2 * sl, ap, halfBar, count);
                float shadow = smoothstep(0.0, 1.0, saturate(dS / sl)) * _ShadowStrength;

                float3 bgCol = _BgColor.rgb * lerp(1.0, saturate(shadeBg), _GrooveDepth) * (1.0 - shadow);
                bgCol += pow(saturate(dot(nBg, H)), 24.0) * 0.12 * sBg;

                // ---------- milestone nodes (single pass) ----------
                float dNode = 1e5;
                float dReach = 1e5;
                float ringCov = 0.0, ringReach = 0.0, coreCov = 0.0, dotCov = 0.0;
                float bestPc = -1.0, bestX = 0.0, bestR = 0.0;
                float snap = max(_NodeSnap, 1e-4);

                [loop] for (int i = 0; i < count; i++)
                {
                    float pc = PB_Percent(i, count);
                    float r = PB_Radius(i, count);
                    float cx = lerp(-ap, ap, pc);
                    float lq = length(p - float2(cx, 0.0));
                    float dc = lq - r;
                    dNode = min(dNode, dc);

                    float reach = smoothstep(pc - snap, pc, prog);
                    float eased = 1.0 - (1.0 - reach) * (1.0 - reach);

                    if (reach > 0.001)
                        dReach = min(dReach, lq - max(r - _FillPadding, 0.0) * eased);

                    float rc = PB_Cov(abs(dc + _NodeRingWidth * 0.5) - _NodeRingWidth * 0.5, px);
                    ringCov = max(ringCov, rc);
                    ringReach = max(ringReach, rc * reach);

                    float breathN = 0.85 + 0.15 * sin(t * 3.0 + (float)i * 1.7);
                    coreCov = max(coreCov, PB_Cov(lq - r * _CoreSize * eased * breathN, px) * saturate(reach * 8.0));
                    dotCov = max(dotCov, PB_Cov(lq - r * 0.2, px) * (1.0 - reach));

                    if (reach > 0.999 && pc >= bestPc)
                    {
                        bestPc = pc; bestX = cx; bestR = r;
                    }
                }
                float nodeCov = PB_Cov(dNode, px);

                // ---------- fill shape (inset, rounded liquid tip, iris-open nodes) ----------
                float dInset = d + _FillPadding;
                float dCut = p.x - edgeX;
                dCut += (1.0 - smoothstep(0.0, 0.004, prog)) * 10.0;   // empty bar at progress 0
                float wB;
                float dFill = PB_SMax(dInset, dCut, max(_FillTipRound, 1e-3), wB);
                dFill = min(dFill, dReach);
                float fillCov = PB_Cov(dFill, px);

                // tube normal (convex)
                float2 gF = lerp(g, float2(1.0, 0.0), wB);
                gF *= rsqrt(max(dot(gF, gF), 1e-8));
                float tF = max(-dFill, 0.0);
                float sF = saturate(1.0 - tF / max(_FillBevel, 1e-3));
                float3 nF = float3(gF * sF, sqrt(max(1.0 - sF * sF, 1e-3)));
                float shadeF = dot(nF, L) / L.z;
                float core = smoothstep(0.0, max(_FillBevel * 1.2, 1e-3), tF);

                // ---------- fill color ----------
                float3 baseF = lerp(_FillColor.rgb, _FillColorB.rgb, saturate(IN.uv.x));
                float3 fillCol = baseF * (0.55 + 0.6 * saturate(shadeF)) * lerp(0.85, 1.25, core);

                // flowing energy
                float2 fp = p * (_SheenFrequency * 0.25);
                float n1 = PB_FBM(fp + float2(-t * 0.35, t * 0.10));
                float n2 = PB_FBM(fp * 1.9 + float2(t * 0.22, -t * 0.30) + 7.3);
                float energy = n1 * 0.6 + n2 * 0.4;
                fillCol *= 1.0 + (energy - 0.5) * _FlowStrength * 1.6;
                fillCol += _PulseColor.rgb * smoothstep(0.62, 0.90, energy) * _FlowStrength * 0.30;

                // travelling glint
                float phase = frac(t * _SheenSpeed * 0.12);
                float glintC = lerp(-W - 0.6, W + 0.6, phase);
                float gu = (p.x + p.y * 0.7 - glintC) / 0.10;
                fillCol += _PulseColor.rgb * exp(-gu * gu) * 0.35 * (0.4 + 0.6 * saturate(shadeF));

                // specular + fresnel rim
                fillCol += pow(saturate(dot(nF, H)), _SpecPower) * _SpecIntensity;
                fillCol += lerp(baseF, _PulseColor.rgb, 0.5) * pow(1.0 - nF.z, 2.5) * _RimIntensity;

                // top gloss band (bar only, hidden over nodes)
                float gl = (p.y - halfBar * 0.45) / max(halfBar * 0.18, 1e-3);
                fillCol += exp(-gl * gl) * _GlossIntensity * (1.0 - PB_Cov(dNode + 0.02, px)) * core;

                // leading edge hot glow
                float behind = edgeX - p.x;
                float tip = exp(-abs(behind) / max(_PulseWidth, 1e-3));
                tip *= (0.85 + 0.15 * sin(t * 6.0)) * (0.4 + 0.6 * core) * smoothstep(0.0, 0.01, prog);
                fillCol += _PulseColor.rgb * tip * 0.9;

                // ---------- composite ----------
                float3 col = bgCol;

                // unreached socket
                float3 socketCol = _MilestoneBgColor.rgb * (1.0 - shadow * 0.5);
                col = lerp(col, socketCol, nodeCov);

                // fill
                col = lerp(col, fillCol, fillCov);

                // ring
                float3 ringUn = _MilestoneBgColor.rgb * 1.7 + 0.03;
                float3 ringRe = lerp(baseF, _PulseColor.rgb, 0.6);
                float3 ringCol = lerp(ringUn, ringRe, saturate(ringReach / max(ringCov, 1e-4)));
                col = lerp(col, ringCol, ringCov);

                // core + dim idle dot
                float3 coreCol = _PulseColor.rgb * (0.9 + 0.1 * sin(t * 3.0));
                col = lerp(col, coreCol, coreCov);
                col = lerp(col, ringUn * 0.8, dotCov);

                // ---------- current-node halo + ripple ----------
                float haloVal = 0.0;
                if (bestPc >= 0.0)
                {
                    float lq = length(p - float2(bestX, 0.0));
                    float breath = 0.5 + 0.5 * sin(t * 3.0);
                    float soft = exp(-max(lq - bestR, 0.0) / 0.06) * (0.35 + 0.25 * breath);
                    float ph = frac(t * 0.45);
                    float rr = bestR + ph * _RippleRadius;
                    float rg = (lq - rr) / 0.02;
                    float ripple = exp(-rg * rg) * (1.0 - ph) * (1.0 - ph) * 0.8;
                    haloVal = soft + ripple;
                }
                float3 haloCol = lerp(_FillColor.rgb, _PulseColor.rgb, 0.5);
                col += haloCol * haloVal * 0.35 * shapeCov;

                // ---------- outer neon glow (alpha extends outside the shape) ----------
                float xMask = 1.0 - smoothstep(edgeX - 0.05, edgeX + 0.25, p.x);
                xMask *= smoothstep(0.0, 0.01, prog);
                float glowFill = exp(-max(d, 0.0) / max(_GlowRadius, 1e-3)) * xMask * _GlowIntensity;
                float borderFade = smoothstep(0.0, 0.06, 0.5 - abs(p.y)) * smoothstep(0.0, 0.06, W - abs(p.x));
                float glowSum = glowFill + haloVal;
                float3 glowCol = (baseF * glowFill + haloCol * haloVal) / max(glowSum, 1e-4);
                float glowA = saturate(glowFill * 0.5 + haloVal * 0.6) * borderFade;

                float aOut = shapeCov + (1.0 - shapeCov) * glowA;
                float3 rgbOut = (col * shapeCov + glowCol * glowA * (1.0 - shapeCov)) / max(aOut, 1e-4);

                float4 res = float4(rgbOut, aOut);

                // ---------- UGUI tint, dither, clipping ----------
                res.rgb *= IN.color.rgb;
                res.a *= IN.color.a;

                float ign = frac(52.9829189 * frac(dot(IN.positionCS.xy, float2(0.06711056, 0.00583715))));
                res.rgb += (ign - 0.5) / 255.0;

                #if defined(UNITY_UI_CLIP_RECT)
                res.a *= PB_Clip(IN.worldPos.xy, _ClipRect);
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
