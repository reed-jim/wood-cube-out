Shader "TextMeshPro/Distance Field (Augment Levels Immersive)"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0
        
        [KeywordEnum(Orbital, Planetary, Stellar, Galactic, Cosmic, Dimensional, Transcendent)] 
        _Level ("Augment Level Override", Float) = 0
        
        _TimeScale ("Effect Speed", Float) = 1.0
        _GlowIntensity ("Glow Intensity", Range(0.5, 5.0)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Pass
        {
            Name "TMP_Augment_Immersive"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float2 uv2          : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float2 localPos     : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _FaceColor;
                float _FaceDilate;
                float _Level;
                float _TimeScale;
                float _GlowIntensity;
            CBUFFER_END

            // --- Procedural Utilities ---
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float val = 0.0;
                float amp = 0.5;
                for (int i = 0; i < 3; i++)
                {
                    val += amp * noise2D(p);
                    p *= 2.02;
                    amp *= 0.5;
                }
                return val;
            }

            // --- Vertex Shader: Real-Time Level Displacement ---
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 posOS = input.positionOS;
                float t = _Time.y * _TimeScale;
                int level = (int)_Level;

                // Vertex motion scales up with higher levels
                if (level == 2) // Stellar: Radiant Solar Pulsation
                {
                    float pulse = sin(t * 3.0 + posOS.x * 2.0) * 0.04;
                    posOS.xy += normalize(posOS.xy + 0.001) * pulse;
                }
                else if (level == 3) // Galactic: Wave Motion
                {
                    posOS.y += sin(t * 2.5 + posOS.x * 1.5) * 0.05;
                }
                else if (level == 4) // Cosmic: Floating Zero-G Motion
                {
                    posOS.x += sin(t * 1.2 + posOS.y * 2.0) * 0.03;
                    posOS.y += cos(t * 1.5 + posOS.x * 2.0) * 0.04;
                }
                else if (level == 5) // Dimensional: Violent Spatial Glitch Jitter
                {
                    float jitterTrigger = step(0.85, frac(sin(t * 15.0) * 43758.54));
                    float2 jitter = float2(
                        hash21(float2(t, posOS.y)) - 0.5,
                        hash21(float2(posOS.x, t)) - 0.5
                    ) * 0.12 * jitterTrigger;
                    posOS.xy += jitter;
                }
                else if (level == 6) // Transcendent: Celestial Ascending Wave
                {
                    float floatWave = sin(t * 3.0 + posOS.x * 3.0) * 0.06;
                    posOS.y += floatWave;
                }

                output.positionCS = TransformObjectToHClip(posOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                output.localPos = posOS.xy;
                return output;
            }

            // --- Fragment Shader: Immersive Tier Renderers ---
            half4 Frag(Varyings input) : SV_Target
            {
                // 1. Core SDF distance sampling
                float sdf = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                float scale = fwidth(sdf);
                
                // Solid glyph core alpha
                float coreAlpha = smoothstep(0.5 - scale - _FaceDilate, 0.5 + scale + _FaceDilate, sdf);
                
                // Extended Outer Glow / Halo alpha pass (Smooth SDF expansion outside the character)
                float glowAlpha = smoothstep(0.1, 0.5, sdf);

                float t = _Time.y * _TimeScale;
                float2 pos = input.localPos;
                int level = (int)_Level;

                float3 coreColor = float3(1, 1, 1);
                float3 glowColor = float3(0, 0, 0);
                float glowStrength = 1.0;

                // --- LEVEL EFFECT CALCULATIONS ---
                switch (level)
                {
                    case 0: // ORBITAL (Cyan satellite laser scanner & smooth pulse)
                    {
                        float scanline = smoothstep(0.02, 0.0, abs(frac(pos.y * 2.0 - t * 0.8) - 0.5) - 0.01);
                        float pulse = sin(t * 2.0) * 0.2 + 0.8;
                        
                        coreColor = lerp(float3(0.05, 0.55, 0.95), float3(0.4, 0.9, 1.0), scanline + pulse * 0.3);
                        glowColor = float3(0.0, 0.6, 1.0);
                        glowStrength = 1.2;
                        break;
                    }

                    case 1: // PLANETARY (Layered Atmospheric Rim & Continental Gradient)
                    {
                        float atmRim = pow(1.0 - saturate(abs(sdf - 0.5) * 4.0), 2.0); // Edge highlight
                        float land = fbm(pos * 4.0 + float2(t * 0.1, 0.0));
                        
                        float3 deepOcean = float3(0.02, 0.2, 0.6);
                        float3 atmosphere = float3(0.1, 0.85, 0.5);
                        
                        coreColor = lerp(deepOcean, atmosphere, land);
                        coreColor += float3(0.3, 0.9, 0.6) * atmRim * 1.5; // Glowing atmosphere rim
                        glowColor = float3(0.0, 0.8, 0.5);
                        glowStrength = 1.4;
                        break;
                    }

                    case 2: // STELLAR (Radiant Solar Flame, Sunspots & Plasma Glow)
                    {
                        float plasma = fbm(pos * 6.0 - float2(0, t * 2.5));
                        float sunspot = smoothstep(0.3, 0.6, noise2D(pos * 12.0 + t));
                        
                        float3 darkCore = float3(0.9, 0.15, 0.0);
                        float3 brightCorona = float3(1.0, 0.95, 0.3);
                        
                        coreColor = lerp(darkCore, brightCorona, plasma) - (sunspot * 0.2);
                        glowColor = float3(1.0, 0.4, 0.0);
                        glowStrength = 2.0 + plasma * 0.8;
                        break;
                    }

                    case 3: // GALACTIC (Rotating Cosmic Core, Accretion Spiral & Deep Violet)
                    {
                        float dist = length(pos);
                        float angle = atan2(pos.y, pos.x) - t * 1.2;
                        float spiral = sin(angle * 3.0 + dist * 12.0);
                        
                        float3 deepSpace = float3(0.15, 0.02, 0.35);
                        float3 nebulaCyan = float3(0.0, 0.95, 0.9);
                        float3 starCore = float3(0.95, 0.2, 0.7);
                        
                        coreColor = lerp(deepSpace, nebulaCyan, spiral * 0.5 + 0.5);
                        coreColor += starCore * (1.0 - saturate(dist * 0.8));
                        glowColor = float3(0.6, 0.1, 0.9);
                        glowStrength = 2.2;
                        break;
                    }

                    case 4: // COSMIC (Deep Void, Twinkling Star Field & Event Horizon Outline)
                    {
                        float starGrid = pow(noise2D(pos * 30.0 + float2(t * 0.1, t * 0.15)), 12.0) * 25.0;
                        float voidNebula = fbm(pos * 2.5 + t * 0.2);
                        
                        float3 voidColor = float3(0.02, 0.01, 0.08);
                        float3 nebulaGlow = float3(0.35, 0.05, 0.55);
                        float3 starFlash = float3(0.95, 0.95, 1.0);
                        
                        coreColor = lerp(voidColor, nebulaGlow, voidNebula) + (starFlash * starGrid);
                        glowColor = float3(0.4, 0.1, 0.8);
                        glowStrength = 2.5;
                        break;
                    }

                    case 5: // DIMENSIONAL (Chromatic Aberration, Cyber RGB Split & Glitch Wave)
                    {
                        float glitch = step(0.88, frac(sin(floor(t * 16.0) + pos.y * 15.0) * 43758.54));
                        float splitOffset = 0.15 * glitch;
                        
                        float r = fbm(pos * 8.0 + float2(splitOffset, 0));
                        float g = fbm(pos * 8.0);
                        float b = fbm(pos * 8.0 - float2(splitOffset, 0));
                        
                        coreColor = float3(r * 1.5, g * 0.8, b * 2.0);
                        coreColor += float3(1, 1, 1) * glitch * 0.6; // High intensity white flash during glitch
                        
                        glowColor = float3(0.9, 0.0, 0.9);
                        glowStrength = 2.8 + glitch * 1.5;
                        break;
                    }

                    case 6: // TRANSCENDENT (Divine Prismatic Rainbow, Solar Gold & Hyper-Emission)
                    {
                        float phase = pos.x * 2.5 + pos.y * 2.5 + t * 3.0;
                        float3 spectralRainbow = 0.5 + 0.5 * cos(phase + float3(0.0, 2.05, 4.1));
                        
                        float goldPulse = sin(t * 6.0) * 0.5 + 0.5;
                        float3 divineGold = float3(1.0, 0.88, 0.45);
                        
                        coreColor = lerp(spectralRainbow, divineGold, 0.45) * (1.8 + goldPulse * 0.6);
                        glowColor = lerp(spectralRainbow, divineGold, 0.5);
                        glowStrength = 3.5;
                        break;
                    }
                }

                // --- Composite Final Render ---
                half4 baseColor = input.color * _FaceColor;
                
                // Combine solid inner character body with expanded outer glow halo
                float3 finalRGB = (coreColor * coreAlpha) + (glowColor * (glowAlpha - coreAlpha) * _GlowIntensity);
                float finalAlpha = saturate(coreAlpha + (glowAlpha * 0.6)) * baseColor.a;

                return half4(finalRGB * glowStrength, finalAlpha);
            }
            ENDHLSL
        }
    }
}