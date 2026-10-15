Shader "Custom/URP_UI_UltraSmoothImpactShockwave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1)
        
        [Header(Transition Controls)]
        _Progress ("Trigger Progress (0 to 1)", Range(0, 1)) = 0.0

        [Header(Banner Colors)]
        _StartColor ("Banner Idle Color (Progress 0)", Color) = (0.08, 0.1, 0.18, 1)
        _SurgeColor ("Surge Peak Color", Color) = (0.0, 0.5, 0.9, 1)

        [Header(Ultra Smooth Impact Shockwave Settings)]
        _ImpactCoreColor ("Impact Core Color", Color) = (1.0, 0.98, 0.75, 1) // Soft Warm Impact Glow
        _LightningColor ("Energy Arc Color", Color) = (0.3, 0.85, 1.0, 1)   // Smooth Cyan Blue
        
        _WaveWidth ("Shockwave Width", Float) = 70.0
        _WaveIntensity ("Shockwave Brightness", Float) = 6.5
        _DisintegrationSpeed ("Dissolve Span", Range(0.1, 1.0)) = 0.7

        [Header(Rounded Corner Settings (For 960x160))]
        _CornerRadius ("Corner Radius (Pixels)", Float) = 60.0
        _RectSize ("Image Size (Width, Height)", Vector) = (960, 160, 0, 0)
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "URP_UI_UltraSmoothImpactShockwavePass"

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

            float4 _Color;
            float _Progress;

            float4 _StartColor;
            float4 _SurgeColor;
            float4 _ImpactCoreColor;
            float4 _LightningColor;
            
            float _WaveWidth;
            float _WaveIntensity;
            float _DisintegrationSpeed;

            float _CornerRadius;
            float4 _RectSize;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float RoundedBoxSDF(float2 samplePos, float2 halfSize, float radius)
            {
                float2 d = abs(samplePos) - (halfSize - radius);
                return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - radius;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 34.45);
                return frac(p.x * p.y);
            }

            float SmoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Ultra-smooth, silky screen drift/shake
                float impactShake = sin(_Time.y * 55.0) * step(0.01, _Progress) * step(_Progress, 0.95) * 0.0015;
                float2 jitteredUV = i.uv + float2(impactShake, impactShake * 0.4);

                float2 pixelPos = (jitteredUV - 0.5) * _RectSize.xy;
                float2 halfSize = _RectSize.xy * 0.5;

                float distToEdge = RoundedBoxSDF(pixelPos, halfSize, _CornerRadius);
                float cornerAlpha = 1.0 - smoothstep(-1.5, 1.5, distToEdge);
                
                if (cornerAlpha <= 0.0)
                    discard;

                if (_Progress <= 0.0)
                {
                    float4 startOutput = i.color * _StartColor;
                    return float4(startOutput.rgb, startOutput.a * cornerAlpha);
                }

                // 1. Silky Smooth 600x100 Filled Impact Crater
                float2 craterHalfSize = float2(300.0, 50.0); 
                float craterSDF = RoundedBoxSDF(pixelPos, craterHalfSize, 25.0);
                float solidCraterFill = 1.0 - smoothstep(-30.0, 50.0, craterSDF);

                // 2. Extra Smooth Circular Shockwave Ring
                float maxRadius = length(halfSize);
                float currentWaveRadius = _Progress * maxRadius * 1.5;
                float pixelDistFromCenter = length(pixelPos);

                // Soft inner/outer sweep interpolation
                float hasBeenSwept = smoothstep(currentWaveRadius - 15.0, currentWaveRadius + 15.0, pixelDistFromCenter);
                hasBeenSwept = 1.0 - hasBeenSwept;

                float distToWaveFront = abs(pixelDistFromCenter - currentWaveRadius);
                float shockwaveMask = smoothstep(_WaveWidth, 0.0, distToWaveFront);
                float coreMask = smoothstep(_WaveWidth * 0.5, 0.0, distToWaveFront);

                // 3. Blended Organic Dissolve Noise
                float noise1 = SmoothNoise(pixelPos * 0.04 + float2(_Time.y * 6.0, 0.0));
                float noise2 = SmoothNoise(pixelPos * 0.12 - float2(0.0, _Time.y * 10.0));
                float combinedNoise = lerp(noise1, noise2, 0.5);

                float timeSincePassed = currentWaveRadius - pixelDistFromCenter;
                float dissolveSpan = _DisintegrationSpeed * maxRadius;

                // 4. Silky Color Blending
                float3 idleRGB = (i.color * _StartColor).rgb;
                float3 bannerBody = lerp(idleRGB, _SurgeColor.rgb * 1.3, hasBeenSwept * 0.6);
                
                // Buttery smooth impact bloom fading out gracefully
                float impactFade = 1.0 - smoothstep(0.0, 0.75, _Progress);
                float3 solidImpactBurst = _ImpactCoreColor.rgb * solidCraterFill * impactFade * 3.5;

                float3 shockwaveEffect = (_ImpactCoreColor.rgb * coreMask * _WaveIntensity) + 
                                         (_LightningColor.rgb * shockwaveMask * 2.2) +
                                         (_LightningColor.rgb * combinedNoise * hasBeenSwept * 2.5) + 
                                         solidImpactBurst;

                float3 finalRGB = bannerBody + shockwaveEffect;

                // 5. Ultra-Smooth Dissolve to Absolute Pure Transparent
                float dissolveDriver = smoothstep(0.0, 1.0, _Progress) * 1.4;
                float dissolveThreshold = combinedNoise - dissolveDriver;
                
                // Wide, soft alpha mask smoothing out any jagged edges
                float alphaMask = 1.0 - (hasBeenSwept * (1.0 - smoothstep(0.0, 0.3, dissolveThreshold)));

                if (hasBeenSwept > 0.5 && timeSincePassed > dissolveSpan * 0.4)
                {
                    alphaMask = 0.0;
                }

                float finalAlpha = (i.color * _StartColor).a * cornerAlpha * alphaMask;

                if (_Progress >= 0.98)
                {
                    finalAlpha = 0.0;
                }

                return float4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}