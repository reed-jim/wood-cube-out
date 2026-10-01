Shader "UI/Custom/SpaceEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Animation Settings)]
        _Speed ("Time Speed", Range(0.0, 5.0)) = 0.5
        _Tiling ("Tiling / Density", Range(1.0, 10.0)) = 2.5

        [Header(Nebula Settings)]
        _NebulaColorA ("Nebula Color A", Color) = (0.1, 0.2, 0.6, 1.0)
        _NebulaColorB ("Nebula Color B", Color) = (0.8, 0.2, 0.5, 1.0)
        _NebulaIntensity ("Nebula Intensity", Range(0.0, 3.0)) = 1.2

        [Header(Star Settings)]
        _StarColor ("Star Color", Color) = (1.0, 0.95, 0.8, 1.0)
        _StarDensity ("Star Density", Range(0.0, 100.0)) = 40.0
        _StarBrightness ("Star Brightness", Range(0.1, 5.0)) = 2.0

        // Required UI Masking Properties
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
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
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
            Name "UISpacePass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma shader_feature_local UNITY_UI_CLIP_RECT
            #pragma shader_feature_local UNITY_UI_ALPHACLIP

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 worldPosition: TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _NebulaColorA;
                float4 _NebulaColorB;
                float4 _StarColor;
                float _Speed;
                float _Tiling;
                float _NebulaIntensity;
                float _StarDensity;
                float _StarBrightness;
                float4 _ClipRect;
            CBUFFER_END

            // --- Procedural Helper Functions ---
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float fbm(float2 st)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * valueNoise(st);
                    st *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            // Simple procedurally placed twinkle stars layer
            float starLayer(float2 uv, float t)
            {
                float2 gridUV = frac(uv) - 0.5;
                float2 id = floor(uv);
                
                float n = hash21(id);
                float star = 0.0;

                if (n > 0.85)
                {
                    float size = frac(n * 123.45) * 0.15 + 0.05;
                    float dist = length(gridUV);
                    
                    // Twinkle modulation
                    float twinkle = sin(t * 3.0 + n * 6.28) * 0.5 + 0.5;
                    star = smoothstep(size, 0.0, dist) * twinkle;
                }
                return star;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.worldPosition = input.positionOS;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _Speed;
                float2 st = (input.uv - 0.5) * _Tiling;

                // --- 1. Procedural Nebula Effect ---
                float2 q = float2(0.0, 0.0);
                q.x = fbm(st + 0.05 * time);
                q.y = fbm(st + float2(1.0, 1.0));

                float2 r = float2(0.0, 0.0);
                r.x = fbm(st + 1.0 * q + float2(1.7, 9.2) + 0.15 * time);
                r.y = fbm(st + 1.0 * q + float2(8.3, 2.8) + 0.126 * time);

                float f = fbm(st + r);
                float3 nebulaColor = lerp(_NebulaColorA.rgb, _NebulaColorB.rgb, clamp(f * f * 4.0, 0.0, 1.0));
                nebulaColor += lerp(float3(0.0, 0.0, 0.1), float3(0.2, 0.5, 0.6), clamp(length(q), 0.0, 1.0));
                nebulaColor *= f * _NebulaIntensity;

                // --- 2. Parallax Star Layers ---
                float stars = 0.0;
                stars += starLayer(st * _StarDensity, time);
                stars += starLayer((st + float2(12.3, 45.6)) * (_StarDensity * 1.5), time * 1.2) * 0.6;

                float3 finalColor = nebulaColor + (stars * _StarBrightness * _StarColor.rgb);

                // --- 3. UI Alpha & Sprite Masking ---
                float4 spriteTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float4 finalPixel = float4(finalColor, (f + stars) * spriteTex.a) * input.color;

                #ifdef UNITY_UI_CLIP_RECT
                    finalPixel.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(finalPixel.a - 0.001);
                #endif

                return finalPixel;
            }
            ENDHLSL
        }
    }
}
