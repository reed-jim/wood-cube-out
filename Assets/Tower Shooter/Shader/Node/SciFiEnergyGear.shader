Shader "UI/Custom/SingularityGear"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Primary Energy Color", Color) = (0.1, 0.7, 1.0, 1.0)
        _AccentColor ("Accretion Color", Color) = (1.0, 0.2, 0.5, 1.0)
        
        [Header(Singularity Dynamics)]
        _NodeCount ("Quantum Nodes", Range(3, 12)) = 6
        _WarpIntensity ("Spacetime Distortion", Range(0.0, 2.0)) = 0.8
        _EventHorizon ("Black Hole Horizon", Range(0.05, 0.3)) = 0.15
        _PhaseSpeed ("Phase Shift Speed", Float) = 2.0
        
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
            Name "UI_SingularityGear"
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
                float4 _AccentColor;
                float _NodeCount;
                float _WarpIntensity;
                float _EventHorizon;
                float _PhaseSpeed;
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

            // Smooth procedural min for fluid energy blending
            float smin(float a, float b, float k)
            {
                float h = saturate(0.5 + 0.5 * (b - a) / k);
                return lerp(b, a, h) - k * h * (1.0 - h);
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Re-center UV to [-0.5, 0.5]
                float2 st = input.uv - 0.5;
                float dist = length(st);
                float angle = atan2(st.y, st.x);

                // --- 1. Non-Euclidean Gravitational UV Warp ---
                // Warps the coordinate space inward as it approaches the center
                float warpFactor = pow(saturate(1.0 - dist), 3.0) * _WarpIntensity;
                float distortedAngle = angle + warpFactor * sin(_Time.y * _PhaseSpeed);

                // --- 2. Quantized Phase-Shifted "Teeth" Nodes ---
                // Creates energy tendrils that rotate at varying speeds depending on radius
                float tier1Angle = distortedAngle + _Time.y * _PhaseSpeed * 0.5;
                float tier2Angle = distortedAngle - _Time.y * _PhaseSpeed * 0.8;

                float nodes1 = cos(tier1Angle * _NodeCount);
                float nodes2 = sin(tier2Angle * (_NodeCount * 0.5));

                // Form spatial field for energy teeth
                float energyField = lerp(nodes1, nodes2, 0.5);
                
                // Construct gear boundary using non-linear math
                float gearBoundary = 0.35 + energyField * 0.08 * sin(_Time.y * 2.0 + dist * 10.0);
                
                // Mask for the gear teeth
                float teethAlpha = smoothstep(gearBoundary + 0.02, gearBoundary - 0.01, dist);

                // --- 3. Event Horizon (Black Hole Center) ---
                // Sharp cut-out at the center with a light-bending edge
                float horizonMask = smoothstep(_EventHorizon - 0.01, _EventHorizon + 0.01, dist);
                float photonRing = smoothstep(0.02, 0.0, abs(dist - _EventHorizon));

                // --- 4. Accretion Pulse Rays ---
                float rays = sin(angle * _NodeCount * 2.0 + _Time.y * 3.0) * 0.5 + 0.5;
                float pulseRings = frac(dist * 8.0 - _Time.y * 0.5);
                float accretionGlow = rays * (1.0 - pulseRings) * step(dist, gearBoundary);

                // --- 5. Color Compositing ---
                float3 primaryCol = _Color.rgb * input.color.rgb;
                float3 accentCol = _AccentColor.rgb;

                float3 finalColor = float3(0,0,0);

                // Main body with chromatic core shift
                finalColor += lerp(primaryCol, accentCol, energyField * 0.5 + 0.5) * teethAlpha;
                
                // Add photon ring bloom at black hole edge
                finalColor += accentCol * photonRing * 2.5;

                // Add accretion ray highlights
                finalColor += primaryCol * accretionGlow * 0.7;

                // Black hole mask (absorbs all inner light)
                finalColor *= horizonMask;

                // --- 6. Final Transparency & Canvas Clipping ---
                float alpha = saturate((teethAlpha + photonRing) * horizonMask);
                alpha *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(alpha - 0.001);
                #endif

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}