Shader "Custom/UIStatHighlightBorder"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Highlight Settings)]
        _HighlightColor ("Highlight Color", Color) = (0.2, 0.9, 1.0, 1.0)
        
        [Header(Border Shimmer Sweep)]
        _Aspect ("Aspect Ratio (Width / Height)", Float) = 2.0
        _BorderThickness ("Border Thickness", Range(0.001, 0.2)) = 0.04
        _ShimmerWidth ("Shimmer Width", Range(0.01, 0.5)) = 0.15
        _ShimmerSpeed ("Shimmer Speed", Float) = 1.5
        _ShimmerBrightness ("Shimmer Brightness", Range(0, 10)) = 4.0
        _SweepAngle ("Sweep Angle (Degrees)", Range(0, 360)) = 45.0

        [Header(Stencil required for UI)]
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
            Name "UIStatHighlightBorderPass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _HighlightColor;
                float _Aspect;
                float _BorderThickness;
                float _ShimmerWidth;
                float _ShimmerSpeed;
                float _ShimmerBrightness;
                float _SweepAngle;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = TransformObjectToHClip(v.vertex.xyz);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord) * IN.color;

                if (texColor.a <= 0.001) discard;

                // 1. Manual Aspect Ratio Correction for Border Mask
                // Scale X coordinate space by the manual aspect ratio (_Aspect = Width / Height)
                float2 adjustedUV = IN.texcoord;
                float2 centeredUV = adjustedUV - 0.5;
                
                // Scale X to normalize coordinate space so top/bottom and left/right borders match thickness
                float normalizedX = centeredUV.x * _Aspect;
                float normalizedY = centeredUV.y;

                // Distance from border edges taking aspect ratio into account
                float2 distFromEdge = min(IN.texcoord, 1.0 - IN.texcoord);
                // Adjust horizontal edge distance by aspect ratio inverse
                float edgeX = distFromEdge.x * _Aspect;
                float edgeY = distFromEdge.y;
                float minEdgeDist = min(edgeX, edgeY);

                float borderMask = 1.0 - smoothstep(0.0, _BorderThickness, minEdgeDist);

                if (borderMask <= 0.001)
                {
                    return texColor;
                }

                // 2. Rotatable Shimmer Sweep Effect with Manual Aspect Correction
                float rad = radians(_SweepAngle);
                float cosA = cos(rad);
                float sinA = sin(rad);

                // Apply aspect correction to the rotation coordinates so the sweep angle doesn't warp
                float2 rotatedUV;
                rotatedUV.x = normalizedX * cosA - normalizedY * sinA;
                rotatedUV.y = normalizedX * sinA + normalizedY * cosA;

                float shimmerTime = frac(_Time.y * _ShimmerSpeed * 0.5) * 1.4 - 0.7;
                float distFromShimmer = abs(rotatedUV.x - shimmerTime);
                float shimmer = smoothstep(_ShimmerWidth, 0.0, distFromShimmer) * _ShimmerBrightness;

                // 3. Apply final glow restricted to border mask
                float4 dynamicGlow = _HighlightColor * shimmer * borderMask;

                float4 finalColor = texColor + dynamicGlow;
                finalColor.a = texColor.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}