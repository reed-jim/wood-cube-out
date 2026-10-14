Shader "Custom/UI_ShockwaveGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Transition / Shockwave Properties
        _Progress ("Transition Progress (0 to 1)", Range(0, 1)) = 0.0
        _ShockwaveWidth ("Shockwave Width", Float) = 0.1
        _Center ("Shockwave Center (UV)", Vector) = (0.5, 0.5, 0, 0)

        // Idle Glow Properties
        _GlowColor ("Glow Color", Color) = (0, 1, 1, 1)
        _GlowRadius ("Glow Radius", Range(0, 0.5)) = 0.3
        _GlowSpeed ("Glow Speed", Float) = 3.0
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.5

        // Required for UI Masking
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
            Comp [_StencilOp]
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

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _GlowColor;
                float4 _Center;
                float _Progress;
                float _ShockwaveWidth;
                float _GlowRadius;
                float _GlowSpeed;
                float _GlowIntensity;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float dist = distance(uv, _Center.xy);
                
                float4 finalColor = float4(0, 0, 0, 0);

                // --- 1. SHOCKWAVE TRANSITION ---
                if (_Progress > 0.0 && _Progress < 1.0)
                {
                    float currentRadius = _Progress * 1.414;
                    float innerEdge = currentRadius - _ShockwaveWidth;
                    float outerEdge = currentRadius;

                    if (dist >= innerEdge && dist <= outerEdge)
                    {
                        float waveFactor = sin((dist - innerEdge) / _ShockwaveWidth * 3.14159);
                        finalColor.rgb += float3(1, 1, 1) * waveFactor * 0.8;
                        finalColor.a = max(finalColor.a, waveFactor);
                    }
                }

                // --- 2. IDLE GLOW CIRCLE ---
                float idleFactor = saturate(_Progress); 
                if (idleFactor >= 1.0)
                {
                    float pulse = (sin(_Time.y * _GlowSpeed) + 1.0) * 0.5;
                    float glowDist = distance(uv, _Center.xy);
                    
                    if (glowDist < _GlowRadius)
                    {
                        float glowFalloff = 1.0 - (glowDist / _GlowRadius);
                        float finalGlow = glowFalloff * pulse * _GlowIntensity;
                        
                        finalColor.rgb += _GlowColor.rgb * finalGlow;
                        finalColor.a = max(finalColor.a, glowFalloff * pulse);
                    }
                }

                return finalColor * i.color;
            }
            ENDHLSL
        }
    }
}