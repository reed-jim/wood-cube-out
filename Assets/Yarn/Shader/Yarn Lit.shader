Shader "Saferio/Yarn"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
        _Tiling("Tiling", Vector) = (1, 1, 0, 0)
        _Offset("Offset", Vector) = (0, 0, 0, 0)
        _ObjectAlpha("Object Alpha", Float) = 1.0

        _WorldMinY("Dissolve Min Y", Float) = 0
        _WorldMaxY("Dissolve Max Y", Float) = 1
        _DissolveHeight("Dissolve Height", Float) = 0.5

        [Enum(Off,0,On,1)] _ZWrite("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float  localY : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _Tiling;
                float4 _Offset;
                float _ObjectAlpha;
                float _WorldMinY;
                float _WorldMaxY;
                float _DissolveHeight;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv * _Tiling.xy + _Offset.xy;
                OUT.localY = IN.positionOS.y;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Base color from texture
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 finalColor = texColor * _BaseColor;

                // Clamp local Y position between MinY and MaxY to 0–1
                float normalizedY = saturate((IN.localY - _WorldMinY) / max(_WorldMaxY - _WorldMinY, 1e-5));

                // Dissolve logic
                float dissolveAlpha = (normalizedY > _DissolveHeight) ? 0.0 : 1.0;

                // Final alpha = dissolveAlpha * ObjectAlpha
                finalColor.a *= dissolveAlpha * _ObjectAlpha;

                return finalColor;
            }

            ENDHLSL
        }
    }
}
