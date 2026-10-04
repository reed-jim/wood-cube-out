Shader "Custom/URP/LoLSpaceButtonV2"
{
    Properties
    {
        [MainTexture] _MainTex ("Base Sprite / Texture", 2D) = "white" {}
        
        [Header(Space Colors Palette)]
        _BodyColor ("Center Space Void Dark", Color) = (0.02, 0.01, 0.05, 1)
        _BorderColor ("Celestial Gold Rim", Color) = (0.95, 0.82, 0.45, 1)
        _GlowColor ("Electric Neon Purple Glow", Color) = (0.75, 0.25, 1.0, 1)
        _GlowSecondary ("Deep Cosmic Indigo Energy", Color) = (0.15, 0.05, 0.5, 1)
        
        [Header(Rim Border Settings)]
        _BorderThickness ("Border Thickness", Range(0.005, 0.15)) = 0.04
        _BevelHardness ("Bevel Specular Sharpness", Range(1.0, 16.0)) = 6.0
        
        [Header(Energy Streaks Animation)]
        _StreakSpeed ("Energy Flow Speed", Range(0.1, 5.0)) = 1.2
        _StreakDensity ("Streak Density", Range(5.0, 50.0)) = 20.0
        _StreakIntensity ("Streak Brightness", Range(0.0, 2.0)) = 0.6
        
        [Header(Sweeping Light Reflection)]
        _ShineSpeed ("Sweep Frequency", Range(0.1, 3.0)) = 0.8
        _ShineWidth ("Sweep Width", Range(0.01, 0.4)) = 0.08
        _ShineAngle ("Sweep Angle Offset", Range(-1.0, 1.0)) = 0.35
        
        [Header(Interactive States)]
        _Hover ("Hover Interpolator", Range(0.0, 1.0)) = 0.0
        _Pressed ("Press Interpolator", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "LoLSpaceButtonV2Pass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BodyColor;
                float4 _BorderColor;
                float4 _GlowColor;
                float4 _GlowSecondary;
                float _BorderThickness;
                float _BevelHardness;
                float _StreakSpeed;
                float _StreakDensity;
                float _StreakIntensity;
                float _ShineSpeed;
                float _ShineWidth;
                float _ShineAngle;
                float _Hover;
                float _Pressed;
            CBUFFER_END

            // Pseudo-random noise for energy streaks
            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float ValueNoise1D(float x)
            {
                float i = floor(x);
                float f = frac(x);
                float u = f * f * (3.0 - 2.0 * f);
                return lerp(Hash11(i), Hash11(i + 1.0), u);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color;

                // 1. Edge Distance Field (Frame Border)
                float2 distToEdge2D = min(uv, 1.0 - uv);
                float distToEdge = min(distToEdge2D.x, distToEdge2D.y);
                float isBorder = step(distToEdge, _BorderThickness);

                // 2. Base Body Gradient & Inner Vignette
                float centerDist = length(uv - 0.5);
                float3 baseBody = lerp(_BodyColor.rgb * 1.6, _BodyColor.rgb * 0.3, centerDist * 1.5);

                // 3. Animated Horizontal Cosmic Energy Streaks
                float time = _Time.y;
                float streakUV = (uv.y + uv.x * 0.1) * _StreakDensity;
                float streakNoise = ValueNoise1D(streakUV + time * _StreakSpeed);
                float horizontalMask = smoothstep(0.4, 0.0, abs(uv.y - 0.5));
                
                float energyFlow = pow(streakNoise, 3.0) * _StreakIntensity * horizontalMask;
                float3 streakColor = lerp(_GlowSecondary.rgb, _GlowColor.rgb, streakNoise);
                baseBody += streakColor * energyFlow;

                // 4. Multi-frequency Breathing Inner Glow (Indigo to Neon Purple)
                float glowDepth = saturate((_BorderThickness * 3.5 - distToEdge) / (_BorderThickness * 3.5));
                glowDepth = pow(glowDepth, 2.5);

                float slowPulse = sin(time * 2.0) * 0.2 + 0.8;
                float fastShimmer = sin(time * 8.0 + uv.x * 10.0) * 0.15 + 0.85;
                float pulseCombined = lerp(slowPulse, fastShimmer, _Hover);

                float3 innerGlow = lerp(_GlowSecondary.rgb, _GlowColor.rgb, glowDepth) * glowDepth * pulseCombined * 1.5;
                baseBody += innerGlow * (1.0 + _Hover * 1.2);

                // 5. Metallic Celestial Gold Border & 3D Bevel Highlight
                float borderNormalized = saturate(distToEdge / _BorderThickness);
                float bevelProfile = sin(borderNormalized * 3.14159);
                float bevelSpec = pow(bevelProfile, _BevelHardness);
                
                float borderLight = saturate((1.0 - uv.y) * 0.6 + (1.0 - uv.x) * 0.4);
                float3 goldBorder = _BorderColor.rgb * (0.4 + 0.8 * borderLight + bevelSpec * 0.9);

                // Hover & Press reaction
                goldBorder += _GlowColor.rgb * _Hover * 0.3;
                goldBorder *= (1.0 - _Pressed * 0.3);

                // Combine Frame & Center Body
                float3 finalColor = lerp(baseBody, goldBorder, isBorder);

                // 6. Sweeping Starlight Reflection Pass
                float sweepPos = frac(time * _ShineSpeed) * 3.0 - 1.0;
                float sweepCoord = uv.x + uv.y * _ShineAngle;
                
                float beamDist = abs(sweepCoord - sweepPos);
                float sharpBeam = smoothstep(_ShineWidth, 0.0, beamDist);
                float softBloom = smoothstep(_ShineWidth * 3.0, 0.0, beamDist) * 0.4;
                
                float totalSweep = (sharpBeam + softBloom) * (0.4 + _Hover * 0.8);
                float3 sweepColor = lerp(_BorderColor.rgb, float3(1.0, 1.0, 1.0), sharpBeam);
                
                finalColor += sweepColor * totalSweep;

                // 7. Interactive State Modifiers
                finalColor += _Hover * 0.08;
                finalColor *= (1.0 - _Pressed * 0.2);

                return float4(finalColor, texColor.a);
            }
            ENDHLSL
        }
    }
}
