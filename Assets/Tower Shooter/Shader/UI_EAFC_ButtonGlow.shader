Shader "UI/URP/EAFC_ButtonGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)

        [Header(Transition Control)]
        _EffectAmount ("Glow & Sheen Blend", Range(0, 1)) = 0.0

        [Header(Specular Sheen Sweep)]
        [HDR] _SheenColor ("Sheen Color", Color) = (2.5, 2.2, 1.5, 1)
        _SheenAngle ("Sweep Angle (Degrees)", Range(0, 360)) = 45.0
        _SheenWidth ("Sweep Beam Width", Range(0.01, 0.5)) = 0.15
        _SheenSpeed ("Sweep Animation Speed", Range(0.0, 5.0)) = 1.2
        _SheenSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1

        [HDR] _EdgeGlowColor ("Outer Edge Glow", Color) = (3.0, 1.8, 0.4, 1)
        _EdgeGlowWidth ("Edge Width", Range(0.0, 0.3)) = 0.08

        // Canvas Masking
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
                float4 _SheenColor;
                float4 _EdgeGlowColor;
                float _EffectAmount;
                float _SheenAngle;
                float _SheenWidth;
                float _SheenSpeed;
                float _SheenSoftness;
                float _EdgeGlowWidth;
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
                // 1. Base UI Texture
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // 2. Angled Specular Sheen Line
                float radiansAngle = _SheenAngle * (3.14159265 / 180.0);
                float2 pos = input.uv - 0.5;
                
                // Rotated UV for directional light line sweep
                float projectedPos = pos.x * cos(radiansAngle) - pos.y * sin(radiansAngle);
                
                // Animate sweep across button with looped timing
                float cycle = frac(_Time.y * _SheenSpeed) * 3.0 - 1.5;
                float distToBeam = abs(projectedPos - cycle);
                
                // Smooth light streak calculation
                float sheenIntensity = smoothstep(_SheenWidth + _SheenSoftness, _SheenWidth, distToBeam);

                // 3. Inner Edge / Border Glow
                float distFromCenter = max(abs(input.uv.x - 0.5), abs(input.uv.y - 0.5));
                float edgeGlow = smoothstep(0.5 - _EdgeGlowWidth, 0.5, distFromCenter);

                // 4. Combine Effects
                float3 activeSheen = _SheenColor.rgb * sheenIntensity;
                float3 activeEdge = _EdgeGlowColor.rgb * edgeGlow;
                
                // Composite additive gold metallic highlight over UI image
                float3 shinyImage = mainTex.rgb + (activeSheen + activeEdge) * mainTex.a;

                // Smoothly blend from standard UI to EA FC shine effect
                float blendFactor = smoothstep(0.0, 1.0, _EffectAmount);
                float3 finalRGB = lerp(mainTex.rgb, shinyImage, blendFactor);

                return float4(finalRGB, mainTex.a);
            }
            ENDHLSL
        }
    }
}
