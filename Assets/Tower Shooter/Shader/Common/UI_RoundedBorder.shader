Shader "Custom/URP_UI_RoundedBorder"
{
    Properties
    {
        _Color ("Fill Color", Color) = (1,1,1,1)
        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _Radius ("Corner Radius", Range(0, 200)) = 20.0
        _BorderThickness ("Border Thickness", Range(0, 50)) = 5.0
        _Smoothness ("Anti-aliasing Smoothness", Range(0.1, 5.0)) = 1.0
        
        [HideInInspector] _RectSize ("Rect Size", Vector) = (100, 100, 0, 0)
        
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
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
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
            Name "UI_RoundedBorder"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float2 localPos     : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _BorderColor;
                float4 _RectSize;
                float _Radius;
                float _BorderThickness;
                float _Smoothness;
            CBUFFER_END

            float sdRoundedBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _Color;
                output.uv = input.uv;
                output.localPos = (input.uv - 0.5) * _RectSize.xy;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 halfSize = _RectSize.xy * 0.5;
                float maxRadius = min(halfSize.x, halfSize.y);
                float radius = min(_Radius, maxRadius);

                float dist = sdRoundedBox(input.localPos, halfSize, radius);

                float smoothing = _Smoothness * 0.707;
                float alpha = 1.0 - smoothstep(-smoothing, smoothing, dist);

                if (alpha <= 0.0)
                    discard;

                float borderInner = -_BorderThickness;
                half4 finalColor = _Color;
                
                if (_BorderThickness > 0.0)
                {
                    float borderAlpha = smoothstep(borderInner - smoothing, borderInner + smoothing, dist);
                    finalColor = lerp(_Color, _BorderColor, borderAlpha);
                }

                finalColor *= input.color;
                finalColor.a *= alpha;

                return finalColor;
            }
            ENDHLSL
        }
    }
}