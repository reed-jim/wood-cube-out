// UI/CarButton  -  Unity URP, uGUI Image, no texture required.
// Procedural "automotive" push button: satin-metal bezel, dark recessed well,
// soft-touch plastic cap with beveled edge + slight dish, light-pipe LED slit,
// contact shadow / AO, and a pressed + hover state.
//
// Requires CarButtonUI.cs on the same GameObject (writes 0..1 UVs into the mesh,
// because an Image with no sprite has all-zero UVs) and drives _Pressed/_Hover/_LedOn.
Shader "UI/CarButton"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite (unused)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Shape  in canvas units)]
        _OuterRadius ("Outer Radius", Float) = 26
        _RimWidth ("Bezel Width", Float) = 7
        _CapInset ("Cap Inset From Edge", Float) = 17
        _CapRadius ("Cap Corner Radius", Float) = 13
        _CapBevel ("Cap Bevel Size", Float) = 9
        _CapDish ("Cap Dish (concavity)", Range(0,1)) = 0.22

        [Header(Materials)]
        _CapColor ("Cap Color", Color) = (0.105,0.11,0.12,1)
        _CapRoughness ("Cap Roughness", Range(0,1)) = 0.5
        _CapSpec ("Cap Specular", Range(0,1)) = 0.45
        _RimColor ("Bezel Color", Color) = (0.62,0.63,0.66,1)
        _RimMetal ("Bezel Metalness", Range(0,1)) = 0.85
        _RimRoughness ("Bezel Roughness", Range(0,1)) = 0.28
        _RimCurve ("Bezel Roundness", Range(0,3)) = 1.3
        _RecessColor ("Recess Color", Color) = (0.012,0.012,0.014,1)

        [Header(Lighting)]
        _LightDir ("Light Direction XY (toward light)", Vector) = (-0.45,0.75,0,0)
        _LightHeight ("Light Height", Range(0.2,2)) = 0.85
        _LightIntensity ("Light Intensity", Range(0,3)) = 1.5
        _Ambient ("Ambient", Range(0,1)) = 0.35
        _ShadowOffset ("Cap Shadow Offset", Float) = 8
        _ShadowSoftness ("Cap Shadow Softness", Float) = 6
        _ShadowStrength ("Cap Shadow Strength", Range(0,1)) = 0.85
        _Grain ("Plastic Grain", Range(0,0.1)) = 0.03

        [Header(LED)]
        [HDR] _LedColor ("LED Color", Color) = (1,0.5,0.08,1)
        _LedOn ("LED On", Range(0,1)) = 1
        _LedPos ("LED Pos (fraction of cap half-size)", Vector) = (0,-0.58,0,0)
        _LedSize ("LED Slit Size", Vector) = (34,4,0,0)
        _LedGlow ("LED Glow Radius", Float) = 14

        [Header(State  driven by CarButtonUI)]
        _Pressed ("Pressed", Range(-0.2,1)) = 0
        _Hover ("Hover", Range(0,1)) = 0

        // --- uGUI plumbing ---
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
            Name "Default"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float2 objPos     : TEXCOORD1;
            };

            float4 _ClipRect;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float  _OuterRadius, _RimWidth, _CapInset, _CapRadius, _CapBevel, _CapDish;
                float4 _CapColor;
                float  _CapRoughness, _CapSpec;
                float4 _RimColor;
                float  _RimMetal, _RimRoughness, _RimCurve;
                float4 _RecessColor;
                float4 _LightDir;
                float  _LightHeight, _LightIntensity, _Ambient;
                float  _ShadowOffset, _ShadowSoftness, _ShadowStrength, _Grain;
                float4 _LedColor;
                float  _LedOn;
                float4 _LedPos, _LedSize;
                float  _LedGlow;
                float  _Pressed, _Hover;
            CBUFFER_END

            // ------------------------------------------------------------------
            // SDF helpers
            // ------------------------------------------------------------------
            float sdRR(float2 p, float2 h, float r)
            {
                r = min(r, min(h.x, h.y));
                float2 q = abs(p) - h + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            // analytic outward gradient of sdRR
            float2 gradRR(float2 p, float2 h, float r)
            {
                r = min(r, min(h.x, h.y));
                float2 q = abs(p) - h + r;
                float2 s = float2(p.x < 0 ? -1.0 : 1.0, p.y < 0 ? -1.0 : 1.0);
                float2 g;
                if (q.x > 0 && q.y > 0) g = normalize(max(q, 1e-5));
                else                    g = (q.x > q.y) ? float2(1, 0) : float2(0, 1);
                return g * s;
            }

            float hash21(float2 x)
            {
                return frac(sin(dot(x, float2(12.9898, 78.233))) * 43758.5453);
            }

            // ------------------------------------------------------------------
            // Fake studio environment: soft top gradient, key softbox, weak fill
            // r = reflection vector, +Z toward viewer, +Y up on screen
            // ------------------------------------------------------------------
            float3 Env(float3 r, float rough, float3 L)
            {
                float up = r.y * 0.5 + 0.5;
                float3 grad = lerp(float3(0.025, 0.028, 0.035), float3(0.42, 0.44, 0.48), smoothstep(0.05, 0.95, up));
                float band = exp(-pow((r.y - 0.02) * 3.5, 2.0)) * 0.12;

                float sharp = lerp(260.0, 5.0, rough);
                float key = pow(saturate(dot(r, L)), sharp) * lerp(3.2, 0.9, rough);

                float3 Lf = normalize(float3(-L.x * 0.9, -L.y * 0.6, 0.55));
                float fill = pow(saturate(dot(r, Lf)), sharp * 0.5) * lerp(0.6, 0.25, rough);

                return grad + band + key * float3(1.0, 0.985, 0.95) + fill * float3(0.75, 0.85, 1.0);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.vertex.xyz);
                o.color = v.color;
                o.uv = v.uv;
                o.objPos = v.vertex.xy;
                return o;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // ---- rect size + local pixel position (works rotated / batched) ----
                float2 gUx = float2(ddx(IN.uv.x), ddy(IN.uv.x));
                float2 gUy = float2(ddx(IN.uv.y), ddy(IN.uv.y));
                float2 gPx = float2(ddx(IN.objPos.x), ddy(IN.objPos.x));
                float2 gPy = float2(ddx(IN.objPos.y), ddy(IN.objPos.y));
                float2 size = float2(length(gPx) / max(length(gUx), 1e-8),
                                     length(gPy) / max(length(gUy), 1e-8));
                float2 hSize = size * 0.5;
                float2 p = (IN.uv - 0.5) * size;
                float pxs = max(0.5 * (length(gPx) + length(gPy)), 1e-5); // units per screen pixel

                float pressed = clamp(_Pressed, -0.2, 1.0);
                float hover = saturate(_Hover);

                // ---- lighting setup ----
                float2 L2 = normalize(_LightDir.xy + 1e-5);
                float3 L3 = normalize(float3(_LightDir.xy, _LightHeight));

                // ---- outer silhouette ----
                float dOuter = sdRR(p, hSize, _OuterRadius);
                float shape = saturate(0.5 - dOuter / pxs);
                float depth = -dOuter;

                // ---- cap geometry (shrinks slightly when pushed, like perspective) ----
                float sc = 1.0 - 0.035 * pressed;
                float2 capH = max(hSize - _CapInset, 1.0);
                float2 pc = p / sc;
                float dCapL = sdRR(pc, capH, _CapRadius);   // cap-local units
                float dCap = dCapL * sc;                    // screen units

                // ================= RECESS (dark well) =================
                float2 shOff = -L2 * _ShadowOffset * (1.0 - 0.7 * pressed);
                float dSh = sdRR((p - shOff) / sc, capH, _CapRadius) * sc;
                float soft = max(_ShadowSoftness * (1.0 - 0.4 * pressed), 0.5);
                float shadow = (1.0 - smoothstep(-soft, soft, dSh)) * _ShadowStrength * lerp(1.0, 0.65, saturate(pressed));
                float aoCap = exp(-max(dCap, 0.0) / (_ShadowSoftness * 0.8 + 1.0));
                float aoWall = exp(-max(depth - _RimWidth, 0.0) / 6.0);

                float3 recess = _RecessColor.rgb * (1.0 - shadow) * (1.0 - 0.6 * aoCap) * (1.0 - 0.5 * aoWall);
                // faint light catching the floor on the side facing the light
                recess += _RecessColor.rgb * 1.5 * saturate(dot(L2, -gradRR(p, hSize, _OuterRadius))) * 0.3 * (1.0 - shadow);

                // ================= BEZEL RING (satin metal) =================
                float t = saturate(depth / max(_RimWidth, 0.5));
                float2 gO = gradRR(p, hSize, _OuterRadius);
                float slopeR = (1.0 - 2.0 * t) * _RimCurve;
                float3 nR = normalize(float3(gO * slopeR, 1.0));
                float3 rR = float3(2.0 * nR.z * nR.x, 2.0 * nR.z * nR.y, 2.0 * nR.z * nR.z - 1.0);
                float3 envR = Env(rR, _RimRoughness, L3);
                float diffR = saturate(dot(nR, L3) * 0.5 + 0.5);
                float3 rimAlb = _RimColor.rgb;
                float3 rimCol = rimAlb * (_Ambient + diffR * diffR * _LightIntensity) * (1.0 - _RimMetal) * 0.8
                              + envR * rimAlb * _RimMetal;
                // subtle brushed look: faint radial streaking along the long edge
                rimCol *= 1.0 + (hash21(floor(p * 0.5)) - 0.5) * 0.04;
                // dark seam where bezel meets recess + soft ambient occlusion at outer rim
                rimCol *= lerp(0.55, 1.0, smoothstep(0.0, 0.18, 1.0 - t));
                rimCol *= lerp(0.8, 1.0, smoothstep(0.0, 0.1, t));

                // ================= CAP (soft-touch plastic) =================
                float dIn = max(-dCapL, 0.0);
                float b = max(_CapBevel, 0.5);
                float tb = saturate(dIn / b);
                float cc = 1.0 - tb;
                float slope = min(cc / sqrt(max(1.0 - cc * cc, 0.04)), 4.0);
                float2 gC = gradRR(pc, capH, _CapRadius);
                float2 nxy = gC * slope;
                float2 dish = -(pc / capH) * _CapDish * 0.8 * smoothstep(0.2, 1.0, tb);
                float3 nC = normalize(float3(nxy + dish, 1.0));

                float3 albedo = _CapColor.rgb * IN.color.rgb * _Color.rgb;
                float grain = (hash21(floor(p / pxs)) - 0.5) * _Grain;
                albedo *= 1.0 + grain;
                albedo *= lerp(1.0, 0.9, saturate(pressed));

                float diffC = saturate(dot(nC, L3) * 0.5 + 0.5);
                diffC *= diffC;
                float ambC = lerp(0.35, 1.0, nC.y * 0.5 + 0.5) * _Ambient;
                float3 capCol = albedo * (ambC + diffC * _LightIntensity);

                float3 rC = float3(2.0 * nC.z * nC.x, 2.0 * nC.z * nC.y, 2.0 * nC.z * nC.z - 1.0);
                float3 envC = Env(rC, _CapRoughness, L3);
                float fres = pow(1.0 - saturate(nC.z), 5.0);
                float specAmt = (_CapSpec * 0.3 + fres * 0.5) * lerp(1.0, 0.55, saturate(pressed));
                capCol += envC * specAmt;

                // cap sinks into the well -> walls occlude its edges
                capCol *= 1.0 - saturate(pressed) * 0.35 * (1.0 - tb);
                // hover: tiny lift in brightness
                capCol += albedo * hover * 0.35;

                // ---- LED light-pipe slit ----
                float2 ledC = capH * _LedPos.xy;
                float2 ledH = max(_LedSize.xy * 0.5, 0.25);
                float dLed = sdRR(pc - ledC, ledH, min(ledH.x, ledH.y));
                float core = saturate(0.5 - dLed / pxs);
                float glow = exp(-max(dLed, 0.0) / max(_LedGlow, 0.5));
                float on = saturate(_LedOn);
                float3 slitOff = float3(0.012, 0.012, 0.015) + capCol * 0.15;
                float3 slitOn = _LedColor.rgb * (0.9 + 0.1 * grain * 10.0);
                capCol = lerp(capCol, lerp(slitOff, slitOn, on), core);
                capCol += _LedColor.rgb * on * glow * glow * 0.35 * albedo * 6.0; // light bleeding into plastic
                capCol += _LedColor.rgb * on * glow * 0.04;

                // ================= COMPOSE =================
                float rimMask = saturate((_RimWidth - depth) / pxs + 0.5);
                float capMask = saturate(0.5 - dCap / pxs);

                float3 col = recess;
                col = lerp(col, rimCol, rimMask);
                col = lerp(col, capCol, capMask);

                float alpha = shape * IN.color.a * _Color.a;

                #ifdef UNITY_UI_CLIP_RECT
                {
                    float2 inside = step(_ClipRect.xy, IN.objPos) * step(IN.objPos, _ClipRect.zw);
                    alpha *= inside.x * inside.y;
                }
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif

                return float4(col, alpha);
            }
            ENDHLSL
        }
    }
}
