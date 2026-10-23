Shader "UI/Custom/RocketSpacetimeVortex"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1)
        
        [Header(Vortex Spacetime Effect)]
        _SpaceColor ("Core Nebular Color", Color) = (0.2, 0.05, 0.4, 1.0)
        _StarColor ("Energy Ring Color", Color) = (0.4, 0.8, 1.0, 1.0)
        _Speed ("Vortex Spin Speed", Range(0.0, 20.0)) = 5.0
        _Density ("Energy Rings Density", Range(2.0, 30.0)) = 10.0

        [Header(Distortion Warp)]
        _PullStrength ("Gravitational Pull", Range(0.0, 2.0)) = 0.5
        _EdgeWarp ("Edge Shape Warp", Range(0.0, 0.2)) = 0.05

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
            Name "UI_SpacetimeVortex"
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
                float4 _SpaceColor;
                float4 _StarColor;
                float _Speed;
                float _Density;
                float _PullStrength;
                float _EdgeWarp;
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

                // Center UV coordinates
                float2 st = input.uv - 0.5;
                float r = length(st);
                float theta = atan2(st.y, st.x);

                // --- GRAVITATIONAL VORTEX UV WARP ---
                // Twist coordinates faster near the center to simulate intense spacetime curvature
                float twist = theta + (time / (r + 0.2)) * _PullStrength;
                
                // Subtle edge warping to distort the sprite's boundary silhouette organically
                float2 warpedUV = input.uv + float2(cos(twist), sin(twist)) * _EdgeWarp * (0.5 - r);

                // Sample the sprite mask using the vortex-warped UVs
                half4 texColor = _MainTex.Sample(sampler_MainTex, warpedUV);
                
                if (texColor.a <= 0.001)
                {
                    return 0;
                }

                // Re-calculate radius from warped coordinates for interior effect
                float2 centerSt = warpedUV - 0.5;
                float distFromCenter = length(centerSt);

                // --- PROCEDURAL VORTEX ENERGY RINGS ---
                float wave = sin(distFromCenter * _Density * 6.2831 - time * 2.0);
                float ringPattern = smoothstep(0.3, 1.0, abs(wave));

                // Color gradients blending from deep core nebula to bright event-horizon rings
                float3 nebula = _SpaceColor.rgb * (1.0 - distFromCenter * 1.5);
                float3 energyRing = _StarColor.rgb * ringPattern * (1.0 / (distFromCenter + 0.2));
                
                float3 vortexEffect = nebula + energyRing;

                // Combine with sprite texture mask
                float3 finalColor = vortexEffect * texColor.rgb;
                
                float alpha = texColor.a;
                finalColor *= alpha;

                half4 col = half4(finalColor, alpha);
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