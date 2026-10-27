Shader "Sprite/Fantasy/MagmaGolem"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        [HDR] _RockColor ("Crusted Rock Color", Color) = (0.15, 0.12, 0.12, 1.0)
        [HDR] _LavaColor ("Magma Fissure Color", Color) = (1.0, 0.4, 0.05, 1.0)
        [HDR] _CoreGlow ("Superhot Core", Color) = (1.0, 0.9, 0.5, 1.0)
        
        _Heat ("Internal Heat Pressure", Range(0.0, 100.0)) = 65.0
        _FlowSpeed ("Magma Flow Speed", Float) = 1.2
        _CrackScale ("Fissure Density", Float) = 6.0
        _Exposure ("Exposure", Range(0.2, 4.0)) = 1.5
        _Aspect ("Aspect Ratio (W/H)", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha

        Pass
        {
            Name "MagmaGolemPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _RockColor;
                float4 _LavaColor;
                float4 _CoreGlow;
                float _Heat;
                float _FlowSpeed;
                float _CrackScale;
                float _Exposure;
                float _Aspect;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv - 0.5;
                float2 pp = float2(uv.x * _Aspect, uv.y);
                float t = _Time.y * _FlowSpeed;
                float power = saturate(_Heat * 0.01);

                float r = length(pp);
                float mask = smoothstep(0.48, 0.35, r);

                // Cracking Voronoi-style simulated fissures
                float2 cellCoord = pp * _CrackScale + float2(0.0, t * 0.3);
                float n1 = vnoise(cellCoord);
                float n2 = vnoise(cellCoord * 2.0 - t * 0.5);
                float fissure = abs(n1 - 0.5) + abs(n2 - 0.5);
                fissure = smoothstep(0.05, 0.35, fissure); // Invert lines

                float heatPulse = 0.8 + 0.2 * sin(_Time.y * 5.0 * power);
                
                // Color composite
                float3 rock = _RockColor.rgb * (0.5 + 0.5 * n1);
                float3 magma = lerp(_LavaColor.rgb, _CoreGlow.rgb, power) * (1.0 - fissure) * 3.0 * heatPulse;
                
                float3 hdr = (rock * fissure + magma) * _Exposure * mask;
                float3 ldr = 1.0 - exp(-hdr);

                ldr += (hash21(input.positionCS.xy) - 0.5) / 255.0;
                float alpha = saturate(mask * (ldr.r + ldr.g + ldr.b) * 1.5);

                half4 finalCol = half4(ldr, alpha);
                finalCol.rgb *= input.color.rgb * input.color.a;
                finalCol.a *= input.color.a;
                return finalCol;
            }
            ENDHLSL
        }
    }
}