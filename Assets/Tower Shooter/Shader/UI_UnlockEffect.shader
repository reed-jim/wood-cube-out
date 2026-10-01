Shader "UI/URP/UnlockEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Progress)]
        _UnlockProgress ("Unlock Progress (0 to 1)", Range(0, 1)) = 0.0

        [Header(Glow Effect)]
        _ShineColor ("Shine Color", Color) = (2, 1.8, 1.0, 1.0) // HDR bright gold
        _ShineWidth ("Shine Width", Range(0.01, 0.3)) = 0.1
        _GradiantSoftness ("Softness", Range(0.01, 0.2)) = 0.05

        // Stencil settings for Canvas UI Masking
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
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShineColor;
                float _UnlockProgress;
                float _ShineWidth;
                float _GradiantSoftness;
            CBUFFER_END

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
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // Locked state base color (desaturated & darkened)
                half gray = dot(texColor.rgb, half3(0.2126, 0.7152, 0.0722));
                half3 lockedRGB = half3(gray, gray, gray) * 0.4;

                // Calculate diagonal wave front position across UV (range 0.0 to 2.0)
                float wavePos = (input.uv.x + input.uv.y) * 0.5;
                
                // Remap progress so the wave fully enters and clears the frame
                float progress = _UnlockProgress * (1.0 + _ShineWidth * 2.0) - _ShineWidth;

                // Blend factor for unlocked state
                float blendMask = smoothstep(progress - _GradiantSoftness, progress, wavePos);

                // Energy shine line along the edge of transition
                float shineDist = abs(wavePos - progress);
                float shineMask = smoothstep(_ShineWidth, 0.0, shineDist) * (1.0 - step(1.0, _UnlockProgress));

                // Combine locked and unlocked textures
                half3 finalRGB = lerp(texColor.rgb, lockedRGB, blendMask);

                // Add bloom shine
                finalRGB += _ShineColor.rgb * shineMask * texColor.a;

                return half4(finalRGB, texColor.a);
            }
            ENDHLSL
        }
    }
}