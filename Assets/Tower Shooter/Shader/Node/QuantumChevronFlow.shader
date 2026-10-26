Shader "UI/Custom/LeagueStandardBeam"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Core Beam Color", Color) = (0.2, 0.7, 1.0, 1.0)
        _RimColor ("Arcane Edge Glow", Color) = (0.0, 0.9, 0.8, 1.0)
        
        [Header(League Standard Flow Control)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 50.0
        _Progress ("Flow Progress (0 - 1)", Range(0.0, 1.0)) = 0.0
        _Speed ("Beam Scroll Speed", Float) = 4.0
        _BeamWidth ("Core Focus Width", Range(0.01, 0.3)) = 0.1
        _Distortion ("Hextech Distortion", Range(0.0, 1.0)) = 0.4

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
            Name "UI_LeagueStandardBeam"
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
                float _Power;
                float _Progress;
                float _Speed;
                float _BeamWidth;
                float _Distortion;
            CBUFFER_END

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

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Meticulously detailed 20-tier League Spell FX Evaluator (Top -> Bottom flow)
            void evaluateLeagueBeam(int tier, float2 uv, float flowTime, out float core, out float edgeGlow, out float lightning, out float3 tint)
            {
                float vertCoord = uv.y * 14.0 + flowTime;
                float horizCoord = uv.x;

                core = 0.0;
                edgeGlow = 0.0;
                lightning = 0.0;
                tint = _Color.rgb;

                // --- TIERS 0 - 3: TRASH / UNSTABLE TIERS ---
                if (tier == 0)
                {
                    float choppyY = floor(vertCoord * 4.0) / 4.0;
                    float jitter = (random(float2(choppyY, 0.1)) - 0.5) * 0.3;
                    float d = abs(horizCoord + jitter);
                    core = smoothstep(_BeamWidth, 0.0, d) * step(0.6, random(float2(choppyY, _Time.y)));
                    edgeGlow = smoothstep(_BeamWidth * 1.5, 0.0, d) * 0.1;
                    tint = _Color.rgb * 0.3;
                }
                else if (tier == 1)
                {
                    float choppyY = floor(vertCoord * 6.0) / 6.0;
                    float jitter = (random(float2(choppyY, 0.2)) - 0.5) * 0.2;
                    float d = abs(horizCoord + jitter);
                    core = smoothstep(_BeamWidth, 0.0, d) * step(0.4, random(float2(choppyY, _Time.y + 1.0)));
                    edgeGlow = smoothstep(_BeamWidth * 2.0, 0.0, d) * 0.2;
                    tint = _Color.rgb * 0.4;
                }
                else if (tier == 2)
                {
                    float w = sin(vertCoord * 1.5) * 0.1;
                    float d = abs(horizCoord + w);
                    core = smoothstep(_BeamWidth, 0.0, d) * (sin(vertCoord * 10.0) * 0.3 + 0.7);
                    edgeGlow = smoothstep(_BeamWidth * 2.2, 0.0, d) * 0.3;
                    tint = lerp(_Color.rgb, _RimColor.rgb, 0.2);
                }
                else if (tier == 3)
                {
                    float w = sin(vertCoord * 2.0) * 0.08;
                    float d = abs(horizCoord + w);
                    core = smoothstep(_BeamWidth, 0.0, d);
                    edgeGlow = smoothstep(_BeamWidth * 2.5, 0.0, d) * 0.4;
                    tint = lerp(_Color.rgb, _RimColor.rgb, 0.3);
                }
                // --- TIERS 4 - 9: LOW TO MID TIERS ---
                else if (tier <= 6)
                {
                    float flowWave = sin(vertCoord * 2.5 + sin(vertCoord * 0.5)) * _Distortion * 0.1;
                    float d = abs(horizCoord + flowWave);
                    core = smoothstep(_BeamWidth, 0.0, d);
                    edgeGlow = smoothstep(_BeamWidth * 2.8, 0.0, d) * 0.6;
                    tint = lerp(_Color.rgb, _RimColor.rgb, 0.4);
                }
                else if (tier <= 9)
                {
                    float strand1 = sin(vertCoord * 3.0) * 0.06;
                    float strand2 = cos(vertCoord * 3.0) * 0.06;
                    float d1 = abs(horizCoord - strand1);
                    float d2 = abs(horizCoord - strand2);
                    core = max(smoothstep(_BeamWidth * 0.8, 0.0, d1), smoothstep(_BeamWidth * 0.8, 0.0, d2));
                    edgeGlow = smoothstep(_BeamWidth * 3.0, 0.0, abs(horizCoord));
                    tint = lerp(_Color.rgb, float3(0.0, 1.4, 1.1), 0.5);
                }
                // --- TIERS 10 - 15: HIGH TO EPIC TIERS ---
                else if (tier <= 12)
                {
                    float plasma = abs(sin(vertCoord * 4.0 + sin(vertCoord * 1.2) * 2.0)) * 0.1;
                    float d = abs(horizCoord + plasma);
                    core = pow(smoothstep(_BeamWidth * 1.1, 0.0, d), 0.7);
                    edgeGlow = smoothstep(_BeamWidth * 3.2, 0.0, d);
                    lightning = pow(abs(cos(vertCoord * 8.0)), 4.0) * smoothstep(_BeamWidth * 0.5, 0.0, d);
                    tint = lerp(_RimColor.rgb, float3(1.8, 0.5, 2.2), 0.6);
                }
                else if (tier <= 15)
                {
                    float lattice = sin(vertCoord * 5.0 - horizCoord * 4.0) * cos(vertCoord * 2.5);
                    float d = abs(horizCoord) + abs(lattice) * 0.05;
                    core = pow(smoothstep(_BeamWidth * 1.2, 0.0, d), 0.5) * 1.2;
                    edgeGlow = smoothstep(_BeamWidth * 3.5, 0.0, d) * 1.4;
                    lightning = pow(abs(sin(vertCoord * 12.0)), 3.0) * 0.7;
                    tint = float3(0.3, 1.6, 3.0);
                }
                // --- TIERS 16 - 19: LEGENDARY / GOD TIERS ---
                else if (tier <= 18)
                {
                    float godSurge = sin(vertCoord * 6.0) * cos(vertCoord * 1.5);
                    float d = abs(horizCoord) + godSurge * 0.035;
                    core = pow(smoothstep(_BeamWidth * 1.3, 0.0, d), 0.4) * 1.4;
                    edgeGlow = smoothstep(_BeamWidth * 3.8, 0.0, d) * 1.8;
                    lightning = pow(abs(sin(vertCoord * 15.0)), 2.5) * 0.9;
                    tint = float3(0.6, 2.0, 3.4);
                }
                else
                {
                    float ultimate = sin(vertCoord * 7.0) * cos(vertCoord * 2.0);
                    float d = abs(horizCoord) + ultimate * 0.025;
                    core = pow(smoothstep(_BeamWidth * 1.4, 0.0, d), 0.3) * 1.6;
                    edgeGlow = smoothstep(_BeamWidth * 4.2, 0.0, d) * 2.2;
                    lightning = pow(abs(sin(vertCoord * 18.0)), 2.0);
                    tint = float3(0.9, 2.3, 3.8); // Majestic Hextech Ultimate Purity
                }
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv - 0.5; // Center UV
                
                float pNorm = saturate(_Power / 100.0);
                float pExp = pow(pNorm, 1.8);

                // Continuous Flow Animation Offset (Top to Bottom)
                float flowTime = (_Time.y * _Speed) + (_Progress * 20.0);

                // Smooth interpolation across the 20 distinct tiers (0 to 19)
                float powerTierVal = pNorm * 19.0;
                int tierA = clamp(int(floor(powerTierVal)), 0, 18);
                int tierB = min(tierA + 1, 19);
                float tierMix = frac(powerTierVal);
                tierMix = tierMix * tierMix * (3.0 - 2.0 * tierMix);

                float coreA, edgeA, lightA, coreB, edgeB, lightB;
                float3 colA, colB;
                evaluateLeagueBeam(tierA, uv, flowTime, coreA, edgeA, lightA, colA);
                evaluateLeagueBeam(tierB, uv, flowTime, coreB, edgeB, lightB, colB);

                float core = lerp(coreA, coreB, tierMix);
                float edgeGlow = lerp(edgeA, edgeB, tierMix);
                float lightning = lerp(lightA, lightB, tierMix);
                float3 tierColor = lerp(colA, colB, tierMix);

                // --- STYLIZED LEAGUE SPELL FX COMPOSITION ---
                float brightness = lerp(0.7, 2.8, pExp);
                float3 finalColor = tierColor * (core * 2.5 + edgeGlow * 1.3 + lightning * 1.6) * brightness;

                // --- SUBTLE CHROMATIC EDGE SPLIT FOR HIGH POWER ---
                if (_Power > 45.0)
                {
                    float split = ((_Power - 45.0) / 55.0) * 0.012;
                    float cR, eR, lR, cB, eB, lB;
                    float3 dummy;
                    evaluateLeagueBeam(tierA, uv + float2(split, 0.0), flowTime, cR, eR, lR, dummy);
                    evaluateLeagueBeam(tierA, uv - float2(split, 0.0), flowTime, cB, eB, lB, dummy);
                    finalColor.r += cR * 1.4;
                    finalColor.b += cB * 1.4;
                }

                // --- TRANSPARENT ISOLATION MASK (Zero Background Fill) ---
                float horizontalMask = smoothstep(0.49, 0.12, abs(uv.x));
                float verticalCapMask = smoothstep(0.49, 0.38, abs(uv.y));
                float totalMask = horizontalMask * verticalCapMask;

                // Alpha is governed strictly by beam components; background remains completely transparent
                float alpha = totalMask * saturate((core * 1.8 + edgeGlow * 0.9) * lerp(0.4, 2.0, pNorm));

                // Trash tier choppy flicker modulation
                if (_Power < 20.0)
                {
                    alpha *= (random(float2(_Time.y, 0.5)) * 0.4 + 0.6);
                }

                // Premultiply Alpha
                finalColor *= alpha;

                half4 col = half4(finalColor, alpha);

                // Apply UI Vertex Color
                col.rgb *= input.color.rgb;
                col.a *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}