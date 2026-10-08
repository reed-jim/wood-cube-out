Shader "UI/HighSpeedRadial"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        [Header(Radial Fill)]
        _FillAmount ("Fill Amount", Range(0,1)) = 1.0
        [Toggle] _FillClockwise ("Clockwise", Float) = 1.0
        _FillRotation ("Fill Starting Angle", Range(0, 360)) = 0.0

        [Header(High Speed Effects)]
        _HighSpeed ("High Speed Toggle/Blend", Range(0,1)) = 0
        [HDR] _StreakColor ("Streak Tint Color", Color) = (2, 2, 2, 1)
        _StreakCount ("Streak Count", Range(1,64)) = 12
        _StreakLength ("Streak Length", Range(0.01,1.5)) = 0.35
        _StreakSoftness ("Streak Softness", Range(0.001,1)) = 0.15
        _Rotation ("Rotation Offset", Float) = 0
        _Intensity ("Intensity", Range(0,3)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIHighSpeed"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_ST;

                half _FillAmount;
                half _FillClockwise;
                half _FillRotation;

                half _HighSpeed;
                half4 _StreakColor;
                half _StreakCount;
                half _StreakLength;
                half _StreakSoftness;
                half _Rotation;
                half _Intensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color       : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half4 color        : COLOR;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            // Clean 0..1 angle mapping starting from top (12 o'clock)
            half Angle01(float2 uv)
            {
                float2 p = uv - 0.5;
                return (half)frac(atan2(p.x, p.y) / (2.0 * PI));
            }

            half RadialDistance(float2 uv)
            {
                return (half)(length(uv - 0.5) * 2.0);
            }

            half StreakPattern(half angle01)
            {
                half angle = angle01 * (2.0 * (half)PI) + _Rotation;
                half repeated = frac(angle / (2.0 * (half)PI) * _StreakCount);
                half d = abs(repeated - 0.5);

                half width = _StreakLength * 0.5;
                half softness = max(_StreakSoftness * 0.5, (half)0.001);

                return (half)1.0 - smoothstep(width, width + softness, d);
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                half radialDist = RadialDistance(input.uv);
                half angle = Angle01(input.uv);

                // --- 360 RADIAL FILL MASK (Branchless) ---
                half fillAngle = frac(angle - (_FillRotation / 360.0));
                fillAngle = lerp(1.0 - fillAngle, fillAngle, step(0.5, _FillClockwise));
                half fillMask = step(fillAngle, _FillAmount);

                // --- STREAK & MASK CALCULATIONS ---
                half normalAlpha = tex.a;
                half streak = StreakPattern(angle);

                half outerMask = smoothstep(0.05, 0.25, radialDist);
                half innerMask = 1.0 - smoothstep(0.85, 1.0, radialDist);
                half radialMask = outerMask * innerMask;

                half highSpeed = streak * radialMask * _Intensity;

                // --- COLOR & ALPHA COMPOSITING ---
                half3 baseColor = tex.rgb * input.color.rgb;
                
                // Additive HDR glow for streaks
                half3 streakGlow = _StreakColor.rgb * highSpeed;
                half3 finalColor = lerp(baseColor, baseColor + streakGlow, _HighSpeed);

                half alpha = lerp(normalAlpha, max(normalAlpha, highSpeed), _HighSpeed);
                alpha *= fillMask * input.color.a;

                return half4(finalColor, alpha);
            }

            ENDHLSL
        }
    }
}