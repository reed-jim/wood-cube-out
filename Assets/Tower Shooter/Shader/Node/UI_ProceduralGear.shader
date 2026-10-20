Shader "URP/UI_ProceduralGear"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GearColor ("Gear Color", Color) = (0.8, 0.8, 0.8, 1)
        _InnerColor ("Inner Cutout Color", Color) = (0.2, 0.2, 0.2, 1)
        
        // Gear Parameters
        _ToothCount ("Tooth Count", Float) = 8
        _ToothDepth ("Tooth Depth", Range(0.0, 0.2)) = 0.08
        _ToothWidth ("Tooth Width", Range(0.1, 0.9)) = 0.5
        _InnerRadius ("Inner Hole Radius", Range(0.0, 0.8)) = 0.3
        _OuterRadius ("Outer Radius", Range(0.1, 0.9)) = 0.7
        _Smoothness ("Edge Smoothness", Range(0.001, 0.05)) = 0.01
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
            Name "GearUI"

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

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _GearColor;
                float4 _InnerColor;
                float _ToothCount;
                float _ToothDepth;
                float _ToothWidth;
                float _InnerRadius;
                float _OuterRadius;
                float _Smoothness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Shift UVs from [0, 1] to [-1, 1] centered at the origin
                float2 uv = input.uv * 2.0 - 1.0;

                // Convert to polar coordinates (radius and angle)
                float r = length(uv);
                float theta = atan2(uv.y, uv.x); // Range [-PI, PI]

                // Periodic wave function to create gear teeth based on angle
                float frequency = _ToothCount;
                float wave = cos(theta * frequency);
                
                // Shape the wave to create flat-topped gear teeth
                // Using smoothstep or sign-based shaping for distinct tooth profiles
                float toothShape = saturate((wave - (1.0 - _ToothWidth * 2.0)) / _ToothWidth);
                
                // Modulate the outer radius with the teeth profile
                float dynamicOuterRadius = _OuterRadius - _ToothDepth * abs(wave);

                // Anti-aliased signed distance fields for outer boundary and inner hole
                float outerEdge = smoothstep(_OuterRadius, _OuterRadius - _Smoothness, r);
                
                // Refined gear shape calculation using polar radius modulation
                float gearBoundary = smoothstep(dynamicOuterRadius + _Smoothness, dynamicOuterRadius, r);
                float innerHole = smoothstep(_InnerRadius - _Smoothness, _InnerRadius, r);

                // Combine shapes: Gear body minus the inner center hole
                float alpha = gearBoundary * innerHole;
                
                // If outside the outer boundary or inside the inner hole, discard or make transparent
                if (r > _OuterRadius + _ToothDepth || r < _InnerRadius)
                {
                    alpha = 0.0;
                }

                // Color blending
                float4 finalColor = _GearColor * input.color;
                
                // Optional inner rim shadow effect near the center hole
                float rim = smoothstep(_InnerRadius, _InnerRadius + 0.1, r);
                finalColor.rgb = lerp(_InnerColor.rgb, finalColor.rgb, rim);

                finalColor.a *= alpha;
                return finalColor;
            }
            ENDHLSL
        }
    }
}