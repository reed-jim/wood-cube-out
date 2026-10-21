Shader "Custom/URP_UIProceduralUnlockGlow"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.1, 0.1, 0.15, 0.95)
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.15
        _RectSize ("Rect Size (Width, Height)", Vector) = (100, 100, 0, 0)
        
        [Header(Unlock State Settings)]
        _UnlockProgress ("Unlock Progress", Range(0, 1)) = 0.0
        _LockColor ("Lock State Tint", Color) = (0.1, 0.08, 0.15, 0.95)
        _LockStripeColor ("Lock Pattern Color", Color) = (0.5, 0.1, 0.15, 0.7)

        [Header(Sweep Effect)]
        _SweepPosition ("Sweep Position", Range(-0.5, 1.5)) = -0.5
        _SweepWidth ("Sweep Width", Range(0.01, 0.5)) = 0.2
        _GlowColor ("Glow Color", Color) = (1, 0.85, 0.3, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 3.5

        [Header(Border Rim Flash)]
        _BorderWidth ("Border Width", Range(0, 20)) = 4.0
        _BorderColor ("Border Glow Color", Color) = (1, 0.9, 0.4, 1.0)
        _UnlockFlash ("Unlock Flash Intensity", Range(0, 2)) = 0.0

        [Header(Docking Effect)]
        _DockingEffect ("Docking Absorption Effect", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
        }

        Pass
        {
            Name "Default"
            ZTest [unity_GUIZTestMode]
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;
            float _CornerRadius;
            float4 _RectSize;
            
            float _UnlockProgress;
            float4 _LockColor;
            float4 _LockStripeColor;

            float _SweepPosition;
            float _SweepWidth;
            float4 _GlowColor;
            float _GlowIntensity;
            float _BorderWidth;
            float4 _BorderColor;
            float _UnlockFlash;
            float _DockingEffect;

            v2f vert(appdata v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = TransformObjectToHClip(v.vertex.xyz);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }

            // Signed Distance Function for Rounded Box
            float RoundedBoxSDF(float2 centerPos, float2 size, float radius)
            {
                float2 q = abs(centerPos) - size + radius;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            half4 frag(v2f IN) : SV_Target
            {
                float2 size = _RectSize.xy;
                float2 localPos = (IN.texcoord - 0.5) * size;
                
                float maxRadius = min(size.x, size.y) * 0.5 * _CornerRadius;

                // Distance field for rounded rectangle
                float dist = RoundedBoxSDF(localPos, size * 0.5, maxRadius);
                
                // Anti-aliased smooth edge
                float alpha = 1.0 - smoothstep(-0.75, 0.75, dist);
                if (alpha <= 0.0) discard;

                // 1. Dynamic Base Color Transition: Locked -> Active Unlocked[cite: 1]
                half4 targetBaseColor = IN.color * _Color;
                half4 lockedBaseColor = IN.color * _LockColor;
                half4 finalColor = lerp(lockedBaseColor, targetBaseColor, _UnlockProgress);

                // 2. High-Voltage Plasma Grid (Active only when locked)
                float gridLine = sin(localPos.x * 0.2 + _Time.y * 5.0) * cos(localPos.y * 0.2 - _Time.y * 3.0);
                float hazardStripe = sin((IN.texcoord.x + IN.texcoord.y) * 35.0 + (_Time.y * 4.0));
                float plasmaMask = saturate(gridLine * hazardStripe) * (1.0 - _UnlockProgress);
                
                half3 plasmaColor = _LockStripeColor.rgb * (1.5 + sin(_Time.y * 10.0) * 0.5);
                finalColor.rgb = lerp(finalColor.rgb, plasmaColor, plasmaMask * 0.85);

                // 3. Border Rim Light
                float borderDist = abs(dist);
                float borderMask = 1.0 - smoothstep(0.0, _BorderWidth, borderDist);
                
                half3 lockedBorder = half3(1.0, 0.2, 0.1) * (1.5 + sin(_Time.y * 12.0) * 0.5);
                half3 activeBorder = _BorderColor.rgb * (2.5 + _UnlockFlash * 4.0);
                half3 finalBorderColor = lerp(lockedBorder, activeBorder, _UnlockProgress);
                half3 borderGlow = finalBorderColor * borderMask;

                // 4. Chromatic Aberration Energy Sweep Beam
                float uvDist = (IN.texcoord.x + IN.texcoord.y) * 0.5;
                float sweepR = saturate(1.0 - (abs((uvDist + 0.008) - _SweepPosition) / _SweepWidth));
                float sweepG = saturate(1.0 - (abs(uvDist - _SweepPosition) / _SweepWidth));
                float sweepB = saturate(1.0 - (abs((uvDist - 0.008) - _SweepPosition) / _SweepWidth));
                
                sweepR = pow(sweepR, 4.0);
                sweepG = pow(sweepG, 4.0);
                sweepB = pow(sweepB, 4.0);

                float sweepPower = 0.3 + 0.7 * _UnlockProgress;
                half3 sweepGlow = _GlowColor.rgb * half3(sweepR * 1.2, sweepG, sweepB * 1.5) * _GlowIntensity * sweepPower;
                
                finalColor.rgb += sweepGlow + borderGlow;

                // 5. Docking Absorption & Implosion Effect (Plays upon arrival at destination)
                float centerDist = length(localPos);
                float maxDim = max(size.x, size.y);
                float absorbRadius = (1.0 - _DockingEffect) * maxDim * 0.7;
                float absorbRing = smoothstep(0.0, 15.0, abs(centerDist - absorbRadius)) * _DockingEffect;
                half3 absorbFlash = _GlowColor.rgb * (1.0 - smoothstep(0.0, maxDim * 0.5 * (1.0 - _DockingEffect), centerDist)) * _DockingEffect * 4.0;
                
                finalColor.rgb += (_GlowColor.rgb * absorbRing * 5.0) + absorbFlash;

                // Collapse alpha smoothly as docking absorption completes
                alpha *= (1.0 - _DockingEffect);
                
                finalColor.a *= alpha;
                return finalColor;
            }
            ENDHLSL
        }
    }
}