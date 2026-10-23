Shader "UI/Custom/RocketAccelerationEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1)
        
        [Header(Acceleration Motion Trail)]
        _BoostIntensity ("Boost Intensity", Range(0.0, 5.0)) = 2.0
        _TrailLength ("Trail Length", Range(0.0, 0.5)) = 0.25
        _Speed ("Speed / Frequency", Range(0.0, 50.0)) = 25.0

        [Header(High Speed Shake)]
        _ShakeIntensity ("Sprite Shake Intensity", Range(0.0, 0.05)) = 0.008

        [Header(Thruster Exhaust Flame)]
        _FlameColor ("Exhaust Flame Color", Color) = (1.0, 0.4, 0.1, 1.0)
        _FlameIntensity ("Flame Glow", Range(0.0, 5.0)) = 2.0

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
            Name "UI_RocketAcceleration"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma shader_feature_local _USEUIALPHACLIP_ON

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

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
                float4 _MainTex_ST;
                float4 _Color;
                float4 _FlameColor;
                float _BoostIntensity;
                float _TrailLength;
                float _Speed;
                float _ShakeIntensity;
                float _FlameIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _Speed;
                float2 uv = input.uv;

                // --- HIGH-SPEED SPRITE SHAKE ---
                // Generate high-frequency pseudo-random offsets based on time to simulate heavy atmospheric/speed buffeting
                float shakeX = sin(time * 3.7) * cos(time * 5.1);
                float shakeY = cos(time * 4.3) * sin(time * 2.9);
                float2 shakeOffset = float2(shakeX, shakeY) * _ShakeIntensity;
                
                // Apply shake offset to core UV mapping
                float2 shakenUV = uv + shakeOffset;

                // --- MOTION BLUR ACCELERATION TRAIL ---
                // Since the rocket travels bottom -> top, the trail must stretch downward (+Y offset in UV space)
                int samples = 6;
                half4 trailColor = 0;
                float totalAlpha = 0;

                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / (float)(samples - 1);
                    // Offset UV downwards (+Y) to cast the blur trail behind the moving rocket
                    float2 offsetUV = shakenUV + float2(0.0, t * _TrailLength * _BoostIntensity);
                    half4 sampleCol = _MainTex.Sample(sampler_MainTex, offsetUV);
                    
                    // Fade out older trail samples
                    float fade = (1.0 - t);
                    trailColor += sampleCol * fade;
                    totalAlpha = max(totalAlpha, sampleCol.a * fade);
                }
                trailColor /= (float)samples;

                // Sample the base sprite using the shaken UVs
                half4 texColor = _MainTex.Sample(sampler_MainTex, shakenUV);

                // --- THRUSTER EXHAUST FLAME (BOTTOM) ---
                // Because the rocket points top, the exhaust emits from the bottom of the UV bounds (near y = 0.0)
                float2 centeredUV = uv - 0.5;
                float flameMask = smoothstep(0.1, -0.4, centeredUV.y) * smoothstep(0.35, -0.1, abs(centeredUV.x));
                
                // Rapid procedural flickering for the exhaust fire
                float flicker = sin(time * 1.5 + centeredUV.x * 30.0) * cos(time * 2.0);
                float3 exhaustFlame = _FlameColor.rgb * flameMask * (_FlameIntensity * (flicker + 1.2));

                // Combine base sprite, motion trail blur, and exhaust flame
                float3 finalRGB = (texColor.rgb * texColor.a) + (trailColor.rgb * _BoostIntensity * 0.4) + exhaustFlame;
                float finalAlpha = max(texColor.a, totalAlpha + flameMask * 0.9);

                finalRGB *= finalAlpha;

                half4 col = half4(finalRGB, finalAlpha);
                col *= input.color;

                #ifdef _USEUIALPHACLIP_ON
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}