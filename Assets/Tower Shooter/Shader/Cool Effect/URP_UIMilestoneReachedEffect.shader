Shader "Custom/URP_UIMilestoneCustomRipple"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.5, 0.5, 0.5, 1)
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
                float4 color        : COLOR;
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

                // 1. Base milestone ring boundary (driven by _BaseRadius & _Thickness)
                float outerRadius = _BaseRadius;
                float innerRadius = outerRadius - _Thickness;
                float ring = smoothstep(innerRadius, innerRadius + 0.01, dist) * 
                             (1.0 - smoothstep(outerRadius - 0.01, outerRadius, dist));

                // State checks
                float isUnlocked = step(0.99, _UnlockProgress);
                float isAnimating = step(_UnlockProgress, 0.99) * step(0.01, _UnlockProgress);

                // 2. One-time Success Burst
                float shockwaveRadius = _UnlockProgress * 0.7;
                float shockwave = smoothstep(shockwaveRadius - 0.04, shockwaveRadius, dist) * 
                                  (1.0 - smoothstep(shockwaveRadius, shockwaveRadius + 0.04, dist)) * 
                                  (1.0 - _UnlockProgress) * isAnimating;

                float centerFlash = (1.0 - smoothstep(0.0, outerRadius, dist)) * (1.0 - _UnlockProgress) * 0.8 * isAnimating;

                // 3. Final Idle State: Solid Core + Breathing Outer Glow Ring
                float edgeSmooth = 0.01;
                
                // Solid white/success core circle
                float solidCore = 1.0 - smoothstep(_IdleCoreRadius - edgeSmooth, _IdleCoreRadius + edgeSmooth, dist);

                // Animated pulsing aura around the core
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float currentGlow = _GlowIntensity * (0.6 + 0.4 * pulse);
                
                float outerGlow = smoothstep(_IdleCoreRadius + _GlowFalloff, _IdleCoreRadius, dist) 
                                * smoothstep(_IdleCoreRadius - 0.05, _IdleCoreRadius, dist) 
                                * currentGlow;

                // Combine visual components for the unlocked idle state
                float idleVisual = (solidCore + outerGlow) * isUnlocked;

                // Color mixing: Core remains bright/white, outer aura matches success tint
                float3 baseIdleColor = lerp(float3(1, 1, 1), _SuccessColor.rgb, outerGlow / max(currentGlow, 0.001));
                float3 activeColor = lerp(_Color.rgb, baseIdleColor, isUnlocked);

                // Burst intensity boost during animation
                float finalIntensity = 1.0 + (shockwave * 5.0) + (centerFlash * 3.0);

                // Final color and alpha blending
                float3 finalColor = activeColor * ((ring * finalIntensity) + idleVisual);
                float finalAlpha = saturate(ring + shockwave + centerFlash + idleVisual) * _Color.a;
                
                // Edge guard to prevent clipping bleeding at sprite quad edges
                float boundaryMask = 1.0 - smoothstep(0.46, 0.49, dist);
                finalAlpha *= boundaryMask;

                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}