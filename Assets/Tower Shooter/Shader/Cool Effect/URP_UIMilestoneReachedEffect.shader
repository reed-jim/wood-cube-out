Shader "Custom/URP_UIMilestoneCustomRipple"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        _SuccessColor ("Success/Prompt Color", Color) = (1, 0.84, 0, 1) // Gold
        _UnlockProgress ("Unlock Progress (0 to 1)", Range(0,1)) = 0
        _Thickness ("Ring Thickness", Range(0.01, 1.3)) = 0.08
        
        // --- New Custom Ripple Properties ---
        _RippleRadius ("Prompt Ripple Max Radius", Range(0.2, 1.5)) = 0.6
        _RippleWidth ("Prompt Ripple Ring Width", Range(0.005, 0.1)) = 0.02
        _RippleSpeed ("Prompt Ripple Speed", Range(0.1, 5.0)) = 1.2
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
                float _RippleRadius;
                float _RippleWidth;
                float _RippleSpeed;
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

                // Base milestone ring boundary
                float outerRadius = 0.45;
                float innerRadius = outerRadius - _Thickness;
                float ring = smoothstep(innerRadius, innerRadius + 0.01, dist) * 
                             (1.0 - smoothstep(outerRadius - 0.01, outerRadius, dist));

                // State checks
                float isUnlocked = step(0.99, _UnlockProgress);
                float isAnimating = step(_UnlockProgress, 0.99) * step(0.01, _UnlockProgress);

                // 1. One-time Success Burst
                float shockwaveRadius = _UnlockProgress * 0.7;
                float shockwave = smoothstep(shockwaveRadius - 0.04, shockwaveRadius, dist) * 
                                  (1.0 - smoothstep(shockwaveRadius, shockwaveRadius + 0.04, dist)) * 
                                  (1.0 - _UnlockProgress) * isAnimating;

                float centerFlash = (1.0 - smoothstep(0.0, outerRadius, dist)) * (1.0 - _UnlockProgress) * 0.8 * isAnimating;

                // 2. Click-Prompt Idle Loop (Now driven by custom properties)
                float promptTime = frac(_Time.y * _RippleSpeed); 
                float promptWaveRadius = promptTime * _RippleRadius;
                
                // Using custom _RippleWidth for edge crispness/thickness
                float promptWave = smoothstep(promptWaveRadius - _RippleWidth, promptWaveRadius, dist) * 
                                   (1.0 - smoothstep(promptWaveRadius, promptWaveRadius + _RippleWidth, dist)) * 
                                   (1.0 - promptTime) * isUnlocked; 

                // Breathing pulse effect
                float promptPulse = 1.0 + 0.35 * sin(_Time.y * 4.0) * isUnlocked;

                // Color transition
                float3 activeColor = lerp(_Color.rgb, _SuccessColor.rgb, saturate(_UnlockProgress + isUnlocked));

                // Intensity composition
                float finalIntensity = 1.0 + (shockwave * 5.0) + (centerFlash * 3.0) + (promptWave * 4.0);
                finalIntensity *= (1.0 + (promptPulse - 1.0) * isUnlocked);

                // Final color and alpha blending
                float3 finalColor = activeColor * (ring + shockwave + centerFlash + promptWave) * finalIntensity;
                float finalAlpha = saturate(ring + shockwave + centerFlash + promptWave) * _Color.a;
                
                float promptRingAlpha = ring * _SuccessColor.a * (0.85 + 0.15 * sin(_Time.y * 4.0)) * isUnlocked;
                finalAlpha = max(finalAlpha, promptRingAlpha);

                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}