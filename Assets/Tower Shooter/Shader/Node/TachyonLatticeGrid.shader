Shader "UI/Custom/TachyonLatticeGrid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Crystal Energy Color", Color) = (0.2, 1.0, 0.6, 1.0)
        _PrismColor ("Refraction Color", Color) = (0.0, 0.6, 1.0, 1.0)
        _LockedColor ("Dormant Matrix Color", Color) = (0.15, 0.18, 0.2, 0.4)
        
        [Header(Grid Control)]
        _Progress ("Animation Progress", Range(0.0, 1.0)) = 0.0
        _Rotations ("Total Spin Frequency", Float) = 3.0
        
        [Header(Node Unlock States)]
        _Node_0_0 ("Top-Left Corner", Range(0,1)) = 0
        _Node_1_0 ("Top Cardinal", Range(0,1)) = 0
        _Node_2_0 ("Top-Right Corner", Range(0,1)) = 0
        _Node_0_1 ("Left Cardinal", Range(0,1)) = 0
        _Node_1_1 ("Center Node (Matrix Core)", Range(0,1)) = 1
        _Node_2_1 ("Right Cardinal", Range(0,1)) = 0
        _Node_0_2 ("Bottom-Left Corner", Range(0,1)) = 0
        _Node_1_2 ("Bottom Cardinal", Range(0,1)) = 0
        _Node_2_2 ("Bottom-Right Corner", Range(0,1)) = 0

        [Header(Crystalline Dynamics)]
        _FacetCount ("Facet Sides", Range(3, 8)) = 4
        _Sharpness ("Laser Sharpness", Range(1.0, 10.0)) = 4.0
        
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
            Name "UI_TachyonLatticeGrid"
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
                float4 _PrismColor;
                float4 _LockedColor;
                float _Progress;
                float _Rotations;
                float _Node_0_0, _Node_1_0, _Node_2_0;
                float _Node_0_1, _Node_1_1, _Node_2_1;
                float _Node_0_2, _Node_1_2, _Node_2_2;
                float _FacetCount;
                float _Sharpness;
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

            half4 renderCrystal(float2 st, float angleOffset, float rotationDirection, float isUnlocked)
            {
                float dist = length(st);
                float angle = atan2(st.y, st.x);

                float activeProgress = isUnlocked > 0.5 ? _Progress : 0.0;
                float spinAngle = (activeProgress * _Rotations * TWO_PI * rotationDirection) + angleOffset;

                // 1. Crystalline Facet Geometry (Polygon Shape)
                float facetAngle = abs(cos(angle * (_FacetCount * 0.5) + spinAngle)) * 2.0;
                float crystalBoundary = 0.38 / (0.7 + 0.3 * facetAngle);
                
                float crystalMask = smoothstep(crystalBoundary + 0.01, crystalBoundary - 0.01, dist);
                float innerCoreMask = smoothstep(0.15, 0.05, dist);

                // 2. Laser Refraction Lines
                float laserRays = pow(abs(sin(angle * _FacetCount - spinAngle * 2.0)), _Sharpness);
                float refractionGlow = laserRays * smoothstep(0.4, 0.1, dist);

                // 3. Holographic Data Rings
                float dataRing = frac(dist * 12.0 - spinAngle);
                float ringMask = smoothstep(0.7, 0.9, dataRing) * smoothstep(crystalBoundary, 0.1, dist);

                float3 finalColor = float3(0,0,0);
                float alpha = 0;

                if (isUnlocked > 0.5)
                {
                    // Active Crystalline State
                    float3 primary = _Color.rgb;
                    float3 accent = _PrismColor.rgb;

                    finalColor += lerp(primary, accent, laserRays) * crystalMask * 0.8;
                    finalColor += accent * refractionGlow * 1.5;
                    finalColor += primary * innerCoreMask * 2.0;
                    finalColor += accent * ringMask * 0.6;

                    alpha = saturate(crystalMask + refractionGlow);
                }
                else
                {
                    // Dormant Matrix Frame
                    float dormantFrame = smoothstep(0.39, 0.37, dist) * smoothstep(0.32, 0.34, dist);
                    finalColor = _LockedColor.rgb * dormantFrame;
                    alpha = dormantFrame * _LockedColor.a;
                }

                return half4(finalColor, alpha);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 gridUV = input.uv * 3.0;
                float2 cellID = floor(gridUV);
                float2 localUV = frac(gridUV) - 0.5;

                int cx = int(cellID.x);
                int cy = int(cellID.y);

                if (cx < 0 || cx > 2 || cy < 0 || cy > 2)
                    return half4(0,0,0,0);

                // Crystalline phase alternation
                float rotationDir = ((cx + cy) % 2 == 0) ? 1.0 : -1.0;
                if (cx == 1 && cy == 1) rotationDir = 1.0;

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

                half4 crystalCol = renderCrystal(localUV, 0.0, rotationDir, isUnlocked);

                float cellDist = length(localUV);
                if (cellDist > 0.48) crystalCol = half4(0,0,0,0);

                crystalCol.rgb *= input.color.rgb;
                crystalCol.a *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(crystalCol.a - 0.001);
                #endif

                return crystalCol;
            }
            ENDHLSL
        }
    }
}