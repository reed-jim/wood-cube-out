Shader "UI/Custom/QuantumRiftParticleSpark"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Spark Core Color", Color) = (0.5, 0.0, 1.0, 1.0)
        _RimColor ("Spark Rim Color", Color) = (0.0, 1.0, 0.8, 1.0)

        [Header(Particle Animated Controls)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 10.0
        _ParticleSize ("Particle Spread", Range(0.01, 0.5)) = 0.2
        _AnimSpeed ("Animation / Drift Speed", Float) = 3.0
        _Intensity ("Glow Intensity", Range(1.0, 10.0)) = 4.0

        [Header(Energy Detail)]
        _Turbulence ("Plasma Turbulence", Range(0.0, 2.0)) = 1.0
        _FlareStreak ("Anamorphic Flare Streaks", Range(0.0, 2.0)) = 1.0
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
        // Premultiplied additive: the shader outputs final light, vertex alpha fades it linearly
        Blend One One
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_QuantumRiftParticleSpark"
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
                float _FlareStreak;
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
            // Per-tier signature.
            //   q    : warped coordinates in particle-size units (r = 1 is one "spread")
            //   bMul : multiplier applied to the plasma body
            //   bAdd : extra bright structure (rings, spikes, sigils) added on top
            //   tint : mid-tone color of the tier
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
                    // Soft pulsing ember
                    tint = cA * 1.5;
                }
                else if (tier == 1)
                {
                    // Rotating four-point star with hard spikes
                    float rot = ang + t * 2.0;
                    float c2 = abs(cos(rot * 2.0));
                    bMul = 0.3 + pow(c2, 4.0) * 0.7;
                    bAdd = pow(c2, 28.0) * exp(-r * 1.3) * 0.9;
                    tint = lerp(cA, float3(0.0, 1.8, 0.8), 0.6);
                }
                else if (tier == 2)
                {
                    // Twin expanding shockwave rings
                    float p1 = frac(t * 0.5);
                    float p2 = frac(t * 0.5 + 0.5);
                    float ring1 = exp(-RfSq((r - p1 * 1.4) / 0.05)) * RfSq(1.0 - p1);
                    float ring2 = exp(-RfSq((r - p2 * 1.4) / 0.05)) * RfSq(1.0 - p2);
                    bAdd = (ring1 + ring2) * 1.8;
                    tint = lerp(cB, float3(2.0, 0.2, 1.8), 0.7);
                }
                else if (tier == 3)
                {
                    // Spinning blades
                    float blade = pow(abs(sin(ang * 4.0 - t * 3.0)), 6.0);
                    bMul = saturate(blade * 2.0 + 0.4);
                    bAdd = blade * exp(-r * 2.0) * 0.6;
                    tint = lerp(cA, float3(2.5, 1.0, 0.2), 0.7);
                }
                else if (tier == 4)
                {
                    // Comet plume trailing downward with licking wisps
                    float along = -q.y;
                    float spread = 2.0 + max(along, 0.0) * 1.6;
                    float plume = smoothstep(-0.2, 0.2, along)
                                * exp(-max(along, 0.0) * 0.9)
                                * exp(-q.x * q.x * spread);
                    bAdd = plume * (0.35 + wisp * 1.1) * 1.4;
                    tint = lerp(cB, float3(0.0, 2.2, 2.5), 0.8);
                }
                else if (tier == 5)
                {
                    // Radial rays
                    float rays = abs(sin(ang * 6.0 + t * 5.0));
                    bMul = rays;
                    bAdd = pow(rays, 8.0) * exp(-r * 1.6) * 0.9;
                    tint = float3(2.5, 0.2, 0.6);
                }
                else if (tier == 6)
                {
                    // Rotating hextech sigil
                    float2 hp = RfRot(q, t * 0.6);
                    float hd = max(abs(hp.x) * 0.8660254 + abs(hp.y) * 0.5, abs(hp.y));
                    float ringH  = exp(-RfSq((hd - 0.55) / 0.04));
                    float ringH2 = exp(-RfSq((hd - 0.22) / 0.035));
                    float hexL = pow(abs(cos(ang * 3.0 - t)), 2.0);
                    bMul = hexL * 1.2 + 0.2;
                    bAdd = (ringH + ringH2 * 0.6) * 1.3;
                    tint = lerp(cA, float3(1.0, 0.3, 2.5), 0.7);
                }
                else if (tier == 7)
                {
                    // Jagged lightning spokes
                    float jag = abs(sin(ang * 8.0 - t * 6.0 + r * 3.0));
                    bMul = jag;
                    bAdd = pow(jag, 12.0) * exp(-r * 1.3) * 1.0;
                    tint = float3(2.2, 2.2, 0.2);
                }
                else if (tier == 8)
                {
                    // Spiral arms
                    float sw = sin(ang * 3.0 - r * 3.2 + t * 4.0) * 0.5 + 0.5;
                    bMul = sw * 1.5;
                    bAdd = pow(sw, 6.0) * exp(-r * 1.5) * 0.7;
                    tint = float3(0.2, 2.5, 2.5);
                }
                else
                {
                    // Prismatic energy shard
                    float s = abs(sin(ang * 5.0 + r * 4.0 - t * 4.0));
                    bMul = saturate(pow(s, 0.7) * 1.5 + 0.3);
                    bAdd = pow(s, 3.0) * exp(-r * 1.8) * 0.6;
                    float3 prism = (0.5 + 0.5 * cos(K_TAU * (r * 0.5 + t * 0.05 + float3(0.0, 0.33, 0.67)))) * 1.6;
                    tint = lerp(lerp(cB, float3(2.5, 2.5, 3.0), 0.5), prism, 0.45);
                }
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 st = input.uv - 0.5;
                float dist = length(st);
                float size = max(_ParticleSize, 0.01);
                float2 q = st / size;               // particle-size units
                float r0 = length(q);
                float t = _Time.y * _AnimSpeed;

                float pn = saturate(_Power / 100.0);
                float gain = _Intensity * 0.35 * lerp(0.85, 1.2, pn);

                // ---------------- Plasma turbulence (domain warp) ----------------
                float2 wp = q * 2.2 + float2(t * 0.35 + _Seed * 13.1, -t * 0.28 + _Seed * 7.7);
                float2 warp = float2(RfFbm(wp), RfFbm(wp + 5.2)) - 0.5;
                float2 qw = q + warp * _Turbulence * 0.55 * saturate(r0 * 0.8);
                float rw = length(qw);
                float aw = atan2(qw.y, qw.x);

                float pulse = sin(t * 4.0 + r0 * 5.0) * 0.2 + 0.8;
                float plasmaN = RfFbm(qw * 3.5 - float2(0.0, t * 0.9) + _Seed * 5.0);
                float falloff = exp(-rw * 5.5 / pulse);
                float body = falloff * (0.65 + 0.7 * plasmaN);
                float wisp = RfNoise(float2(q.x * 3.0, q.y * 2.0 + t * 3.0) + _Seed);

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

                // ---------------- Heat ramp: deep edge -> saturated mid -> hot white ----------------
                float bodyM = body * bMul;
                float heat = saturate(bodyM);
                float3 edgeCol = _Color.rgb * 0.8 + 0.02;
                float3 hotCol = lerp(float3(1, 1, 1), tint, 0.2);
                float3 ramp = lerp(edgeCol, tint, smoothstep(0.03, 0.40, heat));
                ramp = lerp(ramp, hotCol, smoothstep(0.55, 1.0, heat));

                float3 rgb = ramp * bodyM * gain;

                // tier structures (rings, spikes, sigils), slightly lightened for a hot look
                rgb += lerp(tint, float3(1, 1, 1), 0.3) * bAdd * gain;

                // soft chromatic halo (red falls off slowest, blue fastest)
                float3 haloFall = float3(exp(-r0 * 2.1), exp(-r0 * 2.4), exp(-r0 * 2.8));
                rgb += haloFall * lerp(_Color.rgb, tint, 0.5) * 0.2 * gain;

                // ---------------- Living white-hot core ----------------
                float corePulse = sin(t * 6.0) * 0.5 + 0.5;
                float core = exp(-r0 * 14.0) * (0.7 + corePulse * 0.3);
                float coreWide = exp(-r0 * 5.0) * 0.25 * (0.8 + corePulse * 0.2);
                rgb += float3(1.5, 1.5, 1.8) * core * gain;
                rgb += tint * coreWide * 1.5 * gain;

                // ---------------- Anamorphic flare cross ----------------
                if (_FlareStreak > 0.001)
                {
                    float2 qr = RfRot(q, t * 0.25);
                    float streak = exp(-abs(qr.y) * 20.0) * exp(-abs(qr.x) * 1.4)
                                 + exp(-abs(qr.x) * 20.0) * exp(-abs(qr.y) * 1.4) * 0.7;
                    streak *= 0.8 + 0.2 * sin(t * 7.0 + _Seed);
                    rgb += lerp(_RimColor.rgb, float3(1, 1, 1), 0.4) * streak * _FlareStreak * gain * 0.8;
                }

                // ---------------- Orbiting micro sparks with trails (more appear with Power) ----------------
                if (_OrbitSparks > 0.001)
                {
                    float3 sparkAcc = float3(0, 0, 0);
                    [unroll]
                    for (int s = 0; s < 6; s++)
                    {
                        float fs = (float)s;
                        float h1 = RfHash11(fs * 7.13 + _Seed * 3.1 + 1.0);
                        float h2 = RfHash11(fs * 3.71 + _Seed * 5.3 + 2.0);
                        float h3 = RfHash11(fs * 9.37 + _Seed * 1.9 + 3.0);
                        float dirS = (h2 > 0.5) ? 1.0 : -1.0;
                        float spd = (0.5 + h1 * 1.2) * dirS;
                        float rad = 0.55 + 0.75 * h3;
                        float tilt = h1 * K_PI;
                        float act = saturate(pn * 7.0 + 1.0 - fs);

                        float brightSum = 0.0;
                        [unroll]
                        for (int k = 0; k < 3; k++)
                        {
                            float a = t * spd - dirS * (float)k * 0.22 + h2 * K_TAU;
                            float2 sp = RfRot(float2(cos(a) * rad, sin(a) * rad * 0.6), tilt);
                            float2 dd = q - sp;
                            brightSum += exp(-dot(dd, dd) / (0.0035 * (1.0 + (float)k * 0.6))) * (1.0 - (float)k * 0.35);
                        }

                        float tw = 0.6 + 0.4 * sin(t * 2.0 + fs * 2.3);
                        float3 sc = lerp(lerp(_RimColor.rgb, tint, h3), float3(1, 1, 1), 0.45);
                        sparkAcc += sc * brightSum * tw * act;
                    }
                    rgb += sparkAcc * _OrbitSparks * gain * 1.6;
                }

                // ---------------- Finish ----------------
                // fade out before the quad edge so streaks/rings never hard-cut
                rgb *= 1.0 - smoothstep(0.30, 0.5, dist);

                // keep hue in hot areas instead of channel-wise clipping (set 0 when using HDR + bloom)
                rgb = lerp(rgb, 1.0 - exp(-rgb), _SoftClip);

                // vertex color: Particle System / Image color and lifetime fade
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
