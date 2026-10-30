// Unity 6.3 LTS - URP - UI shader for Image / RawImage
// Procedural sci-fi button, 2 states driven by _Locked (0 = normal, 1 = locked).
// Needs UVs that span 0..1 across the rect (see notes in chat).
Shader "UI/SciFiUpgradeButton"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused, leave white)", 2D) = "white" {}

        [Header(State)]
        _Locked ("Locked (0 normal, 1 locked)", Range(0, 1)) = 0

        [Header(Normal Colors)]
        _FillTop ("Fill Top", Color) = (0.05, 0.17, 0.23, 1)
        _FillBottom ("Fill Bottom", Color) = (0.02, 0.06, 0.10, 1)
        _EdgeColor ("Edge", Color) = (0.20, 0.90, 1.00, 1)
        _AccentColor ("Accent", Color) = (0.65, 1.00, 1.00, 1)

        [Header(Locked Colors)]
        _LockedFillTop ("Locked Fill Top", Color) = (0.12, 0.13, 0.16, 1)
        _LockedFillBottom ("Locked Fill Bottom", Color) = (0.06, 0.07, 0.09, 1)
        _LockedEdge ("Locked Edge", Color) = (0.34, 0.37, 0.45, 1)
        _LockedAccent ("Locked Accent", Color) = (0.60, 0.28, 0.22, 1)

        [Header(Shape in canvas units)]
        _Padding ("Padding, room for outer glow", Float) = 10
        _CutBig ("Chamfer Big Corners", Float) = 20
        _CutSmall ("Chamfer Small Corners", Float) = 6
        _BorderWidth ("Border Width", Float) = 2
        _GlowSize ("Glow Size", Float) = 6
        _GlowStrength ("Glow Strength", Range(0, 2)) = 0.8
        _AccentInset ("Accent Bar Inset", Float) = 12
        _AccentWidth ("Accent Bar Width", Float) = 3
        _AccentHeight ("Accent Bar Height", Float) = 36

        [Header(Detail)]
        _ScanPitch ("Scanline Pitch", Float) = 4
        _ScanStrength ("Scanline Strength", Range(0, 1)) = 0.25
        _HatchPitch ("Hatch Pitch, locked", Float) = 12
        _HatchStrength ("Hatch Strength, locked", Range(0, 1)) = 0.18

        [Header(Animation)]
        _PulseSpeed ("Pulse Speed", Float) = 2.5
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.25
        _ShimmerSpeed ("Shimmer Speed", Float) = 0.45
        _ShimmerStrength ("Shimmer Strength", Range(0, 1)) = 0.35

        // UI plumbing (Mask / RectMask2D / alpha clip)
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
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            Name "SciFiUIButton"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
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
                float2 canvasPos  : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Locked;
                float4 _FillTop, _FillBottom, _EdgeColor, _AccentColor;
                float4 _LockedFillTop, _LockedFillBottom, _LockedEdge, _LockedAccent;
                float _Padding, _CutBig, _CutSmall, _BorderWidth, _GlowSize, _GlowStrength;
                float _AccentInset, _AccentWidth, _AccentHeight;
                float _ScanPitch, _ScanStrength, _HatchPitch, _HatchStrength;
                float _PulseSpeed, _PulseAmount, _ShimmerSpeed, _ShimmerStrength;
            CBUFFER_END

            float4 _ClipRect; // set by RectMask2D

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.canvasPos = IN.positionOS.xy;
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            // magnitude of screen-space change of a value per pixel
            float Grad(float v)
            {
                return length(float2(ddx(v), ddy(v)));
            }

            float SdBox(float2 p, float2 center, float2 halfSize)
            {
                float2 q = abs(p - center) - halfSize;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // ---- rect size in canvas units, derived automatically (no script) ----
                float2 pxCanvas = float2(Grad(IN.canvasPos.x), Grad(IN.canvasPos.y)); // canvas units per pixel
                float2 pxUV = float2(Grad(IN.uv.x), Grad(IN.uv.y));                   // uv per pixel
                float2 size = clamp(pxCanvas / max(pxUV, 1e-7), 1.0, 8192.0);
                float px = max(pxCanvas.x, 1e-4);

                float2 p = (IN.uv - 0.5) * size;      // centered position in canvas units
                float2 h = size * 0.5 - _Padding;     // half size of the button body

                float locked = saturate(_Locked);
                float live = 1.0 - locked;
                float t = _Time.y;

                // ---- shape: box with chamfered corners (big on TR/BL, small on TL/BR) ----
                float2 q = abs(p) - h;
                float box = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
                float cut = (p.x * p.y > 0.0) ? _CutBig : _CutSmall;
                float chamfer = (abs(p.x) + abs(p.y) - (h.x + h.y - cut)) * 0.70710678;
                float d = max(box, chamfer);

                float cov = saturate(0.5 - d / px);
                float covIn = saturate(0.5 - (d + _BorderWidth) / px);
                float border = cov - covIn;

                // ---- state colors ----
                float3 fill = lerp(lerp(_FillBottom.rgb, _FillTop.rgb, IN.uv.y),
                                   lerp(_LockedFillBottom.rgb, _LockedFillTop.rgb, IN.uv.y), locked);
                float3 edgeCol = lerp(_EdgeColor.rgb, _LockedEdge.rgb, locked);
                float3 accCol = lerp(_AccentColor.rgb, _LockedAccent.rgb, locked);

                float pulse = 1.0 + _PulseAmount * sin(t * _PulseSpeed) * live;

                // ---- body ----
                float3 rgb = fill;

                // scanlines (slowly scrolling)
                float scan = 0.5 + 0.5 * sin((p.y - t * 6.0) * 6.2831853 / max(_ScanPitch, 0.5));
                rgb *= 1.0 - _ScanStrength * scan;

                // inner edge glow
                float inner = exp(min(d, 0.0) / max(_GlowSize, 0.01));
                rgb += edgeCol * inner * _GlowStrength * 0.35 * lerp(pulse, 0.25, locked);

                // locked: diagonal hatch
                float hs = abs(frac((p.x + p.y) / max(_HatchPitch, 1.0)) - 0.5) * 2.0;
                float aaS = px * 2.0 / max(_HatchPitch, 1.0);
                float stripe = 1.0 - smoothstep(0.35 - aaS, 0.35 + aaS, hs);
                rgb += accCol * stripe * _HatchStrength * locked;

                // normal: shimmer sweep with a pause between passes
                float sweep = frac(t * _ShimmerSpeed) * 2.4 - 0.7;
                float diagU = IN.uv.x + IN.uv.y * 0.35;
                float band = exp(-pow((diagU - sweep) * 8.0, 2.0));
                rgb += _AccentColor.rgb * band * _ShimmerStrength * live;

                // accent bar on the left + small tick near the top right
                float barD = SdBox(p, float2(-h.x + _AccentInset + _AccentWidth * 0.5, 0.0),
                                   float2(_AccentWidth * 0.5, _AccentHeight * 0.5));
                float barCov = saturate(0.5 - barD / px);
                rgb = lerp(rgb, accCol, barCov);

                float tickD = SdBox(p, float2(h.x - _CutBig - 40.0, h.y - 8.0), float2(24.0, 1.0));
                float tickCov = saturate(0.5 - tickD / px);
                rgb = lerp(rgb, accCol, tickCov * 0.7);

                // border
                float3 borderCol = edgeCol * lerp(1.3 * pulse, 0.8, locked);
                rgb = lerp(rgb, borderCol, border);

                // ---- outer glow (lives in the padding area) ----
                float outer = exp(-max(d, 0.0) / max(_GlowSize, 0.01));
                outer *= saturate(1.0 - max(d, 0.0) / max(_Padding, 0.01));
                float outerA = outer * (1.0 - cov) * _GlowStrength * 0.5 * lerp(pulse, 0.15, locked);

                // ---- composite ----
                float a = saturate(cov + outerA);
                float3 outRgb = (rgb * cov + edgeCol * outerA) / max(a, 1e-4);

                float4 col = float4(outRgb, a) * float4(IN.color.rgb, IN.color.a);
                // (rgb * vertex color, alpha * vertex alpha) keeps Button tint and CanvasGroup fade working
                col.rgb = outRgb * IN.color.rgb;
                col.a = a * IN.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                    float2 inside = step(_ClipRect.xy, IN.canvasPos) * step(IN.canvasPos, _ClipRect.zw);
                    col.a *= inside.x * inside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
