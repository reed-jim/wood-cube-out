Shader "UI/URP/2DSwirlTornado"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Tornado Progress", Range(0, 1)) = 0.0
        _Angle ("Swirl Strength", Float) = 15.0
        _Center ("Center Offset", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Progress;
                float _Angle;
                float2 _Center;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv - _Center;
                float dist = length(uv);

                // Vortex Swirl Rotation based on progress and distance
                float percent = (1.0 - dist) * _Progress;
                float theta = percent * percent * _Angle;
                float sinTheta = sin(theta);
                float cosTheta = cos(theta);

                // Rotate UVs
                uv = float2(dot(uv, float2(cosTheta, -sinTheta)), dot(uv, float2(sinTheta, cosTheta)));
                uv += _Center;

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color;
                
                // Pinch/Collapse inward as progress reaches 1
                texColor.a *= smoothstep(1.0, 0.2, dist * _Progress);

                return texColor;
            }
            ENDHLSL
        }
    }
}