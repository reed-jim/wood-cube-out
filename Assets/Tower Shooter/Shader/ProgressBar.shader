Shader "Custom/URP/ProgressBar"
{
    Properties
    {
        _Progress ("Progress", Range(0.0, 1.0)) = 0.5
        _MilestoneCount ("Milestone Count", Int) = 4
        
        [Header(Milestone Sizes)]
        _StartEndRadius ("Start/End Milestone Radius", Range(0.05, 0.4)) = 0.20
        _MidRadius ("Middle Milestone Radius", Range(0.05, 0.4)) = 0.12
        
        [Header(Bar Dimensions)]
        _BarHeight ("Bar Height", Range(0.02, 0.5)) = 0.08
        _FillPadding ("Fill Internal Padding", Range(0.0, 0.1)) = 0.015
        
        [Header(Colors)]
        [HDR] _FillColor ("Fill Color", Color) = (0.0, 0.8, 1.0, 1.0)
        [HDR] _BgColor ("Background Color", Color) = (0.2, 0.2, 0.2, 1.0)
        [HDR] _MilestoneBgColor ("Milestone Unfilled Color", Color) = (0.3, 0.3, 0.3, 1.0)
        
        [Header(Secondary Animations)]
        [HDR] _PulseColor ("Leading Edge Pulse Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _PulseWidth ("Edge Glow Width", Range(0.001, 0.1)) = 0.03
        _SheenSpeed ("Sheen Animation Speed", Range(0.0, 5.0)) = 1.5
        _SheenFrequency ("Sheen Density", Range(1.0, 50.0)) = 20.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
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
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Progress;
                int _MilestoneCount;
                float _StartEndRadius;
                float _MidRadius;
                float _BarHeight;
                float _FillPadding;
                float4 _FillColor;
                float4 _BgColor;
                float4 _MilestoneBgColor;
                
                // New Properties for Animation
                float4 _PulseColor;
                float _PulseWidth;
                float _SheenSpeed;
                float _SheenFrequency;
            CBUFFER_END

            float _MilestonePercents[16];

            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }

            float sdBox(float2 p, float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 st = input.uv - 0.5;

                int count = clamp(_MilestoneCount, 2, 16);
                float halfBar = _BarHeight * 0.5;
                float activePathBound = 0.5 - _StartEndRadius;

                // -------------------------------------------------------------
                // 1. Shapes & SDF Setup
                // -------------------------------------------------------------
                float dTrack = sdBox(st, float2(activePathBound, halfBar));
                float dMilestones = 1e5;
                float milestonePulseRings = 0.0;

                for (int i = 0; i < count; i++)
                {
                    float percent = _MilestonePercents[i];
                    float posX = lerp(-activePathBound, activePathBound, saturate(percent));
                    bool isStartOrEnd = (i == 0 || i == count - 1);
                    float currentRadius = isStartOrEnd ? _StartEndRadius : _MidRadius;

                    float2 milestoneCenter = st - float2(posX, 0.0);
                    float dCircle = sdCircle(milestoneCenter, currentRadius);
                    dMilestones = min(dMilestones, dCircle);

                    // ---------------------------------------------------------
                    // Secondary Anim 1: Reached Milestone Breathing Halo
                    // ---------------------------------------------------------
                    if (_Progress >= percent)
                    {
                        float pulseRadius = currentRadius + sin(_Time.y * 4.0 + i) * 0.015 + 0.015;
                        float ringDist = abs(length(milestoneCenter) - pulseRadius);
                        milestonePulseRings += smoothstep(0.015, 0.0, ringDist);
                    }
                }

                float dFullShape = min(dTrack, dMilestones);
                float aa = fwidth(dFullShape);

                // -------------------------------------------------------------
                // 2. Base Masks
                // -------------------------------------------------------------
                float shapeMask = smoothstep(aa, -aa, dFullShape);
                float dPaddedShape = dFullShape + _FillPadding;
                float paddedMask = smoothstep(aa, -aa, dPaddedShape);
                float milestoneMask = smoothstep(aa, -aa, dMilestones);

                // -------------------------------------------------------------
                // 3. Progress Fill Mask & Edge Calculation
                // -------------------------------------------------------------
                float progressX = lerp(-0.5, 0.5, saturate(_Progress));
                float fillMask = smoothstep(progressX + aa, progressX - aa, st.x) * paddedMask;

                // -------------------------------------------------------------
                // Secondary Anim 2: Leading Tip Glow / Energy Wave
                // -------------------------------------------------------------
                float distToEdge = abs(st.x - progressX);
                float edgeGlow = smoothstep(_PulseWidth, 0.0, distToEdge) * paddedMask;
                // Add rhythm to the leading pulse
                edgeGlow *= (0.8 + 0.2 * sin(_Time.y * 8.0));

                // -------------------------------------------------------------
                // Secondary Anim 3: Moving Diagonal Sheen / Shimmer inside Fill
                // -------------------------------------------------------------
                float sheenPattern = sin((st.x + st.y) * _SheenFrequency - _Time.y * _SheenSpeed * 5.0);
                sheenPattern = smoothstep(0.7, 1.0, sheenPattern) * 0.25;

                // -------------------------------------------------------------
                // 4. Color Compositing
                // -------------------------------------------------------------
                float4 unfilledColor = lerp(_BgColor, _MilestoneBgColor, milestoneMask);
                
                // Fill Base + Sheen Effect
                float4 animatedFillColor = _FillColor + sheenPattern;
                
                // Composite Base Fill
                float4 finalColor = lerp(unfilledColor, animatedFillColor, fillMask);

                // Add Leading Edge Glow
                finalColor.rgb += _PulseColor.rgb * edgeGlow * fillMask;

                // Add Active Milestone Ring Halos
                finalColor.rgb += _PulseColor.rgb * milestonePulseRings * 0.5;

                finalColor.a *= shapeMask;

                return finalColor;
            }
            ENDHLSL
        }
    }
}