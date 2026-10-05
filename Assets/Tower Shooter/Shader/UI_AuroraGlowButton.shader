Shader "Custom/UI_AuroraGlowButton"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture (UI / Shape Mask)", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.0, 0.4, 0.7, 1.0)
        _GlowColor ("Glow Color", Color) = (0.0, 0.9, 1.0, 1.0)
        _BorderColor ("Border Glow Color", Color) = (1.0, 1.0, 1.0, 1.0)
        
        _AuroraSpeed ("Aurora Animation Speed", Float) = 0.8
        _AuroraScale ("Aurora Frequency / Scale", Float) = 5.0
        
        _BorderThickness ("Border Glow Thickness", Range(0.01, 0.3)) = 0.08
        _BorderPower ("Border Brightness Power", Float) = 2.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Unlit"

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
                float4 _MainTex_ST;
                float4 _BaseColor;
                float4 _GlowColor;
                float4 _BorderColor;
                float _AuroraSpeed;
                float _AuroraScale;
                float _BorderThickness;
                float _BorderPower;
            CBUFFER_END

            // Simple pseudo-random hash for sparkling particles
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

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
                float2 uv = input.uv;
                float time = _Time.y * _AuroraSpeed;

                // 1. Base UI Texture Alpha / Shape
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // 2. Animated Aurora Waves (Procedural HLSL math)
                float ray1 = sin(uv.x * _AuroraScale + time + sin(uv.y * 4.0 + time * 0.5)) * 0.5 + 0.5;
                float ray2 = cos(uv.x * (_AuroraScale * 1.5) - time * 1.2 + sin(uv.y * 6.0)) * 0.5 + 0.5;
                float auroraPattern = pow(ray1 * ray2, 1.5) * 2.5;

                // 3. Floating Sparkles / Stars
                float2 gridUV = frac(uv * 12.0) - 0.5;
                float2 gridID = floor(uv * 12.0);
                float randVal = Hash21(gridID);
                float sparkle = smoothstep(0.08, 0.0, length(gridUV)) * step(0.75, randVal);
                sparkle *= (sin(time * 3.0 + randVal * 6.28) * 0.5 + 0.5); // Pulse sparkle

                // Combine Background + Aurora Rays + Sparkles
                float3 finalRGB = _BaseColor.rgb + (_GlowColor.rgb * auroraPattern) + (sparkle * 0.8);

                // 4. Outer Border Glow Highlight
                // Distance from UV edge (0 to 0.5)
                float2 edgeDist = min(uv, 1.0 - uv);
                float minDist = min(edgeDist.x, edgeDist.y);
                float borderFactor = 1.0 - smoothstep(0.0, _BorderThickness, minDist);
                borderFactor = pow(borderFactor, _BorderPower);

                // Add rim/border glow onto final color
                finalRGB = lerp(finalRGB, _BorderColor.rgb, borderFactor);

                // Multiply by Vertex/UI Color & Texture Alpha
                float finalAlpha = texColor.a * input.color.a;

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}
