Shader "Custom/URP_UIMilestoneCustomRipple"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1.0, 0.701, 0.0, 1) // FFB300
        _SuccessColor ("Success/Prompt Color", Color) = (1, 0.84, 0, 1) // Gold
        _UnlockProgress ("Unlock Progress (0 to 1)", Range(0,1)) = 0
        _Thickness ("Ring Thickness", Range(0.01, 0.3)) = 0.08
        
        // --- Milestone Base Customization ---
        _BaseRadius ("Base Ring Radius", Range(0.1, 0.45)) = 0.35

        // --- Final Idle Sun/Glow Customization ---
        _IdleCoreRadius ("Idle Solid Core Radius", Range(0.05, 0.4)) = 0.22
        _GlowFalloff ("Outer Glow Size", Range(0.05, 0.35)) = 0.18
        _GlowIntensity ("Glow Pulse Intensity", Range(0.0, 5.0)) = 2.5
        _PulseSpeed ("Idle Pulse Speed", Range(0.0, 10.0)) = 3.5
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float2 color        : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _SuccessColor;
                float _UnlockProgress;
                float _Thickness;
                float _BaseRadius;
                float _IdleCoreRadius;
                float _GlowFalloff;
                float _GlowIntensity;
                float _PulseSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv - 0.5;
                float dist = length(uv);

                // Smooth transition factors
                float unlockFactor = saturate(_UnlockProgress * 1.05);
                float invUnlock = 1.0 - unlockFactor;

                // 1. Base milestone ring boundary (fades out with bright additive retention)
                float outerRadius = _BaseRadius;
                float innerRadius = outerRadius - _Thickness;
                float baseRing = smoothstep(innerRadius, innerRadius + 0.01, dist) * 
                                 (1.0 - smoothstep(outerRadius - 0.01, outerRadius, dist)) * 
                                 invUnlock;

                // 2. Success Burst Wave
                float shockwaveRadius = _UnlockProgress * 0.7;
                float shockwave = smoothstep(shockwaveRadius - 0.04, shockwaveRadius, dist) * 
                                  (1.0 - smoothstep(shockwaveRadius, shockwaveRadius + 0.04, dist)) * 
                                  (1.0 - _UnlockProgress) * step(_UnlockProgress, 0.99);

                float centerFlash = (1.0 - smoothstep(0.0, outerRadius, dist)) * (1.0 - _UnlockProgress) * 0.8 * step(_UnlockProgress, 0.99);

                // 3. Final Idle State with Bright Additive Morphing
                float edgeSmooth = 0.01;
                
                float currentCoreRadius = lerp(_BaseRadius * 0.5, _IdleCoreRadius, smoothstep(0.5, 1.0, _UnlockProgress));
                float solidCore = (1.0 - smoothstep(currentCoreRadius - edgeSmooth, currentCoreRadius + edgeSmooth, dist)) * smoothstep(0.4, 0.85, _UnlockProgress);

                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float currentGlow = _GlowIntensity * pulse * smoothstep(0.6, 1.0, _UnlockProgress);
                
                float outerGlow = smoothstep(currentCoreRadius + _GlowFalloff, currentCoreRadius, dist) 
                                * smoothstep(currentCoreRadius - 0.02, currentCoreRadius, dist) 
                                * currentGlow;

                // Combined bright visual components using additive addition instead of dark clamping
                float idleVisual = solidCore + outerGlow;

                // Luminous color grading transition starting from the FFB300 base color
                float3 baseIdleColor = lerp(_Color.rgb, _SuccessColor.rgb, saturate(outerGlow / max(currentGlow, 0.001)));
                float3 activeColor = lerp(_Color.rgb, baseIdleColor, smoothstep(0.5, 1.0, _UnlockProgress));

                // Bright intensity composition during transition
                float finalIntensity = 1.0 + (shockwave * 6.0) + (centerFlash * 4.0);

                // Additive combination to prevent dark rings
                float3 finalColor = (_Color.rgb * baseRing * finalIntensity) + (activeColor * idleVisual);
                float finalAlpha = saturate(baseRing + shockwave + centerFlash + idleVisual) * _Color.a;
                
                // Edge guard to prevent quad bleeding
                float boundaryMask = 1.0 - smoothstep(0.46, 0.49, dist);
                finalAlpha *= boundaryMask;

                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}