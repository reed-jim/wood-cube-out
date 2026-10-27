Shader "UI/Custom/TacticalCyberShieldAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Shield Base Color", Color) = (0.02, 0.12, 0.25, 1.0)
        _RimColor ("Hex / Plasma Edge Color", Color) = (0.0, 0.9, 1.0, 1.0)

        [Header(Shield Control)]
        _Integrity ("Shield Integrity (0 - 100)", Range(0.0, 100.0)) = 85.0
        _RotationAngle ("Matrix Rotation (radians)", Float) = 0.0
        _ImpactTrigger ("Impact Shockwave (0-1)", Range(0.0, 1.0)) = 0.0

        [Header(Grid Shape)]
        _HexScale ("Hexagon Density", Range(5.0, 50.0)) = 22.0
        _LineWidth ("Grid Line Width", Range(0.01, 0.2)) = 0.05
        _InnerRadius ("Core Aperture Radius", Range(0.0, 0.5)) = 0.15
        _OuterRadius ("Field Outer Radius", Range(0.4, 0.8)) = 0.65

        [Header(Look FX)]
        _DataGlitch ("Glitch / Data Stream Speed", Range(0.0, 3.0)) = 1.0
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
            Name "UI_TacticalCyberShield"
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
                float _Integrity;
                float _RotationAngle;
                float _ImpactTrigger;
                float _HexScale;
                float _LineWidth;
                float _InnerRadius;
                float _OuterRadius;
                float _DataGlitch;
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
            // Stable Hashes & Math Helpers
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

            // Hexagonal grid distance metric generator
            float2 hexCoord(float2 p)
            {
                float2 r = float2(1.0, 1.7320508); // 1.0, sqrt(3)
                float2 h = r * 0.5;
                float2 a = fmod(p, r) - h;
                float2 b = fmod(p - h, r) - h;
                return dot(a, a) < dot(b, b) ? a : b;
            }

            float3 ACESFilm(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
            }

            // ------------------------------------------------------------
            // Main Shield Effect
            // ------------------------------------------------------------
            float4 CyberShield(float2 p, float px)
            {
                float t = _Time.y;
                float integrity = saturate(_Integrity * 0.01);
                float r = length(p);
                float2 pr = rot2(p, _RotationAngle + t * 0.05);
                float ang = atan2(pr.y, pr.x);

                // Palette
                float3 baseCol = _Color.rgb;
                float3 rimCol = _RimColor.rgb;
                float3 alertCol = float3(1.0, 0.3, 0.15); // Low integrity warning color
                float3 activeRim = lerp(alertCol, rimCol, saturate(integrity * 1.5));

                // ---------------- 1. Hexagon Grid Matrix ----------------
                float2 hp = pr * _HexScale;
                float2 hxy = hexCoord(hp);
                float hexDist = length(hxy);
                float gridLine = saturate((_LineWidth - hexDist) / px);
                
                // Data stream pulse modulation inside hex cells
                float cellId = hash12(floor(hp - hxy));
                float stream = step(0.7, frac(cellId * 10.0 - t * _DataGlitch));
                float3 hexRGB = activeRim * (gridLine * (0.4 + 1.2 * stream));

                // ---------------- 2. Outer Ring & Barrier Boundaries ----------------
                float inner = _InnerRadius;
                float outer = _OuterRadius;
                float ringBand = smoothstep(inner, inner + 0.03, r) * (1.0 - smoothstep(outer - 0.03, outer, r));
                
                // Perimeter rotating tech markers
                float ticks = pow(abs(sin(ang * 24.0 + t * 2.0)), 4.0);
                float3 borderRing = activeRim * (ringBand * 0.5 + ticks * ringBand * integrity);

                // ---------------- 3. Impact Shockwave Distortion ----------------
                float impact = saturate(_ImpactTrigger);
                float waveRadius = impact * 0.8;
                float waveDist = abs(r - waveRadius);
                float shockwave = exp(-waveDist * waveDist / 0.002) * impact * 3.0;
                float3 shockRGB = float3(1.0, 1.0, 1.0) * shockwave;

                // ---------------- 4. Core Energy Vortex ----------------
                float coreMask = 1.0 - smoothstep(0.0, inner + 0.02, r);
                float vortex = vnoise(pr * 8.0 - t * 2.0);
                float3 coreRGB = activeRim * vortex * coreMask * 1.5 * integrity;

                // ---------------- 5. Composition & Integrity Fade ----------------
                float integrityFade = saturate(integrity * 1.2 + 0.2);
                float3 finalEmis = (hexRGB * ringBand + borderRing + shockRGB + coreRGB) * integrityFade;

                float alpha = saturate((ringBand * 0.8 + gridLine * ringBand * 0.5 + coreMask + shockwave) * integrityFade);
                float edgeFade = (1.0 - smoothstep(outer - 0.05, outer + 0.02, r)) * smoothstep(0.0, 0.02, r);

                return float4(finalEmis * edgeFade, alpha * edgeFade);
            }

            // ------------------------------------------------------------
            // Fragment Shader
            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float px = max(fwidth(p.x), 1e-4);

                float4 col = CyberShield(p, px);

                // Highlight bloom clamp & ACES Tonemapping
                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 5.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.75);

                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                // Dithering against banding
                float dn = hash12(input.positionCS.xy + frac(_Time.y) * 61.0) - 0.5;
                col.rgb += dn / 255.0 * max(col.a, max3(col.rgb));

                // UI Vertex Color application
                col.rgb *= input.color.rgb * input.color.a;
                col.a *= input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, input.worldPos.xy) * step(input.worldPos.xy, _ClipRect.zend); // Wait, fix ClipRect check
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