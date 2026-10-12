Shader "UI/URP/MinimalistSpacePortrait"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Background Palette)]
        _BgColor ("Background Color", Color) = (0.05, 0.08, 0.15, 1.0)
        _CircleBgColor ("Circle Backdrop Color", Color) = (0.98, 0.45, 0.15, 1.0)

        [Header(Suit Colors)]
        _SuitMainColor ("Suit Main Base", Color) = (0.92, 0.93, 0.95, 1.0)
        _SuitDetailColor ("Suit Accent / Collar", Color) = (0.22, 0.55, 0.95, 1.0)
        _PatchColor ("Chest Emblem Color", Color) = (0.95, 0.25, 0.25, 1.0)

        [Header(Character Colors)]
        _SkinColor ("Skin Tone", Color) = (0.95, 0.78, 0.68, 1.0)
        _VisorColor ("Visor Glass", Color) = (0.12, 0.15, 0.22, 1.0)
        _VisorGlint ("Visor Highlight", Color) = (0.35, 0.85, 0.95, 1.0)

        // Required UI Masking properties
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
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma shader_feature_local _ UNITY_UI_ALPHACLIP

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float2 maskUV       : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _TextureSampleAdd;
            float4 _ClipRect;

            float4 _BgColor;
            float4 _CircleBgColor;
            float4 _SuitMainColor;
            float4 _SuitDetailColor;
            float4 _PatchColor;
            float4 _SkinColor;
            float4 _VisorColor;
            float4 _VisorGlint;

            // ----------------------------------------------------
            // SDF Helper Functions
            // ----------------------------------------------------
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }

            float sdBox(float2 p, float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            float sdCapsule(float2 p, float2 a, float2 b, float r)
            {
                float2 pa = p - a, ba = b - a;
                float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
                return length(pa - ba * h) - r;
            }

            // Antialiased alpha blend helper
            float smoothFill(float distance, float smoothness)
            {
                return saturate(smoothstep(smoothness, -smoothness, distance));
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.maskUV = input.positionOS.xy;
                output.color = input.color * _Color;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Remap UV coordinates to centered aspect space [-1, 1]
                float2 uv = input.uv * 2.0 - 1.0;
                
                float aa = 0.005; // Smoothness factor for crisp vector anti-aliasing

                // 1. Background Base & Minimalist Backdrop Circle
                float3 finalColor = _BgColor.rgb;
                float circleDist = sdCircle(uv - float2(0.0, 0.05), 0.65);
                finalColor = lerp(finalColor, _CircleBgColor.rgb, smoothFill(circleDist, aa));

                // 2. Neck / Undersuit (Slightly behind shoulders)
                float neckDist = sdBox(uv - float2(0.0, -0.25), float2(0.12, 0.15));
                float3 neckColor = _SkinColor.rgb * 0.85; // subtle shadow tone
                finalColor = lerp(finalColor, neckColor, smoothFill(neckDist, aa));

                // 3. Head & Face Base
                float headDist = sdCircle(uv - float2(0.0, 0.1), 0.28);
                finalColor = lerp(finalColor, _SkinColor.rgb, smoothFill(headDist, aa));

                // 4. Suit Shoulders & Upper Body (1/3 portrait area)
                float bodyDist = sdCapsule(uv, float2(-0.6, -0.9), float2(0.6, -0.9), 0.42);
                finalColor = lerp(finalColor, _SuitMainColor.rgb, smoothFill(bodyDist, aa));

                // 5. Suit Collar Accent
                float collarDist = sdCapsule(uv, float2(-0.25, -0.38), float2(0.25, -0.38), 0.1);
                finalColor = lerp(finalColor, _SuitDetailColor.rgb, smoothFill(collarDist, aa));

                // 6. Chest Emblem / Mission Patch
                float patchDist = sdCircle(uv - float2(-0.18, -0.6), 0.07);
                finalColor = lerp(finalColor, _PatchColor.rgb, smoothFill(patchDist, aa));

                // 7. Visor / Helmet Bubble Glass
                float visorDist = sdBox(uv - float2(0.0, 0.12), float2(0.20, 0.12));
                // Rounding the visor corners using offset circle subtraction concept
                float visorRounded = max(visorDist, sdCircle(uv - float2(0.0, 0.12), 0.23));
                finalColor = lerp(finalColor, _VisorColor.rgb, smoothFill(visorRounded, aa));

                // 8. Visor Curved Glint (Minimalist Highlight)
                float glintDist = sdCapsule(uv, float2(-0.12, 0.20), float2(0.05, 0.18), 0.025);
                // Clip glint to visor shape
                float glintMask = smoothFill(glintDist, aa) * smoothFill(visorRounded, aa);
                finalColor = lerp(finalColor, _VisorGlint.rgb, glintMask * 0.8);

                // UI Alpha Channel & Canvas Masking Support
                float4 col = float4(finalColor, 1.0) * input.color;
                col.a *= (tex2D(_MainTex, input.uv) + _TextureSampleAdd).a;

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}