Shader "UI/Custom/QuantumRiftEngine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Engine Heat Color (low power)", Color) = (1.0, 0.25, 0.05, 1.0)
        _RimColor ("Plasma Color (high power)", Color) = (0.0, 0.9, 1.0, 1.0)

        [Header(Engine Control)]
        _Power ("Engine Power (0 - 100)", Range(0.0, 100.0)) = 10.0
        _RotationAngle ("Rotation Angle (drive from script)", Float) = 0.0
        _HighSpeedTransition ("Hyperdrive Transition", Range(0.0, 1.0)) = 0.0
        _Layers ("Turbine Blade Count", Range(3, 30)) = 10
        _EventHorizon ("Core Radius", Range(0.05, 0.4)) = 0.2

        [Header(Look)]
        _HeatShimmer ("Heat Shimmer", Range(0.0, 2.0)) = 1.0
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
            Name "UI_QuantumRiftEngine"
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
            #define ENGINE_SCALE 0.84   // engine radius inside the tile; the rest is exhaust glow

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
            float hash11(float n) { return frac(sin(n * 127.1) * 43758.5453); }
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

            float fbm3(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    v += a * vnoise(p);
                    p = mul(float2x2(1.6, 1.2, -1.2, 1.6), p) + 17.0;
                    a *= 0.5;
                }
                return v / 0.875;
            }

            float3 ACESFilm(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
            }

            float ringAt(float r, float R, float w)
            {
                float x = (r - R) / w;
                return exp(-x * x);
            }

            // Turbine blades with built-in motion blur (6 angular taps).
            // B must be an integer so the pattern is seamless across atan2's wrap.
            // x = coverage, y = heated leading edge, z = lit shading, w = glint
            float4 BladeStage(float ang, float r, float B, float sweep, float blurAng, float soft)
            {
                float4 acc = 0;
                [unroll]
                for (int i = 0; i < 6; i++)
                {
                    float a = ang + ((float)i / 5.0 - 0.5) * blurAng;
                    float u = a * B / TWO_PI + sweep * r;
                    float f = frac(u);
                    float body = smoothstep(0.0, soft, f) * (1.0 - smoothstep(0.62 - soft, 0.62, f));
                    float lit = saturate(1.0 - f / 0.62);
                    float edge = exp(-f * 16.0) * smoothstep(0.0, soft, f);
                    float glint = pow(saturate(1.0 - abs(f - 0.12) / 0.08), 2.0);
                    acc += float4(body, edge, lit * body, glint * body);
                }
                return acc / 6.0;
            }

            // ------------------------------------------------------------
            // Hyperdrive warp tunnel (blended in by _HighSpeedTransition)
            // ------------------------------------------------------------
            float4 WarpState(float2 p, float r, float theta)
            {
                float pN = saturate(_Power / 100.0);
                float t = _Time.y;
                float rot = _RotationAngle;
                float z = 0.35 / (r + 0.04);
                float flow = t * (2.0 + 4.0 * pN);
                float3 acc = 0;

                [unroll]
                for (int L = 0; L < 3; L++)
                {
                    float cnt = 28.0 + L * 22.0;
                    float u = (theta / TWO_PI + 0.5) * cnt + rot * cnt / TWO_PI * 0.2 * (L + 1);
                    float fl = floor(u);
                    float id = fl - cnt * floor(fl / cnt);
                    float h = hash21(float2(id, L * 17.3));
                    float x = frac(u) - 0.5;
                    float width = lerp(0.08, 0.2, h);
                    float streakLine = 1.0 - smoothstep(0.0, width, abs(x));
                    float v = z * (0.6 + 0.3 * L) - flow * (0.5 + h) * 0.5 + h * 10.0;
                    float ph = frac(v * 0.5);
                    float streak = streakLine * smoothstep(0.0, 0.05, ph) * smoothstep(0.55, 0.15, ph);
                    streak *= smoothstep(0.02, 0.25, r);
                    float3 c = lerp(_Color.rgb, _RimColor.rgb, h);
                    c = lerp(c, float3(1, 1, 1), saturate(1.0 - r * 2.5) * 0.6);
                    acc += c * streak * lerp(2.0, 8.0, pN * pN);
                }
                acc += lerp(_Color.rgb, _RimColor.rgb, 0.5) * pow(abs(sin(z * 8.0 - flow * 3.0)), 12.0) * 0.4 * smoothstep(0.05, 0.3, r);
                acc += _RimColor.rgb * exp(-r * 3.5) * 1.2;
                acc += float3(1, 1, 1) * (1.0 / (r * 18.0 + 1.0)) * 0.6 * (0.5 + pN);
                return float4(acc, saturate(max3(acc) * 0.9));
            }

            // ------------------------------------------------------------
            // Spaceship engine
            //   Stage 0 IDLE      : cold metal, slow sharp blades, ember core
            //   Stage 1 SPOOL-UP  : blade edges heat up, ring arcs, bezel chase, blur begins
            //   Stage 2 THRUST    : plasma plume, shock rings, heat shimmer, exhaust sparks
            //   Stage 3 OVERDRIVE : white-hot core, full blur + spiral streaks, flare,
            //                       chromatic aberration, bezel overheat, outer halo
            // ------------------------------------------------------------
            float4 EngineState(float2 p, float r, float theta, float aa)
            {
                float t = _Time.y;
                float rot = _RotationAngle;
                float pN = saturate(_Power / 100.0);
                float s1 = smoothstep(0.05, 0.30, pN);
                float s2 = smoothstep(0.25, 0.55, pN);
                float s3 = smoothstep(0.50, 0.80, pN);
                float s4 = smoothstep(0.78, 1.00, pN);

                float2 pe = p / ENGINE_SCALE;
                float re = r / ENGINE_SCALE;
                float aaE = aa / ENGINE_SCALE;

                // Layout (engine-space radii)
                float e = saturate((_EventHorizon - 0.05) / 0.35);
                float coreR = 0.12 + 0.26 * e;
                float ringR = coreR + 0.07;
                float rOut = 0.86;
                float rMid = lerp(ringR + 0.03, rOut, 0.42);

                // Heat shimmer: distorts everything inside the housing
                float2 sh = float2(vnoise(pe * 5.0 + float2(0.0, t * 2.0)),
                                   vnoise(pe * 5.0 + float2(7.3, t * 2.3))) - 0.5;
                float2 pd = pe + sh * (0.035 * s3 + 0.02 * s4) * _HeatShimmer
                                    * (0.4 + 0.6 * smoothstep(0.2, 0.9, re));
                float rd = length(pd);
                float2 dird = pd / max(rd, 1e-4);
                float thd = atan2(pd.y, pd.x);

                float2 L = normalize(float2(-0.6, 0.8));
                float lightF = 0.55 + 0.45 * saturate(dot(dird, L));

                float3 metal = lerp(float3(0.06, 0.065, 0.08), _Color.rgb * 0.18, 0.5);
                float3 heatCol = lerp(_Color.rgb, _RimColor.rgb, s2);
                heatCol = lerp(heatCol, float3(1.0, 0.97, 0.9), s4 * 0.5);

                float3 albedo = metal * 0.15;   // dark engine tunnel
                float3 emis = 0;

                float B = max(floor(_Layers + 0.5), 3.0);
                float B2 = max(floor(B * 1.5 + 0.5), 4.0);

                // ---------- Stage 1 turbine (outer) ----------
                {
                    float soft = clamp(aaE * B / (TWO_PI * max(rd, 0.1)), 0.02, 0.25);
                    float blur = TWO_PI / B * lerp(0.0, 1.3, s2);
                    float4 b = BladeStage(thd - rot, rd, B, 1.3, blur, soft);
                    float m = smoothstep(rMid - aaE, rMid + aaE, rd) * (1.0 - smoothstep(rOut - aaE, rOut + aaE, rd));
                    float litN = b.z / max(b.x, 1e-3);
                    float radial = lerp(0.6, 1.0, saturate((rd - rMid) / (rOut - rMid)));
                    float3 bladeCol = (metal * (0.5 + 2.2 * litN) * lightF + b.w * 0.5 * lightF) * radial;
                    float3 c = lerp(metal * 0.1, bladeCol, b.x);
                    albedo = lerp(albedo, c, m);
                    emis += b.y * m * heatCol * s1 * lerp(1.0, 4.0, s3) * 1.5;
                }

                // ---------- Stage 2 turbine (inner, counter-rotating) ----------
                {
                    float soft = clamp(aaE * B2 / (TWO_PI * max(rd, 0.1)), 0.02, 0.25);
                    float blur = TWO_PI / B2 * lerp(0.0, 1.3, s2);
                    float4 b = BladeStage(thd + rot * 1.4, rd, B2, -1.0, blur, soft);
                    float m = smoothstep(ringR + 0.03 - aaE, ringR + 0.03 + aaE, rd) * (1.0 - smoothstep(rMid - aaE, rMid + aaE, rd));
                    float litN = b.z / max(b.x, 1e-3);
                    float3 bladeCol = (metal * (0.5 + 2.2 * litN) * lightF + b.w * 0.5 * lightF) * 0.85;
                    float3 c = lerp(metal * 0.1, bladeCol, b.x);
                    albedo = lerp(albedo, c, m);
                    emis += b.y * m * heatCol * s1 * lerp(1.0, 5.0, s3) * 1.8;
                }

                // Machined grooves between stages
                albedo *= 1.0 - 0.7 * ringAt(rd, rMid, 0.006);
                albedo *= 1.0 - 0.7 * ringAt(rd, rOut, 0.006);

                // ---------- Hub: cone + fast hub fan ----------
                {
                    float m = 1.0 - smoothstep(coreR - aaE, coreR + aaE, rd);
                    float3 cone = metal * (0.6 + 0.8 * (1.0 - rd / max(coreR, 0.01))) * lightF;
                    float soft = clamp(aaE * 6.0 / (TWO_PI * max(rd, 0.1)), 0.02, 0.25);
                    float blur = TWO_PI / 6.0 * lerp(0.0, 1.3, s3);
                    float4 b = BladeStage(thd - rot * 2.5, rd, 6.0, 0.8, blur, soft);
                    float litN = b.z / max(b.x, 1e-3);
                    float3 fan = metal * (0.6 + 2.0 * litN) * lightF + b.w * 0.4;
                    float fanM = b.x * smoothstep(coreR * 0.12, coreR * 0.3, rd);
                    float3 hubCol = lerp(cone, fan, fanM);
                    albedo = lerp(albedo, hubCol, m);
                }

                // ---------- Combustion ring + rotating energy arcs ----------
                {
                    float cr = ringAt(rd, ringR, 0.018 + 0.01 * s3);
                    float arcs = pow(abs(cos(6.0 * (thd - rot * 1.5) + rd * 8.0)), 6.0);
                    emis += heatCol * cr * (0.3 + 3.0 * s1 + 8.0 * s3) * (0.4 + 0.6 * arcs);
                }

                // ---------- Plasma core (flickers, flows outward, swirls with rotation) ----------
                float2 fq = rot2(pd, rot * 0.7 + rd * 3.0) * 7.0 + float2(0.0, -t * (1.0 + 3.0 * pN));
                float fn = fbm3(fq);
                {
                    float coreMask = 1.0 - smoothstep(coreR * 0.25, coreR * 1.05, rd);
                    float3 pc = lerp(_Color.rgb, _RimColor.rgb, s2);
                    pc = lerp(pc, float3(1.0, 0.97, 0.9), s4 * 0.8 * coreMask);
                    float idlePulse = 0.8 + 0.2 * sin(t * 2.0);
                    float flicker = 1.0 + 0.15 * s4 * sin(t * 47.0) * sin(t * 31.0);
                    float gain = lerp(0.35 * idlePulse, 9.0, pN * pN) * flicker;
                    emis += pc * coreMask * (0.35 + 0.9 * fn) * gain;
                }

                // ---------- Exhaust plume over the blades (THRUST+), chromatic at OVERDRIVE ----------
                {
                    float sw = pow(abs(cos(10.0 * (thd - rot * 1.2) + rd * 14.0 - t * 3.0)), 4.0);
                    float ca = 0.03 * s4;
                    float3 rdv = rd * float3(1.0 - ca, 1.0, 1.0 + ca);
                    float3 fall = exp(-max(rdv - coreR, 0.0) * lerp(18.0, 3.5, s3));
                    float3 plumeCol = lerp(_RimColor.rgb, float3(1.0, 0.97, 0.9), s4 * 0.5);
                    emis += plumeCol * fall * (0.5 + 0.8 * fn) * (0.4 + 0.6 * sw)
                            * lerp(0.5, 5.0, s3) * smoothstep(0.1, 0.5, pN);
                }

                // ---------- Shock rings radiating outward ----------
                {
                    float sr = pow(abs(sin(rd * 38.0 - t * lerp(3.0, 14.0, pN))), 10.0);
                    sr *= smoothstep(coreR * 0.8, coreR * 1.6, rd) * (1.0 - smoothstep(0.7, 1.0, rd)) * s2;
                    emis += _RimColor.rgb * sr * lerp(0.3, 2.5, s3) * (0.6 + 0.4 * fn);
                }

                // ---------- Bezel with rotating chase lights ----------
                {
                    float inB = smoothstep(rOut - aaE, rOut + aaE, re);
                    float bev = ringAt(re, 0.875, 0.012) * lightF + ringAt(re, 0.985, 0.01) * 0.5;
                    float3 bezel = metal * (0.9 + 0.3 * sin(re * 90.0)) * lightF + bev * 0.35;
                    albedo = lerp(albedo, bezel, inB);

                    float lu = (theta - rot * 0.5) * 24.0 / TWO_PI;
                    float fr = frac(lu);
                    float lamp = smoothstep(0.08, 0.2, fr) * smoothstep(0.92, 0.8, fr);
                    float lm = smoothstep(0.905, 0.915, re) * (1.0 - smoothstep(0.945, 0.955, re));
                    float tu = (theta - rot * 0.5 * (0.5 + pN)) * 3.0 / TWO_PI;
                    float trail = pow(1.0 - frac(tu), 3.0);
                    float3 lampCol = lerp(_RimColor.rgb, float3(1.0, 0.35, 0.1), s4);
                    emis += lampCol * lamp * lm * lerp(0.08, 1.0, trail) * (0.3 + 2.5 * s1 + 3.0 * s4);
                }

                // ---------- Exhaust embers ----------
                {
                    [unroll]
                    for (int i = 0; i < 10; i++)
                    {
                        float h = hash11((float)i + 1.0);
                        float life = frac(t * (0.2 + 0.3 * h) + h * 9.1);
                        float ro = lerp(coreR, 1.15, life * life);
                        float a = h * TWO_PI + rot * 0.4 * (0.5 + h) + ro * 2.5;
                        float2 sp = float2(cos(a), sin(a)) * ro;
                        float d = length(pe - sp);
                        float w = 0.014 + 0.006 * h;
                        float g = exp(-(d * d) / (w * w)) * sin(PI * life);
                        float vis = step((float)i / 10.0, pN * 1.2 - 0.2);
                        emis += lerp(float3(1.0, 0.6, 0.2), _RimColor.rgb, s3) * g * vis * (3.0 + 8.0 * s4);
                    }
                }

                // ---------- Outer halo (outside the housing) + lens flare ----------
                float halo = exp(-max(re - 1.0, 0.0) * lerp(14.0, 5.0, pN)) * smoothstep(0.96, 1.0, re);
                emis += heatCol * halo * (0.04 + 0.5 * s2 + 2.0 * s4);

                float2 fp = rot2(p, rot * 0.15);
                float2 fp2 = rot2(fp, PI * 0.25);
                float flare = exp(-abs(fp.y) * 26.0) * exp(-abs(fp.x) * 2.0)
                            + exp(-abs(fp2.y) * 26.0) * exp(-abs(fp2.x) * 2.0) * 0.6;
                emis += _RimColor.rgb * flare * s4 * 1.5;

                // ---------- Compose (premultiplied) ----------
                float solid = 1.0 - smoothstep(1.0 - aaE, 1.0 + aaE, re);
                float3 rgb = albedo * solid + emis;
                float alpha = saturate(max(solid, max3(emis) * 0.85));
                return float4(rgb, alpha);
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float r = length(p);
                float theta = atan2(p.y, p.x);
                float aa = max(fwidth(r), 0.002);

                float tr = saturate(_HighSpeedTransition);

                float4 eng = 0;
                float4 warp = 0;
                if (tr < 0.999) eng = EngineState(p, r, theta, aa);
                if (tr > 0.001) warp = WarpState(p, r, theta);

                float4 col = lerp(eng, warp, smoothstep(0.0, 1.0, tr));

                // Soft circular mask
                col *= 1.0 - smoothstep(0.82, 0.99, r);

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
