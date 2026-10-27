Shader "Sprite/Fantasy/VoidWraith"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        [HDR] _CoreColor ("Void Core Color", Color) = (0.4, 0.1, 0.8, 1.0)
        [HDR] _MiasmaColor ("Miasma Edge Color", Color) = (0.05, 0.8, 0.6, 1.0)
        [HDR] _AbyssColor ("Abyss Background", Color) = (0.02, 0.02, 0.05, 1.0)
        
        _Corruption ("Corruption Level", Range(0.0, 1.0)) = 0.6
        _PulseSpeed ("Pulse Speed", Float) = 2.5
        _Distortion ("Warp Intensity", Range(0.0, 1.0)) = 0.4
        _Exposure ("Exposure", Range(0.2, 4.0)) = 1.3
        _Aspect ("Aspect Ratio (W/H)", Float) = 1.0
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
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "VoidWraithPass"
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
                float4 _CoreColor;
                float4 _MiasmaColor;
                float4 _AbyssColor;
                float _Corruption;
                float _PulseSpeed;
                float _Distortion;
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

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float2x2 m = float2x2(0.8, -0.6, 0.6, 0.8);
                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    v += a * vnoise(p);
                    p = mul(m, p) * 2.02 + 17.1;
                    a *= 0.5;
                }
                return v / 0.9375;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv - 0.5;
                float2 pp = float2(uv.x * _Aspect, uv.y);
                float t = _Time.y * _PulseSpeed;

                // Organic shape distortion via FBM
                float2 warpCoord = pp * 3.0 + float2(t * 0.2, -t * 0.3);
                float n = fbm(warpCoord);
                float2 warpedUV = pp + (n - 0.5) * _Distortion * 0.4;

                float r = length(warpedUV);
                float angle = atan2(warpedUV.y, warpedUV.x);

                // Wraith silhouette envelope (spiky organic blob)
                float envelope = smoothstep(0.45, 0.1, r + sin(angle * 6.0 + t) * 0.05 * _Corruption);

                // Internal swirling nebula clouds
                float cloud = fbm(warpedUV * 5.0 - t * 0.5);
                float coreGlow = exp(-r * (3.0 - _Corruption * 1.5)) * (0.8 + 0.2 * sin(t * 3.0));

                // Color blending
                float3 col = lerp(_AbyssColor.rgb, _MiasmaColor.rgb, cloud);
                col = lerp(col, _CoreColor.rgb, coreGlow * 1.5);
                
                // Additive energy wisps
                float wisp = pow(clamp(1.0 - r * 2.0, 0.0, 1.0), 2.0) * cloud;
                col += wisp * _CoreColor.rgb * 2.0;

                float3 hdr = col * _Exposure * envelope;
                float3 ldr = 1.0 - exp(-hdr);

                // Dither to minimize banding
                ldr += (hash21(input.positionCS.xy) - 0.5) / 255.0;
                float alpha = saturate(envelope * (ldr.r + ldr.g + ldr.b) * 1.5);

                half4 finalCol = half4(ldr, alpha);
                finalCol.rgb *= input.color.rgb * input.color.a;
                finalCol.a *= input.color.a;
                return finalCol;
            }
            ENDHLSL
        }
    }
}