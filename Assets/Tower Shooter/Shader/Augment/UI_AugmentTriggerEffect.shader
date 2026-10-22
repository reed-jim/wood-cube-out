Shader "UI/Custom/ModularAugmentShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Core Tint Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _RimColor ("Energy Glow Color", Color) = (1.0, 0.3, 0.9, 1.0)
        _QualityColor ("Augment Quality Color", Color) = (1.0, 1.0, 1.0, 1.0)
        
        [Header(Shared Progress Control)]
        _TriggerProgress ("Trigger Progress (0 - 1)", Range(0.0, 1.0)) = 0.0

        [Header(Effect Toggles)]
        _Effect1Active ("Effect 1: Hyper-Plasma Shockwave", Float) = 0.0
        _Effect2Active ("Effect 2: Quantum Singularity Rift", Float) = 0.0
        _Effect3Active ("Effect 3: Tachyon Overload Surge", Float) = 0.0
        _Effect4Active ("Effect 4: Nanite Matrix Shock", Float) = 0.0
        _Effect5Active ("Effect 5: Supernova Core Eruption", Float) = 0.0

        // Stencil / UI Canvas properties
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_ModularAugmentShader"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float4 _QualityColor;
                float _TriggerProgress;
                float _Effect1Active;
                float _Effect2Active;
                float _Effect3Active;
                float _Effect4Active;
                float _Effect5Active;
            CBUFFER_END

            #define PI 3.14159265359

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            // --- AAA PROCEDURAL NOISE HELPERS ---
            float Hash(float2 p) 
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float Noise(float2 p) 
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i + float2(0.0, 0.0)), Hash(i + float2(1.0, 0.0)), f.x),
                            lerp(Hash(i + float2(0.0, 1.0)), Hash(i + float2(1.0, 1.0)), f.x), f.y);
            }

            // Fractal Brownian Motion for rich organic turbulence
            float Fbm(float2 p) 
            {
                float f = 0.0;
                float w = 0.5;
                for (int i = 0; i < 3; i++) 
                {
                    f += w * Noise(p);
                    p *= 2.0;
                    w *= 0.5;
                }
                return f;
            }

            float GetSmoothEnvelope(float prog)
            {
                return smoothstep(0.0, 0.1, prog) * smoothstep(1.0, 0.75, prog);
            }

            // =========================================================================
            // 5 AAA CINEMATIC TOP-DOWN EFFECTS
            // =========================================================================

            // EFFECT 1: Hyper-Plasma Shockwave (Heavy atmospheric blast wave with chromatic distortion and burning thermal embers)
            void GetEffect1_HyperPlasma(float prog, float2 st, float dist, float angle, out float3 col, out float alpha)
            {
                float envelope = GetSmoothEnvelope(prog);
                float radius = prog * 0.55;
                
                // Organic noise warp on distance field
                float noiseWarp = Fbm(st * 6.0 + prog * 4.0) * 0.08;
                float distortedDist = dist + noiseWarp;
                
                // Shockwave leading edge with RGB chromatic splitting
                float shockR = smoothstep(0.04, 0.0, abs(distortedDist - radius));
                float shockG = smoothstep(0.04, 0.0, abs(distortedDist - (radius - 0.02)));
                float shockB = smoothstep(0.04, 0.0, abs(distortedDist - (radius + 0.02)));
                
                // Trailing energy embers & smoke wake
                float wake = smoothstep(radius, radius - 0.3, dist) * Fbm(st * 12.0 - prog * 6.0) * (1.0 - prog);
                float coreFlash = (1.0 - smoothstep(0.0, 0.25, dist)) * pow(1.0 - prog, 2.0) * 5.0;

                float3 baseTint = lerp(_QualityColor.rgb, _RimColor.rgb, 0.5);
                col = float3(6.0, 6.0, 6.0) * coreFlash 
                    + (baseTint * 4.0 + float3(2.0, 0.5, 4.0)) * wake * 3.0
                    + float3(5.0, 1.5, 1.0) * shockR 
                    + float3(1.0, 4.0, 5.0) * shockG 
                    + float3(1.0, 2.0, 6.0) * shockB;

                alpha = saturate(shockR + shockG + shockB + wake + coreFlash * 0.5) * envelope;
            }

            // EFFECT 2: Quantum Singularity Rift (Warping gravitational space-tears with intense particle suction arcs)
            void GetEffect2_QuantumRift(float prog, float2 st, float dist, float angle, out float3 col, out float alpha)
            {
                float envelope = GetSmoothEnvelope(prog);
                
                // Dynamic spiral domain distortion
                float spiralAngle = angle + (1.0 / (dist + 0.1)) * 0.8 - prog * 5.0;
                float plasmaArms = pow(saturate(sin(spiralAngle * 4.0 + Fbm(st * 8.0) * 3.0) * 0.5 + 0.5), 5.0);
                
                // Inner event horizon void and glowing rim
                float horizonR = 0.2 * sin(prog * PI);
                float eventRing = smoothstep(0.04, 0.0, abs(dist - horizonR));
                float suctionCore = (1.0 - smoothstep(0.0, 0.35, dist)) * plasmaArms * (prog * 3.0);

                float3 tint = lerp(_Color.rgb, float3(3.0, 1.0, 5.0), 0.7);
                col = tint * plasmaArms * 5.0 + float3(6.0, 4.0, 6.0) * eventRing * 4.0 + float3(4.0, 2.0, 6.0) * suctionCore * 3.0;
                
                alpha = saturate(plasmaArms * 0.8 + eventRing + suctionCore) * envelope;
            }

            // EFFECT 3: Tachyon Overload Surge (Multi-layered sci-fi geometric HUD pulse matrix with sharp laser sweeps)
            void GetEffect3_TachyonSurge(float prog, float2 st, float dist, float angle, out float3 col, out float alpha)
            {
                float envelope = GetSmoothEnvelope(prog);
                
                // Rotating precision geometric rings
                float ring1 = smoothstep(0.025, 0.0, abs(dist - (prog * 0.45)));
                float ring2 = smoothstep(0.015, 0.0, abs(dist - (prog * 0.3 - 0.1))) * 0.7;
                
                // High-tech angular radar sweep blades
                float sweepAngle = angle - prog * 15.0;
                float blades = pow(saturate(cos(sweepAngle * 6.0) * 0.5 + 0.5), 10.0) * smoothstep(0.48, 0.1, dist);
                
                // Hexagonal data noise overlay
                float hexGrid = smoothstep(0.15, 0.0, abs(cos(st.x * 20.0) * cos(st.y * 20.0)));

                float3 tint = lerp(_QualityColor.rgb, _Color.rgb, 0.5);
                col = (tint * 5.0 + float3(1.0, 4.0, 5.0)) * (ring1 + ring2 * 2.0 + blades * 1.5 + hexGrid * 0.4 * (1.0 - prog));
                alpha = saturate(ring1 + ring2 + blades + hexGrid * 0.3) * envelope;
            }

            // EFFECT 4: Nanite Matrix Shock (Swarming digital micro-particles erupting and dispersing outward)
            void GetEffect4_NaniteShock(float prog, float2 st, float dist, float angle, out float3 col, out float alpha)
            {
                float envelope = GetSmoothEnvelope(prog);
                
                // Voronoi/Cellular procedural particle burst simulation using noise
                float particleField = step(0.75, Noise(st * (15.0 + prog * 10.0) + prog * 8.0));
                float dispersalMask = smoothstep(prog * 0.5, prog * 0.5 - 0.15, dist);
                
                // Central energy shock boundary
                float boundary = smoothstep(0.05, 0.0, abs(dist - prog * 0.45));

                float3 tint = lerp(_RimColor.rgb, float3(1.0, 5.0, 3.0), 0.6);
                col = (tint * 6.0) * particleField * dispersalMask + (float3(4.0, 6.0, 5.0) * boundary * 4.0);
                alpha = saturate(particleField * dispersalMask + boundary) * envelope;
            }

            // EFFECT 5: Supernova Core Eruption (Blinding star-burst core explosion with massive rays and thermal shock)
            void GetEffect5_SupernovaCore(float prog, float2 st, float dist, float angle, out float3 col, out float alpha)
            {
                float envelope = GetSmoothEnvelope(prog);
                
                // Starburst multi-pointed light rays
                float rays = pow(saturate(sin(angle * 8.0 + prog * 4.0) * 0.5 + 0.5), 8.0);
                float rayMask = rays * smoothstep(0.48, 0.05, dist) * (1.0 - prog * 0.5);
                
                // Blinding core expansion shock
                float blastRadius = prog * 0.5;
                float blastRing = smoothstep(0.08, 0.0, abs(dist - blastRadius));
                
                // Extreme initial white-out flash
                float flash = pow(max(0.0, 1.0 - prog * 1.8), 3.0) * 8.0;
                float coreGlow = (1.0 - smoothstep(0.0, 0.3, dist)) * sin(prog * PI) * 5.0;

                float3 coreColor = float3(6.0, 5.8, 5.0); // HDR White-hot sun core
                float3 rayColor = lerp(_QualityColor.rgb, float3(5.0, 2.0, 0.5), 0.4);

                col = coreColor * (flash + coreGlow) + rayColor * rayMask * 5.0 + float3(6.0, 3.0, 1.0) * blastRing * 4.0;
                alpha = saturate(rayMask + blastRing + coreGlow * 0.4 + flash * 0.6) * envelope;
            }

            void BlendEffects(inout float3 baseCol, inout float baseAlpha, float3 addCol, float addAlpha)
            {
                if (addAlpha <= 0.001) return;
                baseCol = baseCol + addCol * (1.0 - min(baseAlpha, 0.8));
                baseAlpha = saturate(baseAlpha + addAlpha * (1.0 - baseAlpha * 0.5));
            }

            half4 frag(Varyings input) : SV_Target
            {
                if (_TriggerProgress <= 0.001 || _TriggerProgress >= 0.999)
                {
                    return half4(0, 0, 0, 0);
                }

                float2 st = input.uv - 0.5;
                float dist = length(st);
                float angle = atan2(st.y, st.x);

                float3 totalColor = float3(0, 0, 0);
                float totalAlpha = 0.0;
                float3 effCol;
                float effAlpha;

                if (_Effect1Active > 0.5)
                {
                    GetEffect1_HyperPlasma(_TriggerProgress, st, dist, angle, effCol, effAlpha);
                    BlendEffects(totalColor, totalAlpha, effCol, effAlpha);
                }

                if (_Effect2Active > 0.5)
                {
                    GetEffect2_QuantumRift(_TriggerProgress, st, dist, angle, effCol, effAlpha);
                    BlendEffects(totalColor, totalAlpha, effCol, effAlpha);
                }

                if (_Effect3Active > 0.5)
                {
                    GetEffect3_TachyonSurge(_TriggerProgress, st, dist, angle, effCol, effAlpha);
                    BlendEffects(totalColor, totalAlpha, effCol, effAlpha);
                }

                if (_Effect4Active > 0.5)
                {
                    GetEffect4_NaniteShock(_TriggerProgress, st, dist, angle, effCol, effAlpha);
                    BlendEffects(totalColor, totalAlpha, effCol, effAlpha);
                }

                if (_Effect5Active > 0.5)
                {
                    GetEffect5_SupernovaCore(_TriggerProgress, st, dist, angle, effCol, effAlpha);
                    BlendEffects(totalColor, totalAlpha, effCol, effAlpha);
                }

                if (totalAlpha <= 0.001) return half4(0, 0, 0, 0);

                // Clean circular UI clipping mask
                float circularMask = smoothstep(0.48, 0.4, dist);
                totalAlpha *= circularMask;

                totalColor *= totalAlpha;
                half4 finalCol = half4(totalColor, totalAlpha);

                finalCol.rgb *= input.color.rgb;
                finalCol.a *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(finalCol.a - 0.001);
                #endif

                return finalCol;
            }
            ENDHLSL
        }
    }
}