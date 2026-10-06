Shader "UI/CosmicPlanetWithRings"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [Header(Planet Core)]
        _BaseColor ("Planet Base Blue", Color) = (0.1, 0.45, 0.95, 1.0)
        _BandColor ("Atmosphere Band Blue", Color) = (0.05, 0.25, 0.65, 1.0)
        _BrightBandColor ("Highlight Band", Color) = (0.4, 0.75, 1.0, 1.0)
        _PlanetRadius ("Planet Radius", Range(0.1, 0.45)) = 0.28
        _BandScale ("Atmospheric Band Frequency", Range(1.0, 50.0)) = 18.0

        [Header(Planet Lighting Atmosphere)]
        _AtmosphereColor ("Atmosphere Edge Glow", Color) = (0.3, 0.6, 1.0, 1.0)
        _AtmospherePower ("Atmosphere Sharpness", Range(0.5, 10.0)) = 3.0
        _LightDirection ("Light Angle (Degrees)", Range(0, 360)) = 135.0

        [Header(Rings System)]
        _RingInnerRadius ("Ring Inner Radius", Range(0.2, 0.6)) = 0.32
        _RingOuterRadius ("Ring Outer Radius", Range(0.3, 0.8)) = 0.48
        _RingColor ("Ring Base Color", Color) = (0.2, 0.45, 0.75, 0.8)
        _RingTilt ("Ring Tilt Angle", Range(-45, 45)) = -18.0
        _RingYScale ("Ring Perspective Flattening", Range(0.1, 0.6)) = 0.28

        [Header(Background)]
        _SpaceColor ("Space Dark Base", Color) = (0.01, 0.02, 0.05, 1.0)
        _DustColor ("Cosmic Dust Streak", Color) = (0.05, 0.15, 0.3, 1.0)

        [Header(Animation Controls)]
        _GlobalSpeed ("Master Animation Speed", Range(0.0, 2.0)) = 0.2
        _AtmosphereRotationSpeed ("Atmosphere Drift Speed", Range(-2.0, 2.0)) = 0.4
        _RingSpinSpeed ("Ring Rotation Speed", Range(-2.0, 2.0)) = 0.6
        _CloudRibbonSpeed ("Cloud Ribbon Speed", Range(0.0, 3.0)) = 1.2
        _PulseSpeed ("Atmosphere Pulse Speed", Range(0.0, 5.0)) = 1.5

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
                float4 _BandColor;
                float4 _BrightBandColor;
                float4 _AtmosphereColor;
                float4 _RingColor;
                float4 _SpaceColor;
                float4 _DustColor;

                float _PlanetRadius;
                float _BandScale;
                float _AtmospherePower;
                float _LightDirection;

                float _RingInnerRadius;
                float _RingOuterRadius;
                float _RingTilt;
                float _RingYScale;

                float _GlobalSpeed;
                float _AtmosphereRotationSpeed;
                float _RingSpinSpeed;
                float _CloudRibbonSpeed;
                float _PulseSpeed;

                float4 _ClipRect;
            CBUFFER_END

            float hash12(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash12(i);
                float b = hash12(i + float2(1.0, 0.0));
                float c = hash12(i + float2(0.0, 1.0));
                float d = hash12(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 uv)
            {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    v += a * noise(uv);
                    uv *= 2.1;
                    a *= 0.5;
                }
                return v;
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
                
                // --- Animation Timers ---
                float t = _Time.y * _GlobalSpeed;
                float atmoTime = t * _AtmosphereRotationSpeed;
                float ringTime = t * _RingSpinSpeed;
                float ribbonTime = t * _CloudRibbonSpeed;
                float pulseTime = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;

                // Light directional vector
                float lightRad = radians(_LightDirection);
                float2 lightDir = float2(cos(lightRad), sin(lightRad));

                // 1. Background (Smooth Cosmic Nebular Dust Streaks - Stars Removed)
                float2 dustUV = rotateUV(centeredUV, -20.0);
                dustUV.y *= 3.0;
                float dustAnimation = fbm(dustUV * 2.5 + float2(t * 0.15, sin(t * 0.1) * 0.05));
                half3 spaceBackground = _SpaceColor.rgb + _DustColor.rgb * dustAnimation * 0.7;

                // 2. Transformed Coordinates for Ring Plane
                float2 ringUV = rotateUV(centeredUV, _RingTilt);
                ringUV.y /= max(_RingYScale, 0.001);
                float ringDist = length(ringUV);

                // Ring Mask
                float ringMask = step(_RingInnerRadius, ringDist) * step(ringDist, _RingOuterRadius);

                // Animated Ring Density & Orbital Ripples
                float ringAngle = atan2(ringUV.y, ringUV.x);
                float ringOrbitNoise = fbm(float2(ringDist * 40.0, ringAngle * 3.0 - ringTime));
                
                float ringPattern = sin(ringDist * 180.0 + ringOrbitNoise * 2.0) * 0.5 + 0.5;
                ringPattern *= sin(ringDist * 60.0) * 0.5 + 0.5;
                ringPattern = pow(ringPattern, 0.7);

                half4 ringColor = half4(_RingColor.rgb * (0.6 + 0.5 * ringPattern + ringOrbitNoise * 0.2), 
                                        ringMask * _RingColor.a * (0.4 + 0.6 * ringPattern));

                // 3. Planet Sphere Render
                float distToCenter = length(centeredUV);
                float planetRadius = _PlanetRadius;
                bool isPlanet = distToCenter < planetRadius;

                half3 finalColor = spaceBackground;

                // Determine Front vs Back Ring Segments
                bool isRingInFront = (ringUV.y < 0.0) && (ringMask > 0.0);

                if (isPlanet)
                {
                    // 3D Sphere projection
                    float N = distToCenter / planetRadius;
                    float z = sqrt(max(0.0, 1.0 - N * N));
                    float3 normal = float3(centeredUV / planetRadius, z);

                    // Latitude & Longitude UV mapping for atmospheric bands
                    float2 sphereUV = rotateUV(centeredUV, _RingTilt);
                    float latitude = (sphereUV.y / planetRadius) * 0.5 + 0.5;
                    float longitude = (sphereUV.x / planetRadius) * 0.5 + 0.5;

                    // --- Animated Atmospheric Features ---
                    // Differential rotation: Bands drift faster near equator
                    float equatorFactor = sin(latitude * 3.1415);
                    float bandSpeed = atmoTime * (0.8 + 0.4 * equatorFactor);

                    // Fluid Turbulence Noise
                    float bandNoise = fbm(float2(longitude * 3.0 + bandSpeed, latitude * _BandScale));
                    float bandPattern = sin(latitude * _BandScale * 3.1415 + bandNoise * 2.5) * 0.5 + 0.5;

                    // Moving High-altitude Cloud Ribbons/Storm Strips
                    float ribbonNoise = fbm(float2(longitude * 5.0 - ribbonTime, latitude * 8.0 + atmoTime * 0.5));
                    float cloudRibbons = smoothstep(0.55, 0.75, ribbonNoise);

                    // Combine Planet Base Colors
                    half3 planetSurface = lerp(_BaseColor.rgb, _BandColor.rgb, bandPattern);
                    planetSurface = lerp(planetSurface, _BrightBandColor.rgb, pow(bandPattern, 3.0) * 0.5);
                    planetSurface = lerp(planetSurface, _BrightBandColor.rgb * 1.3, cloudRibbons * 0.6); // Animated bright ribbons

                    // Directional Lighting
                    float NdotL = max(0.0, dot(normal.xy, lightDir) * 0.7 + normal.z * 0.7);
                    planetSurface *= (0.15 + 0.85 * NdotL);

                    // Animated Ring Shadow cast on Planet Surface
                    float shadowOffset = dot(lightDir, float2(0.0, 0.1));
                    float shadowPos = sphereUV.y + shadowOffset;
                    float shadowMask = smoothstep(_RingInnerRadius - 0.05, _RingInnerRadius, abs(shadowPos)) *
                                       (1.0 - smoothstep(_RingOuterRadius - 0.05, _RingOuterRadius, abs(shadowPos)));
                    planetSurface *= lerp(1.0, 0.35, shadowMask * step(0.0, shadowPos));

                    // Dynamic Pulsing Atmosphere Limb Glow & Fresnel
                    float animatedFresnelPower = _AtmospherePower + pulseTime * 0.5;
                    float fresnel = pow(1.0 - z, animatedFresnelPower);
                    half3 dynamicAtmosphereColor = _AtmosphereColor.rgb * (1.0 + pulseTime * 0.25);
                    planetSurface += dynamicAtmosphereColor * fresnel * 0.75;

                    finalColor = planetSurface;
                }

                // Render Rings
                if (!isPlanet && ringMask > 0.0)
                {
                    float ringLighting = max(0.3, dot(normalize(ringUV), lightDir) * 0.5 + 0.5);
                    finalColor = lerp(finalColor, ringColor.rgb * ringLighting, ringColor.a);
                }
                else if (isPlanet && isRingInFront)
                {
                    float ringLighting = max(0.4, dot(normalize(ringUV), lightDir) * 0.5 + 0.5);
                    finalColor = lerp(finalColor, ringColor.rgb * ringLighting, ringColor.a * 0.85);
                }

                // Outer Pulsing Atmosphere Edge Glow
                if (!isPlanet)
                {
                    float outerAtmosphere = exp(-max(0.0, distToCenter - planetRadius) * (_AtmospherePower + pulseTime * 0.5) * 15.0);
                    finalColor += _AtmosphereColor.rgb * outerAtmosphere * (0.5 + pulseTime * 0.15);
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
