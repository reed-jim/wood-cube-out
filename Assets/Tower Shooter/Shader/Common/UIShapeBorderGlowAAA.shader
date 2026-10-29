Shader "UI/URP/UIShapeBorderGlowAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        
        [Header(AAA Border Glow Colors)]
        [HDR] _BorderColor ("Inner Frame Rim Color", Color) = (0.0, 0.95, 0.85, 1.0)
        [HDR] _GlowColor ("Outer Aura Glow Color", Color) = (0.15, 0.45, 1.0, 1.0)
        [Header(Core Tint)]
        _FillColor ("Center Fill Color", Color) = (0.02, 0.04, 0.08, 0.5)

        [Header(Layout and Padding)]
        _InternalPadding ("Frame Inset (Glow Margin)", Range(0.0, 1.0)) = 0.12
        _BorderWidth ("Border Thickness", Range(0.001, 0.2)) = 0.03
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.08
        _GlowSpread ("Glow Spread / Bloom Size", Range(0.00, 0.3)) = 0.08
        _Exposure ("HDR Exposure", Range(0.2, 5.0)) = 2.0
        _Aspect ("Image Aspect (0 = Auto)", Float) = 0.0

        // Stencil / UI Canvas properties
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            "RenderPipeline"="UniversalPipeline"
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_ShapeBorderGlowAAA"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma shader_feature_local _USEUIALPHACLIP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BorderColor;
                float4 _GlowColor;
                float4 _FillColor;
                float _InternalPadding;
                float _BorderWidth;
                float _CornerRadius;
                float _GlowSpread;
                float _Exposure;
                float _Aspect;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            // Rounded Box Signed Distance Field function
            float roundedBoxSdf(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            float gaussian(float d, float w)
            {
                float q = d / w;
                return exp(-q * q);
            }

            float lorentz(float d, float w)
            {
                float q = d / w;
                return 1.0 / (1.0 + q * q);
            }

            float hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Auto aspect calculation
                float autoAspect = abs(ddy(input.uv.y)) / max(abs(ddx(input.uv.x)), 1e-8);
                float aspect = clamp((_Aspect > 0.001) ? _Aspect : autoAspect, 0.25, 8.0);

                float2 uv = input.uv - 0.5;
                float2 pp = float2(uv.x * aspect, uv.y);

                // Scale down coordinate space slightly based on padding so the bloom fits inside the rect
                float scaleFactor = 1.0 + max(_InternalPadding, 0.0);
                pp *= scaleFactor;

                // Outer boundary half-extents of the inset frame
                float2 boxSize = float2(0.5 * aspect, 0.5);
                float r = min(_CornerRadius, min(boxSize.x, boxSize.y));

                // Signed distance to the inset rounded box edge
                float sdOuterBox = roundedBoxSdf(pp, boxSize, r);
                float aaX = max(fwidth(pp.x * scaleFactor), 1e-4);

                // Outer container body mask
                float outerMask = smoothstep(aaX, -aaX, sdOuterBox);

                // Border ring distance field
                float borderDist = abs(sdOuterBox + _BorderWidth * 0.5) - (_BorderWidth * 0.5);
                float borderMask = smoothstep(aaX, -aaX, borderDist);

                // Outer bloom glow spreading away from the outer edge into the padding margin
                float outerDist = max(0.0, sdOuterBox);
                float w = max(_GlowSpread, 1e-4);
                float bloomGlow = lorentz(outerDist, w);
                float haloGlow = exp(-outerDist / (w * 2.5));

                // Inside fill mask
                float innerFillMask = smoothstep(aaX, -aaX, sdOuterBox + _BorderWidth);

                // Compose HDR Lighting channels
                float3 fillColor = _FillColor.rgb * innerFillMask;
                float3 borderRim = borderMask * _BorderColor.rgb * 3.5;
                float3 outerAura = (bloomGlow * _GlowColor.rgb * 2.5 + haloGlow * _GlowColor.rgb * 1.2) * (1.0 - innerFillMask);

                float3 hdr = (fillColor + borderRim + outerAura) * _Exposure;

                // Filmic Tonemapping
                float3 ldr = 1.0 - exp(-hdr);

                // Anti-banding dither
                ldr += (hash21(input.positionCS.xy) - 0.5) / 255.0;
                ldr = max(ldr, 0.0);

                // Combined Alpha blending frame boundary and outer glow falloff
                float alpha = saturate(outerMask + bloomGlow * 0.8 + haloGlow * 0.4);

                half4 col = half4(ldr, alpha);
                col.rgb *= input.color.rgb * input.color.a;
                col.a *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}