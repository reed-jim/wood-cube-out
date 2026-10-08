Shader "UI/URP/PlanetGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Planet Colors)]
        _OceanColor ("Ocean Color", Color) = (0.05, 0.2, 0.5, 1)
        _LandColor ("Land Color", Color) = (0.1, 0.6, 0.2, 1)
        _AtmosphereColor ("Atmosphere Color", Color) = (0.2, 0.7, 1.0, 1)

        [Header(Planet Settings)]
        _PlanetRadius ("Planet Radius (0-0.5)", Range(0.1, 0.48)) = 0.38
        _RotationSpeed ("Rotation Speed", Range(0.0, 2.0)) = 0.2
        _ContinentScale ("Continent Detail Scale", Range(1.0, 10.0)) = 3.5

        [Header(Glow Border)]
        _GlowThickness ("Glow Thickness", Range(0.01, 0.2)) = 0.08
        _GlowPower ("Glow Falloff Power", Range(1.0, 8.0)) = 3.0

        [Header(Lighting)]
        _LightDir ("Light Direction (XY)", Vector) = (0.5, 0.5, 1.0, 0.0)

        // UI Masking Properties
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma shader_feature_local _USEUIALPHACLIP_ON

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

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OceanColor;
                float4 _LandColor;
                float4 _AtmosphereColor;
                float4 _ClipRect;
                float4 _LightDir;
                float _PlanetRadius;
                float _RotationSpeed;
                float _ContinentScale;
                float _GlowThickness;
                float _GlowPower;
            CBUFFER_END

            // 3D Procedural Hash Noise for landmasses
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float noise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(lerp(hash(i + float3(0,0,0)), hash(i + float3(1,0,0)), f.x),
                                 lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x), f.y),
                            lerp(lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
                                 lerp(hash(i + float3(0,1,1)), hash(i + float3(1,1,1)), f.x), f.y), f.z);
            }

            float fbm(float3 p)
            {
                float val = 0.0;
                float amp = 0.5;
                for(int i = 0; i < 4; i++)
                {
                    val += amp * noise(p);
                    p *= 2.02;
                    amp *= 0.5;
                }
                return val;
            }

            // UI ClipRect check helper
            float UnityGet2DClipping(float2 position, float4 clipRect)
            {
                float2 inside = step(clipRect.xy, position) * step(position, clipRect.zw);
                return inside.x * inside.y;
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

            half4 frag(Varyings input) : SV_Target
            {
                // Center UV from (0..1) to (-0.5..0.5)
                float2 p = input.uv - float2(0.5, 0.5);
                float dist = length(p);

                float r = _PlanetRadius;
                float glowOuter = r + _GlowThickness;

                half4 finalColor = half4(0, 0, 0, 0);

                if (dist < r)
                {
                    // --- PLANET SURFACE (3D Raymarch onto Hemisphere) ---
                    float z = sqrt(r * r - dist * dist);
                    float3 sphereNormal = normalize(float3(p.x, p.y, z));

                    // Rotate sphere over time
                    float angle = _Time.y * _RotationSpeed;
                    float cosA = cos(angle);
                    float sinA = sin(angle);
                    float3 rotNormal = float3(
                        sphereNormal.x * cosA - sphereNormal.z * sinA,
                        sphereNormal.y,
                        sphereNormal.x * sinA + sphereNormal.z * cosA
                    );

                    // Map Procedural Noise for continents
                    float n = fbm(rotNormal * _ContinentScale);
                    float landMask = smoothstep(0.48, 0.52, n);

                    float3 baseSurface = lerp(_OceanColor.rgb, _LandColor.rgb, landMask);

                    // Directional lighting
                    float3 lightDir = normalize(_LightDir.xyz);
                    float diff = max(0.15, dot(sphereNormal, lightDir)); // 0.15 ambient light

                    // Surface Rim Reflection
                    float rim = 1.0 - max(0.0, sphereNormal.z);
                    float3 atmosphereRim = _AtmosphereColor.rgb * pow(rim, 2.5);

                    float3 planetRGB = baseSurface * diff + atmosphereRim;
                    finalColor = half4(planetRGB, 1.0);
                }
                else if (dist < glowOuter)
                {
                    // --- OUTER ATMOSPHERE GLOW BORDER ---
                    float glowFactor = (dist - r) / _GlowThickness; // 0 at sphere, 1 at edge
                    float alphaGlow = pow(1.0 - glowFactor, _GlowPower);

                    finalColor = half4(_AtmosphereColor.rgb, alphaGlow * _AtmosphereColor.a);
                }

                // Apply UI Canvas Tint (including CanvasGroup alpha)
                finalColor *= input.color;

                // Handle UI RectMask2D clipping
                #ifdef UNITY_UI_ALPHACLIP
                    finalColor.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                    clip(finalColor.a - 0.001);
                #else
                    finalColor.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                return finalColor;
            }
            ENDHLSL
        }
    }
}