Shader "UI/Custom/SingularityBlackHole"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Accretion Disk Color (low power)", Color) = (1.0, 0.5, 0.1, 1.0)
        _RimColor ("Photon Ring Color (high power)", Color) = (0.2, 0.7, 1.0, 1.0)

        [Header(Singularity Control)]
        _Power ("Gravitational Power (0 - 100)", Range(0.0, 100.0)) = 40.0
        _RotationAngle ("Rotation Angle (drive from script)", Float) = 0.0
        _HighSpeedTransition ("Collapse Transition", Range(0.0, 1.0)) = 0.0
        _Layers ("Accretion Density", Range(3, 30)) = 15
        _EventHorizon ("Black Hole Radius", Range(0.05, 0.4)) = 0.22

        [Header(Look)]
        _HeatShimmer ("Space Warp", Range(0.0, 2.0)) = 1.0
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
            Name "UI_SingularityBlackHole"
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
            // Helpers
            // ------------------------------------------------------------
            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float max3(float3 v) { return max(v.x, max(v.y, v.z)); }

            float2 rot2(float2 p, float a)
            {
                float s, c;
                sincos(a, s, c);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            float3 ACESFilm(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
            }

            // ------------------------------------------------------------
            // Singularity State Shader Function
            // ------------------------------------------------------------
            float4 SingularityState(float2 p, float r, float theta, float aa)
            {
                float t = _Time.y;
                float rot = _RotationAngle;
                float pN = saturate(_Power / 100.0);
                float collapse = saturate(_HighSpeedTransition);

                // Gravitational lensing coordinate distortion
                float warpFactor = _EventHorizon / max(r, 0.01);
                float2 pWarp = rot2(p, rot + t * 0.5 + warpFactor * _HeatShimmer);
                float rWarp = length(pWarp);
                float thetaWarp = atan2(pWarp.y, pWarp.x);

                float3 emis = 0;
                float coreRadius = _EventHorizon * lerp(1.0, 0.45, collapse);

                // Absolute Event Horizon Shadow (Black Hole Center)
                float shadow = smoothstep(coreRadius - aa, coreRadius + aa, r);

                // Accretion Disk Swirl Layers
                int density = max((int)(_Layers + 0.5), 3);
                float diskNoise = sin(thetaWarp * (float)density + t * 4.0 - rWarp * 10.0);
                float diskMask = smoothstep(coreRadius, coreRadius * 1.6, r) * (1.0 - smoothstep(0.65, 0.85, r));

                float3 diskColor = lerp(_Color.rgb, _RimColor.rgb, saturate(r - coreRadius));
                emis += diskColor * (0.5 + 0.5 * diskNoise) * diskMask * (2.0 + 4.0 * pN);

                // Ultra-Bright Photon Ring / Rim Glow
                float photonRing = exp(-pow(r - coreRadius * 1.15, 2.0) * 400.0);
                emis += lerp(_RimColor.rgb, float3(1, 1, 1), 0.7) * photonRing * (4.0 + 6.0 * collapse);

                // Outer Space Dust & Star Particles
                float2 starUV = p * 4.0;
                float starField = pow(hash21(floor(starUV * 12.0 + t * 0.05)), 20.0);
                emis += float3(1, 1, 1) * starField * smoothstep(0.5, 0.95, r) * 0.8;

                // Apply Event Horizon shadow masking
                emis *= shadow;

                float solid = 1.0 - smoothstep(0.82, 0.99, r);
                float3 rgb = emis * solid;
                float alpha = saturate(max3(emis) * 0.9) * solid;

                return float4(rgb, alpha);
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float r = length(p);
                float theta = atan2(p.y, p.x);
                float aa = max(fwidth(r), 0.002);

                float4 col = SingularityState(p, r, theta, aa);

                // Hot highlights bleed toward white
                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 6.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.7);

                // Exposure + blended ACES
                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                // Dither
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