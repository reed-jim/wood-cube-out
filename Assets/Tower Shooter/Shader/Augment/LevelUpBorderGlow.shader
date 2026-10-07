Shader "Custom/UI/LevelUpBorderGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _GlowColor ("Glow Color", Color) = (0.2, 1.0, 0.9, 1.0)
        
        _ShimmerSpeed ("Shimmer Speed", Float) = 1.5
        _ShimmerWidth ("Shimmer Width", Range(0.01, 0.5)) = 0.12
        _ShimmerAngle ("Shimmer Angle Deg", Range(-180, 180)) = 45.0
        
        _PulseSpeed ("Pulse Speed", Float) = 3.0
        _PulseMinMax ("Pulse Intensity Min/Max", Vector) = (0.7, 1.3, 0, 0)

        // UI Masking Properties
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
            "CanScaleLarge"="True"
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
            #pragma target 2.0

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
                float4 _GlowColor;
                float4 _PulseMinMax;
                float _ShimmerSpeed;
                float _ShimmerWidth;
                float _ShimmerAngle;
                float _PulseSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample main texture (UI Sprite mask/border)
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // 1. Calculate Pulsing Energy
                float pulse = lerp(_PulseMinMax.x, _PulseMinMax.y, 0.5 + 0.5 * sin(_Time.y * _PulseSpeed));

                // 2. Calculate Directional Shimmer Sweep
                float rad = _ShimmerAngle * (3.14159265 / 180.0);
                float2 dir = float2(cos(rad), sin(rad));
                
                // Project UV along direction vector
                float projectedUV = dot(input.uv - float2(0.5, 0.5), dir);
                
                // Continuous animated sweep across card coordinates [-1.5, 1.5]
                float sweepPos = frac(_Time.y * _ShimmerSpeed * 0.3) * 3.0 - 1.5;
                
                // Distance to shimmer line centered at sweepPos
                float dist = abs(projectedUV - sweepPos);
                float shimmerIntensity = smoothstep(_ShimmerWidth, 0.0, dist);

                // 3. Combine Base Border Color with Shimmer and Pulse
                half3 finalColor = lerp(col.rgb * _Color.rgb * pulse, _GlowColor.rgb * 1.5, shimmerIntensity);

                return half4(finalColor, col.a * _Color.a);
            }
            ENDHLSL
        }
    }
}