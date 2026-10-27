Shader "UI/Custom/CyberpunkHUDScanner"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Primary HUD Color", Color) = (0.0, 1.0, 0.4, 1.0)
        _RimColor ("Alert Glow Color", Color) = (1.0, 0.05, 0.2, 1.0)

        [Header(Scanner Control)]
        _Power ("Scan Intensity (0 - 100)", Range(0.0, 100.0)) = 50.0
        _RotationAngle ("Rotation Angle", Float) = 0.0
        _HighSpeedTransition ("Alert Mode Transition", Range(0.0, 1.0)) = 0.0
        _Layers ("Grid Density", Range(3, 30)) = 12
        _EventHorizon ("Center Deadzone", Range(0.05, 0.4)) = 0.15

        [Header(Look)]
        _HeatShimmer ("Glitch Distortion", Range(0.0, 2.0)) = 0.5
        _Exposure ("Exposure", Range(0.25, 3.0)) = 1.0
        _Tonemap ("Filmic Tonemap Amount", Range(0.0, 1.0)) = 0.4

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
            Name "UI_CyberpunkHUDScanner"
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

            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float max3(float3 v) { return max(v.x, max(v.y, v.z)); }
            float3 ACESFilm(float3 x) { return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14)); }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float r = length(p);
                float theta = atan2(p.y, p.x);
                float aa = max(fwidth(r), 0.002);
                float t = _Time.y;
                float pN = saturate(_Power / 100.0);
                float alert = saturate(_HighSpeedTransition);

                // Digital glitch distortion
                float glitch = (hash21(floor(p * 15.0 + t)) - 0.5) * 0.05 * _HeatShimmer * alert;
                p += glitch;
                r = length(p);
                theta = atan2(p.y, p.x);

                float3 col = 0;
                float3 activeColor = lerp(_Color.rgb, _RimColor.rgb, alert);

                // Target Lock Crosshair
                float crossH = (abs(p.x) < 0.02 && abs(p.y) < 0.7) || (abs(p.y) < 0.02 && abs(p.x) < 0.7) ? 1.0 : 0.0;
                col += activeColor * crossH * 0.3;

                // Concentric HUD Circles
                int numRings = (int)_Layers;
                for(int i = 1; i <= 6; i++)
                {
                    float ringTargetR = _EventHorizon + (float)i * 0.1;
                    float ring = exp(-pow(r - ringTargetR, 2.0) * 800.0);
                    col += activeColor * ring * (0.8 + 0.5 * sin(t * 5.0 + (float)i));
                }

                // Rotating Radar Sweep Blade
                float sweepAngle = theta - (_RotationAngle + t * 2.0);
                float sweep = frac(sweepAngle / TWO_PI);
                float radarBeam = smoothstep(0.85, 1.0, sweep) * smoothstep(0.95, _EventHorizon, r);
                col += activeColor * radarBeam * (1.5 + pN * 2.0);

                // Outer Segmented Track
                float outerRing = smoothstep(0.78 - aa, 0.78, r) * (1.0 - smoothstep(0.82, 0.82 + aa, r));
                float ticks = pow(abs(sin(theta * numRings)), 16.0);
                col += activeColor * outerRing * (0.5 + 1.5 * ticks);

                // Center Core Pulse
                float core = 1.0 - smoothstep(0.0, _EventHorizon, r);
                col += activeColor * core * (1.0 + 2.0 * pN * sin(t * 10.0));

                // Soft circular mask
                float solid = 1.0 - smoothstep(0.82, 0.99, r);
                float3 rgb = col * solid;
                float alpha = saturate(max3(col) * 0.9) * solid;

                // Hot highlights bleed toward white
                float mx = max3(rgb);
                float ex = saturate((mx - 1.0) / 6.0);
                rgb = lerp(rgb, mx.xxx, ex * 0.7);

                // Exposure + ACES Tonemap
                rgb *= _Exposure;
                rgb = lerp(rgb, ACESFilm(rgb), _Tonemap);

                // Dither
                float dn = hash21(input.positionCS.xy + frac(t)) - 0.5;
                rgb += dn / 255.0 * alpha;

                // UI vertex color (premultiplied-correct)
                rgb *= input.color.rgb * input.color.a;
                alpha *= input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, input.worldPos.xy) * step(input.worldPos.xy, _ClipRect.zw);
                rgb *= inside.x * inside.y;
                alpha *= inside.x * inside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}