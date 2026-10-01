Shader "UI/Custom/DirectionalArrowEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (Arrow Silhouette)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Direction and Movement)]
        _DirectionAngle ("Direction Angle (Degrees)", Range(0.0, 360.0)) = 0.0
        _Speed ("Flow Speed", Range(-10.0, 10.0)) = 3.0
        _RepeatCount ("Pattern Repeat / Density", Range(1.0, 20.0)) = 6.0

        [Header(Arrow Style)]
        [KeywordEnum(Chevrons, EnergyPulse, DashedFlow)] _Style ("Arrow Style", Float) = 0
        
        [Header(Colors and Glow)]
        [HDR] _BaseColor ("Base Color (Transparent Background)", Color) = (0.0, 0.4, 0.9, 0.0)
        [HDR] _PulseColor ("Pulse / Arrow Color", Color) = (0.2, 0.9, 1.0, 1.0)
        _GlowIntensity ("Glow Boost", Range(1.0, 5.0)) = 2.0

        [Header(Edge Tuning)]
        _Softness ("Pattern Softness", Range(0.01, 0.5)) = 0.05
        _HeadFade ("Head Fade Power", Range(0.0, 3.0)) = 0.0

        // Required UI Masking Properties
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIDirectionalArrowPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma shader_feature_local _STYLE_CHEVRONS
            #pragma shader_feature_local _STYLE_ENERGYPULSE
            #pragma shader_feature_local _STYLE_DASHEDFLOW

            #pragma shader_feature_local UNITY_UI_CLIP_RECT
            #pragma shader_feature_local UNITY_UI_ALPHACLIP

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _BaseColor;
                float4 _PulseColor;
                float _DirectionAngle;
                float _Speed;
                float _RepeatCount;
                float _GlowIntensity;
                float _Softness;
                float _HeadFade;
                float4 _ClipRect;
            CBUFFER_END

            float2 rotateUV(float2 uv, float angleDegrees)
            {
                float rad = angleDegrees * (3.14159265359 / 180.0);
                float s = sin(rad);
                float c = cos(rad);
                
                uv -= 0.5;
                float2 rotatedUV;
                rotatedUV.x = uv.x * c - uv.y * s;
                rotatedUV.y = uv.x * s + uv.y * c;
                return rotatedUV + 0.5;
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

            float4 frag(Varyings input) : SV_Target
            {
                float4 spriteTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float2 rotUV = rotateUV(input.uv, _DirectionAngle);
                float time = _Time.y * _Speed;

                float pattern = 0.0;

                #if defined(_STYLE_CHEVRONS)
                    float chevronOffset = abs(rotUV.y - 0.5) * 0.8;
                    float progress = frac((rotUV.x - chevronOffset) * _RepeatCount - time);
                    pattern = smoothstep(0.0, _Softness, progress) - smoothstep(0.5, 0.5 + _Softness, progress);

                #elif defined(_STYLE_ENERGYPULSE)
                    float wave = sin((rotUV.x * _RepeatCount - time) * 6.28318);
                    pattern = smoothstep(-0.2, 0.8, wave);

                #elif defined(_STYLE_DASHEDFLOW)
                    float dash = frac(rotUV.x * _RepeatCount - time);
                    pattern = step(0.4, dash);

                #else
                    float dashFallback = frac(rotUV.x * _RepeatCount - time);
                    pattern = step(0.4, dashFallback);
                #endif

                float3 finalRGB = lerp(_BaseColor.rgb, _PulseColor.rgb * _GlowIntensity, pattern);
                float finalAlpha = lerp(_BaseColor.a, _PulseColor.a, pattern);

                if (_HeadFade > 0.0)
                {
                    finalAlpha *= pow(clamp(rotUV.x, 0.0, 1.0), _HeadFade);
                }

                float4 finalPixel = float4(finalRGB, finalAlpha * spriteTex.a) * input.color;

                #ifdef UNITY_UI_CLIP_RECT
                    finalPixel.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(finalPixel.a - 0.001);
                #endif

                return finalPixel;
            }
            ENDHLSL
        }
    }
}