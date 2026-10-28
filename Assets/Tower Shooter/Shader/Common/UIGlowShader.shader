Shader "UI/URP/UIShapeGlowAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        
        [Header(AAA Glow Colors)]
        [HDR] _HotColor ("Hot Core Color", Color) = (1.0, 0.97, 0.90, 1.0)
        [HDR] _RimColor ("Arcane Edge Glow", Color) = (0.0, 0.95, 0.85, 1.0)
        [HDR] _AuraColor ("Outer Aura Color", Color) = (0.15, 0.45, 1.0, 1.0)

        [Header(Controls)]
        _Inset ("Core Inset (Shrink Inner)", Range(0.0, 0.49)) = 0.25
        _GlowSpread ("Glow Spread / Bloom Size", Range(0.01, 0.5)) = 0.15
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
            Name "UI_ShapeGlowAAA"
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
                float4 _HotColor;
                float4 _RimColor;
                float4 _AuraColor;
                float _Inset;
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

            float boxSdf(float2 p, float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            // High-end mathematical falloff functions from reference
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

                // Shrunken core dimensions
                float2 innerBoxSize = float2(0.5 * aspect, 0.5) - _Inset;
                float sdBox = boxSdf(pp, innerBoxSize);

                float aaX = max(fwidth(pp.x), 1e-4);

                // Core body mask (anti-aliased)
                float coreMask = smoothstep(aaX, -aaX, sdBox);

                // Distance field radiating outwards from the shrunken core edge
                float outerDist = max(0.0, sdBox);

                // Multi-layered AAA glow composition
                float w = max(_GlowSpread, 1e-4);
                float coreGlow = gaussian(outerDist, w * 0.4);
                float bloomGlow = pow(lorentz(outerDist, w * 1.2), 1.5);
                float haloGlow = exp(-outerDist / (w * 3.0));

                // Layer HDR colors together
                float3 hdr = (coreMask * _HotColor.rgb * 3.0
                            + coreGlow * _HotColor.rgb * 2.2
                            + bloomGlow * _RimColor.rgb * 2.5
                            + haloGlow * _AuraColor.rgb * 1.2);

                hdr *= _Exposure;

                // Filmic Tonemapping (prevents harsh color clipping, keeps gradients rich)
                float3 ldr = 1.0 - exp(-hdr);

                // Subtle dither to remove gradient banding
                ldr += (hash21(input.positionCS.xy) - 0.5) / 255.0;
                ldr = max(ldr, 0.0);

                // Smooth combined alpha covering both core and extended bloom
                float alpha = saturate(coreMask + bloomGlow * 0.85 + haloGlow * 0.5);

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