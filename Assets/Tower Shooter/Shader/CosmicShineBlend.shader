Shader "UI/URP/CosmicShineBlend"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Effect Blend Control)]
        _EffectAmount ("Cosmic Intensity", Range(0, 1)) = 0.0
        _BlendMode ("Blend Softness", Range(0.01, 1.0)) = 0.5

        [Header(Cosmic Colors)]
        [HDR] _NebulaColor1 ("Nebula Color 1 (Purple/Blue)", Color) = (0.3, 0.1, 1.5, 1)
        [HDR] _NebulaColor2 ("Nebula Color 2 (Cyan/Pink)", Color) = (1.5, 0.2, 0.9, 1)
        [HDR] _StarColor ("Star / Supernova Sparkle", Color) = (3.0, 3.0, 4.5, 1)
        
        _CosmicScale ("Cosmic Pattern Scale", Range(1.0, 20.0)) = 5.0
        _CosmicSpeed ("Shine Motion Speed", Range(0.0, 5.0)) = 0.8
        _StarDensity ("Star Sparkle Density", Range(1.0, 50.0)) = 25.0
        _OverlayOpacity ("Max Cosmic Glow Strength", Range(0.0, 2.0)) = 1.0

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

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _NebulaColor1;
                float4 _NebulaColor2;
                float4 _StarColor;
                float _EffectAmount;
                float _BlendMode;
                float _CosmicScale;
                float _CosmicSpeed;
                float _StarDensity;
                float _OverlayOpacity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            // Pseudo-random noise for animated star sparkles
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Base UI Texture
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                
                // 2. Continuous Motion Time
                float time = _Time.y * _CosmicSpeed;

                // 3. Dynamic Swirling Plasma Pattern
                float2 cosmicUV = input.uv * _CosmicScale;
                float wave1 = sin(cosmicUV.x + time) * cos(cosmicUV.y - time);
                float wave2 = cos(cosmicUV.x * 1.5 - time * 0.8) * sin(cosmicUV.y * 1.5 + time * 0.8);
                float nebulaPattern = saturate((wave1 + wave2) * 0.5 + 0.5);
                
                float3 nebulaRGB = lerp(_NebulaColor1.rgb, _NebulaColor2.rgb, nebulaPattern);

                // 4. Twinkling Star Sparkles
                float2 grid = floor(input.uv * _StarDensity);
                float randomVal = hash21(grid);
                float starFlicker = pow(sin(time * 4.0 + randomVal * 6.28) * 0.5 + 0.5, 6.0) * step(0.65, randomVal);
                
                float3 cosmicEffect = nebulaRGB + (_StarColor.rgb * starFlicker);

                // 5. Smooth Transition Control
                // Linear to smoothstep easing for silky smooth crossfading
                float smoothBlend = smoothstep(0.0, 1.0, _EffectAmount);

                // Combine normal texture with cosmic overlay using Screen/Additive style blending
                float3 cosmicLayer = mainTex.rgb + (cosmicEffect * mainTex.rgb * _OverlayOpacity);
                float3 finalRGB = lerp(mainTex.rgb, cosmicLayer, smoothBlend);

                return float4(finalRGB, mainTex.a);
            }
            ENDHLSL
        }
    }
}