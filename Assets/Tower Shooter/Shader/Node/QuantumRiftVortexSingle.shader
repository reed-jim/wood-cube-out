Shader "UI/Custom/QuantumRiftVortexSingle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Vortex Core Color", Color) = (0.5, 0.0, 1.0, 1.0)
        _RimColor ("Dimensional Rim Color", Color) = (0.0, 1.0, 0.8, 1.0)
        
        [Header(Vortex Control)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 10.0
        _Progress ("Jump Charge Progress", Range(0.0, 1.0)) = 0.0
        _HighSpeedTransition ("High Speed State Transition", Range(0.0, 1.0)) = 0.0
        _Layers ("Rift Layer Density", Range(3, 30)) = 8
        _EventHorizon ("Rift Core Radius", Range(0.05, 0.4)) = 0.2

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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_QuantumRiftVortexSingle"
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
                float _HighSpeedTransition;
                float _Layers;
                float _EventHorizon;
            CBUFFER_END

            #define PI 3.14159265359
            #define TWO_PI 6.28318530718

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
                // Center UV coordinates across the entire image
                float2 st = input.uv - 0.5;
                float dist = length(st);
                float angle = atan2(st.y, st.x);

                // Progress drives rotation, scaled by power intensity
                float progressRotation = _Progress * TWO_PI * (1.0 + _Power * 0.5);

                // Dynamic immersion scaling based on power
                float dynamicLayers = _Layers + (_Power * 0.2);
                float immersionGlow = 1.0 + (_Power * 0.04);

                // --- 1. NORMAL VORTEX STATE ---
                float spiral = angle * (2.0 + _Power * 0.02) + (dist * dynamicLayers) - progressRotation;
                float arms = sin(spiral) * 0.5 + 0.5;
                float ripple = sin(dist * (20.0 + _Power * 0.2) - progressRotation * 1.5) * 0.5 + 0.5;
                float vortexPattern = lerp(arms, ripple, 0.4);

                float coreVoid = smoothstep(_EventHorizon - 0.02, _EventHorizon + 0.05, dist);
                float photonRim = smoothstep(0.02, 0.0, abs(dist - _EventHorizon));

                float3 coreCol = _Color.rgb;
                float3 rimCol = _RimColor.rgb;

                float3 vortexColor = lerp(coreCol, rimCol, vortexPattern) * (1.0 - coreVoid) * 1.5 * immersionGlow;
                vortexColor += rimCol * photonRim * (3.0 + _Power * 0.08);

                // --- 2. HIGH-SPEED HYPER-DRIVE STATE (Exclusive) ---
                float hyperspaceTunnel = sin(dist * (40.0 + _Power * 0.5) - progressRotation * 2.0) * 0.5 + 0.5;
                float whiteHotCore = smoothstep(0.3, 0.0, dist);
                float3 hyperColor = lerp(_RimColor.rgb, float3(1.0, 1.0, 1.0), hyperspaceTunnel);
                hyperColor += hyperColor * (1.5 + whiteHotCore * (2.0 + _Power * 0.08));

                // --- 3. BLEND BETWEEN STATES VIA _HighSpeedTransition ---
                float3 finalColor = lerp(vortexColor, hyperColor, saturate(_HighSpeedTransition));
                float alpha = smoothstep(0.48, 0.45, dist) * saturate((1.0 - coreVoid) + photonRim + 0.2);

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