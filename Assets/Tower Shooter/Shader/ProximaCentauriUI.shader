Shader "UI/ProximaCentauri"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [Header(Star Colors)]
        _BaseColor ("Base Surface Color", Color) = (0.9, 0.25, 0.05, 1.0)
        _HotSpotColor ("Flare/Hotspot Color", Color) = (1.0, 0.75, 0.2, 1.0)
        _GlowColor ("Corona Glow Color", Color) = (0.8, 0.15, 0.02, 1.0)

        [Header(Surface Motion)]
        _Speed ("Animation Speed", Range(0.05, 1.0)) = 0.2
        _Turbulence ("Granulation Turbulence", Range(1.0, 10.0)) = 4.0

        [Header(Atmosphere and Flares)]
        _RimPower ("Limb Darkening Sharpness", Range(1.0, 8.0)) = 3.5
        _FlareIntensity ("Prominence Intensity", Range(0.1, 3.0)) = 1.2

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
        Blend SrcAlpha One // Additive blending for atmospheric bloom effect
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _HotSpotColor;
                float4 _GlowColor;
                float _Speed;
                float _Turbulence;
                float _RimPower;
                float _FlareIntensity;
            CBUFFER_END

            // Simplex-style noise helper functions
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

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Re-center UVs to (-0.5 to 0.5)
                float2 uv = input.uv - 0.5;
                float dist = length(uv);

                float t = _Time.y * _Speed;

                // 1. Sphere spherical mapping distortion
                float radius = 0.35;
                float N = dist / radius;

                // Outer Atmosphere (Corona Glow)
                float glow = exp(-dist * 8.0) * 0.8;

                if (N > 1.0)
                {
                    // Solar Flares / Prominence along rim
                    float angle = atan2(uv.y, uv.x);
                    float flareNoise = fbm(float2(angle * 3.0, t * 1.5)) * _FlareIntensity;
                    float flare = max(0.0, (1.0 - (dist - radius) * 8.0)) * flareNoise;

                    half4 col = _GlowColor * (glow + flare);
                    return col * input.color;
                }

                // 2. Surface Granulation (FBM Noise)
                float z = sqrt(1.0 - N * N);
                float2 sphericalUV = uv / (1.0 + z); // Stereographic projection
                
                float noiseVal = fbm(sphericalUV * _Turbulence + float2(t * 0.2, t * 0.1));
                float noiseVal2 = fbm(sphericalUV * (_Turbulence * 1.8) - float2(t * 0.15, -t * 0.2));

                float combinedNoise = saturate((noiseVal + noiseVal2) * 0.7 + 0.3);

                // 3. Color Blending
                half3 surfaceColor = lerp(_BaseColor.rgb, _HotSpotColor.rgb, pow(combinedNoise, 2.0));

                // 4. Limb Darkening (Denser atmosphere look towards the edges)
                float limbDarkening = pow(z, 1.0 / _RimPower);
                surfaceColor *= limbDarkening;

                // Add inner core glow
                surfaceColor += _GlowColor.rgb * glow * 0.5;

                // Alpha fade out edge
                float alpha = smoothstep(radius, radius - 0.01, dist);

                return half4(surfaceColor, alpha) * input.color;
            }
            ENDHLSL
        }
    }
}