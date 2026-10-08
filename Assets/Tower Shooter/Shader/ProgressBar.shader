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
            CBUFFER_END

            // Max array buffer for up to 16 custom milestone positions
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

                // Active horizontal path bound keeping Start/End circles fully inside texture rect
                float activePathBound = 0.5 - _StartEndRadius;

                // -------------------------------------------------------------
                // 1. Unfilled Background Shape SDF
                // -------------------------------------------------------------
                // Connect track center exactly across active path bounds
                float dTrack = sdBox(st, float2(activePathBound, halfBar));

                float dMilestones = 1e5;

                for (int i = 0; i < count; i++)
                {
                    float percent = _MilestonePercents[i];

                    // Map [0, 1] percent into [-activePathBound, activePathBound]
                    float posX = lerp(-activePathBound, activePathBound, saturate(percent));

                    bool isStartOrEnd = (i == 0 || i == count - 1);
                    float currentRadius = isStartOrEnd ? _StartEndRadius : _MidRadius;

                    float dCircle = sdCircle(st - float2(posX, 0.0), currentRadius);
                    dMilestones = min(dMilestones, dCircle);
                }

                float dFullShape = min(dTrack, dMilestones);

                // Anti-aliasing screen width
                float aa = fwidth(dFullShape);

                // -------------------------------------------------------------
                // 2. Masks & Padding
                // -------------------------------------------------------------
                float shapeMask = smoothstep(aa, -aa, dFullShape);

                float dPaddedShape = dFullShape + _FillPadding;
                float paddedMask = smoothstep(aa, -aa, dPaddedShape);

                float milestoneMask = smoothstep(aa, -aa, dMilestones);

                // -------------------------------------------------------------
                // 3. Progress Fill Mask
                // -------------------------------------------------------------
                // Map progress bar edge from start circle left bound to current fill target
                float progressX = lerp(-0.5, 0.5, saturate(_Progress));
                float fillMask = smoothstep(progressX + aa, progressX - aa, st.x) * paddedMask;

                // -------------------------------------------------------------
                // 4. Color Output
                // -------------------------------------------------------------
                float4 unfilledColor = lerp(_BgColor, _MilestoneBgColor, milestoneMask);
                float4 finalColor = lerp(unfilledColor, _FillColor, fillMask);
                finalColor.a *= shapeMask;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
