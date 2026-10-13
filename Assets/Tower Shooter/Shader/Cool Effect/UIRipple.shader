Shader "Custom/URP_UI_Ripple"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _RippleSpeed ("Ripple Speed", Float) = 4.0
        _RippleFrequency ("Ripple Frequency", Float) = 20.0
        _RippleAmplitude ("Ripple Amplitude", Float) = 0.01
        _EdgeFade ("Edge Feathering", Range(0.0, 0.5)) = 0.1
        
        // Required properties to prevent UI clipping issues
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
            Name "UI Ripple"

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
                float _RippleSpeed;
                float _RippleFrequency;
                float _RippleAmplitude;
                float _EdgeFade;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Shift UVs to center (-0.5 to 0.5)
                float2 centeredUV = input.uv - 0.5;
                float distanceToCenter = length(centeredUV);

                // Create an edge mask that drops amplitude to 0 near the 0.5 borders
                // This prevents UV sampling outside the texture bounds (clipping)
                float2 edgeFactor = smoothstep(0.5, 0.5 - _EdgeFade, abs(centeredUV));
                float edgeMask = edgeFactor.x * edgeFactor.y;

                // Calculate ripple distortion scaled down by the edge mask
                float ripple = sin(distanceToCenter * _RippleFrequency - _Time.y * _RippleSpeed);
                float2 distortedUV = input.uv + (normalize(centeredUV) * ripple * _RippleAmplitude * edgeMask);

                // Sample texture with the safely clamped/faded UVs
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);

                // Multiply by vertex color
                return texColor * input.color;
            }
            ENDHLSL
        }
    }
}