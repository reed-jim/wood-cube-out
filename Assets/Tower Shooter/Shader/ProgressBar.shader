Shader "Custom/URP/ProgressBar"
{
    Properties
    {
        _Progress ("Progress", Range(0.0, 1.0)) = 0.5
        _MilestoneCount ("Milestone Count", Int) = 4
        _MilestoneRadius ("Milestone Radius", Range(0.05, 0.4)) = 0.25
        _BarHeight ("Bar Height", Range(0.05, 0.5)) = 0.15
        _FillPadding ("Fill Internal Padding", Range(0.0, 0.1)) = 0.02
        
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
                float _MilestoneRadius;
                float _BarHeight;
                float _FillPadding;
                float4 _FillColor;
                float4 _BgColor;
                float4 _MilestoneBgColor;
            CBUFFER_END

            // Signed Distance Function (SDF) for a Circle
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }

            // Signed Distance Function (SDF) for a Box
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
                // Remap UV coordinates to centered coordinate space [-0.5, 0.5]
                float2 st = input.uv - 0.5;

                int count = max(_MilestoneCount, 2);
                float radius = _MilestoneRadius;
                float halfBar = _BarHeight * 0.5;

                // -------------------------------------------------------------
                // 1. Calculate Unfilled Geometry Distances (SDF)
                // -------------------------------------------------------------
                // Horizontal track line distance
                float trackXBound = 0.5 - radius;
                float dTrack = sdBox(st, float2(trackXBound, halfBar));

                // Milestone circles distance
                float dMilestones = 1e5;
                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / (float)(count - 1);
                    float posX = lerp(-trackXBound, trackXBound, t);
                    float dCircle = sdCircle(st - float2(posX, 0.0), radius);
                    dMilestones = min(dMilestones, dCircle);
                }

                // Combine track and milestone circles into one unified background shape
                float dFullShape = min(dTrack, dMilestones);

                // Anti-aliasing threshold based on screen pixel derivatives
                float aa = fwidth(dFullShape);

                // -------------------------------------------------------------
                // 2. Shape Masks & Padding
                // -------------------------------------------------------------
                // Base background shape mask
                float shapeMask = smoothstep(aa, -aa, dFullShape);

                // Padded interior shape for progress fill
                float dPaddedShape = dFullShape + _FillPadding;
                float paddedMask = smoothstep(aa, -aa, dPaddedShape);

                // Distinguish between track background and milestone circles
                float milestoneMask = smoothstep(aa, -aa, dMilestones);

                // -------------------------------------------------------------
                // 3. Progress Fill Mask
                // -------------------------------------------------------------
                // Progress boundary mapped along horizontal UV range [-0.5, 0.5]
                float progressX = lerp(-0.5, 0.5, saturate(_Progress));
                float fillMask = smoothstep(progressX + aa, progressX - aa, st.x) * paddedMask;

                // -------------------------------------------------------------
                // 4. Color Compositing
                // -------------------------------------------------------------
                // Base background split: Milestone vs. Center Track
                float4 unfilledColor = lerp(_BgColor, _MilestoneBgColor, milestoneMask);

                // Composite fill color over background
                float4 finalColor = lerp(unfilledColor, _FillColor, fillMask);
                finalColor.a *= shapeMask; // Apply smooth alpha cutout for border edges

                return finalColor;
            }
            ENDHLSL
        }
    }
}
