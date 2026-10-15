Shader "Custom/URP_UI_VerticalShockwaveEmberBlend"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.1, 0.12, 0.18, 1)
        
        [Header(Transition Controls)]
        _Progress ("Shockwave Progress", Range(0, 1)) = 0.0

        [Header(Shockwave Fire Settings)]
        _WaveColor ("Wave Color", Color) = (0, 0.9, 1, 1)
        _FireColor ("Fire Trail Color", Color) = (1, 0.4, 0.1, 1)
        _EmberAshColor ("Ember Cooling Color", Color) = (0.2, 0.08, 0.05, 1)
        _BorderPulseColor ("Border Pulse Color", Color) = (0, 1, 1, 1)
        
        _WaveWidth ("Wave Width (Pixels)", Float) = 6.0
        _WaveIntensity ("Wave Brightness", Float) = 4.0
        _FireDuration ("Fire Residual Life (0-1)", Range(0.05, 0.8)) = 0.3
        _BorderWidth ("Border Energy Width (Pixels)", Float) = 4.0

        [Header(Rounded Corner Settings)]
        _CornerRadius ("Corner Radius (Pixels)", Float) = 16.0
        _RectSize ("Image Size (Width, Height)", Vector) = (200, 100, 0, 0)
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
            Name "UI_VerticalShockwaveEmberBlendPass"

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

            float4 _WaveColor;
            float4 _FireColor;
            float4 _EmberAshColor;
            float4 _BorderPulseColor;
            
            float _WaveWidth;
            float _WaveIntensity;
            float _FireDuration;
            float _BorderWidth;

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

            float4 frag(v2f i) : SV_Target
            {
                // 1. Aspect-Correct Pixel Positions from Center
                float2 pixelPos = (i.uv - 0.5) * _RectSize.xy;
                float2 halfSize = _RectSize.xy * 0.5;

                // Rounded Corners & Outer Edge Mask
                float distToEdge = RoundedBoxSDF(pixelPos, halfSize, _CornerRadius);
                float cornerAlpha = 1.0 - smoothstep(-1.0, 1.0, distToEdge);
                
                if (cornerAlpha <= 0.0)
                    discard;

                float4 baseColor = i.color;

                if (_Progress <= 0.0)
                    return float4(baseColor.rgb, baseColor.a * cornerAlpha);

                // 2. Aspect-Correct Coordinates & Waves
                float maxRadius = halfSize.x;
                float currentWaveRadiusPixels = _Progress * maxRadius;
                float pixelDistFromCenterX = abs(pixelPos.x);

                float hasBeenSwept = step(pixelDistFromCenterX, currentWaveRadiusPixels);
                float distToWaveFront = abs(pixelDistFromCenterX - currentWaveRadiusPixels);
                float waveFrontMask = smoothstep(_WaveWidth, 0.0, distToWaveFront);

                // 3. Smooth Fire-to-Ash Cooling Decay (Removes the hard line)
                float timeSincePassedPixels = currentWaveRadiusPixels - pixelDistFromCenterX;
                float fireWindowPixels = _FireDuration * maxRadius;
                
                // Normalized life ratio of the fire trail (0 at wave front, 1 at the tail end)
                float fireDecay = saturate(timeSincePassedPixels / fireWindowPixels);
                float activeFireFactor = (1.0 - fireDecay) * hasBeenSwept;

                // Organic cooling noise so the ash dissolves smoothly instead of a flat wall
                float coolNoise = sin(pixelPos.y * 0.15 + pixelPos.x * 0.1 + _Time.y * 3.0) * 0.5 + 0.5;
                
                // Smooth falloff curve for the trailing edge
                float trailFade = smoothstep(1.0, 0.2, fireDecay) * hasBeenSwept;

                // 4. Energy Border Pulse
                float borderDist = abs(distToEdge);
                float borderMask = smoothstep(_BorderWidth, 0.0, borderDist) * hasBeenSwept;
                float borderPulse = sin(_Time.y * 15.0 - pixelPos.x * 0.1) * 0.5 + 0.5;
                float3 borderEffect = _BorderPulseColor.rgb * borderMask * (borderPulse * 2.0 + 1.0);

                // 5. Color Blending: Base -> Cooling Ash -> Bright Fire -> Leading Shockwave
                float3 waveEffect = _WaveColor.rgb * waveFrontMask * _WaveIntensity;
                
                // Blend from bright fire color down to a warm cooling ash/ember tone before merging with base color
                float3 fireTrailColor = lerp(_FireColor.rgb, _EmberAshColor.rgb, fireDecay);
                float3 burnedArea = fireTrailColor * activeFireFactor * (coolNoise * 0.5 + 0.7);

                // Smoothly fade the alpha of the trail so it melts back into the badge's original texture/color
                float finalAlpha = baseColor.a * cornerAlpha;
                
                // Combine layers
                float3 finalRGB = lerp(baseColor.rgb, burnedArea + borderEffect, trailFade) + waveEffect;

                return float4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}