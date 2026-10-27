Shader "UI/Custom/TacticalIrisApertureAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Housing & Blade Color", Color) = (0.08, 0.12, 0.18, 1.0)
        _RimColor ("Energy Core / Laser Color", Color) = (1.0, 0.3, 0.1, 1.0)

        [Header(Iris Control)]
        _Power ("Iris Opening Progress (0 - 100)", Range(0.0, 100.0)) = 50.0
        _RotationAngle ("Base Angle Offset", Float) = 0.0
        _HighSpeedTransition ("Overcharge Mode", Range(0.0, 1.0)) = 0.0
        _Layers ("Shutter Blade Count", Range(3, 30)) = 8
        _EventHorizon ("Center Core Radius", Range(0.05, 0.4)) = 0.18

        [Header(Look)]
        _HeatShimmer ("Mechanical Shimmer", Range(0.0, 2.0)) = 1.0
        _Exposure ("Exposure", Range(0.25, 3.0)) = 1.0
        _Tonemap ("Filmic Tonemap Amount", Range(0.0, 1.0)) = 0.6

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
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha   // premultiplied alpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_TacticalIrisApertureAAA"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float4 worldPos   : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _ClipRect;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float _Power;
                float _RotationAngle;
                float _HighSpeedTransition;
                float _Layers;
                float _EventHorizon;
                float _HeatShimmer;
                float _Exposure;
                float _Tonemap;
            CBUFFER_END

            #define PI 3.14159265359
            #define TWO_PI 6.28318530718

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPos = input.positionOS;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color;
                return o;
            }

            // ------------------------------------------------------------
            // AAA Helpers & Noise
            // ------------------------------------------------------------
            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float max3(float3 v) { return max(v.x, max(v.y, v.z)); }

            float2 rot2(float2 p, float a)
            {
                float s, c;
                sincos(a, s, c);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float3 ACESFilm(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
            }

            // ------------------------------------------------------------
            // Tactical Iris Shutter State Function
            // ------------------------------------------------------------
            float4 IrisState(float2 p, float r, float theta, float aa)
            {
                float t = _Time.y;
                float progress = saturate(_Power / 100.0); // 0.0 (closed) to 1.0 (open)
                float highSpeed = saturate(_HighSpeedTransition);

                // Rotate coordinates
                float2 pRot = rot2(p, _RotationAngle);
                float angle = atan2(pRot.y, pRot.x);

                // Shutter blade count
                int blades = max((int)(_Layers + 0.5), 3);
                float bladeFreq = (float)blades;

                // Aperture radius expands/retracts based on progress
                float minRadius = _EventHorizon;
                float maxRadius = 0.75;
                float targetIrisR = lerp(minRadius, maxRadius, progress);

                // Mechanical blade edge calculation
                float bladePattern = sin(angle * bladeFreq + r * 8.0 - lerp(0.0, PI, progress));
                
                // Ring housing bounds
                float outerR = 0.82;
                float ringMask = smoothstep(targetIrisR - aa, targetIrisR + aa, r) * (1.0 - smoothstep(outerR - aa, outerR + aa, r));

                // Base structural metal housing
                float3 metalCol = _Color.rgb * (0.6 + 0.4 * bladePattern);
                float3 activeCol = lerp(_RimColor.rgb, float3(1.0, 1.0, 1.0), highSpeed * 0.5);

                // Shutter blades overlay
                float bladeShadow = smoothstep(-0.2, 0.2, bladePattern);
                float3 bladeMaterial = metalCol * lerp(0.7, 1.2, bladeShadow);

                // Glowing energy edges along the iris blades
                float edgeGlow = exp(-pow(r - targetIrisR, 2.0) * 300.0);
                float3 edgeCol = activeCol * edgeGlow * (2.0 + 4.0 * highSpeed);

                // Center core opening glow (visible when iris opens)
                float coreMask = 1.0 - smoothstep(0.0, targetIrisR * 1.05, r);
                float corePulse = 1.0 + 0.3 * sin(t * 8.0) * _HeatShimmer;
                float3 coreCol = activeCol * coreMask * progress * corePulse * 3.0;

                // Outer tactical HUD ring markers
                float outerRing = smoothstep(0.78 - aa, 0.78, r) * (1.0 - smoothstep(0.81, 0.81 + aa, r));
                float ticks = pow(abs(sin(angle * bladeFreq * 2.0)), 8.0);
                float3 hudCol = activeCol * outerRing * (0.5 + 1.5 * ticks);

                // Combine layers
                float3 emis = bladeMaterial * ringMask + edgeCol + coreCol + hudCol;

                // High-speed energy plasma wash over the whole shutter
                float3 plasmaWash = activeCol * vnoise(p * 5.0 + t * 3.0) * highSpeed * 2.0;
                emis += plasmaWash * ringMask;

                float solid = 1.0 - smoothstep(outerR - aa, outerR + aa, r);
                float3 rgb = emis * solid;
                float alpha = saturate(max3(emis) * 0.95) * solid;

                return float4(rgb, alpha);
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float r = length(p);
                float theta = atan2(p.y, p.x);
                float aa = max(fwidth(r), 0.002);

                float4 col = IrisState(p, r, theta, aa);

                // Cinematic hot highlights bleed toward white
                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 5.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.75);

                // Exposure + ACES Filmic Tonemapping
                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                // Film dither
                float dn = hash21(input.positionCS.xy + frac(_Time.y)) - 0.5;
                col.rgb += dn / 255.0 * col.a;

                // UI vertex color (premultiplied-correct)
                col.rgb *= input.color.rgb * input.color.a;
                col.a *= input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, input.worldPos.xy) * step(input.worldPos.xy, _ClipRect.zw);
                col *= inside.x * inside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return half4(col);
            }
            ENDHLSL
        }
    }
}