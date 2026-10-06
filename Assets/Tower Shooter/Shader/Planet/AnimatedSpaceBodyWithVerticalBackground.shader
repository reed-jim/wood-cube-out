Shader "UI/CosmicNebulaWithCenterTransparentGradient"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [Header(Global Control)]
        _GlobalSpeed ("Animation Speed", Range(0.0, 2.0)) = 0.15
        _BackgroundBrightness ("Background Brightness", Range(0.0, 3.0)) = 1.2

        [Header(Aspect Ratio and Rounded Corners)]
        _Width ("Rect Width", Float) = 800.0
        _Height ("Rect Height", Float) = 400.0
        _CornerRadius ("Corner Radius (Pixels)", Range(0.0, 200.0)) = 32.0
        _CornerSmoothness ("Edge Anti-Aliasing (Pixels)", Range(0.5, 5.0)) = 1.5

        [Header(1 to 0 to 1 Gradient Mask Logic)]
        _CenterPosition ("Center Zero-Alpha Offset", Range(-0.5, 0.5)) = 0.0
        _GradientWidth ("Center Transparency Width", Range(0.01, 10.0)) = 0.5
        _GradientAngle ("Gradient Angle", Range(0, 360)) = 90.0

        [Header(Space Base Colors)]
        _SpaceColor ("Space Dark Base (Hex #050814)", Color) = (0.02, 0.03, 0.08, 1.0)
        _DustColor ("Space Highlight Base (Hex #0D1B36)", Color) = (0.05, 0.11, 0.21, 1.0)

        [Header(Nebula Dust and Clouds)]
        _NebulaMagenta ("Magenta Gas Color", Color) = (0.75, 0.20, 0.65, 1.0)
        _NebulaCyan ("Cyan Filament Color", Color) = (0.15, 0.65, 0.90, 1.0)
        _NebulaPurple ("Purple Transition Color", Color) = (0.35, 0.10, 0.55, 1.0)

        [Header(Dust Cloud Density and Noise)]
        _CloudScale ("Nebula Scale", Range(0.5, 10.0)) = 2.5
        _WarpIntensity ("Turbulence / Distortion", Range(0.0, 3.0)) = 1.4
        _DustDensity ("Dust Filament Density", Range(0.5, 3.0)) = 1.3

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use UI Alpha Clip", Float) = 0

        // UI Masking
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
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;

                #if defined(UNITY_UI_CLIP_RECT)
                float4 worldPosition : TEXCOORD1;
                #endif
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _GlobalSpeed;
                float _BackgroundBrightness;

                float _Width;
                float _Height;
                float _CornerRadius;
                float _CornerSmoothness;

                float _CenterPosition;
                float _GradientWidth;
                float _GradientAngle;

                float4 _SpaceColor;
                float4 _DustColor;

                float4 _NebulaMagenta;
                float4 _NebulaCyan;
                float4 _NebulaPurple;

                float _CloudScale;
                float _WarpIntensity;
                float _DustDensity;

                float4 _ClipRect;
            CBUFFER_END

            // --- Noise Helper Functions ---
            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float perlinNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(dot(hash22(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                                 dot(hash22(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                            lerp(dot(hash22(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                                 dot(hash22(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
            }

            float fbm(float2 uv)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 5; i++)
                {
                    value += amplitude * perlinNoise(uv);
                    uv *= 2.03;
                    amplitude *= 0.5;
                }
                return value;
            }

            float domainWarpFBM(float2 uv, float time, out float2 warpOffset)
            {
                float2 q = float2(fbm(uv + float2(0.0, 0.0)),
                                 fbm(uv + float2(5.2, 1.3)));

                float2 r = float2(fbm(uv + 4.0 * q + float2(1.7 - time * 0.15, 9.2 + time * 0.1)),
                                 fbm(uv + 4.0 * q + float2(8.3 - time * 0.1, 2.8 + time * 0.2)));

                warpOffset = r;
                return fbm(uv + _WarpIntensity * r);
            }

            // Aspect-Corrected Pixel Space SDF Box
            float sdfRoundedBoxPixelSpace(float2 posPixel, float2 halfSizePixel, float radiusPixel)
            {
                float2 q = abs(posPixel) - halfSizePixel + radiusPixel;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - radiusPixel;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;

                #if defined(UNITY_UI_CLIP_RECT)
                output.worldPosition = input.positionOS;
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centeredUV = input.uv - 0.5;
                float masterTime = _Time.y * _GlobalSpeed;

                // 1. Aspect-Corrected SDF Rounded Corner Mask
                float2 elementDimensions = float2(max(_Width, 1.0), max(_Height, 1.0));
                float2 pixelPos = centeredUV * elementDimensions;
                float2 halfSize = elementDimensions * 0.5;

                float maxRadius = min(halfSize.x, halfSize.y);
                float radius = min(_CornerRadius, maxRadius);

                float cornerDistPixel = sdfRoundedBoxPixelSpace(pixelPos, halfSize, radius);
                float cornerMask = 1.0 - smoothstep(0.0, _CornerSmoothness, cornerDistPixel);

                // 2. Directional 1 -> 0 -> 1 Alpha Gradient Mask Calculation
                float angleRad = radians(_GradientAngle);
                float2 gradDir = float2(cos(angleRad), sin(angleRad));
                float rawGradientPos = dot(centeredUV, gradDir) - _CenterPosition;

                // Absolute distance from center offset mapped into a 1 -> 0 -> 1 range
                float halfWidth = max(_GradientWidth * 0.5, 0.0001);
                float distFromCenter = saturate(abs(rawGradientPos) / halfWidth);
                float bgGradient = smoothstep(0.0, 1.0, distFromCenter);

                // Base space color interpolation
                half3 spaceBase = lerp(_SpaceColor.rgb, _DustColor.rgb, bgGradient);

                // 3. Aspect-Corrected Cloud Noise
                float aspect = elementDimensions.x / elementDimensions.y;
                float2 st = float2(centeredUV.x * aspect, centeredUV.y) * _CloudScale;
                
                float2 warpOffset;
                float mainCloud = domainWarpFBM(st, masterTime, warpOffset);
                float fineDust = fbm(st * 2.5 + warpOffset * 1.5 + float2(masterTime * 0.2, -masterTime * 0.1));

                // 4. Blend Dust Tendrils
                float magentaMask = smoothstep(-0.2, 0.5, mainCloud);
                float cyanMask = smoothstep(0.1, 0.7, fineDust) * smoothstep(-0.1, 0.6, mainCloud);
                float purpleMask = smoothstep(-0.5, 0.2, mainCloud);

                half3 cloudColor = lerp(spaceBase, _NebulaPurple.rgb, purpleMask * 0.7);
                cloudColor = lerp(cloudColor, _NebulaMagenta.rgb, magentaMask * 0.85);
                cloudColor = lerp(cloudColor, _NebulaCyan.rgb, cyanMask * 0.9);

                // Highlights
                float highlightMask = pow(saturate(mainCloud * fineDust * _DustDensity), 2.0);
                cloudColor += _NebulaCyan.rgb * highlightMask * 0.6;

                half3 finalColor = cloudColor * _BackgroundBrightness;
                
                // Composite Alpha (Center-Faded Gradient * Corner SDF Mask)
                float finalAlpha = bgGradient * cornerMask;

                half4 result = half4(finalColor, finalAlpha) * input.color;

                #if defined(UNITY_UI_CLIP_RECT)
                float2 inside = step(_ClipRect.xy, input.worldPosition.xy) * step(input.worldPosition.xy, _ClipRect.zw);
                result.a *= inside.x * inside.y;
                #endif

                #if defined(UNITY_UI_ALPHACLIP)
                clip(result.a - 0.001);
                #endif

                return result;
            }
            ENDHLSL
        }
    }
}