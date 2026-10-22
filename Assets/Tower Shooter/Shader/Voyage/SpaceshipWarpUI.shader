Shader "UI/LoL_SpaceshipWarp"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Motion and Speed)]
        _Speed ("Warp Speed", Range(0.1, 20.0)) = 4.0
        _Density ("Warp Density", Range(5.0, 50.0)) = 18.0
        
        [Header(Stylized VFX)]
        _CoreColor ("Core Energy Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _OuterColor ("Outer Arc Color", Color) = (0.9, 0.1, 0.6, 1.0)
        _ChromaticAberration ("Chromatic Distortion", Range(0.0, 0.05)) = 0.015
        _NebulaGlow ("Cosmic Dust Intensity", Range(0.0, 2.0)) = 1.0
        _PulseFrequency ("Energy Pulse Speed", Range(0.0, 10.0)) = 3.0

        // Required UI Masking & Stencil Properties
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
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
            WriteMask [_StencilWriteMask]
            ReadMask [_StencilReadMask]
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _CoreColor;
                float4 _OuterColor;
                float _Speed;
                float _Density;
                float _ChromaticAberration;
                float _NebulaGlow;
                float _PulseFrequency;
            CBUFFER_END

            // High-quality hash function for procedural noise
            float2 Hash2D(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453123);
            }

            // Procedural Value Noise for Cosmic Dust/Nebula
            float ValueNoise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                // Smoothstep interpolation (cubic Hermite curve)
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(dot(Hash2D(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                                 dot(Hash2D(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                            lerp(dot(Hash2D(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                                 dot(Hash2D(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
            }

            // Single Radial Trail Layer evaluation
            float GetRadialStreaks(float angle, float dist, float speed, float density, float seed)
            {
                float time = _Time.y * speed;
                float2 polarUV = float2(angle * density, dist * 8.0 - time + seed);
                
                float cellID = floor(polarUV.x);
                float randVal = Hash2D(float2(cellID, seed)).x;

                // Vary streak intensity per radial slice
                if (randVal < 0.25) return 0.0;

                float streakUV = frac(polarUV.y);
                
                // Stylized tapered streak shape with exponential falloff
                float streak = smoothstep(0.0, 0.9, streakUV) * pow(1.0 - streakUV, 1.5);
                
                // Edge sharpness along radial lines
                float radialLine = 1.0 - abs(frac(polarUV.x) - 0.5) * 2.0;
                radialLine = pow(radialLine, 4.0);

                return streak * radialLine * randVal;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Shift UV center to (0,0)
                float2 uv = input.uv - 0.5;
                float dist = length(uv);
                float angle = atan2(uv.y, uv.x) / 6.28318530718; // Normalized angle [-0.5, 0.5]

                // --- 1. Chromatic Aberration & Radial Warp ---
                float2 distVec = normalize(uv) * _ChromaticAberration * dist;
                
                // Calculate layered radial trails with chromatic offset
                float streakR = GetRadialStreaks(angle, length(uv + distVec), _Speed, _Density, 1.0);
                float streakG = GetRadialStreaks(angle, dist, _Speed, _Density, 2.0);
                float streakB = GetRadialStreaks(angle, length(uv - distVec), _Speed, _Density, 3.0);

                // --- 2. Color Gradient Ramping (LoL Stylized Aesthetic) ---
                float3 streakColor = lerp(_CoreColor.rgb, _OuterColor.rgb, saturate(dist * 2.0));
                float3 radialVFX = float3(streakR, streakG, streakB) * streakColor * 2.5;

                // --- 3. Cosmic Dust / Energy Field (Procedural Noise) ---
                float2 noiseUV = float2(angle * 6.0, dist * 3.0 - _Time.y * (_Speed * 0.3));
                float cosmicNoise = saturate(ValueNoise(noiseUV) + 0.3);
                float3 cosmicGlow = lerp(_OuterColor.rgb, _CoreColor.rgb, cosmicNoise) * cosmicNoise * _NebulaGlow;
                cosmicGlow *= smoothstep(0.0, 0.5, dist); // Fade center out slightly for depth

                // --- 4. Central Energy Core Pulse ---
                float pulse = sin(_Time.y * _PulseFrequency) * 0.15 + 0.85;
                float core = pow(saturate(1.0 - dist * 2.2), 3.0) * pulse;
                float3 coreVFX = _CoreColor.rgb * core * 3.0;

                // --- 5. Composite Layers ---
                float3 finalRGB = radialVFX + cosmicGlow + coreVFX;
                
                // Dynamic Alpha based on total luminance and distance falloff
                float totalIntensity = streakG + cosmicNoise * 0.5 + core;
                float alpha = saturate(totalIntensity * 1.5) * input.color.a;

                // Soft border masking for clean UI blending
                alpha *= smoothstep(0.5, 0.42, dist);

                return half4(finalRGB * input.color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}