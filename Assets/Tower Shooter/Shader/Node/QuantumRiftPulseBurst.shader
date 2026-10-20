Shader "UI/Custom/QuantumRiftPulseBurst"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Energy Pulse Color", Color) = (0.0, 1.0, 0.8, 1.0)
        
        [Header(Burst Control)]
        _Progress ("Burst Progress (0 - 1)", Range(0.0, 1.0)) = 0.0
        _Thickness ("Ring Thickness", Range(0.001, 0.2)) = 0.04
        _GlowIntensity ("Glow Intensity", Range(1.0, 10.0)) = 4.0
        _RayCount ("Energy Sparkle Rays", Range(4, 32)) = 12

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
        // Premultiplied Alpha Blending to prevent dark fringes
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_QuantumRiftPulseBurst"
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
                float _Progress;
                float _Thickness;
                float _GlowIntensity;
                float _RayCount;
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
                float2 st = input.uv - 0.5;
                float dist = length(st);
                float angle = atan2(st.y, st.x);

                // Map progress (0 to 1) to an expanding radius (0 to 0.5)
                float currentRadius = _Progress * 0.5;

                // Sharp expanding ring shockwave
                float ring = smoothstep(_Thickness, 0.0, abs(dist - currentRadius));
                
                // Fade out the shockwave intensity as it expands toward the edge
                float fadeOut = 1.0 - saturate(_Progress);
                ring *= fadeOut;

                // Radiating energy sparks/rays that burst outward along with the wave
                float rays = pow(saturate(sin(angle * _RayCount + _Progress * 15.0)), 4.0);
                float energySparks = ring * rays * 2.0;

                // Total pattern combination
                float pattern = saturate(ring + energySparks);

                // Final vivid color calculation with high contrast boost
                float3 finalColor = _Color.rgb * pattern * _GlowIntensity * 3.0;

                // Add a bright white-hot center core flash at the exact moment of trigger (_Progress near 0)
                float coreFlash = smoothstep(0.15, 0.0, dist) * saturate(1.0 - _Progress * 5.0);
                finalColor += float3(3.0, 3.0, 3.5) * coreFlash;

                // Clean circular outer mask to match UI rect boundaries
                float circularMask = smoothstep(0.49, 0.42, dist);
                float alpha = circularMask * saturate(pattern + coreFlash);

                // Premultiply color with alpha for clean blending
                finalColor *= alpha;

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