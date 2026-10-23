Shader "URP/UI/SpriteLoLGlowSmooth"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Smooth Glow Settings)]
        [HDR] _GlowColor ("Glow Color (HDR)", Color) = (0, 0.8, 1, 1)
        _GlowIntensity ("Glow Power", Range(0, 10)) = 4.0
        _GlowRadius ("Glow Radius (Spread)", Range(0.01, 0.3)) = 0.15
        _InnerBleed ("Inner Core Bleed", Range(0, 1)) = 0.35
        
        [Header(Pulse Animation)]
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseMin ("Pulse Min Scale", Range(0.2, 1.0)) = 0.7
        
        [Header(UI Stenciling)]
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
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            Name "LoLGlowSmoothPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
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
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowRadius;
            float _InnerBleed;
            float _PulseSpeed;
            float _PulseMin;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.worldPosition = v.vertex;
                o.vertex = TransformObjectToHClip(o.worldPosition.xyz);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                
                return o;
            }

            // High-density smooth radial sampling to eliminate petal artifacts
            float SampleSmoothAura(float2 uv)
            {
                float totalAlpha = 0.0;
                float totalWeight = 0.0;
                
                // 4 concentric rings with 12 samples each for smooth circular distribution
                const int rings = 4;
                const int samplesPerRing = 12;
                const float angleStep = 6.28318530718 / float(samplesPerRing);

                [unroll(4)]
                for (int r = 1; r <= rings; r++)
                {
                    float t = float(r) / float(rings);
                    // Non-linear distribution spreads samples densely near the edge and softer outward
                    float dist = _GlowRadius * t; 
                    float weight = exp(-t * 2.0); // Exponential decay falloff weight

                    [unroll(12)]
                    for (int s = 0; s < samplesPerRing; s++)
                    {
                        // Add a small angular offset per ring to prevent overlapping grid lines
                        float angle = (float(s) + (float(r) * 0.5)) * angleStep;
                        float2 offset = float2(cos(angle), sin(angle)) * dist;
                        
                        float a = _MainTex.Sample(sampler_MainTex, uv + offset).a;
                        totalAlpha += a * weight;
                        totalWeight += weight;
                    }
                }

                return totalAlpha / totalWeight;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 texColor = _MainTex.Sample(sampler_MainTex, i.texcoord) * i.color;

                // Pulsing animation math
                float wave = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float pulse = lerp(_PulseMin, 1.0, wave * wave);

                // Sample smooth radial aura
                float smoothAura = SampleSmoothAura(i.texcoord);

                float outerGlow = saturate(smoothAura - texColor.a);
                float innerGlow = smoothAura * texColor.a * _InnerBleed;
                float glowMask = outerGlow + innerGlow;

                // HDR glow accumulation
                float3 finalGlowRGB = _GlowColor.rgb * (_GlowIntensity * pulse) * glowMask;

                float3 finalRGB = (texColor.rgb * texColor.a) + finalGlowRGB;
                float finalAlpha = saturate(texColor.a + (glowMask * _GlowColor.a));

                return float4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}