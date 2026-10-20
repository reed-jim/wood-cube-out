Shader "UI/Custom/QuantumRiftVortexGrid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Vortex Core Color", Color) = (0.5, 0.0, 1.0, 1.0)
        _RimColor ("Dimensional Rim Color", Color) = (0.0, 1.0, 0.8, 1.0)
        _LockedColor ("Dormant Node Color", Color) = (0.15, 0.18, 0.2, 0.4)
        
        [Header(Grid Control)]
        _Progress ("Jump Charge Progress", Range(0.0, 1.0)) = 0.0
        _VortexSpeed ("Vortex Spin Speed (Cycles/s)", Float) = 15.0
        _Layers ("Rift Layer Density", Range(3, 15)) = 8
        _EventHorizon ("Rift Core Radius", Range(0.05, 0.4)) = 0.2
        
        [Header(Node Unlock States)]
        _Node_0_0 ("Top-Left Corner", Range(0,1)) = 0
        _Node_1_0 ("Top Cardinal", Range(0,1)) = 0
        _Node_2_0 ("Top-Right Corner", Range(0,1)) = 0
        _Node_0_1 ("Left Cardinal", Range(0,1)) = 0
        _Node_1_1 ("Center Node (Driver)", Range(0,1)) = 1
        _Node_2_1 ("Right Cardinal", Range(0,1)) = 0
        _Node_0_2 ("Bottom-Left Corner", Range(0,1)) = 0
        _Node_1_2 ("Bottom Cardinal", Range(0,1)) = 0
        _Node_2_2 ("Bottom-Right Corner", Range(0,1)) = 0

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
            Name "UI_QuantumRiftVortexGrid"
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
                float4 _LockedColor;
                float _Progress;
                float _VortexSpeed;
                float _Layers;
                float _EventHorizon;
                float _Node_0_0, _Node_1_0, _Node_2_0;
                float _Node_0_1, _Node_1_1, _Node_2_1;
                float _Node_0_2, _Node_1_2, _Node_2_2;
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

            half4 renderVortexCell(float2 st, float rotationDirection, float isUnlocked)
            {
                float dist = length(st);
                float angle = atan2(st.y, st.x);

                float activeProgress = isUnlocked > 0.5 ? _Progress : 0.0;
                
                // Smooth blend factor from normal vortex to high-speed mode between 50 and 65 speed
                float hyperBlend = smoothstep(50.0, 65.0, _VortexSpeed);

                // Time angle accumulated via progress and spin speed
                float timeVal = activeProgress * _VortexSpeed * TWO_PI * rotationDirection;

                float3 finalColor = float3(0,0,0);
                float alpha = 0;

                if (isUnlocked > 0.5)
                {
                    // --- 1. STANDARD MULTIVERSE VORTEX MODE ---
                    float spiral = angle * 2.0 + (dist * _Layers) - timeVal;
                    float arms = sin(spiral) * 0.5 + 0.5;
                    float ripple = sin(dist * 20.0 - timeVal) * 0.5 + 0.5;
                    float vortexPattern = lerp(arms, ripple, 0.4);

                    float coreVoid = smoothstep(_EventHorizon - 0.02, _EventHorizon + 0.05, dist);
                    float photonRim = smoothstep(0.02, 0.0, abs(dist - _EventHorizon));

                    float3 coreCol = _Color.rgb;
                    float3 rimCol = _RimColor.rgb;

                    float3 vortexColor = lerp(coreCol, rimCol, vortexPattern) * (1.0 - coreVoid) * 1.5;
                    vortexColor += rimCol * photonRim * 3.0;

                    // --- 2. HIGH-SPEED HYPER-DRIVE MODE ---
                    float hyperspaceTunnel = sin(dist * 40.0 - timeVal * 3.0) * 0.5 + 0.5;
                    float whiteHotCore = smoothstep(0.3, 0.0, dist);
                    float3 hyperColor = lerp(_RimColor.rgb, float3(1.0, 1.0, 1.0), hyperspaceTunnel);
                    hyperColor += hyperColor * (1.5 + whiteHotCore * 2.0);

                    // --- 3. BLEND BETWEEN MODES SMOOTHLY ---
                    finalColor = lerp(vortexColor, hyperColor, hyperBlend);
                    alpha = smoothstep(0.48, 0.45, dist) * saturate((1.0 - coreVoid) + photonRim + 0.2);
                }
                else
                {
                    // --- DORMANT LOCKED NODE ---
                    float dormantRing = smoothstep(0.46, 0.44, dist) * smoothstep(0.38, 0.4, dist);
                    finalColor = _LockedColor.rgb * dormantRing;
                    alpha = dormantRing * _LockedColor.a;
                }

                return half4(finalColor, alpha);
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Scale UV into 3x3 grid cells [0, 3)
                float2 gridUV = input.uv * 3.0;
                float2 cellID = floor(gridUV);
                float2 localUV = frac(gridUV) - 0.5;

                int cx = int(cellID.x);
                int cy = int(cellID.y);

                if (cx < 0 || cx > 2 || cy < 0 || cy > 2)
                    return half4(0,0,0,0);

                // Checkerboard rotation direction rule
                float rotationDir = 1.0;
                if (cx == 1 && cy == 1) rotationDir = 1.0;
                else if ((cx + cy) % 2 == 1) rotationDir = -1.0;
                else rotationDir = 1.0;

                // Lookup individual cell unlock state
                float isUnlocked = 0.0;
                if      (cx == 0 && cy == 0) isUnlocked = _Node_0_0;
                else if (cx == 1 && cy == 0) isUnlocked = _Node_1_0;
                else if (cx == 2 && cy == 0) isUnlocked = _Node_2_0;
                else if (cx == 0 && cy == 1) isUnlocked = _Node_0_1;
                else if (cx == 1 && cy == 1) isUnlocked = _Node_1_1;
                else if (cx == 2 && cy == 1) isUnlocked = _Node_2_1;
                else if (cx == 0 && cy == 2) isUnlocked = _Node_0_2;
                else if (cx == 1 && cy == 2) isUnlocked = _Node_1_2;
                else if (cx == 2 && cy == 2) isUnlocked = _Node_2_2;

                half4 cellCol = renderVortexCell(localUV, rotationDir, isUnlocked);

                // Margin cutoff between individual cells
                float cellDist = length(localUV);
                if (cellDist > 0.48) cellCol = half4(0,0,0,0);

                cellCol.rgb *= input.color.rgb;
                cellCol.a *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(cellCol.a - 0.001);
                #endif

                return cellCol;
            }
            ENDHLSL
        }
    }
}