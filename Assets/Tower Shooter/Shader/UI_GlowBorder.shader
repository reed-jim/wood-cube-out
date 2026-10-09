Shader "UI/URP/GlowBorder"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Glow Border Settings)]
        [HDR] _GlowColor ("Glow Edge Color", Color) = (1, 1, 1, 1)
        [HDR] _AtmosphereColor ("Atmosphere Color", Color) = (0.3, 0.6, 1.0, 1.0)
        _GlowAmount ("Glow Transition Amount", Range(0, 1)) = 1.0
        
        [Header(Atmosphere Power)]
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
                float4 _GlowColor;
                float4 _AtmosphereColor;
                float _GlowAmount;
                float _AtmospherePower;
                float _RimPower;
                float _BorderThickness;
                float _CornerRadius;
            CBUFFER_END

            // Signed Distance Function for Rounded Rect
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

                // Center UV coords from -0.5 to 0.5
                float2 centeredUV = input.uv - 0.5;

                // 1. Outer Box Alpha Mask (Rounded Rect Clipping)
                float outerDist = sdRoundedBox(centeredUV, float2(0.5, 0.5), _CornerRadius);
                float maskAlpha = smoothstep(0.005, -0.005, outerDist);

                // 2. Compute Exponential Atmosphere Glow Gradient
                // Distance inwards from the outer border normalized by thickness
                float distFromEdge = saturate(-outerDist / max(_BorderThickness, 0.0001));
                
                // Inverse distance (1 at edge, 0 inside towards center)
                float rimFactor = 1.0 - distFromEdge;
                
                // Exponential power falloff (recalculating atmosphere behavior)
                float atmosphereGlow = pow(saturate(rimFactor), _AtmospherePower) * _RimPower;

                // Blend rim border color (bright white/cyan edge) into atmosphere body color (blue)
                half4 glowColorResult = lerp(_AtmosphereColor, _GlowColor, pow(saturate(rimFactor), 2.0));

                // Final glow layer scaled by animated _GlowAmount transition
                half4 glowOverlay = glowColorResult * atmosphereGlow * _GlowAmount;

                // 3. Combine original texture with atmospheric glow
                half4 finalColor = mainTexColor + glowOverlay;
                finalColor.a = mainTexColor.a * maskAlpha;

                return finalColor;
            }
            ENDHLSL
        }
    }
}