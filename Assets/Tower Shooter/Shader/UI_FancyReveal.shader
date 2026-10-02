Shader "UI/URP/FancyReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Transition Settings)]
        _NoiseTex ("Noise / Pattern Map (R)", 2D) = "white" {}
        _Progress ("Reveal Progress", Range(0, 1)) = 0.0
        _Feather ("Edge Softness", Range(0.001, 0.5)) = 0.05
        
        [Header(Glow Edge)]
        _EdgeWidth ("Glow Width", Range(0.0, 0.2)) = 0.05
        [HDR] _EdgeColor ("Glow Color", Color) = (2, 1, 0, 1)

        // UI Canvas Masking Support
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
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float4 _Color;
                float4 _EdgeColor;
                float _Progress;
                float _Feather;
                float _EdgeWidth;
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
                // Main UI texture
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                
                // Noise / Dissolve pattern
                float2 noiseUV = TRANSFORM_TEX(input.uv, _NoiseTex);
                float noiseVal = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                // Smooth step reveal mask
                float cutout = smoothstep(_Progress - _Feather, _Progress + _Feather, noiseVal);
                
                // Glowing border calculation
                float glowMask = smoothstep(_Progress - _Feather - _EdgeWidth, _Progress, noiseVal) - cutout;
                
                // Combine main image with glow outline
                float3 finalRGB = lerp(mainTex.rgb, _EdgeColor.rgb, glowMask * _EdgeColor.a);
                float finalAlpha = mainTex.a * (1.0 - cutout);

                return float4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}