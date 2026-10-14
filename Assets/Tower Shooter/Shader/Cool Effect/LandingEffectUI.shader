Shader "URP/UI/SpaceshipTopDownLandingSmoothStart"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _SkyColor ("Beam Core Color", Color) = (0.2, 0.6, 1.0, 1.0)
        _GroundColor ("Touchdown Shockwave Color", Color) = (1.0, 0.8, 0.2, 1.0)
        _Progress ("Landing Progress (0 = Start, 1 = Finished)", Range(0, 1)) = 0
        _CoreRadius ("Descent Core Radius", Range(0.01, 0.5)) = 0.1
        _HeatDistortionStrength ("Atmospheric Heat Distortion", Range(0, 0.1)) = 0.03
        _EdgeSoftness ("Outer Edge Feathering", Range(0.01, 0.5)) = 0.3
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
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SkyColor;
                float4 _GroundColor;
                float _Progress;
                float _CoreRadius;
                float _HeatDistortionStrength;
                float _EdgeSoftness;
            CBUFFER_END

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // If progress has reached 1.0, completely turn off/hide graphic
                if (_Progress >= 1.0)
                {
                    return float4(0, 0, 0, 0);
                }

                float2 uv = i.uv;
                float2 center = float2(0.5, 0.5);

                // 1. Atmospheric Heat Distortion Wave
                float timeVal = _Time.y * 4.0;
                float heatNoiseX = noise(uv * 15.0 + timeVal);
                float heatNoiseY = noise(uv * 15.0 - timeVal);
                float2 distortedUV = (uv - center) + (float2(heatNoiseX, heatNoiseY) - 0.5) * _HeatDistortionStrength * (1.0 - _Progress);

                float dist = length(distortedUV);

                // 2. Descent Beam (collapsing inward as progress moves)
                float currentBeamRadius = lerp(0.8, _CoreRadius, _Progress);
                float beamMask = smoothstep(currentBeamRadius, 0.0, dist);
                float coreGlow = smoothstep(_CoreRadius * 1.5, 0.0, dist) * 2.0;

                // 3. Ground Touchdown Shockwave Rings
                float shockwaveProgress = saturate((_Progress - 0.4) / 0.6);
                float ringRadius = shockwaveProgress * 0.7;
                float ringWidth = 0.05;
                
                float shockwaveRing = smoothstep(ringRadius - ringWidth, ringRadius, dist) 
                                    - smoothstep(ringRadius, ringRadius + ringWidth, dist);

                float touchdownFlash = saturate((_Progress - 0.85) / 0.15) * saturate(1.0 - dist * 2.0);

                // 4. Color Blending
                float3 col = _SkyColor.rgb * (beamMask * 2.0 + coreGlow);
                col += _GroundColor.rgb * (shockwaveRing * 5.0 + touchdownFlash * 4.0);

                // 5. SMRAH / START FADE-IN WIPE
                // Smoothly fades in the master opacity from 0 to 1 during the very first 15% of progress
                float fadeIn = saturate(_Progress / 0.15);

                // 6. Outer Edge Vignette / Feather Mask
                float edgeMask = 1.0 - smoothstep(0.5 - _EdgeSoftness, 0.5, dist);

                // 7. End Fade-Out Wipe
                float fadeOut = 1.0 - saturate((_Progress - 0.85) / 0.15);

                // Combine all alpha factors for a seamless start and end
                float finalAlpha = saturate(beamMask + shockwaveRing + coreGlow + touchdownFlash) * edgeMask * fadeIn * fadeOut;

                return float4(col, finalAlpha);
            }
            ENDHLSL
        }
    }
}