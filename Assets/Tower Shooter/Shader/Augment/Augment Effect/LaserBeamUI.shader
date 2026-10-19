Shader "UI/LaserBeamURP"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _LaserColor ("Laser Color", Color) = (0.2, 0.8, 1.0, 1.0)
        [HDR] _CoreColor ("Core Color", Color) = (1.0, 1.0, 1.0, 1.0)
        
        _BeamWidth ("Beam Core Width", Range(0.01, 0.5)) = 0.15
        _GlowWidth ("Glow Width", Range(0.05, 0.5)) = 0.4
        _NoiseScale ("Noise Scale", Float) = 20.0
        _ScrollSpeed ("Scroll Speed", Float) = 4.0
        _PulseSpeed ("Pulse Speed", Float) = 8.0
        _Intensity ("Laser Intensity", Float) = 2.0

        // Required for UI Canvas Masking
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
        Blend SrcAlpha One // Additive Blending for Laser Energy Glow
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_Laser_Beam"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

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
                float4 worldPosition: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _LaserColor;
            float4 _CoreColor;
            float _BeamWidth;
            float _GlowWidth;
            float _NoiseScale;
            float _ScrollSpeed;
            float _PulseSpeed;
            float _Intensity;

            // Simple 1D procedural noise for energy displacement
            float Hash1D(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float Noise1D(float x)
            {
                float i = floor(x);
                float f = frac(x);
                // Fixed: changed (3.0 - 2.0 * u) to (3.0 - 2.0 * f)
                float u = f * f * (3.0 - 2.0 * f); 
                return lerp(Hash1D(i), Hash1D(i + 1.0), u);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.worldPosition = input.positionOS;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // Center vertical UV (0 at center, 0.5 at edges)
                float distFromCenter = abs(uv.y - 0.5);

                // Add energy distortion along laser length (x-axis)
                float noiseVal = Noise1D(uv.x * _NoiseScale + _Time.y * _ScrollSpeed);
                float animatedDist = distFromCenter + (noiseVal - 0.5) * 0.05;

                // Pulse effect
                float pulse = 1.0 + 0.15 * sin(_Time.y * _PulseSpeed);

                // Core and Glow Falloffs
                float core = smoothstep(_BeamWidth * pulse, 0.0, animatedDist);
                float glow = smoothstep(_GlowWidth * pulse, 0.0, animatedDist);

                // Combine Core and Outer Energy Colors
                half4 finalColor = lerp(_LaserColor, _CoreColor, core) * (glow * _Intensity);
                finalColor.a = glow;

                // Multiply UI Tint & Vertex Color
                finalColor *= input.color;

                return finalColor;
            }
            ENDHLSL
        }
    }
}