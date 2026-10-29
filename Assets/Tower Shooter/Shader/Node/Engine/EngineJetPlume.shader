// Unity 6.3 LTS - URP - UI Image (no sprite / no texture)
// Procedural spaceship engine plume (Raptor-style methalox look + optional ion preset)
// Driven by: _Progress (0..1) and _HighCycle (0..1). Use EngineJetDriver.cs to feed it.
Shader "UI/Jet/EnginePlume"
{
    Properties
    {
        [HideInInspector] _MainTex ("Unused", 2D) = "white" {}

        [Header(Control)]
        _Progress ("Progress", Range(0, 1)) = 0.5
        _HighCycle ("High Cycle Mode Blend", Range(0, 1)) = 0
        _HighLevel ("High Cycle Power Level", Range(0, 1)) = 0.92
        [HideInInspector] _Phases ("Phases (flow, boil, flick, breath)", Vector) = (0, 0, 0, 0)
        [HideInInspector] _Rect ("Canvas-space rect (xMin,yMin,xMax,yMax)", Vector) = (-0.5, -0.5, 0.5, 0.5)

        [Header(Layout)]
        _NozzleY ("Nozzle Y (0 bottom - 1 top)", Range(0.5, 1)) = 0.88
        _NozzleRadius ("Nozzle Radius (height units)", Range(0.01, 0.12)) = 0.045
        _LengthMin ("Length at Progress 0", Range(0.05, 0.8)) = 0.24
        _LengthMax ("Length at Progress 1", Range(0.1, 0.85)) = 0.74
        _WidthMin ("Width Scale at 0", Range(0.3, 2)) = 0.85
        _WidthMax ("Width Scale at 1", Range(0.3, 2.5)) = 1.25
        _SafeMargin ("Safe Margin (fade to rect edge)", Range(0.01, 0.2)) = 0.06

        [Header(Look)]
        _Intensity ("Intensity", Range(0, 4)) = 1
        _TurbStrength ("Turbulence Strength", Range(0, 2)) = 1
        _TurbScale ("Turbulence Scale", Range(0.3, 3)) = 1
        _Flicker ("Combustion Flicker", Range(0, 1.5)) = 0.5
        _DiamondContrast ("Mach Diamond Contrast", Range(0, 2)) = 1
        _DiamondSpacing ("Mach Diamond Spacing", Range(0.03, 0.2)) = 0.07
        _DiamondDecay ("Mach Diamond Decay", Range(0.2, 6)) = 1.6
        _SparkAmount ("Sparks / Embers", Range(0, 2)) = 1
        _GlowIntensity ("In-shader Glow (fake bloom)", Range(0, 3)) = 1
        _BloomBoost ("Real Bloom Mode (0 = soft clip, 1 = raw HDR)", Range(0, 1)) = 0
        [Toggle(_ION_PRESET)] _IonPreset ("Ion / Hall thruster preset", Float) = 0

        [Header(Colors Cool Stable burn)]
        [HDR] _ColOuter ("Outer", Color) = (0.30, 0.16, 0.85, 1)
        [HDR] _ColMid ("Mid", Color) = (0.25, 0.60, 1.0, 1)
        [HDR] _ColCore ("Core", Color) = (0.70, 0.92, 1.2, 1)
        [HDR] _ColHot ("White hot", Color) = (1.6, 1.6, 1.7, 1)
        [Header(Colors Warm Low power)]
        [HDR] _WarmOuter ("Warm Outer", Color) = (0.50, 0.12, 0.60, 1)
        [HDR] _WarmMid ("Warm Mid", Color) = (1.0, 0.38, 0.08, 1)
        [HDR] _WarmCore ("Warm Core", Color) = (1.3, 0.80, 0.40, 1)
        [HDR] _ColSpark ("Spark", Color) = (2.0, 1.0, 0.45, 1)

        // UI plumbing
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
        [HideInInspector] _TextureSampleAdd ("Unused", Vector) = (0, 0, 0, 0)
        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend (1 = additive, 10 = premultiplied alpha)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
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
        Blend [_SrcBlend] [_DstBlend]
        ColorMask [_ColorMask]

        Pass
        {
            Name "EnginePlume"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma shader_feature_local _ _ION_PRESET

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Progress, _HighCycle, _HighLevel;
                float4 _Phases;
                float4 _Rect;
                float _NozzleY, _NozzleRadius, _LengthMin, _LengthMax, _WidthMin, _WidthMax, _SafeMargin;
                float _Intensity, _TurbStrength, _TurbScale, _Flicker;
                float _DiamondContrast, _DiamondSpacing, _DiamondDecay;
                float _SparkAmount, _GlowIntensity, _BloomBoost;
                float4 _ColOuter, _ColMid, _ColCore, _ColHot;
                float4 _WarmOuter, _WarmMid, _WarmCore, _ColSpark;
                float4 _ClipRect;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 posOS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.posOS = IN.positionOS.xy;     // canvas-space position (UI batching)
                OUT.color = IN.color;
                return OUT;
            }

            // ------------------------------------------------------------------
            // Periodic 3D gradient noise.
            // Axis y (flow) and z (boil/time) wrap with a period, so the CPU can
            // keep phases wrapped in [0, WRAP) forever with NO visible pop and NO
            // float precision loss after hours of idle running.
            // ------------------------------------------------------------------
            #define WRAP 256

            uint3 pcg3d(uint3 v)
            {
                v = v * 1664525u + 1013904223u;
                v.x += v.y * v.z; v.y += v.z * v.x; v.z += v.x * v.y;
                v ^= v >> 16u;
                v.x += v.y * v.z; v.y += v.z * v.x; v.z += v.x * v.y;
                return v;
            }

            int wrapi(int a, int p) { return ((a % p) + p) % p; }

            float3 hashGrad(int3 c)
            {
                uint3 u = pcg3d(asuint(c));
                return float3(u) * (2.0 / 4294967295.0) - 1.0;
            }

            float gnoise(float3 p, int perY, int perZ)
            {
                float3 i = floor(p);
                float3 f = p - i;
                int3 ii = int3(i);
                float3 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

                int x0 = ii.x, x1 = ii.x + 1;
                int y0 = wrapi(ii.y, perY), y1 = wrapi(ii.y + 1, perY);
                int z0 = wrapi(ii.z, perZ), z1 = wrapi(ii.z + 1, perZ);

                float a = dot(hashGrad(int3(x0, y0, z0)), f);
                float b = dot(hashGrad(int3(x1, y0, z0)), f - float3(1, 0, 0));
                float c = dot(hashGrad(int3(x0, y1, z0)), f - float3(0, 1, 0));
                float d = dot(hashGrad(int3(x1, y1, z0)), f - float3(1, 1, 0));
                float e = dot(hashGrad(int3(x0, y0, z1)), f - float3(0, 0, 1));
                float g = dot(hashGrad(int3(x1, y0, z1)), f - float3(1, 0, 1));
                float h = dot(hashGrad(int3(x0, y1, z1)), f - float3(0, 1, 1));
                float k = dot(hashGrad(int3(x1, y1, z1)), f - float3(1, 1, 1));

                float r = lerp(lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y),
                               lerp(lerp(e, g, u.x), lerp(h, k, u.x), u.y), u.z);
                return r * 1.5;
            }

            // fbm: every octave doubles frequency AND period, so the whole stack
            // stays periodic with WRAP in y and z.
            float fbmN(float3 p, int oct)
            {
                float s = 0.0, a = 0.5, norm = 0.0;
                int per = WRAP;
                [loop] for (int k = 0; k < oct; k++)
                {
                    s += a * gnoise(p, per, per);
                    norm += a;
                    p = float3(p.x * 2.0 + 17.1, p.y * 2.0, p.z * 2.0);
                    per *= 2;
                    a *= 0.5;
                }
                return s / norm;
            }

            float3 Ramp(float s, float3 cOuter, float3 cMid, float3 cCore, float3 cHot)
            {
                float3 c = lerp(cOuter, cMid, smoothstep(0.0, 0.45, s));
                c = lerp(c, cCore, smoothstep(0.35, 0.80, s));
                c = lerp(c, cHot, smoothstep(0.75, 1.10, s));
                return c * smoothstep(0.0, 0.12, s);   // fades to black at the cold edge
            }

            // spark layers (cell width, cell height, flow multiplier - must be integer)
            static const float SP_CW[2] = { 0.034, 0.022 };
            static const float SP_CH[2] = { 0.110, 0.060 };
            static const int   SP_K[2]  = { 3, 6 };

            float4 frag(Varyings IN) : SV_Target
            {
                // ---------- space ----------
                float2 size = max(_Rect.zw - _Rect.xy, 1e-4);
                float2 uv = (IN.posOS - _Rect.xy) / size;
                float aspect = size.x / size.y;

                // ---------- control: progress + high cycle mode ----------
                float h = smoothstep(0.0, 1.0, _HighCycle);
                // progress is IGNORED as h -> 1, replaced by a fixed power level
                float ep = saturate(lerp(_Progress, _HighLevel, h));
                float pc = ep * (1.6 - 0.6 * ep);                 // steeper at low end
                float over = smoothstep(0.85, 1.0, ep);           // overdrive
                float warmAmt = 1.0 - smoothstep(0.05, 0.55, ep);
                float diamondAmt = smoothstep(0.45, 0.80, ep) * lerp(1.0, 0.55, h);
                float sparkAmt = smoothstep(0.20, 0.70, ep);
                float instab = lerp(1.0, 0.35, smoothstep(0.0, 0.6, ep));

                float flow = _Phases.x, boil = _Phases.y, flick = _Phases.z, breath = _Phases.w;

                float3 cOuter = _ColOuter.rgb, cMid = _ColMid.rgb, cCore = _ColCore.rgb, cHot = _ColHot.rgb;
                float3 wOuter = _WarmOuter.rgb, wMid = _WarmMid.rgb, wCore = _WarmCore.rgb;
                float widthK = 1.0, lenK = 1.0;
            #if defined(_ION_PRESET)
                widthK = 0.45; lenK = 1.15;
                diamondAmt = 0.0; warmAmt = 0.0; sparkAmt *= 0.2;
                cOuter = float3(0.35, 0.18, 1.0); cMid = float3(0.45, 0.55, 1.0);
                cCore = float3(0.85, 0.90, 1.3);  cHot = float3(1.5, 1.5, 1.8);
            #endif

                // ---------- flicker / breathing ----------
                float f1 = gnoise(float3(3.7, 0.5, flick), WRAP, WRAP);
                float f2 = gnoise(float3(9.1, 2.5, flick * 5.0 + 17.3), WRAP, WRAP);
                float fl = 1.0 + _Flicker * (1.0 - h) * instab * (f1 * 0.55 + f2 * 0.25);
                float br = 1.0 + 0.035 * sin(breath) + 0.015 * sin(breath * 3.0 + 1.3);

                // ---------- plume geometry ----------
                float x = (uv.x - 0.5) * aspect;
                float tt = _NozzleY - uv.y;
                float tp = max(tt, 0.0);
                float L = (lerp(_LengthMin, _LengthMax, pc) + 0.06 * over) * lenK
                          * (1.0 + 0.35 * (fl - 1.0) + 0.03 * sin(breath + 0.7));
                float wScale = (lerp(_WidthMin, _WidthMax, pc) + 0.15 * over) * widthK;
                float r0 = _NozzleRadius * wScale;
                float w = r0 * (1.0 + 1.5 * (1.0 - exp(-tp * 10.0)))      // bell expansion
                             * (1.0 - 0.30 * smoothstep(0.0, L * 1.2, tp)); // slow taper
                w = max(w, 1e-3);

                // ---------- turbulence ----------
                float turbY = lerp(7.0, 3.5, h) * _TurbScale;   // high cycle = more laminar/streaky
                float turbAmt = smoothstep(0.0, L * 0.8, tp) * _TurbStrength * lerp(1.0, 0.6, h);
                float xn0 = x / w;
                float wn = fbmN(float3(xn0 * 1.6 * _TurbScale + 5.2, tp * turbY * 0.5 - flow, boil * 0.6), 2);
                float xw = x + wn * w * 0.45 * turbAmt;
                float xn = xw / w;
                float n = fbmN(float3(xn * 2.4 * _TurbScale, tp * turbY - flow, boil), 5);
                float n01 = saturate(n * 0.9 + 0.5);

                float q = abs(xw) / w;
                float edge = 1.0 - smoothstep(0.55, 1.0, q + n * 0.32 * turbAmt);
                float tipN = tp + n * 0.07 * L;
                float axialFade = 1.0 - smoothstep(L * 0.30, L, tipN);
                float nozzleMask = smoothstep(-0.002, 0.012, tt);
                float dens = edge * axialFade * nozzleMask;

                float rad = saturate(1.0 - q);
                float streak = lerp(0.45, 1.0, n01);

                // ---------- Mach diamonds + hot core ----------
                float spacing = _DiamondSpacing * (0.9 + 0.2 * pc) * (1.0 + 0.02 * sin(breath));
                float cell = frac((tp + 0.01 * n) / spacing);
                float u2 = abs(cell - 0.5) * 2.0;
                float dx = abs(xw) / (w * 0.62);
                float dm = pow(saturate(1.0 - (dx + u2 * 0.9)), 1.3);
                float dDecay = exp(-(tp / max(L, 0.05)) * _DiamondDecay * 2.2);
                float dIn = smoothstep(spacing * 0.4, spacing * 1.2, tp);
                float dAmt = saturate(diamondAmt * _DiamondContrast * dDecay * dIn);
                float mach = lerp(1.0, 0.30 + 1.9 * dm, dAmt);

                float coreW = w * 0.30;
                float coreR = exp(-(xw * xw) / (coreW * coreW));
                float coreL = pow(1.0 - smoothstep(0.0, L * 0.60, tipN), 1.4);
                float coreI = coreR * coreL * mach * nozzleMask;

                float body = dens * (0.25 + 0.75 * rad) * streak * lerp(1.0, 0.7 + 0.6 * dm, 0.5 * dAmt);
                float billow = dens * smoothstep(0.35, 0.95, q) * n01 * 0.5;

                // ---------- color ----------
                float T = lerp(0.55, 1.0, pc) + 0.15 * over;
                float heat = saturate((rad * 0.85 + coreI * 0.6) * (1.0 - 0.6 * saturate(tp / L))) * T;
                float3 cCool = Ramp(heat, cOuter, cMid, cCore, cHot);
                float3 cWarm = Ramp(heat, wOuter, wMid, wCore, cHot);
                float3 c = lerp(cCool, cWarm, warmAmt);

                float bright = (lerp(0.9, 1.9, pc) + 1.4 * over) * _Intensity * fl * br;
                float3 col = c * (body * 1.6 + billow * 0.9) * bright;
                col += cHot * coreI * 1.4 * bright;
                // orange shear fringe
                col += float3(1.0, 0.40, 0.10) * dens * smoothstep(0.70, 1.0, q) * 0.25
                       * (0.3 + 0.7 * (1.0 - pc)) * bright * 0.5;

                // ---------- nozzle hot spot ----------
                float yy = (tt > 0.0) ? tt * 1.2 : -tt * 3.0;
                float2 pn = float2(x, yy);
                float nozzleHot = exp(-dot(pn, pn) / (r0 * r0 * 2.2));
                col += lerp(cHot, wCore, warmAmt * 0.6) * nozzleHot * (1.2 + 1.4 * pc) * fl * _Intensity;

                // ---------- sparks / embers ----------
                float sparkThr = 0.02 + 0.16 * sparkAmt * _SparkAmount;
                float spread = exp(-pow(abs(x) / (w * 2.6 + 0.02), 2.0));
                float lifeFade = (1.0 - smoothstep(0.25 * L, 1.05 * L, tp)) * nozzleMask;
                float sparkSum = 0.0;
                [unroll] for (int li = 0; li < 2; li++)
                {
                    float2 g = float2(x / SP_CW[li], tp / SP_CH[li] - (float)SP_K[li] * flow);
                    float2 gf = floor(g);
                    int2 id = int2(gf);
                    float2 fr = g - gf;
                    int per = WRAP * SP_K[li];
                    uint3 hv = pcg3d(uint3(asuint(id.x + 4096), asuint(wrapi(id.y, per)), (uint)(li * 977 + 13)));
                    float3 r = float3(hv) * (1.0 / 4294967295.0);
                    float spawn = step(r.x, sparkThr);
                    float2 d = (fr - (0.25 + 0.5 * r.yz)) * float2(SP_CW[li], SP_CH[li]);
                    float sx = SP_CW[li] * 0.07, sy = SP_CH[li] * 0.11;
                    float e = (d.x * d.x) / (sx * sx) + (d.y * d.y) / (sy * sy);
                    sparkSum += exp(-e) * spawn * (0.4 + 0.6 * r.z);
                }
                col += _ColSpark.rgb * sparkSum * lifeFade * spread * (0.6 + 0.8 * pc) * _Intensity;

                // ---------- fake bloom (in-shader glow) ----------
                float m = max(_SafeMargin, 1e-3);
                float edgeFade = smoothstep(0.0, m, uv.x) * smoothstep(0.0, m, 1.0 - uv.x)
                               * smoothstep(0.0, m, uv.y) * smoothstep(0.0, m, 1.0 - uv.y);
                float glowK = _GlowIntensity * lerp(1.0, 0.4, _BloomBoost);
                float3 haloCol = lerp(cMid, wMid, warmAmt);
                float halo1 = exp(-length(float2(x, yy)) / (0.10 * wScale + 0.04 * pc));
                float halo2 = exp(-(x * x) / (0.18 * 0.18 * (0.7 + 0.6 * pc + 0.5 * over)))
                              * exp(-tp / (L * 0.8 + 0.05)) * smoothstep(-0.10, 0.0, tt)
                              * (0.85 + 0.30 * n01);
                float haloI = glowK * (0.25 + 0.9 * pc + 0.8 * over) * fl * br * _Intensity;
                float3 halo = haloCol * (halo1 * 0.55 + halo2 * 0.35) * haloI * edgeFade * edgeFade;

                float3 hdr = col * edgeFade + halo;

                // ---------- output ----------
                float3 soft = 1.0 - exp(-hdr * 1.15);
                float3 outc = lerp(soft, hdr, _BloomBoost);
                outc *= IN.color.rgb * IN.color.a;
                float a = saturate(max(outc.r, max(outc.g, outc.b)));

            #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, IN.posOS) * step(IN.posOS, _ClipRect.zw);
                float clipV = inside.x * inside.y;
                outc *= clipV; a *= clipV;
            #endif
            #ifdef UNITY_UI_ALPHACLIP
                clip(a - 0.001);
            #endif
                return float4(outc, a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
