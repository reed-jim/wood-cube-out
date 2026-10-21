Shader "Custom/URP_UIProceduralUnlockGlow"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.15, 0.15, 0.2, 0.95)
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.15
        _RectSize ("Rect Size (Width, Height)", Vector) = (100, 100, 0, 0)
        
        // Unlock sweep effect properties
        _SweepPosition ("Sweep Position", Range(-0.5, 1.5)) = -0.5
        _SweepWidth ("Sweep Width", Range(0.01, 0.5)) = 0.2
        _GlowColor ("Glow Color", Color) = (1, 0.85, 0.3, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.5
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
        }

        Pass
        {
            Name "Default"
            ZTest [unity_GUIZTestMode]
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

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

            float4 _Color;
            float _CornerRadius;
            float4 _RectSize;
            float _SweepPosition;
            float _SweepWidth;
            float4 _GlowColor;
            float _GlowIntensity;

            v2f vert(appdata v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = TransformObjectToHClip(v.vertex.xyz);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Signed Distance Function for a Rounded Box with any Aspect Ratio
            float RoundedBoxSDF(float2 centerPos, float2 size, float radius)
            {
                float2 q = abs(centerPos) - size + radius;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            half4 frag(v2f IN) : SV_Target
            {
                // Convert UV [0,1] to centered local space scaled by actual rect dimensions
                float2 size = _RectSize.xy;
                float2 localPos = (IN.texcoord - 0.5) * size;
                
                // Calculate max allowed corner radius based on smallest dimension
                float maxRadius = min(size.x, size.y) * 0.5 * _CornerRadius;
                float halfBoxSize = min(size.x, size.y) * 0.5; // Normalized bounding box for SDF

                // Distance field for rounded rectangle
                float dist = RoundedBoxSDF(localPos, size * 0.5, maxRadius);
                
                // Anti-aliased edge smoothing
                float alpha = 1.0 - smoothstep(-0.75, 0.75, dist);
                if (alpha <= 0.0) discard;

                half4 finalColor = IN.color;
                finalColor.a *= alpha;

                // Diagonal energy shine sweep effect
                float uvDist = (IN.texcoord.x + IN.texcoord.y) * 0.5;
                float distFromSweep = abs(uvDist - _SweepPosition);
                
                float sweep = saturate(1.0 - (distFromSweep / _SweepWidth));
                sweep = pow(sweep, 3.0); // Sharp beam falloff

                // Add procedural glow to the shape
                half3 glow = _GlowColor.rgb * sweep * _GlowIntensity * alpha;
                finalColor.rgb += glow;

                return finalColor;
            }
            ENDHLSL
        }
    }
}