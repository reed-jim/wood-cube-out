Shader "UI/URP/GlowBorder"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Normal Hover Glow)]
        [HDR] _GlowColor ("Hover Edge Color", Color) = (0.3, 0.8, 1.0, 1.0)
        [HDR] _AtmosphereColor ("Hover Atmosphere", Color) = (0.1, 0.4, 0.8, 1.0)
        _GlowAmount ("Hover Intensity", Range(0, 1)) = 0.0
        
        [Header(Material Design Ripple Effect)]
        [HDR] _RippleColor ("Ripple Color", Color) = (1.0, 1.0, 1.0, 0.9)
        _RippleProgress ("Ripple Progress", Range(0, 1)) = 0.0
        _RippleIntensity ("Ripple Intensity Multiplier", Range(0, 5)) = 2.0
        _ClickLocalPos ("Click Position (Local Rect Space)", Vector) = (0, 0, 0, 0)
        _RectSize ("Button Size (Width, Height)", Vector) = (300, 100, 0, 0)

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
                float3 positionOS   : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _GlowColor;
                float4 _AtmosphereColor;
                float _GlowAmount;
                float4 _RippleColor;
                float _RippleProgress;
                float _RippleIntensity;
                float4 _ClickLocalPos;
                float4 _RectSize;
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
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                float2 centeredUV = input.uv - 0.5;

                float outerDist = sdRoundedBox(centeredUV, float2(0.5, 0.5), _CornerRadius);
                float maskAlpha = smoothstep(0.005, -0.005, outerDist);

                // Standard Hover Glow
                float distFromEdge = saturate(-outerDist / max(_BorderThickness, 0.0001));
                float rimFactor = 1.0 - distFromEdge;
                float atmosphereGlow = pow(saturate(rimFactor), _AtmospherePower) * _RimPower;
                half4 glowColorResult = lerp(_AtmosphereColor, _GlowColor, pow(saturate(rimFactor), 2.0));
                half4 hoverOverlay = glowColorResult * atmosphereGlow * _GlowAmount;

                // 9-Slice Safe Ripple Effect (Calculated via Local Object-Space Pixels)
                // input.positionOS.xy gives local rect coordinates centered around (0,0)
                float2 currentPixelPos = input.positionOS.xy;
                float distFromClick = distance(currentPixelPos, _ClickLocalPos.xy);

                // Max radius based on the diagonal distance of the whole button rectangle
                float maxRadius = length(_RectSize.xy);
                float currentRadius = _RippleProgress * maxRadius;

                float rippleMask = step(0.001, _RippleProgress) * 
                                 smoothstep(currentRadius + 40.0, currentRadius - 10.0, distFromClick) * 
                                 smoothstep(currentRadius - 80.0, currentRadius, distFromClick) *
                                 (1.0 - _RippleProgress);
                
                half4 rippleOverlay = _RippleColor * rippleMask * _RippleIntensity;

                // Combine layers
                half4 finalColor = mainTexColor + hoverOverlay + rippleOverlay;
                finalColor.a = mainTexColor.a * maskAlpha;

                return finalColor;
            }
            ENDHLSL
        }
    }
}