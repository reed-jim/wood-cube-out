Shader "UI/URP/LockState"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Lock Settings)]
        _Desaturation ("Desaturation Amount", Range(0, 1)) = 0.85
        _Darken ("Darken Factor", Range(0, 1)) = 0.5
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.15
        _ScanlineFrequency ("Scanline Frequency", Float) = 150.0

        // Required for UI Canvas masking
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
                float _Desaturation;
                float _Darken;
                float _ScanlineIntensity;
                float _ScanlineFrequency;
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

                // 1. Grayscale Conversion (Luminance weights)
                half gray = dot(texColor.rgb, half3(0.2126, 0.7152, 0.0722));
                half3 lockedRGB = lerp(texColor.rgb, half3(gray, gray, gray), _Desaturation);

                // 2. Darken Factor
                lockedRGB *= _Darken;

                // 3. Diagonal Scanlines
                float scanline = sin((input.uv.x + input.uv.y) * _ScanlineFrequency) * 0.5 + 0.5;
                lockedRGB -= scanline * _ScanlineIntensity * texColor.a;

                return half4(saturate(lockedRGB), texColor.a);
            }
            ENDHLSL
        }
    }
}
