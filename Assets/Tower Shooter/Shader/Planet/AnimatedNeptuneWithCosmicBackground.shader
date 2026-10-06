Shader "UI/AnimatedNeptuneWithRingsAndCosmicBackground"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [Header(Master Speed)]
        _GlobalSpeed ("Global Animation Speed", Range(0.0, 2.0)) = 0.2

        [Header(Neptune Atmosphere and Bands)]
        _DeepAtmosphereColor ("Deep Methane Ocean Blue", Color) = (0.02, 0.12, 0.42, 1.0)
        _MidAtmosphereColor ("Azure Methane Blue", Color) = (0.08, 0.32, 0.78, 1.0)
        _BandColor ("Cyan Jet Stream Color", Color) = (0.22, 0.58, 0.95, 1.0)
        _CloudColor ("Bright Cirrus Cloud Color", Color) = (0.85, 0.95, 1.0, 1.0)
        _DarkSpotColor ("Great Dark Spot Color", Color) = (0.01, 0.05, 0.22, 1.0)

        [Header(Neptune Rings)]
        _RingColor ("Ring Base Color", Color) = (0.35, 0.65, 0.85, 0.5)
        _RingInnerRadius ("Ring Inner Radius", Range(0.1, 0.5)) = 0.32
        _RingOuterRadius ("Ring Outer Radius", Range(0.2, 0.6)) = 0.44
        _RingTilt ("Ring Inclination Pitch", Range(0.0, 85.0)) = 70.0

        [Header(Planet Transform)]
        _PlanetRadius ("Neptune Radius Size", Range(0.1, 0.45)) = 0.28
        _PlanetTilt ("Planet Axis Tilt Angle", Range(-180, 180)) = 45.0
        _RotationSpeed ("Jet Stream Drift Speed", Range(-5.0, 5.0)) = 0.5
        _Turbulence ("Atmospheric Turbulence Scale", Range(1.0, 10.0)) = 3.0

        [Header(Glow and Rim)]
        _AtmosphereColor ("Electric Blue Limb Glow", Color) = (0.25, 0.65, 1.0, 1.0)
        _AtmospherePower ("Atmosphere Glow Sharpness", Range(0.5, 10.0)) = 4.0
        _PulseSpeed ("Atmosphere Pulse Speed", Range(0.0, 5.0)) = 1.0
        _RimPower ("Limb Darkening Sharpness", Range(1.0, 20.0)) = 5.0
        _MinEdgeBrightness ("Minimum Edge Brightness", Range(0.1, 1.0)) = 0.5

        [Header(Background Gradient Properties)]
        _GradientStart ("Gradient Start", Range(0, 1)) = 0.0
        _GradientEnd ("Gradient End", Range(0, 1)) = 1.0
        _GradientAngle ("Gradient Angle", Range(0, 360)) = 0.0
        _BackgroundBrightness ("Background Brightness", Range(0.0, 2.0)) = 1.0

        [Header(Animated Cosmic Background)]
        _SpaceColor ("Space Dark Base", Color) = (0.01, 0.02, 0.06, 1.0)
        _DustColor ("Cosmic Dust Streak Color", Color) = (0.04, 0.12, 0.28, 1.0)
        _NebulaColorA ("Nebula Color 1", Color) = (0.02, 0.05, 0.18, 1.0)
        _NebulaColorB ("Nebula Color 2", Color) = (0.01, 0.02, 0.08, 1.0)
        _BackgroundSpeed ("Background Drift Speed", Range(0.0, 2.0)) = 0.25

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

                float4 _DeepAtmosphereColor;
                float4 _MidAtmosphereColor;
                float4 _BandColor;
                float4 _CloudColor;
                float4 _DarkSpotColor;

                float4 _RingColor;
                float _RingInnerRadius;
                float _RingOuterRadius;
                float _RingTilt;

                float _PlanetRadius;
                float _PlanetTilt;
                float _RotationSpeed;
                float _Turbulence;

                float4 _AtmosphereColor;
                float _AtmospherePower;
                float _PulseSpeed;

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

                // --- Planetary Coordinate Transformations ---
                float2 tiltedCenteredUV = rotateUV(centeredUV, _PlanetTilt);

                // Ring UV calculations in tilted space
                float ringPitchRad = radians(_RingTilt);
                float cosPitch = cos(ringPitchRad);
                float sinPitch = sin(ringPitchRad);

                float2 ringUV = tiltedCenteredUV;
                ringUV.y /= max(cosPitch, 0.001); // Flatten for perspective inclination
                float ringDist = length(ringUV);

                // Ring Masking
                bool inRingBounds = (ringDist >= _RingInnerRadius) && (ringDist <= _RingOuterRadius);
                bool isRingInFront = tiltedCenteredUV.y < 0.0; // Depth sorting relative to tilted plane

                // Ring Color and Banding Noise
                half4 ringFinalColor = half4(0, 0, 0, 0);
                if (inRingBounds)
                {
                    float ringBanding = sin(ringDist * 180.0) * 0.5 + 0.5;
                    ringBanding += fbm(float2(ringDist * 40.0, masterTime * 0.2)) * 0.3;

                    // Arc clumps (Galle, Le Verrier, and Adams ring arcs)
                    float arcAngle = atan2(ringUV.y, ringUV.x);
                    float arcClumping = sin(arcAngle * 3.0 + masterTime) * 0.3 + 0.7;

                    float ringAlpha = _RingColor.a * smoothstep(_RingInnerRadius, _RingInnerRadius + 0.02, ringDist);
                    ringAlpha *= smoothstep(_RingOuterRadius, _RingOuterRadius - 0.02, ringDist);
                    ringAlpha *= saturate(ringBanding * arcClumping);

                    // Shadow cast by planet onto the back half of the ring
                    if (!isRingInFront && abs(tiltedCenteredUV.x) < _PlanetRadius)
                    {
                        ringAlpha *= 0.2;
                    }

                    ringFinalColor = half4(_RingColor.rgb * (ringBanding * 0.6 + 0.4), ringAlpha);
                }

                // 3. Neptune Sphere Rendering
                float planetRadius = _PlanetRadius;
                bool isPlanet = distToCenter < planetRadius;

                half3 finalColor = spaceBackground;
                float finalAlpha = bgAlpha;

                // Render Back Ring (behind planet)
                if (inRingBounds && !isRingInFront && !isPlanet)
                {
                    finalColor = lerp(finalColor, ringFinalColor.rgb, ringFinalColor.a);
                    finalAlpha = saturate(finalAlpha + ringFinalColor.a);
                }

                if (isPlanet)
                {
                    // Calculate 3D sphere surface
                    float N = distToCenter / planetRadius;
                    float z = sqrt(max(0.0, 1.0 - N * N));
                    float2 sphericalUV = tiltedCenteredUV / (1.0 + z);

                    // Jet Stream Atmospheric Drift
                    float rotOffset = _Time.y * _RotationSpeed * 0.1;

                    // High-latitude atmospheric banding
                    float2 bandUV = sphericalUV * float2(1.2, 5.0) * (_Turbulence * 0.4) + float2(rotOffset, 0.0);
                    float bandNoise = fbm(bandUV);

                    half3 surfaceColor = lerp(_DeepAtmosphereColor.rgb, _MidAtmosphereColor.rgb, smoothstep(-0.3, 0.4, bandNoise));
                    surfaceColor = lerp(surfaceColor, _BandColor.rgb, smoothstep(0.1, 0.6, bandNoise) * 0.5);

                    // Great Dark Spot Storm Vortex
                    float2 spotCenter = float2(-0.15 + rotOffset * 0.8, -0.1);
                    float2 spotUV = (sphericalUV - spotCenter) * float2(1.8, 1.0);
                    float distToSpot = length(spotUV);
                    float darkSpotMask = smoothstep(0.18, 0.05, distToSpot);
                    surfaceColor = lerp(surfaceColor, _DarkSpotColor.rgb, darkSpotMask * 0.8);

                    // High-altitude Methane Ice Cirrus Clouds
                    float2 cloudUV = sphericalUV * float2(3.0, 12.0) * (_Turbulence * 0.6) + float2(rotOffset * 1.6, masterTime * 0.04);
                    float cloudNoise = fbm(cloudUV);
                    float cloudMask = smoothstep(0.2, 0.7, cloudNoise);
                    surfaceColor = lerp(surfaceColor, _CloudColor.rgb, cloudMask * 0.6);

                    // Ring Shadow cast onto Neptune's surface
                    if (abs(tiltedCenteredUV.y) < (_RingOuterRadius * cosPitch) && abs(tiltedCenteredUV.y) > (_RingInnerRadius * cosPitch))
                    {
                        surfaceColor *= 0.65;
                    }

                    // Limb Darkening
                    float limbDarkening = lerp(_MinEdgeBrightness, 1.0, pow(z, 1.0 / _RimPower));
                    surfaceColor *= limbDarkening;

                    // Deep Electric Blue Atmosphere Limb Scattering
                    float animatedFresnelPower = _AtmospherePower + pulseTime * 0.5;
                    float fresnel = pow(1.0 - z, animatedFresnelPower);
                    surfaceColor += _AtmosphereColor.rgb * fresnel * 0.9;

                    finalColor = surfaceColor;
                    finalAlpha = 1.0;
                }

                // Render Front Ring (over planet)
                if (inRingBounds && isRingInFront)
                {
                    finalColor = lerp(finalColor, ringFinalColor.rgb, ringFinalColor.a);
                    finalAlpha = saturate(finalAlpha + ringFinalColor.a);
                }

                // Outer Atmosphere Glow
                if (!isPlanet)
                {
                    float atmosFringe = exp(-max(0.0, distToCenter - planetRadius) * (_AtmospherePower + pulseTime * 0.5) * 11.0);
                    float glowIntensity = atmosFringe * (0.75 + pulseTime * 0.2);
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