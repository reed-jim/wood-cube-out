Shader "Custom/URP_UIPulseHighlight"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (0, 0.8, 1, 1)
        _RingWidth ("Ring Width", Range(0.01, 0.3)) = 0.05
        _GlowIntensity ("Glow Intensity", Range(0.0, 5.0)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0.0, 10.0)) = 3.0
        
        // Stencil properties for Unity UI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
            Name "UIPulseHighlightPass"

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
                float _RingWidth;
                float _GlowIntensity;
                float _PulseSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 centeredUV = input.uv - 0.5;
                float dist = length(centeredUV);

                // Keep total radius (node + glow) well within 0.5 (the quad boundary)
                float nodeRadius = 0.30; 
                float glowFalloff = 0.16; // 0.30 + 0.16 = 0.46 (fades completely before 0.5 edge)

                float edgeSmooth = 0.01;
                
                // Outer ring mask
                float innerRing = smoothstep(nodeRadius - _RingWidth - edgeSmooth, nodeRadius - _RingWidth, dist);
                float outerRing = smoothstep(nodeRadius, nodeRadius + edgeSmooth, dist);
                float ring = innerRing * (1.0 - outerRing);

                // Pulse factor
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float currentGlow = _GlowIntensity * (0.5 + 0.5 * pulse);

                // Outer glow aura fading cleanly to 0 at nodeRadius + glowFalloff
                float aura = smoothstep(nodeRadius + glowFalloff, nodeRadius, dist) * pulse;

                // Center fill light
                float centerFill = smoothstep(nodeRadius - _RingWidth, 0.0, dist) * 0.15 * pulse;

                float alphaMask = saturate(ring + aura + centerFill);
                
                // Edge guard: strictly mask out anything beyond 0.48 to guarantee zero edge bleeding
                float boundaryMask = 1.0 - smoothstep(0.46, 0.49, dist);
                alphaMask *= boundaryMask;

                float3 finalColor = input.color.rgb * (ring * currentGlow + aura * currentGlow * 0.6 + centerFill);

                return float4(finalColor, alphaMask * input.color.a);
            }
            ENDHLSL
        }
    }
}