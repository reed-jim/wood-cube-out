Shader "UI/Custom/OrbitalParticleReactorAAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Magnetic Housing Color", Color) = (0.05, 0.12, 0.2, 1.0)
        _RimColor ("Flux Plasma Color", Color) = (0.2, 0.9, 1.0, 1.0)

        [Header(Reactor Control)]
        _Power ("Flux Charge Progress (0 - 100)", Range(0.0, 100.0)) = 40.0
        _RotationAngle ("Base Angle Offset", Float) = 0.0
        _HighSpeedTransition ("Magnetic Overcharge", Range(0.0, 1.0)) = 0.0
        _Layers ("Orbital Ring Count", Range(3, 30)) = 8
        _EventHorizon ("Reactor Core Radius", Range(0.05, 0.4)) = 0.2

        [Header(Look)]
        _HeatShimmer ("Plasma Shimmer", Range(0.0, 2.0)) = 1.0
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
            Name "UI_OrbitalParticleReactorAAA"
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
            // Reactor State Function
            // ------------------------------------------------------------
            float4 ReactorState(float2 p, float r, float theta, float aa)
            {
                float t = _Time.y;
                float progress = saturate(_Power / 100.0);
                float highSpeed = saturate(_HighSpeedTransition);

                // Heat shimmer distortion
                float2 sh = float2(vnoise(p * 5.0 + t * 2.5), vnoise(p * 5.0 + 4.1)) - 0.5;
                float2 pd = p + sh * 0.025 * _HeatShimmer;
                float rd = length(pd);
                float thd = atan2(pd.y, pd.x);

                // Coordinate rotation
                float2 pRot = rot2(pd, _RotationAngle);
                float angle = atan2(pRot.y, pRot.x);

                float innerR = _EventHorizon;
                float outerR = 0.8;
                float ringMask = smoothstep(innerR - aa, innerR + aa, rd) * (1.0 - smoothstep(outerR - aa, outerR + aa, rd));

                float3 baseHousing = _Color.rgb * 0.5 * ringMask;
                float3 activeCol = lerp(_RimColor.rgb, float3(1.0, 1.0, 1.0), highSpeed * 0.4);

                // Concentric magnetic flux rings scaled by layer count
                int rings = max((int)(_Layers + 0.5), 3);
                float ringField = 0;
                [unroll]
                for(int i = 1; i <= 5; i++)
                {
                    float targetR = innerR + (float)i * ((outerR - innerR) / 6.0);
                    ringField += exp(-pow(rd - targetR, 2.0) * 400.0);
                }
                
                // Progress scaling on magnetic intensity
                float fluxIntensity = lerp(0.2, 1.0, progress) * lerp(1.0, 2.5, highSpeed);
                float3 fluxCol = activeCol * ringField * fluxIntensity * ringMask;

                // Orbiting energy nodes / particles traveling along magnetic paths
                float orbitAngle = angle + t * lerp(2.0, 10.0, highSpeed + progress);
                float particles = pow(abs(sin(orbitAngle * (float)rings)), 12.0);
                float3 particleCol = float3(1.0, 1.0, 1.0) * particles * ringMask * progress * (1.5 + 2.0 * highSpeed);

                // Reactor Core Plasma Sphere
                float coreMask = 1.0 - smoothstep(0.0, innerR * 1.2, rd);
                float coreNoise = vnoise(pd * 8.0 - t * 4.0);
                float corePulse = 1.0 + 0.3 * sin(t * 6.0) * _HeatShimmer;
                float3 coreCol = activeCol * coreMask * (0.5 + 1.5 * coreNoise) * progress * corePulse * 3.0;

                // Outer atmospheric containment bloom
                float outerHalo = exp(-max(rd - outerR, 0.0) * 16.0) * (progress + highSpeed * 1.5);
                float3 haloCol = activeCol * outerHalo;

                // Combine all layers
                float3 emis = baseHousing + fluxCol + particleCol + coreCol + haloCol;

                float solid = 1.0 - smoothstep(outerR - aa, outerR + aa, rd);
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

                float4 col = ReactorState(p, r, theta, aa);

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