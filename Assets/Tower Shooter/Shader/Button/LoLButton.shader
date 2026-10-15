Shader "Custom/URP/LoLSpaceButtonComplete"
{
    Properties
    {
        [MainTexture] _MainTex ("Base Sprite / Texture", 2D) = "white" {}
        _EffectIntensity ("Effect Intensity", Range(0.0, 1.0)) = 1.0
        
        [Header(Space Color Palette)]
        _BodyColor ("Center Space Void Dark", Color) = (0.02, 0.01, 0.05, 1)
        _BorderColor ("Celestial Gold Rim", Color) = (0.95, 0.82, 0.45, 1)
        _GlowColor ("Electric Neon Purple Glow", Color) = (0.75, 0.25, 1.0, 1)
        _GlowSecondary ("Deep Cosmic Indigo Energy", Color) = (0.15, 0.05, 0.5, 1)
        
        [Header(Frame Settings)]
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.15
        _AspectRatio ("Aspect Ratio (Width / Height)", Float) = 2.0
        _BorderThickness ("Border Thickness", Range(0, 0.5)) = 0.04
        _BevelHardness ("Bevel Specular Sharpness", Range(1.0, 16.0)) = 6.0
        
        [Header(Energy Streaks Parallax Nebula)]
        _StreakSpeed ("Energy Flow Speed", Range(0.1, 5.0)) = 1.2
        _StreakDensity ("Streak Density", Range(5.0, 50.0)) = 20.0
        _StreakIntensity ("Streak Brightness", Range(0.0, 2.0)) = 0.6
        _NebulaSpeed ("Nebula Drift Speed", Range(0.01, 1.0)) = 0.15
        
        [Header(Starfield Grid)]
        _StarGridSize ("Star Grid Resolution", Range(10.0, 50.0)) = 25.0
        _StarBrightness ("Star Brightness", Range(0.2, 3.0)) = 1.5
        
        [Header(Sweeping Light Reflection)]
        _ShineSpeed ("Sweep Frequency", Range(0.1, 3.0)) = 0.8
        _ShineWidth ("Sweep Width", Range(0.01, 0.4)) = 0.08
        _ShineAngle ("Sweep Angle Offset", Range(-1.0, 1.0)) = 0.35
        
        [Header(Interactive States)]
        _GlowAmount ("Hover Intensity", Range(0, 1)) = 0.0

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

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]

        Pass
        {
            Name "LoLSpaceButtonCompletePass"

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
                float _EffectIntensity;
                float4 _BodyColor;
                float4 _BorderColor;
                float4 _GlowColor;
                float4 _GlowSecondary;
                float _CornerRadius;
                float _AspectRatio;
                float _BorderThickness;
                float _BevelHardness;
                float _StreakSpeed;
                float _StreakDensity;
                float _StreakIntensity;
                float _NebulaSpeed;
                float _StarGridSize;
                float _StarBrightness;
                float _ShineSpeed;
                float _ShineWidth;
                float _ShineAngle;
                float _GlowAmount;
            CBUFFER_END

            float sdRoundedBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float2 Hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453123);
            }

            float ValueNoise1D(float x)
            {
                float i = floor(x);
                float f = frac(x);
                float u = f * f * (3.0 - 2.0 * f);
                return lerp(Hash11(i), Hash11(i + 1.0), u);
            }

            float StarFlare(float2 uv, float size)
            {
                float d = length(uv);
                float core = size / (d + 0.001);
                float rayX = max(0.0, 1.0 - abs(uv.x) * 25.0);
                float rayY = max(0.0, 1.0 - abs(uv.y) * 25.0);
                return saturate(core * 0.08 + (rayX + rayY) * 0.25 * smoothstep(size, 0.0, d));
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
                float4 defaultTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color;
                float time = _Time.y;

                float hover = _GlowAmount;

                float aspect = max(_AspectRatio, 0.0001);
                float2 centeredUV = (uv - 0.5) * float2(aspect, 1.0);
                float2 boxHalfSize = float2(0.5 * aspect, 0.5);
                
                float maxRadius = min(boxHalfSize.x, boxHalfSize.y);
                float radius = min(_CornerRadius, maxRadius);

                float distSDF = sdRoundedBox(centeredUV, boxHalfSize, radius);
                float aa = fwidth(distSDF);
                float outerAlpha = 1.0 - smoothstep(-aa, 0.0, distSDF);

                float distToEdge = -distSDF;
                float isBorder = step(distToEdge, _BorderThickness);

                float centerDist = length(uv - 0.5);
                float3 baseBody = lerp(_BodyColor.rgb * 1.6, _BodyColor.rgb * 0.3, centerDist * 1.5);

                float2 nebulaUV1 = uv + float2(time * _NebulaSpeed * 0.2, time * _NebulaSpeed * 0.1);
                float2 nebulaUV2 = uv * 1.5 - float2(time * _NebulaSpeed * 0.15, -time * _NebulaSpeed * 0.25);
                float nebulaLayer1 = ValueNoise1D(nebulaUV1.x * 12.0 + nebulaUV1.y * 12.0);
                float nebulaLayer2 = ValueNoise1D(nebulaUV2.x * 14.0 - nebulaUV2.y * 10.0);
                float3 cosmicCloud = lerp(_GlowSecondary.rgb, _GlowColor.rgb, nebulaLayer1 * nebulaLayer2);
                baseBody += cosmicCloud * 0.3 * (1.0 + hover * 0.6);

                float streakUV = (uv.y + uv.x * 0.1) * _StreakDensity;
                float streakNoise = ValueNoise1D(streakUV + time * _StreakSpeed);
                float horizontalMask = smoothstep(0.45, 0.0, abs(uv.y - 0.5));
                float energyFlow = pow(streakNoise, 3.0) * _StreakIntensity * horizontalMask;
                float3 streakColor = lerp(_GlowSecondary.rgb, _GlowColor.rgb, streakNoise);
                baseBody += streakColor * energyFlow;

                float2 starGrid = floor(uv * _StarGridSize);
                float2 starLocalUV = frac(uv * _StarGridSize) - 0.5;
                float2 starOffset = Hash22(starGrid) - 0.5;
                float2 starPos = starLocalUV - starOffset * 0.6;

                float twinkle = sin(time * 5.0 + starGrid.x * 13.0 + starGrid.y * 7.0) * 0.5 + 0.5;
                float star = StarFlare(starPos, 0.025 * twinkle * (1.0 + hover * 0.8));
                baseBody += lerp(_BorderColor.rgb, float3(1,1,1), 0.5) * star * _StarBrightness;

                float glowDepth = saturate((_BorderThickness * 3.5 - distToEdge) / (_BorderThickness * 3.5));
                glowDepth = pow(glowDepth, 2.5);

                float slowPulse = sin(time * 2.0) * 0.2 + 0.8;
                float fastShimmer = sin(time * 8.0 + uv.x * 10.0) * 0.15 + 0.85;
                float pulseCombined = lerp(slowPulse, fastShimmer, hover);

                float3 innerGlow = lerp(_GlowSecondary.rgb, _GlowColor.rgb, glowDepth) * glowDepth * pulseCombined * 1.5;
                baseBody += innerGlow * (1.0 + hover * 1.2);

                float borderNormalized = saturate(distToEdge / max(_BorderThickness, 0.0001));
                float bevelProfile = sin(borderNormalized * 3.14159);
                float bevelSpec = pow(bevelProfile, _BevelHardness);
                
                float borderLight = saturate((1.0 - uv.y) * 0.6 + (1.0 - uv.x) * 0.4);
                float3 goldBorder = _BorderColor.rgb * (0.4 + 0.8 * borderLight + bevelSpec * 0.9);

                goldBorder += _GlowColor.rgb * hover * 0.35;

                float3 customEffectColor = lerp(baseBody, goldBorder, isBorder);

                float sweepPos = frac(time * _ShineSpeed) * 3.0 - 1.0;
                float sweepCoord = uv.x + uv.y * _ShineAngle;
                
                float beamDist = abs(sweepCoord - sweepPos);
                float sharpBeam = smoothstep(_ShineWidth, 0.0, beamDist);
                float softBloom = smoothstep(_ShineWidth * 3.0, 0.0, beamDist) * 0.4;
                
                float totalSweep = (sharpBeam + softBloom) * (0.4 + hover * 0.8);
                float3 sweepColor = lerp(_BorderColor.rgb, float3(1.0, 1.0, 1.0), sharpBeam);
                
                customEffectColor += sweepColor * totalSweep;
                customEffectColor += hover * 0.08;

                float3 finalRGB = lerp(defaultTexColor.rgb, customEffectColor, _EffectIntensity);
                float finalAlpha = defaultTexColor.a * outerAlpha;

                return float4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}