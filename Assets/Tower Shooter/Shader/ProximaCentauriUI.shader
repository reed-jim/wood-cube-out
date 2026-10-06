Shader "UI/EarthWithCosmicBackground"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [Header(Earth Surface)]
        _OceanColor ("Deep Ocean Color", Color) = (0.01, 0.08, 0.28, 1.0)
        _ShallowOceanColor ("Shallow Coast Color", Color) = (0.02, 0.35, 0.55, 1.0)
        _LandColor ("Land Color", Color) = (0.1, 0.45, 0.12, 1.0)
        _MountainColor ("Mountain/Desert Color", Color) = (0.55, 0.45, 0.25, 1.0)
        _StarRadius ("Earth Radius Size", Range(0.1, 0.45)) = 0.28

        [Header(Clouds and Atmosphere)]
        _CloudColor ("Cloud Color", Color) = (0.95, 0.98, 1.0, 1.0)
        _CloudCoverage ("Cloud Density", Range(0.0, 1.0)) = 0.48
        _AtmosphereColor ("Atmosphere Blue Glow", Color) = (0.2, 0.65, 1.0, 1.0)
        _AtmospherePower ("Atmosphere Glow Sharpness", Range(0.5, 10.0)) = 3.5

        [Header(Polar Caps)]
        _PolarCapColor ("Polar Ice Color", Color) = (0.9, 0.95, 1.0, 1.0)
        _PolarCapSize ("Polar Cap Size", Range(0.0, 1.0)) = 0.4

        [Header(Surface Motion)]
        _Speed ("Animation Speed", Range(0.01, 0.5)) = 0.08
        _RotationSpeed ("Earth Rotation Speed", Range(-5.0, 5.0)) = 0.4
        _CloudSpeed ("Cloud Drift Speed Factor", Range(1.0, 3.0)) = 1.3
        _Turbulence ("Continent Noise Scale", Range(1.0, 10.0)) = 2.8

        [Header(Atmosphere and Lighting)]
        _RimPower ("Limb Darkening Sharpness", Range(1.0, 20.0)) = 5.0
        _MinEdgeBrightness ("Minimum Edge Brightness", Range(0.1, 1.0)) = 0.6

        [Header(Gradient Properties)]
        _GradientStart ("Gradient Start", Range(0,1)) = 0
        _GradientEnd ("Gradient End", Range(0,1)) = 1
        _GradientAngle ("Gradient Angle", Range(0,360)) = 0
        _BackgroundBrightness ("Background Brightness", Range(0.0, 2.0)) = 0.8

        [Header(Cosmic Background)]
        _NebulaColorA ("Nebula Color 1", Color) = (0.02, 0.04, 0.12, 1.0)
        _NebulaColorB ("Nebula Color 2", Color) = (0.01, 0.01, 0.05, 1.0)

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use UI Alpha Clip", Float) = 0

        // UI Masking properties
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
                float4 _OceanColor;
                float4 _ShallowOceanColor;
                float4 _LandColor;
                float4 _MountainColor;
                float4 _CloudColor;
                float _CloudCoverage;
                float _StarRadius;
                float _Speed;
                float _RotationSpeed;
                float _CloudSpeed;
                float _Turbulence;
                float _RimPower;
                float _MinEdgeBrightness;

                float4 _PolarCapColor;
                float _PolarCapSize;

                float4 _AtmosphereColor;
                float _AtmospherePower;

                float _GradientStart;
                float _GradientEnd;
                float _GradientAngle;
                float _BackgroundBrightness;

                float4 _NebulaColorA;
                float4 _NebulaColorB;

                float4 _ClipRect;
            CBUFFER_END

            // Noise Helper Functions
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
                    uv *= 2.02;
                    amplitude *= 0.5;
                }
                return value;
            }

            half3 renderNebulaBackground(float2 uv, float time)
            {
                float nebNoiseA = fbm(uv * 2.5 + float2(time * 0.01, 0.0));
                float nebNoiseB = fbm(uv * 4.0 - float2(0.0, time * 0.015));
                half3 background = lerp(_NebulaColorA.rgb, _NebulaColorB.rgb, nebNoiseA * 0.5 + 0.5);
                background += _NebulaColorA.rgb * nebNoiseB;

                return background;
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
                float dist = length(centeredUV);
                float t = _Time.y * _Speed;

                // 1. Render Background
                half3 background = renderNebulaBackground(input.uv, t);

                float angle = radians(_GradientAngle);
                float2 direction = float2(cos(angle), sin(angle));
                float gradientPosition = dot(centeredUV, direction) + 0.5;

                float range = max(_GradientEnd - _GradientStart, 0.0001);
                float gradient = saturate((gradientPosition - _GradientStart) / range);
                gradient = smoothstep(0.0, 1.0, gradient);

                // Alpha gradients from 0.0 (transparent) to 1.0 (opaque nebula)
                float bgAlpha = gradient;
                half3 finalColor = background * _BackgroundBrightness;

                // 2. Atmosphere Fringe Outside
                float radius = _StarRadius;
                float atmosFringe = exp(-max(0.0, dist - radius) * _AtmospherePower * 12.0);
                half3 atmosphereGlow = _AtmosphereColor.rgb * atmosFringe * 0.9;

                float finalAlpha = bgAlpha;

                if (dist > radius)
                {
                    // Outside Earth
                    finalColor += atmosphereGlow;
                    finalAlpha = saturate(bgAlpha + atmosFringe * 0.9);
                }
                else
                {
                    // Inside Earth: Calculate 3D sphere surface
                    float N = dist / radius;
                    float z = sqrt(max(0.0, 1.0 - N * N));
                    float2 sphericalUV = centeredUV / (1.0 + z);

                    // Planet Rotation
                    float rotOffset = _Time.y * _RotationSpeed * 0.1;
                    float2 groundUV = sphericalUV + float2(rotOffset, 0.0);

                    // --- Land vs Ocean Generation ---
                    float terrainNoise = fbm(groundUV * _Turbulence);
                    float elevation = terrainNoise * 0.5 + 0.5;

                    // Sharp continent threshold with coastal shelf
                    float landMask = smoothstep(0.48, 0.53, elevation);
                    float coastMask = smoothstep(0.42, 0.48, elevation);

                    half3 ocean = lerp(_OceanColor.rgb, _ShallowOceanColor.rgb, coastMask);
                    half3 land = lerp(_LandColor.rgb, _MountainColor.rgb, smoothstep(0.55, 0.75, elevation));
                    half3 surfaceColor = lerp(ocean, land, landMask);

                    // --- Cloud Layer (Moving faster than land) ---
                    float2 cloudUV = sphericalUV + float2(rotOffset * _CloudSpeed, t * 0.05);
                    float cloudNoise = fbm(cloudUV * (_Turbulence * 1.5));
                    float cloudMask = smoothstep(1.0 - _CloudCoverage, 0.9, cloudNoise * 0.5 + 0.5);

                    // Cloud shadow on terrain below
                    surfaceColor *= lerp(1.0, 0.6, cloudMask * 0.5);
                    // Add white cloud layer
                    surfaceColor = lerp(surfaceColor, _CloudColor.rgb, cloudMask * 0.85);

                    // --- Polar Ice Caps ---
                    float poleFactor = smoothstep(1.0 - _PolarCapSize, 1.0, abs(sphericalUV.y * 2.0));
                    surfaceColor = lerp(surfaceColor, _PolarCapColor.rgb, poleFactor * 0.9);

                    // Clamped Limb Darkening (day/night edge roll-off)
                    float limbDarkening = lerp(_MinEdgeBrightness, 1.0, pow(z, 1.0 / _RimPower));
                    surfaceColor *= limbDarkening;

                    // Blue Rayleigh Atmospheric Scattering (Fresnel edge)
                    float fresnel = pow(1.0 - z, _AtmospherePower);
                    surfaceColor += _AtmosphereColor.rgb * fresnel * 0.8;

                    // Smooth edge blending right at the radius border
                    float earthMask = smoothstep(radius, radius - 0.005, dist);
                    finalColor = lerp(finalColor + atmosphereGlow, surfaceColor, earthMask);
                    
                    // Planet surface renders opaque, blending smoothly into outer alpha background
                    finalAlpha = lerp(saturate(bgAlpha + atmosFringe * 0.9), 1.0, earthMask);
                }

                half4 result = half4(finalColor, finalAlpha) * input.color;

                // UI Clipping
                #if defined(UNITY_UI_CLIP_RECT)
                float2 inside = step(_ClipRect.xy, input.worldPosition.xy) * step(input.worldPosition.xy, _ClipRect.zw);
                result.a *= inside.x * inside.y;
                #endif

                // Alpha Clip
                #if defined(UNITY_UI_ALPHACLIP)
                clip(result.a - 0.001);
                #endif

                return result;
            }
            ENDHLSL
        }
    }
}
