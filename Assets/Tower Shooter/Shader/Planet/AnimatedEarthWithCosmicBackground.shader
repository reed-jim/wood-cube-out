Shader "UI/AnimatedEarthWithCosmicBackground"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [Header(Master Speed)]
        _GlobalSpeed ("Global Animation Speed", Range(0.0, 2.0)) = 0.2

        [Header(Earth Surface)]
        _OceanColor ("Deep Ocean Color", Color) = (0.01, 0.08, 0.28, 1.0)
        _ShallowOceanColor ("Shallow Coast Color", Color) = (0.02, 0.35, 0.55, 1.0)
        _LandColor ("Land Color", Color) = (0.1, 0.45, 0.12, 1.0)
        _MountainColor ("Mountain/Desert Color", Color) = (0.55, 0.45, 0.25, 1.0)
        _PlanetRadius ("Earth Radius Size", Range(0.1, 0.45)) = 0.28
        _PlanetTilt ("Planet Axis Tilt Angle", Range(-180, 180)) = 45.0
        _RotationSpeed ("Earth Rotation Speed", Range(-5.0, 5.0)) = 0.4
        _Turbulence ("Continent Noise Scale", Range(1.0, 10.0)) = 2.8

        [Header(Clouds and Atmosphere)]
        _CloudColor ("Cloud Color", Color) = (0.95, 0.98, 1.0, 1.0)
        _CloudCoverage ("Cloud Density", Range(0.0, 1.0)) = 0.48
        _CloudSpeed ("Cloud Drift Speed Factor", Range(1.0, 3.0)) = 1.3
        _AtmosphereColor ("Atmosphere Blue Glow", Color) = (0.3, 0.6, 1.0, 1.0)
        _AtmospherePower ("Atmosphere Glow Sharpness", Range(0.5, 10.0)) = 3.5
        _PulseSpeed ("Atmosphere Pulse Speed", Range(0.0, 5.0)) = 1.5

        [Header(Polar Caps)]
        _PolarCapColor ("Polar Ice Color", Color) = (0.9, 0.95, 1.0, 1.0)
        _PolarCapSize ("Polar Cap Size", Range(0.0, 1.0)) = 0.4

        [Header(Atmosphere and Lighting)]
        _RimPower ("Limb Darkening Sharpness", Range(1.0, 20.0)) = 5.0
        _MinEdgeBrightness ("Minimum Edge Brightness", Range(0.1, 1.0)) = 0.6

        [Header(Background Gradient Properties)]
        _GradientStart ("Gradient Start", Range(0, 1)) = 0.0
        _GradientEnd ("Gradient End", Range(0, 1)) = 1.0
        _GradientAngle ("Gradient Angle", Range(0, 360)) = 0.0
        _BackgroundBrightness ("Background Brightness", Range(0.0, 2.0)) = 1.0

        [Header(Animated Cosmic Background)]
        _SpaceColor ("Space Dark Base", Color) = (0.01, 0.02, 0.05, 1.0)
        _DustColor ("Cosmic Dust Streak Color", Color) = (0.05, 0.15, 0.3, 1.0)
        _NebulaColorA ("Nebula Color 1", Color) = (0.02, 0.04, 0.12, 1.0)
        _NebulaColorB ("Nebula Color 2", Color) = (0.01, 0.01, 0.05, 1.0)
        _BackgroundSpeed ("Background Drift Speed", Range(0.0, 2.0)) = 0.3

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
                float _GlobalSpeed;

                float4 _OceanColor;
                float4 _ShallowOceanColor;
                float4 _LandColor;
                float4 _MountainColor;
                float _PlanetRadius;
                float _PlanetTilt;
                float _RotationSpeed;
                float _Turbulence;

                float4 _CloudColor;
                float _CloudCoverage;
                float _CloudSpeed;
                float4 _AtmosphereColor;
                float _AtmospherePower;
                float _PulseSpeed;

                float4 _PolarCapColor;
                float _PolarCapSize;

                float _RimPower;
                float _MinEdgeBrightness;

                float _GradientStart;
                float _GradientEnd;
                float _GradientAngle;
                float _BackgroundBrightness;

                float4 _SpaceColor;
                float4 _DustColor;
                float4 _NebulaColorA;
                float4 _NebulaColorB;
                float _BackgroundSpeed;

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

            float2 rotateUV(float2 uv, float angleDeg)
            {
                float rad = radians(angleDeg);
                float c = cos(rad);
                float s = sin(rad);
                return float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
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
                float distToCenter = length(centeredUV);

                // --- Global Timers ---
                float masterTime = _Time.y * _GlobalSpeed;
                float bgTime = masterTime * _BackgroundSpeed;
                float pulseTime = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;

                // 1. Directional Gradient Mask
                float angleRad = radians(_GradientAngle);
                float2 gradDir = float2(cos(angleRad), sin(angleRad));
                float gradientPosition = dot(centeredUV, gradDir) + 0.5;

                float gradRange = max(_GradientEnd - _GradientStart, 0.0001);
                float bgGradient = saturate((gradientPosition - _GradientStart) / gradRange);
                bgGradient = smoothstep(0.0, 1.0, bgGradient);

                // 2. Animated Cosmic Background
                float2 dustUV = rotateUV(centeredUV, -20.0);
                dustUV.y *= 3.0;
                float dustAnimation = fbm(dustUV * 2.5 + float2(bgTime * 0.4, sin(bgTime * 0.2) * 0.1));

                float nebNoiseA = fbm(input.uv * 2.5 + float2(bgTime * 0.15, bgTime * 0.05));
                float nebNoiseB = fbm(input.uv * 4.0 - float2(bgTime * 0.1, bgTime * 0.2));

                half3 spaceBackground = lerp(_SpaceColor.rgb, _NebulaColorA.rgb, nebNoiseA * 0.5 + 0.5);
                spaceBackground = lerp(spaceBackground, _NebulaColorB.rgb, nebNoiseB * 0.5 + 0.5);
                spaceBackground += _DustColor.rgb * dustAnimation * 0.8;
                spaceBackground *= _BackgroundBrightness;

                float bgAlpha = bgGradient;

                // 3. Earth Sphere Rendering
                float planetRadius = _PlanetRadius;
                bool isPlanet = distToCenter < planetRadius;

                half3 finalColor = spaceBackground;
                float finalAlpha = bgAlpha;

                if (isPlanet)
                {
                    // Pre-rotate centered UV coordinates to tilt the planet's axis
                    float2 tiltedCenteredUV = rotateUV(centeredUV, _PlanetTilt);

                    // Calculate 3D sphere surface using tilted coordinates
                    float N = distToCenter / planetRadius;
                    float z = sqrt(max(0.0, 1.0 - N * N));
                    float2 sphericalUV = tiltedCenteredUV / (1.0 + z);

                    // Planet Rotation along tilted axis
                    float rotOffset = _Time.y * _RotationSpeed * 0.1;
                    float2 groundUV = sphericalUV + float2(rotOffset, 0.0);

                    // Terrain Generation
                    float terrainNoise = fbm(groundUV * _Turbulence);
                    float elevation = terrainNoise * 0.5 + 0.5;

                    float landMask = smoothstep(0.48, 0.53, elevation);
                    float coastMask = smoothstep(0.42, 0.48, elevation);

                    half3 ocean = lerp(_OceanColor.rgb, _ShallowOceanColor.rgb, coastMask);
                    half3 land = lerp(_LandColor.rgb, _MountainColor.rgb, smoothstep(0.55, 0.75, elevation));
                    half3 surfaceColor = lerp(ocean, land, landMask);

                    // Moving Cloud Layer
                    float2 cloudUV = sphericalUV + float2(rotOffset * _CloudSpeed, masterTime * 0.1);
                    float cloudNoise = fbm(cloudUV * (_Turbulence * 1.5));
                    float cloudMask = smoothstep(1.0 - _CloudCoverage, 0.9, cloudNoise * 0.5 + 0.5);

                    surfaceColor *= lerp(1.0, 0.6, cloudMask * 0.5);
                    surfaceColor = lerp(surfaceColor, _CloudColor.rgb, cloudMask * 0.85);

                    // Polar Ice Caps (Aligned with tilted poles)
                    float poleFactor = smoothstep(1.0 - _PolarCapSize, 1.0, abs(sphericalUV.y * 2.0));
                    surfaceColor = lerp(surfaceColor, _PolarCapColor.rgb, poleFactor * 0.9);

                    // Limb Darkening
                    float limbDarkening = lerp(_MinEdgeBrightness, 1.0, pow(z, 1.0 / _RimPower));
                    surfaceColor *= limbDarkening;

                    // Dynamic Rayleigh Atmospheric Scattering
                    float animatedFresnelPower = _AtmospherePower + pulseTime * 0.5;
                    float fresnel = pow(1.0 - z, animatedFresnelPower);
                    surfaceColor += _AtmosphereColor.rgb * fresnel * 0.8;

                    finalColor = surfaceColor;
                    finalAlpha = 1.0;
                }

                // Outer Atmosphere Glow
                if (!isPlanet)
                {
                    float atmosFringe = exp(-max(0.0, distToCenter - planetRadius) * (_AtmospherePower + pulseTime * 0.5) * 12.0);
                    float glowIntensity = atmosFringe * (0.7 + pulseTime * 0.2);
                    finalColor += _AtmosphereColor.rgb * glowIntensity;
                    finalAlpha = saturate(finalAlpha + glowIntensity);
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