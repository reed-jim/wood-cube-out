Shader "Custom/URP/IdleRewardStainedGlassAAA"
{
    Properties
    {
        // Kept for UI Image compatibility (Image with no sprite => white texture)
        _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Crystal Voronoi Settings)]
        _CellSize ("Crystal Cells (across short side)", Range(5.0, 50.0)) = 18.0
        _RefractionStrength ("Refraction Distortion", Range(0.0, 0.05)) = 0.015
        _EdgeWidth ("Lead Line Width", Range(0.01, 0.1)) = 0.03
        _EdgeColor ("Lead Line Color", Color) = (0.05, 0.03, 0.02, 1.0)

        [Header(Glass Tint Sparkle)]
        _GlassTint ("Stained Glass Tint", Color) = (0.9, 0.7, 0.2, 0.6)
        _Speed ("Crystal Shimmer Speed", Float) = 0.5

        [Header(Glass Quality)]
        _FacetTilt ("Facet Tilt (per-shard refraction)", Range(0.0, 1.0)) = 0.5
        _Dome ("Shard Dome Curvature", Range(0.0, 1.5)) = 0.6
        _Dispersion ("Chromatic Dispersion", Range(0.0, 1.0)) = 0.35
        _Specular ("Specular Intensity", Range(0.0, 3.0)) = 1.0
        _Gloss ("Specular Sharpness", Range(8.0, 256.0)) = 64.0
        _RimGlow ("Fresnel Rim Glow", Range(0.0, 2.0)) = 0.35
        _Grain ("Surface Grain", Range(0.0, 0.15)) = 0.03

        [Header(Frame)]
        _FrameWidth ("Lead Frame Width", Range(0.0, 0.1)) = 0.03

        [Header(Rounded Corners)]
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.1
        _CornerSmoothness ("Corner Smoothness", Range(0.001, 0.05)) = 0.005

        [Header(Two Stage Transition)]
        _Transition ("Transition Progress", Range(0, 1)) = 0.0
        [HDR] _InterimColor ("Interim Solid Color", Color) = (0.1, 0.08, 0.2, 1.0)
        [HDR] _SweepGlow ("Transition Glow", Color) = (1.0, 0.85, 0.5, 1.0)
        _SweepSoftness ("Sweep Edge Softness", Range(0.01, 0.3)) = 0.1

        // UI stencil / masking support
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
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
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
                float4 screenPos  : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // Requires "Opaque Texture" enabled on the URP asset and a Screen Space - Camera / World Space canvas.
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EdgeColor;
                float4 _GlassTint;
                float4 _InterimColor;
                float4 _SweepGlow;
                float _CellSize;
                float _RefractionStrength;
                float _EdgeWidth;
                float _Speed;
                float _FacetTilt;
                float _Dome;
                float _Dispersion;
                float _Specular;
                float _Gloss;
                float _RimGlow;
                float _Grain;
                float _FrameWidth;
                float _CornerRadius;
                float _CornerSmoothness;
                float _Transition;
                float _SweepSoftness;
            CBUFFER_END

            // ------------------------------------------------------------------
            // Hashing / noise (sin-free hashes: stable across GPUs and precision)
            // ------------------------------------------------------------------
            float Hash12(float2 p)
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

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash12(i);
                float b = Hash12(i + float2(1, 0));
                float c = Hash12(i + float2(0, 1));
                float d = Hash12(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float3 HueShift(float3 c, float a)
            {
                const float3 k = float3(0.57735027, 0.57735027, 0.57735027);
                float cs = cos(a);
                float sn = sin(a);
                return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
            }

            // ------------------------------------------------------------------
            // Animated Voronoi with EXACT distance to the cell border (iq method).
            // Gives perfectly even lead lines, unlike F2 - F1.
            // ------------------------------------------------------------------
            float2 CellPoint(float2 cell)
            {
                float2 h = Hash22(cell);
                return 0.5 + 0.45 * sin(_Time.y * _Speed + h * 6.2831853);
            }

            struct VoronoiResult
            {
                float edgeDist;   // exact distance to nearest border (cell units)
                float2 toCenter;  // pixel -> nearest feature point
                float2 cellId;    // integer id of nearest cell
            };

            VoronoiResult VoronoiExact(float2 x)
            {
                float2 n = floor(x);
                float2 f = frac(x);

                float2 mg = 0.0;
                float2 mr = 0.0;
                float md = 8.0;

                [unroll]
                for (int j = -1; j <= 1; j++)
                {
                    [unroll]
                    for (int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(i, j);
                        float2 r = g + CellPoint(n + g) - f;
                        float d = dot(r, r);
                        if (d < md)
                        {
                            md = d;
                            mr = r;
                            mg = g;
                        }
                    }
                }

                float me = 8.0;
                [unroll]
                for (int j2 = -1; j2 <= 1; j2++)
                {
                    [unroll]
                    for (int i2 = -1; i2 <= 1; i2++)
                    {
                        float2 g = mg + float2(i2, j2);
                        float2 r = g + CellPoint(n + g) - f;
                        float2 dr = r - mr;
                        if (dot(dr, dr) > 1e-5)
                            me = min(me, dot(0.5 * (mr + r), normalize(dr)));
                    }
                }

                VoronoiResult res;
                res.edgeDist = me;
                res.toCenter = mr;
                res.cellId = n + mg;
                return res;
            }

            // Rounded box SDF that also returns the outward gradient (for frame lighting)
            float RoundedBoxSDF(float2 p, float2 b, float r, out float2 grad)
            {
                float2 s = float2(p.x >= 0.0 ? 1.0 : -1.0, p.y >= 0.0 ? 1.0 : -1.0);
                float2 q = abs(p) - b + r;
                float2 qm = max(q, 0.0);
                float len = length(qm);
                float2 gAbs = len > 1e-5 ? qm / len : (q.x > q.y ? float2(1, 0) : float2(0, 1));
                grad = gAbs * s;
                return len + min(max(q.x, q.y), 0.0) - r;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float t = _Time.y;

                // ==============================================================
                // 1. Aspect-corrected rounded rectangle (units = short side)
                // ==============================================================
                float pixelWidth = length(ddx(uv));
                float pixelHeight = length(ddy(uv));
                float rectAspect = (pixelHeight > 0.0 && pixelWidth > 0.0) ? (pixelHeight / pixelWidth) : 1.0;

                float2 size = rectAspect > 1.0 ? float2(rectAspect, 1.0) : float2(1.0, 1.0 / rectAspect);
                float2 p = (uv - 0.5) * size;
                float2 halfSize = 0.5 * size;

                float2 boxGrad;
                float radius = min(_CornerRadius, min(halfSize.x, halfSize.y));
                float boxDist = RoundedBoxSDF(p, halfSize, radius, boxGrad);
                float aaBox = max(_CornerSmoothness, fwidth(boxDist));
                float cornerMask = 1.0 - smoothstep(0.0, aaBox, boxDist);

                // ==============================================================
                // 2. Crystal cells
                // ==============================================================
                float2 cuv = p * _CellSize;
                VoronoiResult v = VoronoiExact(cuv);

                float2 h2 = Hash22(v.cellId);
                float h3 = Hash12(v.cellId + 17.31);
                float cellID = h2.x;               // same role as original cellID

                float d = v.edgeDist;
                float aaE = fwidth(d) + 1e-4;
                float bevel = 1.0 - smoothstep(_EdgeWidth, 0.38, d);   // 1 near lead, 0 in the middle
                float centerGlow = saturate(1.0 - length(v.toCenter) * 1.5);

                // Per-shard pseudo normal: facet tilt + dome curvature (stronger at the bevel)
                float2 tilt = (h2 - 0.5) * 2.0 * _FacetTilt;
                float2 dome = -v.toCenter * _Dome * (0.4 + 0.6 * bevel);
                float3 n = normalize(float3(tilt * 0.6 + dome, 1.0));

                // Light that slowly orbits -> shards sparkle in turn
                float la = t * _Speed * 0.6;
                float3 L = normalize(float3(cos(la) * 0.55, sin(la * 0.8) * 0.45 + 0.35, 0.75));
                float3 V = float3(0, 0, 1);
                float3 Hv = normalize(L + V);

                // ---- Refraction with per-channel dispersion ----
                float fade = 1.0 - saturate(_Transition * 2.0);
                float2 off = n.xy * _RefractionStrength * fade;
                float2 offR = off * (1.0 + _Dispersion) + float2(0.0015, 0.0);
                float2 offB = off * (1.0 - _Dispersion) - float2(0.0015, 0.0);

                float bgR = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, saturate(screenUV + offR)).r;
                float bgG = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, saturate(screenUV + off)).g;
                float bgB = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, saturate(screenUV + offB)).b;
                float3 bg = float3(bgR, bgG, bgB);

                // ---- Per-shard colour: hue drift, brightness variance, thickness absorption ----
                float3 tint = HueShift(_GlassTint.rgb, (h2.y - 0.5) * 0.3) * lerp(1.0, 1.3, cellID);
                float thickness = lerp(0.85, 1.3, h3);
                float3 glass = bg * pow(max(tint, 1e-3), thickness);

                // Imperfections: slow value-noise streaks and bubbles-ish variance
                float imperfection = ValueNoise(cuv * 1.7 + h2 * 9.0) * 0.6
                                   + ValueNoise(float2(dot(cuv, float2(0.8, 0.6)) * 0.35, dot(cuv, float2(-0.6, 0.8)) * 3.5)) * 0.4;
                glass *= 1.0 + (imperfection - 0.5) * 0.22;

                // Subsurface-style inner glow (lit-from-within shards)
                glass += tint * centerGlow * 0.12;

                // Fresnel rim + procedural environment reflection
                float3 R = reflect(-V, n);
                float3 env = lerp(float3(0.08, 0.07, 0.12), float3(1.0, 0.92, 0.8), saturate(R.y * 0.5 + 0.5));
                float rim = pow(1.0 - saturate(n.z), 3.0) * _RimGlow;
                glass += env * rim * (0.4 + 0.6 * tint);

                // Blinn-Phong glints (per-shard sparkle variance)
                float spec = pow(saturate(dot(n, Hv)), _Gloss) * _Specular * (0.4 + 1.6 * h3);
                float3 specCol = spec * float3(1.0, 0.95, 0.85);
                glass += specCol;

                // Travelling light band
                float band = pow(saturate(sin(dot(p, float2(0.85, 0.55)) * 5.0 - t * _Speed * 2.0) * 0.5 + 0.5), 10.0);
                glass += tint * band * (0.08 + 0.22 * bevel) * _Specular;

                // Ambient occlusion next to lead, inner shadow near the frame
                glass *= lerp(0.7, 1.0, smoothstep(_EdgeWidth, _EdgeWidth * 5.0 + 0.08, d));
                float edgeDepth = -boxDist;
                glass *= lerp(0.78, 1.0, smoothstep(_FrameWidth, _FrameWidth + 0.12, edgeDepth));

                // Translucency: alpha lifts where highlights are, so glints stay visible
                float glassA = saturate(_GlassTint.a * lerp(1.0, 1.3, cellID)
                                        + max(specCol.r, max(specCol.g, specCol.b)) * 0.7
                                        + rim * 0.15 + band * 0.05);

                // ---- Lead came (cylindrical profile with warm metallic sheen) ----
                float leadMask = 1.0 - smoothstep(_EdgeWidth - aaE, _EdgeWidth + aaE, d);
                float lh = saturate(d / max(_EdgeWidth, 1e-4));
                float leadNoise = ValueNoise(cuv * 6.0);
                float3 leadCol = _EdgeColor.rgb * (0.75 + 0.5 * leadNoise) * lerp(0.55, 1.0, 1.0 - lh);
                float leadSheen = pow(1.0 - lh, 2.0) * (0.25 + 0.75 * band + 0.35 * saturate(dot(normalize(float2(cos(la), sin(la))), normalize(v.toCenter + 1e-4))));
                leadCol += leadSheen * float3(1.0, 0.9, 0.7) * _Specular * 0.35;

                float3 col = lerp(glass, leadCol, leadMask);
                float alpha = lerp(glassA, 1.0, leadMask);

                // ---- Outer lead frame with bevel lighting ----
                float frameW = max(_FrameWidth, 1e-4);
                float aaB = max(fwidth(boxDist), 1e-4);
                float frameMask = (1.0 - smoothstep(frameW - aaB, frameW + aaB, edgeDepth)) * step(1e-4, _FrameWidth);
                float fh = saturate(edgeDepth / frameW);
                float3 nf = normalize(float3(boxGrad * (1.0 - fh) * 1.4, 1.0));
                float fDiff = saturate(dot(nf, L));
                float fSpec = pow(saturate(dot(nf, Hv)), 40.0) * _Specular;
                float3 frameCol = _EdgeColor.rgb * (0.6 + 1.2 * fDiff)
                                + fSpec * float3(1.0, 0.92, 0.75) * 0.8
                                + pow(1.0 - fh, 4.0) * 0.04;
                col = lerp(col, frameCol, frameMask);
                alpha = lerp(alpha, 1.0, frameMask);

                // Fine surface grain (static, per pixel)
                col += (Hash12(floor(input.positionCS.xy)) - 0.5) * _Grain;

                float4 voronoiEffect = float4(col, alpha * input.color.a);

                // ==============================================================
                // 3. Two-stage transition (same logic as before, softened + glow)
                // ==============================================================
                float4 exactTintedImage = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                float4 interim = _InterimColor;
                interim.a *= input.color.a;

                float4 baseColor = voronoiEffect;
                float3 glow = 0.0;

                if (_Transition > 0.0)
                {
                    // Phase 1: shards dissolve into the solid interim colour
                    float phase1 = saturate(_Transition * 2.0);
                    float threshold = phase1 * 1.3;
                    float x = (threshold - cellID) / 0.15;
                    float dissolve = smoothstep(0.0, 1.0, x);
                    baseColor = lerp(voronoiEffect, interim, dissolve);

                    float front = dissolve * (1.0 - dissolve) * 4.0;      // peaks mid-dissolve
                    glow += _SweepGlow.rgb * front * 0.6 * (0.5 + 0.5 * h3);

                    // Phase 2: diagonal sweep to the exact image tint
                    if (_Transition > 0.5)
                    {
                        float phase2 = saturate((_Transition - 0.5) * 2.0);
                        float sweepCoord = uv.x + uv.y;
                        float sweepThreshold = lerp(-0.2, 2.2, phase2);

                        float sweepMask = smoothstep(sweepThreshold - _SweepSoftness, sweepThreshold, sweepCoord);
                        baseColor = lerp(exactTintedImage, baseColor, sweepMask);

                        // Hot glowing leading edge (zero at both ends of the phase)
                        float k = (sweepCoord - (sweepThreshold - _SweepSoftness * 0.5)) / max(_SweepSoftness * 0.7, 1e-3);
                        float sweepBand = exp(-k * k) * sin(phase2 * 3.14159265);
                        glow += _SweepGlow.rgb * sweepBand * 1.2;
                    }
                }

                return float4(baseColor.rgb + glow, baseColor.a * cornerMask);
            }
            ENDHLSL
        }
    }
}
