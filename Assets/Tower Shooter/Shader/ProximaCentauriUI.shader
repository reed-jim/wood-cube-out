Shader "UI/ProximaCentauriWithCosmicBackground"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [Header(Star Properties)]
        _BaseColor ("Base Surface Color", Color) = (0.9, 0.25, 0.05, 1.0)
        _HotSpotColor ("Flare/Hotspot Color", Color) = (1.0, 0.75, 0.2, 1.0)
        _GlowColor ("Corona Glow Color", Color) = (0.8, 0.15, 0.02, 1.0)
        _StarRadius ("Star Radius Size", Range(0.1, 0.45)) = 0.28

        [Header(Atmospheric Glow)]
        _AtmosphereColor ("Atmosphere Edge Glow Color", Color) = (0.2, 0.6, 1.0, 1.0)
        _AtmospherePower ("Atmosphere Glow Sharpness", Range(0.5, 10.0)) = 4.0

        [Header(Surface Motion)]
        _Speed ("Animation Speed", Range(0.05, 1.0)) = 0.2
        _RotationSpeed ("Planet Y-Rotation Speed", Range(-5.0, 5.0)) = 1.0
        _Turbulence ("Granulation Turbulence", Range(1.0, 10.0)) = 4.0

        [Header(Atmosphere and Flares)]
        _RimPower ("Limb Darkening Sharpness", Range(1.0, 20.0)) = 8.0
        _MinEdgeBrightness ("Minimum Edge Brightness", Range(0.1, 1.0)) = 0.7
        _FlareIntensity ("Prominence Intensity", Range(0.1, 3.0)) = 1.2

        [Header(Gradient Properties)]
        _GradientColor ("Gradient Color", Color) = (1,0,0,1)
        _GradientStart ("Gradient Start", Range(0,1)) = 0
        _GradientEnd ("Gradient End", Range(0,1)) = 1
        _GradientAngle ("Gradient Angle", Range(0,360)) = 0
        _BackgroundBrightness ("Background Brightness", Range(0.0, 2.0)) = 0.8

        [Header(Cosmic Background)]
        _NebulaColorA ("Nebula Color 1", Color) = (0.05, 0.02, 0.15, 1.0)
        _NebulaColorB ("Nebula Color 2", Color) = (0.1, 0.02, 0.05, 1.0)

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
                float4 _BaseColor;
                float4 _HotSpotColor;
                float4 _GlowColor;
                float _StarRadius;
                float _Speed;
                float _RotationSpeed;
                float _Turbulence;
                float _RimPower;
                float _MinEdgeBrightness;
                float _FlareIntensity;

                float4 _AtmosphereColor;
                float _AtmospherePower;

                float4 _GradientColor;
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
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * perlinNoise(uv);
                    uv *= 2.05;
                    amplitude *= 0.5;
                }
                return value;
            }

            half3 renderNebulaBackground(float2 uv, float time)
            {
                float nebNoiseA = fbm(uv * 3.0 + float2(time * 0.02, 0.0));
                float nebNoiseB = fbm(uv * 5.0 - float2(0.0, time * 0.03));
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

                half3 finalColor = lerp(_GradientColor.rgb, background, gradient) * _BackgroundBrightness;

                // 2. Continuous Radial Glow & Flares (Calculated smoothly across both inside and outside)
                float radius = _StarRadius;
                float glow = exp(-dist * 10.0 / radius) * 1.2;

                float flareAngle = atan2(centeredUV.y, centeredUV.x);
                float flareNoise = fbm(float2(flareAngle * 4.0, t * 1.5)) * _FlareIntensity;
                float flare = max(0.0, (1.0 - max(0.0, dist - radius) * 10.0)) * flareNoise;

                half3 coronaGlow = _GlowColor.rgb * (glow + flare);

                // Add outer atmospheric fringe
                float atmosFringe = exp(-max(0.0, dist - radius) * _AtmospherePower * 10.0);
                half3 atmosphereGlow = _AtmosphereColor.rgb * atmosFringe;

                if (dist > radius)
                {
                    // Outside planet: background + glow + atmosphere
                    finalColor += coronaGlow + atmosphereGlow;
                }
                else
                {
                    // Inside planet: calculate 3D sphere surface
                    float N = dist / radius;
                    float z = sqrt(max(0.0, 1.0 - N * N));
                    float2 sphericalUV = centeredUV / (1.0 + z);

                    // Y-Axis rotation
                    float rotOffset = _Time.y * _RotationSpeed;
                    float2 rotatingUV = sphericalUV + float2(rotOffset, 0.0);

                    // Surface noise
                    float noiseVal = fbm(rotatingUV * _Turbulence + float2(0.0, t * 0.1));
                    float noiseVal2 = fbm(rotatingUV * (_Turbulence * 1.8) - float2(0.0, -t * 0.2));
                    float combinedNoise = saturate((noiseVal + noiseVal2) * 0.7 + 0.3);

                    half3 surfaceColor = lerp(_BaseColor.rgb, _HotSpotColor.rgb, pow(combinedNoise, 2.0));

                    // Clamped Limb Darkening (prevents rim from darkening to black)
                    float limbDarkening = lerp(_MinEdgeBrightness, 1.0, pow(z, 1.0 / _RimPower));
                    surfaceColor *= limbDarkening;

                    // Additive Fresnel & Corona Glow seamlessly over surface
                    float fresnel = pow(1.0 - z, _AtmospherePower);
                    surfaceColor += _AtmosphereColor.rgb * fresnel + coronaGlow;

                    // Smooth edge blending right at the radius border
                    float starMask = smoothstep(radius, radius - 0.005, dist);
                    finalColor = lerp(finalColor + coronaGlow + atmosphereGlow, surfaceColor, starMask);
                }

                half4 result = half4(finalColor, 1.0) * input.color;

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
