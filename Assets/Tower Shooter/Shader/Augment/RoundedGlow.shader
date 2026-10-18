Shader "Custom/URP_SaferioRoundedGlowButton"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (0, 0.8, 1, 1)
        
        // SaferioButton integration property
        _GlowAmount ("Glow Amount", Range(0.0, 1.0)) = 0.0
        
        // Aspect Ratio control for non-square images (Width / Height)
        _AspectRatio ("Aspect Ratio (Width / Height)", Float) = 0.6977
        
        // Rounded Rect & Glow parameters
        _CornerRadius ("Corner Radius", Range(0.0, 0.2)) = 0.08
        _GlowIntensity ("Glow Intensity", Range(0.0, 5.0)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0.0, 10.0)) = 3.0
        
        // Stencil properties for Unity UI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "URPSaferioRoundedGlowPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _GlowAmount;
                float _AspectRatio;
                float _CornerRadius;
                float _GlowIntensity;
                float _PulseSpeed;
            CBUFFER_END

            // Signed Distance Field for a rounded box
            float RoundedBoxDistance(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Sample texture safely within [0, 1]
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Center coordinates from [0, 1] to [-0.5, 0.5]
                float2 p = input.uv - 0.5;
                
                // Make coordinate space isotropic
                p.x *= _AspectRatio;

                // Inner rectangle extents
                float rectHeight = 0.35; 
                float rectWidth = rectHeight * _AspectRatio;
                float2 rectExtent = float2(rectWidth, rectHeight);

                // Lower glow range so it stays tight and fades completely to 0 at the limits
                float glowFalloff = 0.08; 
                float edgeSmooth = 0.008;

                // Distance from the rounded box edge
                float dist = RoundedBoxDistance(p, rectExtent, _CornerRadius);

                // Pulse factor blended with SaferioButton's hover state (_GlowAmount)
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float hoverMultiplier = lerp(0.3, 1.0, _GlowAmount); 
                float currentGlow = _GlowIntensity * (0.5 + 0.5 * pulse) * hoverMultiplier;

                // Base solid shape mask (inside the rounded box)
                float bodyMask = smoothstep(edgeSmooth, -edgeSmooth, dist);

                // Tight outer glow aura fading smoothly to 0 before hitting the UV bounds
                float aura = smoothstep(glowFalloff, 0.0, dist) * pulse * _GlowAmount;

                // Combine elements together (Solid body + outer glowing aura)
                float alphaMask = saturate(bodyMask + aura);

                float3 finalColor = input.color.rgb * texColor.rgb * (bodyMask + aura * currentGlow * 0.6);

                return float4(finalColor, alphaMask * input.color.a * texColor.a);
            }
            ENDHLSL
        }
    }
}