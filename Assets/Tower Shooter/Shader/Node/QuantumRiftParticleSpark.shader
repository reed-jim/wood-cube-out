Shader "UI/Custom/QuantumParticleCore"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Particle Core Color", Color) = (0.5, 0.0, 1.0, 1.0)
        _RimColor ("Particle Rim Color", Color) = (0.0, 1.0, 0.8, 1.0)

        [Header(Particle Animated Controls)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 10.0
        _ParticleSize ("Particle Spread", Range(0.01, 0.5)) = 0.2
        _AnimSpeed ("Animation / Drift Speed", Float) = 3.0
        _Intensity ("Glow Intensity", Range(1.0, 10.0)) = 4.0

        [Header(Energy Detail)]
        _Turbulence ("Plasma Turbulence", Range(0.0, 2.0)) = 1.0
        _OrbitSparks ("Orbiting Micro Sparks", Range(0.0, 2.0)) = 1.0
        _Seed ("Random Seed (per particle look)", Range(0.0, 100.0)) = 0.0
        _SoftClip ("Soft Highlight Rolloff (0 for HDR/Bloom)", Range(0.0, 1.0)) = 0.7

        // Stencil / UI Canvas properties
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
        Blend One One
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_QuantumParticleCore"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define K_PI  3.14159265
            #define K_TAU 6.28318531

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
                #if defined(UNITY_UI_CLIP_RECT)
                float4 worldPosition : TEXCOORD1;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float _Power;
                float _ParticleSize;
                float _AnimSpeed;
                float _Intensity;
                float _Turbulence;
                float _OrbitSparks;
                float _Seed;
                float _SoftClip;
                float4 _ClipRect;
            CBUFFER_END

            // ------------------------------------------------------------------
            // Helpers
            // ------------------------------------------------------------------
            float RfSq(float x) { return x * x; }

            float RfHash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float RfHash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float RfNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = RfHash12(i);
                float b = RfHash12(i + float2(1, 0));
                float c = RfHash12(i + float2(0, 1));
                float d = RfHash12(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float RfFbm(float2 p)
            {
                float v = 0.0, a = 0.5;
                for (int i = 0; i < 3; i++)
                {
                    v += a * RfNoise(p);
                    p = p * 2.03 + float2(1.7, 9.2);
                    a *= 0.5;
                }
                return v / 0.875;
            }

            float2 RfRot(float2 v, float a)
            {
                float c = cos(a), s = sin(a);
                return float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;

                #if defined(UNITY_UI_CLIP_RECT)
                output.worldPosition = input.positionOS;
                #endif

                return output;
            }

            // ------------------------------------------------------------------
            // Single-Orb Evolution Tiers (Zero Rings, Zero Rays - Pure Orb Variations)
            // ------------------------------------------------------------------
            void RfTier(int tier, float2 q, float r, float ang, float t, float wisp,
                        float3 cA, float3 cB,
                        out float bMul, out float bAdd, out float3 tint)
            {
                bMul = 1.0;
                bAdd = 0.0;
                tint = cA;

                if (tier == 0)
                {
                    // Tier 0: Soft dormant glowing orb
                    bMul = 1.0 - saturate(r * 1.8);
                    tint = cA * 1.2;
                }
                else if (tier == 1)
                {
                    // Tier 1: Gentle pulsing plasma sphere
                    float spherePulse = 0.8 + 0.2 * sin(t * 3.0);
                    bMul = saturate((1.0 - r * 1.5) * spherePulse);
                    bAdd = exp(-r * 3.5) * 0.4;
                    tint = lerp(cA, float3(0.0, 1.5, 0.8), 0.5);
                }
                else if (tier == 2)
                {
                    // Tier 2: Internal soft fluid core knot
                    float knot = pow(abs(cos(ang * 2.0 + t * 2.0)), 2.0);
                    bMul = saturate(1.0 - r * 1.3);
                    bAdd = knot * exp(-r * 2.5) * 0.6;
                    tint = lerp(cB, float3(1.8, 0.3, 1.5), 0.6);
                }
                else if (tier == 3)
                {
                    // Tier 3: Rotating soft blob lobes inside the orb
                    float lobes = pow(abs(sin(ang * 3.0 - t * 2.5)), 3.0);
                    bMul = saturate((1.0 - r * 1.4) * (0.5 + lobes * 0.8));
                    bAdd = lobes * exp(-r * 2.2) * 0.5;
                    tint = lerp(cA, float3(2.2, 1.1, 0.3), 0.6);
                }
                else if (tier == 4)
                {
                    // Tier 4: Dense compressed plasma core
                    float denseCore = exp(-r * 2.8);
                    bMul = 1.2;
                    bAdd = denseCore * 1.2;
                    tint = lerp(cB, float3(0.2, 2.0, 2.3), 0.7);
                }
                else if (tier == 5)
                {
                    // Tier 5: High-frequency swirling core micro-texture
                    float swirl = abs(sin(ang * 4.0 + r * 4.0 + t * 3.0));
                    bMul = saturate(1.0 - r * 1.2);
                    bAdd = swirl * exp(-r * 2.0) * 0.7;
                    tint = float3(2.3, 0.3, 0.7);
                }
                else if (tier == 6)
                {
                    // Tier 6: Dual-layer organic bubble core
                    float bubble = pow(abs(cos(ang * 3.0 - t * 2.0)), 4.0);
                    bMul = saturate(1.0 - r * 1.3);
                    bAdd = bubble * exp(-r * 2.3) * 0.9;
                    tint = lerp(cA, float3(0.8, 0.4, 2.3), 0.7);
                }
                else if (tier == 7)
                {
                    // Tier 7: Electric energetic ball turbulence
                    float elec = abs(sin(ang * 5.0 - t * 4.0 + wisp * 2.0));
                    bMul = saturate(1.0 - r * 1.2);
                    bAdd = elec * exp(-r * 2.0) * 0.8;
                    tint = float3(2.0, 2.0, 0.3);
                }
                else if (tier == 8)
                {
                    // Tier 8: Dense swirling vortex orb
                    float vortex = sin(ang * 3.0 - r * 3.0 + t * 4.0) * 0.5 + 0.5;
                    bMul = saturate(1.0 - r * 1.3) * (0.6 + vortex * 0.8);
                    bAdd = vortex * exp(-r * 2.2) * 0.7;
                    tint = float3(0.3, 2.3, 2.3);
                }
                else
                {
                    // Tier 9: Fully supercharged prismatic radiant core
                    float s = abs(sin(r * 6.0 - t * 4.0));
                    bMul = saturate(1.0 - r * 1.1) * (0.8 + s * 0.8);
                    bAdd = pow(s, 2.0) * exp(-r * 1.8) * 0.8;
                    float3 prism = (0.5 + 0.5 * cos(K_TAU * (r * 0.4 + t * 0.05 + float3(0.0, 0.33, 0.67)))) * 1.5;
                    tint = lerp(lerp(cB, float3(2.3, 2.3, 2.8), 0.5), prism, 0.5);
                }
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 st = input.uv - 0.5;
                float dist = length(st);
                float size = max(_ParticleSize, 0.01);
                float2 q = st / size;               
                float r0 = length(q);
                float t = _Time.y * _AnimSpeed;

                float pn = saturate(_Power / 100.0);
                float gain = _Intensity * 0.35 * lerp(0.85, 1.2, pn);

                // ---------------- Plasma turbulence (bounded strictly to the orb) ----------------
                float2 wp = q * 2.2 + float2(t * 0.35 + _Seed * 13.1, -t * 0.28 + _Seed * 7.7);
                float2 warp = float2(RfFbm(wp), RfFbm(wp + 5.2)) - 0.5;
                float2 qw = q + warp * _Turbulence * 0.35 * saturate(1.0 - r0);
                float rw = length(qw);
                float aw = atan2(qw.y, qw.x);

                float pulse = sin(t * 4.0 + r0 * 3.0) * 0.15 + 0.85;
                float plasmaN = RfFbm(qw * 3.0 - float2(0.0, t * 0.6) + _Seed * 5.0);
                float falloff = exp(-rw * 4.5 / pulse);
                float body = falloff * (0.7 + 0.6 * plasmaN);
                float wisp = RfNoise(float2(q.x * 2.5, q.y * 2.5 + t * 2.0) + _Seed);

                // ---------------- Tier selection (smooth blend over Power) ----------------
                float normalizedPower = min(_Power / 100.0, 0.9999);
                float powerTierVal = normalizedPower * 9.0;
                int tierA = clamp((int)floor(powerTierVal), 0, 8);
                int tierB = min(tierA + 1, 9);
                float tierMix = frac(powerTierVal);
                tierMix = tierMix * tierMix * (3.0 - 2.0 * tierMix);

                float mulA, addA, mulB, addB;
                float3 tintA, tintB;
                RfTier(tierA, qw, rw, aw, t, wisp, _Color.rgb, _RimColor.rgb, mulA, addA, tintA);
                RfTier(tierB, qw, rw, aw, t, wisp, _Color.rgb, _RimColor.rgb, mulB, addB, tintB);

                float bMul = lerp(mulA, mulB, tierMix);
                float bAdd = lerp(addA, addB, tierMix);
                float3 tint = lerp(tintA, tintB, tierMix);

                // ---------------- Heat ramp (Single Orb Body) ----------------
                float bodyM = body * bMul;
                float heat = saturate(bodyM);
                float3 edgeCol = _Color.rgb * 0.8 + 0.02;
                float3 hotCol = lerp(float3(1, 1, 1), tint, 0.2);
                float3 ramp = lerp(edgeCol, tint, smoothstep(0.03, 0.40, heat));
                ramp = lerp(ramp, hotCol, smoothstep(0.55, 1.0, heat));

                float3 rgb = ramp * bodyM * gain;
                rgb += lerp(tint, float3(1, 1, 1), 0.3) * bAdd * gain;

                // Soft spherical aura falloff
                float3 haloFall = float3(exp(-r0 * 2.5), exp(-r0 * 2.8), exp(-r0 * 3.2));
                rgb += haloFall * lerp(_Color.rgb, tint, 0.5) * 0.15 * gain;

                // ---------------- Living white-hot center core ----------------
                float corePulse = sin(t * 6.0) * 0.5 + 0.5;
                float core = exp(-r0 * 16.0) * (0.7 + corePulse * 0.3);
                float coreWide = exp(-r0 * 6.0) * 0.2 * (0.8 + corePulse * 0.2);
                rgb += float3(1.5, 1.5, 1.8) * core * gain;
                rgb += tint * coreWide * 1.4 * gain;

                // ---------------- Orbiting micro sparks strictly bound inside the orb boundary ----------------
                if (_OrbitSparks > 0.001)
                {
                    float3 sparkAcc = float3(0, 0, 0);
                    [unroll]
                    for (int s = 0; s < 5; s++)
                    {
                        float fs = (float)s;
                        float h1 = RfHash11(fs * 7.13 + _Seed * 3.1 + 1.0);
                        float h2 = RfHash11(fs * 3.71 + _Seed * 5.3 + 2.0);
                        float h3 = RfHash11(fs * 9.37 + _Seed * 1.9 + 3.0);
                        float dirS = (h2 > 0.5) ? 1.0 : -1.0;
                        float spd = (0.4 + h1 * 0.8) * dirS;
                        float rad = 0.2 + 0.3 * h3; // tightly constrained within the orb radius
                        float tilt = h1 * K_PI;
                        float act = saturate(pn * 7.0 + 1.0 - fs);

                        float brightSum = 0.0;
                        [unroll]
                        for (int k = 0; k < 3; k++)
                        {
                            float a = t * spd - dirS * (float)k * 0.2 + h2 * K_TAU;
                            float2 sp = RfRot(float2(cos(a) * rad, sin(a) * rad), tilt);
                            float2 dd = q - sp;
                            brightSum += exp(-dot(dd, dd) / (0.003 * (1.0 + (float)k * 0.5))) * (1.0 - (float)k * 0.3);
                        }

                        float tw = 0.6 + 0.4 * sin(t * 2.0 + fs * 2.3);
                        float3 sc = lerp(lerp(_RimColor.rgb, tint, h3), float3(1, 1, 1), 0.4);
                        sparkAcc += sc * brightSum * tw * act;
                    }
                    rgb += sparkAcc * _OrbitSparks * gain * 1.2;
                }

                // ---------------- Finish (Clean single-orb circular clipping) ----------------
                rgb *= 1.0 - smoothstep(0.38, 0.48, dist);
                rgb = lerp(rgb, 1.0 - exp(-rgb), _SoftClip);

                rgb *= input.color.rgb * input.color.a;

                #if defined(UNITY_UI_CLIP_RECT)
                float2 inside = step(_ClipRect.xy, input.worldPosition.xy) * step(input.worldPosition.xy, _ClipRect.zw);
                rgb *= inside.x * inside.y;
                #endif

                float a = saturate(max(rgb.r, max(rgb.g, rgb.b)));

                #if defined(UNITY_UI_ALPHACLIP)
                clip(a - 0.001);
                #endif

                return half4(rgb, a);
            }
            ENDHLSL
        }
    }
}