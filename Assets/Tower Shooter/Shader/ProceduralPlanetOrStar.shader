Shader "Custom/2D/ProceduralPlanetOrStar"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Mode)]
        [Toggle(_IS_STAR)] _IsStar ("Is Star Mode?", Float) = 0
        _Seed ("Random Seed", Range(0.0, 100.0)) = 1.0
        
        [Header(Planet Settings)]
        _BaseColor ("Base Surface Color", Color) = (0.1, 0.4, 0.8, 1)
        _DetailColor ("Accent / Land Color", Color) = (0.2, 0.7, 0.3, 1)
        _AtmosphereColor ("Atmosphere Glow", Color) = (0.4, 0.7, 1.0, 1)
        _NoiseScale ("Terrain Scale", Range(1, 50)) = 8.0
        
        [Header(Star Settings)]
        _StarCoreColor ("Star Core Color", Color) = (1.0, 0.9, 0.5, 1)
        _StarCoronaColor ("Star Corona Color", Color) = (1.0, 0.2, 0.0, 1)
        _GlowIntensity ("Glow / Emission Intensity", Range(1, 10)) = 3.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _IS_STAR

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _BaseColor;
                float4 _DetailColor;
                float4 _AtmosphereColor;
                float4 _StarCoreColor;
                float4 _StarCoronaColor;
                float _NoiseScale;
                float _GlowIntensity;
                float _Seed;
            CBUFFER_END

            // Simple 2D Pseudo-Noise Helper
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * valueNoise(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Mask by original circle sprite alpha
                float4 spriteTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Centered UV coords (-0.5 to 0.5)
                float2 centerUV = input.uv - 0.5;
                float dist = length(centerUV) * 2.0; // 0 at center, 1 at edge
                
                // Animated noise offset using time and seed
                float2 noiseUV = (input.uv + float2(_Seed, _Seed * 1.618)) * _NoiseScale;
                noiseUV += float2(_Time.y * 0.05, _Time.y * 0.02);

                float n = fbm(noiseUV);
                float4 finalColor = float4(0, 0, 0, 0);

                #if defined(_IS_STAR)
                    // --- STAR LOGIC ---
                    // Solar flares / turbulent noise blend
                    float starNoise = fbm(noiseUV * 1.5 + float2(_Time.y * 0.1, 0));
                    float3 starBody = lerp(_StarCoronaColor.rgb, _StarCoreColor.rgb, pow(1.0 - dist, 1.5) + starNoise * 0.3);
                    
                    // Center brightness boost
                    starBody *= _GlowIntensity;
                    finalColor = float4(starBody, spriteTex.a);

                #else
                    // --- PLANET LOGIC ---
                    // Pseudorandom biomes / bands
                    float3 planetBody = lerp(_BaseColor.rgb, _DetailColor.rgb, smoothstep(0.35, 0.65, n));
                    
                    // Simple fake 2D spherical shading (darkens edges slightly, brightens center)
                    float sphereShade = sqrt(saturate(1.0 - dist * dist));
                    planetBody *= saturate(sphereShade + 0.25);
                    
                    // Rim atmosphere glow
                    float rim = pow(dist, 3.0);
                    planetBody = lerp(planetBody, _AtmosphereColor.rgb, rim * _AtmosphereColor.a);

                    finalColor = float4(planetBody, spriteTex.a);
                #endif

                return finalColor * input.color;
            }
            ENDHLSL
        }
    }
}
