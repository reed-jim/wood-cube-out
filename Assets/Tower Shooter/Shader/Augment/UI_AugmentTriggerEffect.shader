Shader "UI/Custom/FlexibleTriggerEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Core Tint Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _RimColor ("Energy Glow Color", Color) = (1.0, 0.3, 0.9, 1.0)
        
        [Header(Shared Progress Control)]
        _TriggerProgress ("Trigger Progress (0 - 1)", Range(0.0, 1.0)) = 0.0

        [Header(Mix and Match Toggles)]
        _Effect1Active ("Effect 1: Implosion Active", Float) = 0.0
        _Effect2Active ("Effect 2: Glitch Active", Float) = 0.0
        _Effect3Active ("Effect 3: Chrono-Ring Active", Float) = 0.0

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
            Name "UI_FlexibleTriggerEffect"
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
                float _TriggerProgress;
                float _Effect1Active;
                float _Effect2Active;
                float _Effect3Active;
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

            half4 frag(Varyings input) : SV_Target
            {
                // If progress is at 0 or 1, render nothing
                if (_TriggerProgress <= 0.001 || _TriggerProgress >= 0.999)
                {
                    return half4(0, 0, 0, 0);
                }

                float2 st = input.uv - 0.5;
                float dist = length(st);
                float angle = atan2(st.y, st.x);

                float3 totalColor = float3(0, 0, 0);
                float totalAlpha = 0.0;

                // --- EFFECT 1: IMPLOSION ---
                if (_Effect1Active > 0.5)
                {
                    float shockPhase = saturate((_TriggerProgress - 0.3) / 0.7);
                    float ringRadius = shockPhase * 0.45;
                    float shockRing = smoothstep(0.04, 0.0, abs(dist - ringRadius));
                    float corePull = (1.0 - smoothstep(0.0, 0.25, dist)) * (1.0 - saturate(_TriggerProgress / 0.3));

                    float3 col1 = (_RimColor.rgb * 4.0 + float3(3.0, 3.0, 3.0)) * (shockRing + corePull);
                    float alpha1 = saturate(shockRing * (1.0 - shockPhase) + corePull);

                    totalColor += col1;
                    totalAlpha += alpha1;
                }

                // --- EFFECT 2: HOLOGRAPHIC GLITCH ---
                if (_Effect2Active > 0.5)
                {
                    float flashEnvelope = sin(_TriggerProgress * PI);
                    float scanlines = step(0.6, sin(st.y * 70.0 + _TriggerProgress * 50.0));
                    float radialStripe = abs(sin(angle * 12.0 - _TriggerProgress * 20.0));
                    float pattern = scanlines * radialStripe;

                    float3 col2 = lerp(_Color.rgb, _RimColor.rgb * 3.5, pattern) * flashEnvelope * 3.0;
                    float alpha2 = pattern * flashEnvelope * smoothstep(0.45, 0.2, dist);

                    totalColor += col2;
                    totalAlpha += alpha2;
                }

                // --- EFFECT 3: CHRONO-RING CASCADE ---
                if (_Effect3Active > 0.5)
                {
                    float spinSpeed = _TriggerProgress * 15.0;
                    float ring1 = abs(sin(dist * 40.0 - spinSpeed));
                    float ring2 = abs(cos(angle * 8.0 + spinSpeed * 2.0));
                    float combined = pow(saturate(ring1 * ring2), 1.5);
                    float fadeOut = 1.0 - _TriggerProgress;

                    float3 col3 = (_RimColor.rgb * 3.0 + float3(2.0, 1.5, 0.5)) * combined * fadeOut * 4.0;
                    float alpha3 = combined * fadeOut * smoothstep(0.48, 0.1, dist);

                    totalColor += col3;
                    totalAlpha += alpha3;
                }

                totalAlpha = saturate(totalAlpha);
                if (totalAlpha <= 0.001) return half4(0, 0, 0, 0);

                // Circular UI mask
                float circularMask = smoothstep(0.48, 0.4, dist);
                totalAlpha *= circularMask;

                // Premultiplied alpha output
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