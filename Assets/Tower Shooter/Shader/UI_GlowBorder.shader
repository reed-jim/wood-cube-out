Shader "UI/URP/GlowBorder"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Horizontal Glow Gradient)]
        [HDR] _Color1 ("Color 1 (Edges)", Color) = (0.3, 0.8, 1.0, 1.0)
        [HDR] _Color2 ("Color 2 (Center)", Color) = (0.1, 0.4, 0.8, 1.0)
        _GlowAmount ("Hover Intensity", Range(0, 1)) = 0.0
        _GradientPower ("Horizontal Blend Power", Range(0.1, 5.0)) = 1.0

        [Header(Atmosphere Settings)]
        _AtmospherePower ("Atmosphere Power", Range(0.1, 10.0)) = 3.5
        _RimPower ("Rim Intensity Multiplier", Range(1.0, 10.0)) = 2.0
        _BorderThickness ("Glow Range", Range(0.01, 0.5)) = 0.25
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.1

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
            Name "Default"
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
                float4 _Color1;
                float4 _Color2;
                float _GlowAmount;
                float _GradientPower;
                float _AtmospherePower;
                float _RimPower;
                float _BorderThickness;
                float _CornerRadius;
            CBUFFER_END

            float sdRoundedBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                float2 centeredUV = input.uv - 0.5;

                float outerDist = sdRoundedBox(centeredUV, float2(0.5, 0.5), _CornerRadius);
                float maskAlpha = smoothstep(0.005, -0.005, outerDist);

                // Glow rim factor
                float distFromEdge = saturate(-outerDist / max(_BorderThickness, 0.0001));
                float rimFactor = 1.0 - distFromEdge;
                float atmosphereGlow = pow(saturate(rimFactor), _AtmospherePower) * _RimPower;

                // Horizontal gradient: Color 1 (Edges) -> Color 2 (Center) -> Color 1 (Edges)
                float horizontalFactor = pow(saturate(abs(centeredUV.x) * 2.0), _GradientPower);
                half4 horizontalGradientColor = lerp(_Color2, _Color1, horizontalFactor);

                half4 hoverOverlay = horizontalGradientColor * atmosphereGlow * _GlowAmount;

                // Combine layers
                half4 finalColor = mainTexColor + hoverOverlay;
                finalColor.a = mainTexColor.a * maskAlpha;

                return finalColor;
            }
            ENDHLSL
        }
    }
}