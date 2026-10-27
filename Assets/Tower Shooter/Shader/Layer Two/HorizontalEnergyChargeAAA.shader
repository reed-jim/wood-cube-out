Shader "UI/Custom/HorizontalEnergyChargeAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Background Track Color", Color) = (0.05, 0.08, 0.15, 1.0)
        _RimColor ("Active Charge / Plasma Color", Color) = (0.0, 0.85, 1.0, 1.0)

        [Header(Progress Control)]
        _Power ("Progress (0 - 100)", Range(0.0, 100.0)) = 25.0
        _HighSpeedTransition ("High-Speed / Overcharge Mode", Range(0.0, 1.0)) = 0.0

        [Header(Shape)]
        _Layers ("Segment Count", Range(3, 50)) = 24
        _SegmentGap ("Segment Gap Width", Range(0.0, 0.03)) = 0.006
        _BarHeight ("Bar Thickness / Height", Range(0.05, 0.8)) = 0.35

        [Header(Look)]
        _HeatShimmer ("Plasma Shimmer & Distortion", Range(0.0, 2.0)) = 1.0
        _Sparks ("Embers / Sparks", Range(0.0, 1.0)) = 1.0
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_HorizontalEnergyChargeAAA"
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
                float _HighSpeedTransition;
                float _Layers;
                float _SegmentGap;
                float _BarHeight;
                float _HeatShimmer;
                float _Sparks;
                float _Exposure;
                float _Tonemap;
            CBUFFER_END

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

            // Helpers
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

            float4 EnergyGaugeHorizontal(float2 p, float px)
            {
                float t = _Time.y;
                float progress = saturate(_Power * 0.01);
                float hs = saturate(_HighSpeedTransition);
                float hasProg = step(0.001, progress);
                float full = step(0.999, progress);

                // Map p.x from [-1, 1] to normalized [0, 1] progress space along the bar length
                // Add a small horizontal margin for rounded caps/glows
                float barLengthAspect = 3.5; // Stretches coordinates so noise looks balanced on wide bars
                float2 bp = float2(p.x * barLengthAspect, p.y);
                
                float nx = p.x * 0.5 + 0.5; // 0 (left) to 1 (right)
                float ny = abs(p.y);        // distance from center line vertically

                // Distorted coordinates for plasma/shimmer
                float2 sh = float2(vnoise(bp * 3.0 + float2(t * 2.5, 0.0)),
                                   vnoise(bp * 3.0 + float2(0.0, t * 2.2))) - 0.5;
                float2 pd = bp + sh * 0.03 * _HeatShimmer * (0.5 + 0.5 * hs);

                // Palette
                float3 baseCol = _RimColor.rgb;
                float3 activeCol = lerp(baseCol, float3(1.0, 0.90, 0.70), hs * 0.5);
                float3 hotCol = lerp(activeCol, float3(1.0, 0.97, 0.90), 0.6);
                float3 trackBase = _Color.rgb;

                // Lighting direction
                float3 L = normalize(float3(-0.45, 0.55, 0.70));
                float3 H = normalize(L + float3(0, 0, 1));

                // ---------------- Bar Geometry ----------------
                float halfHeight = 0.5 * _BarHeight;
                float dBand = ny - halfHeight; // SDF of the vertical bounds
                float band = saturate(0.5 - dBand / px);

                // Fake 3D tube normal along height
                float tr = clamp(p.y / halfHeight, -1.0, 1.0);
                float h = sqrt(saturate(1.0 - tr * tr));
                float3 n = float3(0.0, tr * sign(p.y), h);

                // Segment gaps along the X axis
                float N = max(round(_Layers), 3.0);
                float cell = nx * N;
                float fc = frac(cell);
                float dEdge = min(fc, 1.0 - fc) / N * 2.0; // distance to nearest segment slice
                float seg = saturate((dEdge - _SegmentGap) / px + 0.5);

                // ---------------- Progress calculation ----------------
                float progressX = lerp(-1.0, 1.0, progress);
                float distToTip = p.x - progressX; // > 0 means ahead of progress, < 0 means filled side
                
                float fillEdge = saturate((-distToTip) / px + 0.5) * hasProg;
                float fillAmt = lerp(fillEdge, 1.0, hs);
                float softFill = lerp(smoothstep(0.08, -0.04, distToTip) * hasProg, 1.0, hs);

                // Comet / Energy trail falloff
                float behind = saturate(progress - nx);
                float trailNormal = exp(-behind * 4.0);
                float headPos = frac(t * 0.5);
                float trailHS = exp(-frac(headPos - nx) * 3.0);
                float trail = lerp(trailNormal, trailHS, hs);

                // ---------------- Plasma Energy ----------------
                float flowSpeed = 1.0 + 2.0 * hs;
                float flow1 = fbm4(pd * 3.5 + float2(-t * 2.0 * flowSpeed, 0.0));
                float flow2 = fbm4(pd * 6.0 - float2(t * 1.5, 0.0));
                float energy = lerp(0.65, 1.35, flow1) * lerp(0.85, 1.25, flow2);

                float plasmaWave = pow(0.5 + 0.5 * cos(12.0 * (pd.x - t * 2.0)), 3.0);
                energy *= lerp(1.0, 0.6 + 0.8 * plasmaWave, hs);

                float3 tubeCol = lerp(activeCol, hotCol, pow(h, 3.0) * 0.85);
                float fillGain = energy * lerp(0.55, 1.7, trail) * (1.0 + 1.2 * hs);
                float specF = pow(saturate(dot(n, H)), 30.0) * 0.35;
                float3 fillRGB = (tubeCol * fillGain + specF) * fillAmt * seg * band;

                // ---------------- Track (Groove) ----------------
                float diff = saturate(dot(n, L));
                float specT = pow(saturate(dot(n, H)), 40.0);
                float rim = pow(saturate(1.0 - h), 4.0);
                float3 trackRGB = trackBase * (0.30 + 0.65 * diff)
                                + specT * 0.18 * float3(0.7, 0.8, 1.0)
                                + rim * 0.35 * lerp(trackBase, float3(0.6, 0.8, 1.0), 0.6);
                trackRGB *= (1.0 - fillAmt * 0.8) * seg * band;

                // ---------------- Glow around bar ----------------
                float dOut = max(dBand, 0.0);
                float glowShape = exp(-dOut * 40.0) * 0.8 + exp(-dOut * 16.0) * 0.45;
                float3 glowRGB = activeCol * glowShape * softFill * (0.35 + 0.65 * trail) * (0.6 + 1.0 * hs);
                glowRGB *= (1.0 - band);

                // ---------------- Progress Tip Head ----------------
                float tipG = exp(-distToTip * distToTip / 0.004) * hasProg * (1.0 - full) * (1.0 - hs);
                float headDist = p.x - lerp(-1.0, 1.0, headPos);
                float headG = exp(-headDist * headDist / 0.004) * hs;
                float3 tipRGB = hotCol * (tipG + headG) * 2.6 * band;

                // ---------------- Embers / Sparks ----------------
                float3 sparkRGB = 0.0;
                if (_Sparks > 0.001)
                {
                    float SN = 40.0;
                    float c = nx * SN;
                    float id = floor(c);
                    float h1 = hash11(id);
                    float h2 = hash11(id + 17.3);
                    float h3 = hash11(id + 41.7);
                    float exists = step(0.4 - 0.25 * hs, hash11(id + 7.7));
                    float life = frac(t * (0.25 + 0.35 * h2) + h1);
                    float cx = (id + 0.5) / SN;
                    float targetX = lerp(-1.0, 1.0, cx);
                    float dy = p.y - (life - 0.5) * 0.3;
                    float sz = 0.008 + 0.006 * h2;
                    float s = exp(-((p.x - targetX) * (p.x - targetX) + dy * dy) / (sz * sz));
                    float flick = 0.6 + 0.4 * sin(t * 20.0 * h3);
                    float fade = (1.0 - life) * smoothstep(0.0, 0.08, life);
                    float alive = lerp(step(cx, progress) * hasProg, 1.0, hs);
                    sparkRGB = hotCol * s * fade * flick * alive * exists * 3.0 * _Sparks;
                }

                // ---------------- Compose ----------------
                float vig = 1.0 - smoothstep(0.85, 0.99, abs(p.x));
                float3 soft = (glowRGB + tipRGB + sparkRGB) * vig;
                float3 emis = trackRGB + fillRGB + soft;

                float bodyA = max(band * seg * 0.92, 0.0);
                float alpha = saturate(bodyA + max3(soft) * 0.3);

                float edgeFade = 1.0 - smoothstep(0.96, 1.0, abs(p.x));
                return float4(emis * edgeFade, alpha * edgeFade);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float px = max(fwidth(p.x), 1e-4);

                float4 col = EnergyGaugeHorizontal(p, px);

                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 5.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.75);

                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                float dn = hash12(input.positionCS.xy + frac(_Time.y) * 61.0) - 0.5;
                col.rgb += dn / 255.0 * max(col.a, max3(col.rgb));

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