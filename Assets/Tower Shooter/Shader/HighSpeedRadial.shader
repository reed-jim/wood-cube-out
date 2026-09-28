Shader "UI/HighSpeedRadial"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        // Radial Fill controls
        _FillAmount ("Fill Amount", Range(0,1)) = 1.0
        _FillClockwise ("Clockwise (1=True, 0=False)", Float) = 1.0
        _FillRotation ("Fill Starting Angle", Range(0, 360)) = 0.0

        // 0 = normal image
        // 1 = high-speed visualization
        _HighSpeed ("High Speed", Range(0,1)) = 0

        // Number of radial streaks
        _StreakCount ("Streak Count", Range(1,64)) = 12

        // Length of each streak in radians
        _StreakLength ("Streak Length", Range(0.01,1.5)) = 0.35

        // Softness of streak edges
        _StreakSoftness ("Streak Softness", Range(0.001,1)) = 0.15

        // Additional angular movement
        _Rotation ("Rotation", Float) = 0

        // Brightness of the high-speed effect
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

                float4 _Color;
                float4 _MainTex_ST;

                float _FillAmount;
                float _FillClockwise;
                float _FillRotation;

                float _HighSpeed;
                float _StreakCount;
                float _StreakLength;
                float _StreakSoftness;
                float _Rotation;
                float _Intensity;

            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
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

            // Returns 0..1 starting from top (12 o'clock)
            float Angle01(float2 uv)
            {
                float2 p = uv - 0.5;

                // atan2(x, y) starts at 12 o'clock (0 rad pointing up)
                float angle = atan2(p.x, p.y);

                // Convert from -PI..PI to 0..1
                float angle01 = angle / (2.0 * PI);
                if (angle01 < 0.0)
                {
                    angle01 += 1.0;
                }

                return angle01;
            }

            float RadialDistance(float2 uv)
            {
                return length(uv - 0.5) * 2.0;
            }

            float StreakPattern(float angle01)
            {
                float angle = angle01 * 2.0 * PI;

                // Move pattern around the circle
                angle += _Rotation;

                // Repeat
                float repeated = frac(angle / (2.0 * PI) * _StreakCount);

                // Center each streak around 0.5
                float d = abs(repeated - 0.5);

                // Convert streak length
                float width = _StreakLength * 0.5;
                float softness = max(_StreakSoftness * 0.5, 0.001);

                float streak = 1.0 - smoothstep(width, width + softness, d);

                return streak;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float radialDistance = RadialDistance(input.uv);
                float angle = Angle01(input.uv);

                // --- 360 RADIAL FILL MASK ---
                // Adjust for custom fill rotation angle
                float fillAngle = frac(angle - (_FillRotation / 360.0));
                
                // Reverse direction if counter-clockwise
                if (_FillClockwise < 0.5)
                {
                    fillAngle = 1.0 - fillAngle;
                }

                // Step mask to hide pixels past the fill amount
                float fillMask = step(fillAngle, _FillAmount);

                // --- STREAK & HIGH SPEED CALCULATIONS ---
                float normal = tex.a;
                float streak = StreakPattern(angle);

                float outerMask = smoothstep(0.05, 0.25, radialDistance);
                float innerMask = 1.0 - smoothstep(0.85, 1.0, radialDistance);
                float radialMask = outerMask * innerMask;

                float highSpeed = streak * radialMask * _Intensity;

                // Mix normal image and high-speed representation
                float alpha = lerp(normal, max(normal, highSpeed), _HighSpeed);

                float3 color = tex.rgb * input.color.rgb;

                // Slightly brighten high-speed streaks
                color = lerp(color, color * (1.0 + highSpeed), _HighSpeed);

                // Apply fill mask to final alpha
                alpha *= fillMask;

                return half4(color, alpha * input.color.a);
            }

            ENDHLSL
        }
    }
}
