Shader "Custom/URP_UI_StatUpgrade_Seamless"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        [HDR] _Color ("Core Flare Color", Color) = (1.0, 0.6, 0.1, 1.0)
        [HDR] _RimColor ("Energy Aura Color", Color) = (0.2, 0.8, 1.0, 1.0)
        [HDR] _QualityColor ("Stellar Highlight Color", Color) = (1.0, 0.95, 0.7, 1.0)

        _Progress ("Upgrade Progress", Range(0.0, 1.0)) = 0.0
        _Intensity ("Intensity", Range(0.0, 3.0)) = 1.6
        _SoftClip ("Soft Clip (1 = LDR canvas, 0 = HDR + Bloom)", Range(0.0, 1.0)) = 1.0
        _Seed ("Random Seed", Float) = 133.7

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
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIStatUpgrade_Seamless_Pass"
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
                float2 posOS      : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float4 _QualityColor;
                float _Progress;
                float _Intensity;
                float _SoftClip;
                float _Seed;
            CBUFFER_END

            float4 _ClipRect;

            #define PI  3.14159265359
            #define TAU 6.28318530718
            #define SPARK_COUNT 24

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color;
                o.posOS = input.positionOS.xy;
                return o;
            }

            struct FxCtx
            {
                float2 st;
                float  dist;
                float  angle;
                float  prog;
                float  t;
                float  aa;
                float  seed;
            };

            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float Hash21(float2 p)
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

            float VNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float FBM(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float2x2 m = float2x2(0.8, -0.6, 0.6, 0.8);
                [unroll] for (int i = 0; i < 4; i++)
                {
                    v += a * VNoise(p);
                    p = mul(m, p) * 2.02 + 1.7;
                    a *= 0.5;
                }
                return v;
            }

            float EaseOutCubic(float t) { t = saturate(t); return 1.0 - pow(1.0 - t, 3.0); }
            float EaseOutExpo(float t)  { t = saturate(t); return (1.0 - exp2(-10.0 * t)) / (1.0 - exp2(-10.0)); }

            float3 Heat(float h, float3 outer, float3 mid)
            {
                h = saturate(h);
                float3 c = lerp(outer * 0.35, outer, smoothstep(0.0, 0.35, h));
                c = lerp(c, mid, smoothstep(0.3, 0.7, h));
                c = lerp(c, lerp(mid, float3(1, 1, 1), 0.75) * 1.6, smoothstep(0.65, 1.0, h));
                return c;
            }

            float Sparks(FxCtx c, float seed, float minR, float maxR, float size, float streak)
            {
                float acc = 0.0;
                [unroll] for (int i = 0; i < SPARK_COUNT; i++)
                {
                    float fi = (float)i + seed * 17.0 + c.seed;
                    float2 h = Hash22(float2(fi, fi * 1.7 + 3.1));
                    float a = h.x * TAU;
                    float2 dir = float2(cos(a), sin(a));
                    float delay = h.y * 0.25;
                    float life = saturate((c.prog - delay) / (1.0 - delay));
                    float travel = EaseOutCubic(life);
                    float r = lerp(minR, maxR * (0.55 + 0.45 * Hash11(fi * 3.7)), travel);
                    float2 pos = dir * r;
                    float2 rel = c.st - pos;
                    float2 perp = float2(-dir.y, dir.x);
                    float al = dot(rel, dir) / (streak * (1.0 - life * 0.6));
                    float ac = dot(rel, perp);
                    float d = length(float2(al, ac));
                    float s = max(size * (1.0 - life), 1e-4);
                    float fade = (1.0 - life) * smoothstep(0.0, 0.05, life);
                    acc += ((1.0 - smoothstep(0.0, s, d)) + 0.3 * exp(-d / (s * 2.0 + 0.002))) * fade;
                }
                return acc;
            }

            half4 frag(Varyings i) : SV_Target
            {
                FxCtx c;
                c.st = i.uv - 0.5;
                c.dist = length(c.st);
                c.angle = atan2(c.st.y, c.st.x + 1e-5);
                c.prog = _Progress;
                c.t = _Time.y;
                c.aa = max(fwidth(c.dist) * 1.5, 0.0015);
                c.seed = _Seed;

                if (c.prog <= 0.001 || c.prog >= 0.999)
                    return half4(0, 0, 0, 0);

                float p = c.prog;
                float flash = pow(1.0 - p, 2.8);

                // --- 1. Core Explosion & Noise Distortion ---
                float plasmaNoise = FBM(c.st * 4.5 - c.t * 1.5);
                float distortedDist = c.dist + (plasmaNoise - 0.5) * 0.1 * flash;
                
                float coreGlow = exp(-distortedDist * 15.0) * flash * 4.0;
                float coreFlash = (1.0 - smoothstep(0.0, 0.2 * flash + 0.02, distortedDist)) * flash * 3.0;

                // --- 2. Bounded Anamorphic Lens Flare (Prevents Edge Clipping) ---
                // Multiplying by smooth edge masks ensures rays fade out safely before touching boundaries
                float flareFadeX = smoothstep(0.48, 0.2, abs(c.st.x));
                float flareFadeY = smoothstep(0.48, 0.2, abs(c.st.y));
                float horizFlare = exp(-abs(c.st.y) * 50.0) * exp(-abs(c.st.x) * 4.5) * flash * 3.0 * flareFadeX;
                float vertFlare  = exp(-abs(c.st.x) * 50.0) * exp(-abs(c.st.y) * 4.5) * flash * 3.0 * flareFadeY;

                // --- 3. Polished Sparks / Shooting Particles ---
                float sp = Sparks(c, 3.1, 0.04, 0.45, 0.01, 2.2);

                // --- Color Composition ---
                float3 coreColor = Heat(plasmaNoise, _Color.rgb, _QualityColor.rgb);
                float3 finalRGB = coreColor * (coreGlow + coreFlash * 2.0)
                                + lerp(_RimColor.rgb, float3(1, 1, 1), 0.8) * (horizFlare + vertFlare)
                                + lerp(_QualityColor.rgb, float3(1, 1, 1), 0.6) * sp * 2.8;

                float finalAlpha = saturate(coreGlow * 0.6 + coreFlash + (horizFlare + vertFlare) * 0.4 + sp * 0.75);

                // --- 4. Smooth Radial Vignette Mask (Guarantees Zero RectTransform Edges) ---
                // Smoothly tapers everything to 0 alpha by radius 0.46 (fully inside the 0.5 uv square)
                float radialVignette = 1.0 - smoothstep(0.32, 0.46, c.dist);
                float env = smoothstep(0.0, 0.04, c.prog) * (1.0 - smoothstep(0.75, 1.0, c.prog));

                finalRGB *= env * radialVignette * _Intensity;
                finalAlpha = saturate(finalAlpha * env * radialVignette);

                // Filmic shoulder curve
                finalRGB = lerp(finalRGB, 1.0 - exp(-finalRGB * 1.8), _SoftClip);

                // Canvas Tint Integration
                finalRGB *= i.color.rgb * i.color.a;
                finalAlpha *= i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, i.posOS) * step(i.posOS, _ClipRect.zw);
                float clipV = inside.x * inside.y;
                finalRGB *= clipV;
                finalAlpha *= clipV;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalAlpha - 0.001);
                #endif

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}